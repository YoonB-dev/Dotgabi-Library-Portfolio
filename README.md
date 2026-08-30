# CardGame / Dotgabi-Library Unity Project (Portfolio ver.)

> Unity 기반 로그라이크 덱빌딩 카드게임 포트폴리오

## 🎬 Gameplay Video

[![Gameplay Video](https://www.youtube.com/watch?v=s6zWVLyH_Q0)](https://www.youtube.com/watch?v=s6zWVLyH_Q0)

## 🖼️ Screenshots / Gallery


---

## 📌 프로젝트 개요 (Overview)

이 프로젝트는 Unity 엔진을 기반으로 한 턴제 카드 배틀 게임입니다. 플레이어는 카드 기반 행동을 수행하고, 적의 패턴을 읽으며, 유물과 버프를 조합해 전투를 진행합니다. 전투는 단순한 공격-방어를 넘어서, 상태 이상, 방어막, 회복, 스킬 발동, 전투 시작/종료 시 이벤트를 포함한 시스템 중심 구조로 설계되어 있습니다.

프로젝트는 크게 다음과 같은 흐름으로 구성됩니다.

- 메인 시나리오 / 챌린지 시나리오 진행
- 카드 획득 및 덱 관리
- 턴 기반 전투 루프
- 적 AI와 패시브/액션 처리
- 유물, 스탯, 버프/디버프 시스템
- 저장 데이터와 서버 연동 기반의 진행 관리

이 구조는 모바일 카드 RPG의 전투 로직과 시나리오 진행 시스템을 모두 포함하는 실전형 프로젝트로 볼 수 있습니다.

---

## ✨ 주요 기능 (Key Features)

### 1) 턴제 전투 시스템
- `BattleManager`, `TurnManager`, `CardSystem`을 중심으로 한 전투 루프 구성
- 플레이어 턴과 적 턴이 명확히 분리되어 있으며, 액션 수, 카드 사용 여부, 상태 정리 흐름을 관리
- 적의 공격/방어/회복/특수 행동을 순차적으로 실행

### 2) 적 AI 행동 패턴
- `Enemy` 클래스는 각 적의 상태, 공격 의도, 다음 행동, 패시브, 스킬을 관리
- `nextActions` 리스트를 통해 다음 행동을 미리 설정하고, 턴 시작 시 순서대로 실행
- 적 특성에 따라 공격, 방어막, 회복, 고유 능력 발동이 분기됩니다.
- 적의 패시브는 전투 시작, 적 피해 입힘, 턴 시작, 플레이어 방어막 획득 등 다양한 트리거에서 발동됩니다.

### 3) 카드 시스템 및 조합형 전투
- `CardSystem`에서 카드 풀, 드로우, 사용, 정렬, 저주 카드 처리 등을 관리
- 카드 업그레이드, 덱 셔플, 손패 유지, 사용 이력 추적 기능 포함
- 카드의 효과는 `CardFunction`과 `ActionRegistry`/`ConditionRegistry` 기반으로 확장 가능

### 4) 유물 / 버프 / 디버프 시스템
- `ArtifactFunction`에서 유물 효과를 트리거 시점에 맞춰 처리
- 공격력 증가, 회복, 방어막 획득, 카드 드로우, 저주 차단, 부활 등 다양한 효과 지원
- `CharacterBase`의 `StatusList`를 통해 버프와 디버프를 구조적으로 관리
- 상태 값은 누적, 조건식, 특수 효과로 확장 가능

### 5) 오브젝트 풀링 최적화
- `EffectPoolManager`는 공격/피격/이펙트 객체를 미리 생성하고 재사용하는 풀 구조를 사용
- 전투 중 연속적인 이펙트 생성 시 메모리 할당과 GC 부담을 줄이도록 설계
- 이펙트 소비 후 다시 큐에 반환하여 재사용합니다.

### 6) 시나리오 및 진행 관리
- 메인 시나리오, 챌린지 시나리오, 스토리 보스, 이벤트, 보상 구조를 포함
- `UserData`, `GameData`, `Supabase*` 연동 클래스를 통해 사용자 진행 상태와 서버 데이터 동기화
- 도깨비 키, 유물, 카드, 스테이지, 업적 정보까지 확장 가능한 구조

---

## 🏗️ 시스템 아키텍처 / 주요 코드 구조 (Architecture & Code Structure)

### 전투 흐름

```text
BattleManager
  ├─ SetScenarioData()
  ├─ SummonEnemy()
  ├─ SetBattleStart()
  └─ EndBattle()
        ↓
TurnManager
  ├─ StartTurn()
  ├─ EndTurnCo()
  └─ EnemysTurn()
        ↓
CardSystem
  ├─ SetCard()
  ├─ DrawCard()
  ├─ AddCard()
  └─ UseToCan()
        ↓
Enemy / Player / CharacterBase
  ├─ GetDamage / GetHeal / GetShield
  ├─ ApplyStatus
  └─ HP / Shield / UI sync
```

### 핵심 클래스 역할

#### `BattleManager`
- 전투 시작 시 시나리오 데이터 설정
- 적 소환, 배경 구성, 시작 페이드인/아웃 처리
- 승리/패배 흐름과 전투 종료 관리

#### `TurnManager`
- 플레이어와 적의 턴 순서를 제어
- 시작 턴 버프 정리, 카드 드로우, 행동력 계산, 적 턴 실행 담당
- 전투 종료 후 다시 플레이어 턴으로 복귀

#### `CardSystem`
- 카드 생성, 덱 셔플, 손패 정렬, 카드 사용 이력 처리
- 카드 장착/업그레이드/저주 카드 처리 포함
- 카드 개수와 드로우 로직 관리

#### `EnemyManager`
- 적 생성, 난이도 조절, 적 배열 관리, 정렬
- 스테이지 조건에 따라 보스/엘리트/일반 적 생성
- 적의 소환 위치 및 배치 로직 담당

#### `Enemy`
- 개별 적의 hp, 상태, 패시브, 행동 패턴을 관리
- `nextActions` 기반으로 공격/방어/회복/특수 능력 실행
- 피격 이펙트, 애니메이션, 사망 처리 통합

#### `Player`
- 플레이어의 체력, 방어막, 행동력, 직업 정보를 관리
- 데미지 처리, 회복 처리, 턴 시작/종료 상태 반영
- 직업별 보너스 및 변신 상태 처리 가능

#### `CharacterBase`
- 공통 캐릭터 로직 추상화
- 체력바, 방어막, 상태 아이콘, 이동 텍스트 UI 관리
- 데미지, 회복, 방어막, 상태량 처리의 공통 로직 제공

#### `EffectPoolManager`
- 공격/피격/효과 파티클 오브젝트의 재사용 풀 관리
- 전투 중 프레임 드랍을 줄이고 연속 이펙트 처리 효율화

#### `ArtifactFunction`
- 유물에 의한 각종 트리거 효과 처리
- 공격, 회복, 방어막, 행동력, 카드 드로우, 부활, 저주 차단, 카드 강화 등 구현
- 시나리오별 전투/메인 씬 효과를 통합 관리

### 데이터 계층
- `Assets/Scripts/DTOs`, `Entities`, `DAOs` 구조로 데이터 모델 분리
- `GameData`, `UserData`는 현재 시나리오 상태와 유저 진행 상태를 전역적으로 관리
- 서버 연동은 `Supabase` 관련 클래스에서 추상화되어 프로젝트 전반에 연결됨

---

## 🧩 프로젝트 구조

```text
CardGame/
├─ Assets/
│  ├─ Scripts/
│  │  ├─ Battle/
│  │  ├─ Cards/
│  │  ├─ Character/
│  │  ├─ Enemy/
│  │  ├─ Managers/
│  │  ├─ DTOs/
│  │  ├─ DAOs/
│  │  ├─ Entities/
│  │  ├─ Utils/
│  │  └─ Supabase/
│  ├─ Scenes/
│  ├─ Plugins/
│  ├─ Spine/
│  ├─ TextMesh Pro/
│  ├─ GoogleMobileAds/
│  └─ JMO Assets/
├─ Packages/
├─ ProjectSettings/
├─ Library/
├─ Build/
├─ README.md
├─ CardGame.sln
├─ ProjectVersion.txt
└─ ...
```

---

## 🔧 기술 스택 (Tech Stack)

- Unity 6000.2.12f1
- C# / Unity Script
- Unity UI Toolkit / UGUI
- DOTween
- Spine Animation
- TextMeshPro
- Unity Localization
- Supabase (서버/데이터 연동)
- Google Mobile Ads
- JMO Assets / Cartoon FX
- NuGet for Unity

이 프로젝트는 일반적인 모바일 카드 RPG 구조에 맞춰, 시각 효과, 애니메이션, 서버 데이터 동기화, 전투 루프를 모두 포함하는 구성으로 설계되었습니다.

---

## 🚀 설치 및 실행 방법 (Getting Started)

### 사전 요구사항
- Unity Hub
- Unity 6000.2.12f1 이상 설치
- Android 빌드 지원이 필요한 경우 Android Build Support 추가
- Git 설치 (선택 사항)

### 1) 저장소 클론

```bash
git clone <repository-url>
cd CardGame
```

### 2) Unity에서 프로젝트 열기
1. Unity Hub를 실행합니다.
2. `Open` 버튼을 눌러 프로젝트 루트 폴더를 선택합니다.
3. Unity가 패키지와 에셋을 자동으로 임포트할 때까지 기다립니다.
4. 프로젝트 로딩이 완료되면 씬 목록을 열어 메인 흐름 씬을 확인합니다.

### 3) 실행
- Unity Editor에서 `Play` 버튼을 눌러 게임을 실행합니다.
- 전투는 메인 시나리오 / 챌린지 시나리오 흐름을 기준으로 진행되며, 시작 화면에서 스토리와 전투 로직을 연결합니다.

### 4) Android 빌드

```text
File > Build Settings > Android > Switch Platform
```

- 프로젝트에 필요한 Signing / Keystore 설정을 구성합니다.
- `Build` 또는 `Build And Run`을 실행하면 APK/AAB 제작이 가능합니다.

> 참고: 현재 프로젝트 루트에는 `user.keystore` 관련 파일이 포함되어 있어, Android 빌드용 서명 환경을 구성할 수 있습니다.

---

## 📝 개발 메모

이 프로젝트는 단순한 카드 배틀이 아니라, 다음 요소들이 조합된 구조로 설계되어 있습니다.

- 전투 흐름과 UI 흐름 분리
- 세션 기반 상태 관리
- 캐릭터 스탯과 상태 이상 관리
- 유물에 의한 파라미터 변형
- 시나리오 진행 데이터를 기반으로 한 보상 및 성장 구조

특히 `Enemy`, `BattleManager`, `TurnManager`, `CardSystem`, `ArtifactFunction`, `EffectPoolManager`는 전체 게임의 핵심 엔진 역할을 수행하며, 기능 확장 시 가장 먼저 이해해야 하는 중심 모듈입니다.

---

## ✅ 프로젝트 요약

본 프로젝트는 Unity 기반의 턴제 카드 전투 시스템을 중심으로, 적 AI, 유물/버프, 오브젝트 풀링, 시나리오 진행, 서버 동기화까지 아우르는 게임 구조를 갖춘 프로젝트입니다. 의사결정 중심의 카드 조합과 전투 패턴 읽기, 상태 관리, 확장 가능한 데이터 구조를 핵심으로 설계되어 있으며, 향후 신규 카드, 적, 유물, 스테이지를 추가하기에 적합한 코드 구조를 가지고 있습니다.

---

## 📎 참고

- 본 프로젝트는 Unity 6000 버전 기반으로 작성되었습니다.
- 코드 구조는 대체로 `Managers`, `Enemy`, `Character`, `Cards`, `DTOs/DAOs/Entities` 패턴으로 구성되어 있습니다.
- 서버 연동 및 데이터 저장 로직은 `Supabase` 클래스를 통해 관리됩니다.

본 README는 프로젝트 이해를 위한 문서이며, 개발 환경, 시나리오 구조, 전투 로직을 빠르게 파악할 수 있도록 정리한 문서입니다.
