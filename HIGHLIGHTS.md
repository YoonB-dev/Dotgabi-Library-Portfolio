# 코드 하이라이트 (Code Highlights)

> 약 25,000줄의 코드 중 **먼저 보시면 좋은 부분**과, 각 부분에서 **어떤 문제를 어떤 구조로 풀었는지** 정리한 문서입니다.
> 모든 링크는 해당 코드 줄로 바로 이동합니다.

## 한눈에 보기

| # | 주제 | 핵심 | 대표 코드 |
|---|---|---|---|
| 1 | **데이터 드리븐 카드 효과** | 카드 효과는 DB 행으로 정의합니다. 조건·추가 효과는 문자열 키로 레지스트리에서 찾아 실행합니다. 카드 실행 경로에 카드 ID 분기가 없습니다. | [`CardFunction.cs`](Scripts/Gameplay/CardSystem/CardFunction/CardFunction.cs#L15-L33), [`Condition/`](Scripts/Gameplay/CardSystem/CardFunction/Condition), [`Action/`](Scripts/Gameplay/CardSystem/CardFunction/Action) |
| 2 | **선택지 시스템 3종** | 콘텐츠 성격에 맞춰 이벤트(가중치 결과 묶음), 적 대화(노드 타입 트리), 메인 스토리를 나눠 설계했습니다. | [`EventDAO.cs`](Scripts/Data/DAOs/DAOs/EventDAO.cs#L83-L140), [`MysteryManager.cs`](Scripts/App/Managers/MysteryManager/MysteryManager.cs#L115), [`EnemyText.cs`](Scripts/Gameplay/Enemies/Enemy/EnemyText.cs#L89) |
| 3 | **시드 기반 결정론** | 맵, 이벤트 순서, 랜덤 결과를 시드로 재현합니다. 재접속해도 결과가 같고, 맵은 시드만 저장합니다. | [`StageGenerator.cs`](Scripts/Gameplay/Stage/StageMap/StageGenerator.cs#L21) |
| 4 | **서버 권한 진행** | 재화·체력이 바뀔 때 클라이언트는 결과 ID만 보내고, 수치는 서버가 결정합니다. | [`MysteryResult.cs`](Scripts/App/Managers/MysteryManager/MysteryResult.cs#L188-L215) |
| 5 | **트리거 기반 유물·패시브** | 트리거 18종과 효과 20종을 데이터로 조합합니다. 전투 코드에는 트리거 호출 지점만 둡니다. | [`ArtifactFunction.cs`](Scripts/App/Managers/ArtifactFunction.cs#L134) |
| 6 | **런타임 기반 구조** | Entity와 DTO를 분리하고 로케일 테이블도 따로 둡니다. 20개 테이블을 병렬로 로딩하고, 싱글톤은 생명주기별로 3종을 씁니다. | [`InGameDataLoadManager.cs`](Scripts/Data/GameData/GameData/InGameDataLoadManager.cs), [`Generics/`](Scripts/Shared/Generics/Generics) |

### 10분 읽기 순서

1. [`CardFunction.cs` L15-33](Scripts/Gameplay/CardSystem/CardFunction/CardFunction.cs#L15-L33): 조건과 액션을 레지스트리에 등록하는 부분
2. [`ShieldAmountCondition.cs`](Scripts/Gameplay/CardSystem/CardFunction/Condition/ShieldAmountCondition.cs): 조건 평가기 하나가 어떻게 생겼는지
3. [`CardFunction.cs` L236-280](Scripts/Gameplay/CardSystem/CardFunction/CardFunction.cs#L236-L280): 공격 카드에 조건→액션이 적용되는 흐름
4. [`EventDAO.cs` L83-140](Scripts/Data/DAOs/DAOs/EventDAO.cs#L83-L140): 이벤트 결과 묶음과 가중치 추첨
5. [`EnemyText.cs` L89-155](Scripts/Gameplay/Enemies/Enemy/EnemyText.cs#L89-L155): 대화 노드 타입별 처리
6. [`StageGenerator.cs`](Scripts/Gameplay/Stage/StageMap/StageGenerator.cs#L21): 시드 기반 맵 생성과 연결성 보장

---

## 1. 카드 효과: switch-case 대신 데이터와 Condition/Action 레지스트리

### 풀고 싶었던 문제

카드가 늘 때마다 `if (card.Id == 37) { ... }` 같은 분기가 붙으면, 수치 조정이나 카드 추가 하나에도 코드 수정과 빌드가 필요하고 분기도 끝없이 늘어납니다.
그래서 **카드를 코드가 아닌 데이터로 정의**하고, 코드는 그 데이터를 해석하는 역할만 맡도록 했습니다.

### 카드 = 행동(action) 행들의 목록

카드 하나는 DB(`card_view.actions_data`)에 담긴 행동 행 여러 개로 이뤄집니다.

| 필드 | 의미 |
|---|---|
| `order_index` | 실행 순서 |
| `action` | `attack` `shield` `heal` `buff` `debuff` `draw` `repeat` `upgrade` `equip` `action` `get_action_point` |
| `target` | `self` `enemy` `enemys` |
| `value` / `value_upgrade` / `value_upgrade2` | 강화 단계별 수치 (기획자가 강화 밸런스를 데이터로 조정) |
| `effect` | 재생할 이펙트 종류 (오브젝트 풀 키) |
| `extra_data` (JSON) | `condition`, `action`, `percent`, `value`, `probability` 등 수식어 |

```mermaid
flowchart LR
    DB[("card_view<br/>actions_data JSON")] --> E[CardEntity]
    E -->|CardDAO.ParseCardAction| D[CardActionDTO]
    D --> F["CardFunction<br/>행동 타입별 기본 처리"]
    F -->|"condition 키"| CR[ConditionRegistry]
    F -->|"action 키 · ref amount"| AR[ActionRegistry]
    CR --> C1[StatusCheck] & C2[SummonCount] & C3[ShieldAmount] & C4[IsChange] & C5[IsPlayInstrument]
    AR --> A1["ValueChange<br/>(percent / value)"] & A2[GetStatus]
```

- 파싱: [`CardDAO.ParseCardAction`](Scripts/Data/DAOs/DAOs/CardDAO.cs#L27). 문자열을 enum으로 바꾸고, 행 하나가 파싱에 실패해도 그 행만 건너뛰고 로그를 남깁니다.
- 실행: [`CardFunction.CardAbility`](Scripts/Gameplay/CardSystem/CardFunction/CardFunction.cs#L40-L73). `order_index` 순서대로 행을 실행합니다.

### 레지스트리: 문자열 키 → 평가기·실행기

```csharp
// CardFunction.cs
conditionRegistry.Register("status_exist",       new StatusCheckCondition());
conditionRegistry.Register("is_summon",          new SummonCountCondition());
conditionRegistry.Register("shield_amount",      new ShieldAmountCondition());
// ...
actionRegistry.Register("percent",    new ValueChangeAction());
actionRegistry.Register("value",      new ValueChangeAction());
actionRegistry.Register("get_status", new GetStatusAction());
```

```csharp
public interface IConditionEvaluator { bool Evaluate(CardActionDTO cardAction, Player player, GameObject enemy); }
public interface IActionExecutor     { void Execute(CardActionDTO cardAction, ref int amount, CharacterBase target); }
```

액션은 `ref int amount`로 피해량·방어도·회복량을 **파이프라인처럼 변형**합니다. 그래서 같은 `percent` 액션이 공격, 방어, 회복, 드로우 어디서든 그대로 재사용됩니다.

### `extra_data` 예시

파서가 해석하는 형태를 보여드리기 위한 예시입니다.

```jsonc
// 내 방어도가 10 이상이면 피해 2배
{ "condition": { "shield_amount": 10, "op": ">=" }, "action": { "percent": 2 } }

// 대상에게 특정 디버프가 있으면 상태이상 추가 부여
{ "condition": { "status_exist": { "debuff": 3, "target": "enemy" } }, "action": { "get_status": 5 } }

// 피해량 = 현재 내 방어도
{ "value": { "shield_value": "self" } }

// 손패 장수만큼 배율
{ "percent": "hand_count" }
```

### 데이터만으로 표현되는 카드 패턴

| 패턴 | 표현 방법 | 코드 |
|---|---|---|
| 조건부 추가 효과 | `condition` + `action` | [L236-260](Scripts/Gameplay/CardSystem/CardFunction/CardFunction.cs#L236-L260) |
| 조건 불만족 시 아예 발동 안 함 | `condition`만 있고 `action` 없음 | [L251-258](Scripts/Gameplay/CardSystem/CardFunction/CardFunction.cs#L251-L258) |
| 다단 히트 | `attack` 행 바로 뒤에 `repeat` 행. `repeat`에도 조건 가능 (예: 소환수가 있을 때만 N회) | [L120-141](Scripts/Gameplay/CardSystem/CardFunction/CardFunction.cs#L120-L141) |
| 확률형 카드 | 여러 행에 `probability` 가중치. 하나만 추첨해서 실행 | [L76-109](Scripts/Gameplay/CardSystem/CardFunction/CardFunction.cs#L76-L109) |
| 지연 실행 | `late_action`. 다른 행동이 다 끝난 뒤 방어도 적용 | [L448-455](Scripts/Gameplay/CardSystem/CardFunction/CardFunction.cs#L448-L455) |
| 상태 기반 수치 | `value: { status_count / shield_value / summon_count }` | [`ValueChangeAction.cs`](Scripts/Gameplay/CardSystem/CardFunction/Action/ValueChangeAction.cs) |

### 새 조건을 추가할 때 바뀌는 곳

1. `IConditionEvaluator`를 구현한 클래스 하나 추가 (예: `HpBelowCondition`)
2. `conditionRegistry.Register("hp_below", new HpBelowCondition());` 한 줄
3. DB의 `extra_data`에 `"condition": { "hp_below": 30 }` 입력

**기존 카드 코드와 다른 조건 클래스는 건드리지 않습니다.**

### 카드 UI도 같은 데이터에서 만들어집니다

- [`CardDTOToObj.SetDescription`](Scripts/Shared/Utils/Utils/CardDTOToObj.cs#L86): 설명 문구의 `^` 자리에 실제 행동 수치를 넣습니다. 강화나 버프로 값이 원본보다 바뀌면 초록·빨강으로 표시하고, 대상이 나인지 적인지에 따라 유리/불리 색을 반대로 씁니다. 그래서 **설명 문구와 실제 효과가 어긋나지 않습니다.**
- [`CardDTOToObj.SetCardAbilitys`](Scripts/Shared/Utils/Utils/CardDTOToObj.cs#L245): 카드에 들어 있는 상태이상 키워드(`get_status`, `ethereal` 등)를 읽어 키워드 설명 박스를 자동으로 만듭니다.

---

## 2. 선택지 처리: 콘텐츠 성격에 맞춘 3가지 구조

선택지가 나오는 콘텐츠는 세 종류이고, 흐름이 서로 달라 구조도 따로 설계했습니다.
**셋 다 선택지 텍스트는 `*_locales` 테이블(`lan_code`)로 분리**해서, 언어를 추가해도 코드는 바뀌지 않습니다.

### 2-1. 미스터리 이벤트: 여러 페이지 + 가중치 결과 묶음

```mermaid
flowchart LR
    EV["이벤트 (event_num)<br/>페이지 1..n (event_order)"] --> CH[선택지]
    CH --> B{"결과 묶음<br/>가중치 추첨"}
    B --> R1["결과 액션 실행<br/>hp / coin / card / artifact / battle ..."]
    B --> R2{"흐름 제어"}
    R2 -->|continue| EV
    R2 -->|end| END[종료 · 서버 기록]
    R2 -->|upgrade / delete| POP[카드 강화·삭제 팝업]
    R2 -->|mini_game_shell| MG[미니게임]
```

- **결과 묶음 (chain)**: [`EventDAO.GroupResultBundles`](Scripts/Data/DAOs/DAOs/EventDAO.cs#L83-L126)
  `result_type = chain`인 결과들을 다음 `end`/`continue`까지 하나로 묶습니다. 그래서 "코인 -30 + 유물 획득 + 종료" 같은 **복합 결과를 가중치 후보 하나로** 다룰 수 있습니다.
- **가중치 추첨**: [`GetRandomResultByWeight`](Scripts/Data/DAOs/DAOs/EventDAO.cs#L128-L140)
- **선택 가능 여부 판단**: [`MysteryManager.SetEventSelect`](Scripts/App/Managers/MysteryManager/MysteryManager.cs#L139-L161)
  결과에 코인 소모가 있는데 가진 코인이 부족하거나, `item_exist`로 요구한 유물이 없으면 해당 선택지 버튼을 비활성화합니다.
- **결과 액션 9종**: [`MysteryResult.ResultAction`](Scripts/App/Managers/MysteryManager/MysteryResult.cs#L22)
  `hp_max_get` `hp_get` `artifact_get`(지정 또는 희귀도별 랜덤) `artifact_delete` `card_get` `card_delete` `coin_get`(고정값 또는 비율) `set_next_event` `battle_enemy`
- **연계 이벤트**: `set_next_event`로 다음 미스터리 노드에 나올 이벤트를 고정합니다 ([L72](Scripts/App/Managers/MysteryManager/MysteryManager.cs#L72)). 여러 노드에 걸친 이야기(예: 사또 이벤트)를 데이터로 이어 붙일 수 있습니다.

### 2-2. 적 대화: 노드 타입 기반 대화 트리

보스와의 대화는 노드 타입(`root` `branch` `continue_` `action` `leaf`)으로 흐름을 표현합니다. [`EnemyText.SetTextBox`](Scripts/Gameplay/Enemies/Enemy/EnemyText.cs#L89-L155)

| 노드 | 동작 |
|---|---|
| `root` | 시작 노드. 여러 개면 **스토리 진행 조건**으로 하나를 고릅니다 ([L41-58](Scripts/Gameplay/Enemies/Enemy/EnemyText.cs#L41-L58), [`EnemyTextConditionChecker`](Scripts/Gameplay/Enemies/Enemy/EnemyTextConditionChecker.cs)). 같은 적이라도 이전 선택에 따라 대사가 달라집니다. |
| `branch` | 선택지를 보여주고, 고른 선택지의 `next_index`로 이동 |
| `continue_` | 화면을 터치하면 `text_id` 노드로 이동 |
| `action` | 증거 제시 단계. 가진 스토리 카드를 손패로 꺼내 설득에 씁니다 ([`EnemyTextActionHandler`](Scripts/Gameplay/Enemies/Enemy/EnemyTextActionHandler.cs)). |
| `leaf` | 대화 종료. `is_final`이면 보스전 승리로 처리 |

노드에 들어갈 때의 부수 효과도 `extra_data`로 정의합니다. [`EnemyTextChoiceResult`](Scripts/Gameplay/Enemies/Enemy/EnemyTextChoiceResult.cs)
`get_artifact`, `get_coin`/`lose_coin`(고정값 또는 비율), `spine_change`/`image_change`(적 외형 변경), `arrest_tiger`(스토리 플래그 저장)가 있습니다.
여기서 저장한 **스토리 플래그가 다시 다른 대화의 `root` 선택 조건이 되기 때문에**, 한 번의 선택이 이후 스토리 분기로 이어집니다.

### 2-3. 메인 스토리

[`MainStoryChooseManager`](Scripts/App/Managers/MainStory/MainStoryChooseManager.cs): 선택지 하나가 세 갈래 중 하나로 이어집니다.
- `next_text_id`: 다음 대사로 이동
- `result` 목록: 결과 텍스트를 보여준 뒤 이어서 진행
- `extra_data.scene_move`: 씬 전환

### 2-4. 스토리 카드 사용: 전략 패턴

같은 스토리 카드라도 놓인 씬에 따라 동작이 다릅니다. 적 설득 전투에서는 설득 포인트가 되고, 오누이 집 퍼즐에서는 도끼로 나무를 베거나 우물을 비웁니다.
[`StoryCard`](Scripts/Gameplay/Cards/Cards/StoryCard.cs#L23-L27)는 [`IStoryCardUse`](Scripts/Gameplay/Story/Story/CardUse/IStoryCardUse.cs) 인터페이스만 알고, 어떤 구현([`Battle`](Scripts/Gameplay/Story/Story/CardUse/StoryCardUseBattle.cs) / [`Onu`](Scripts/Gameplay/Story/Story/CardUse/StoryCardUseOnu.cs))이 쓰이는지는 모릅니다.

---

## 3. 시드 기반 결정론: 재접속 리롤 방지와 저장량 최소화

로그라이크 구조라서 **앱을 껐다 켜서 결과를 다시 뽑는 것**(세이브 스커밍)을 막아야 했습니다.

- **맵**: [`StageGenerator.Generate`](Scripts/Gameplay/Stage/StageMap/StageGenerator.cs#L21)
  맵 전체를 저장하지 않고 `MapSeed`만 서버에 저장합니다. 같은 시드로 같은 맵을 다시 만듭니다.
  아래 규칙으로 **도달할 수 없는 노드가 생기지 않도록** 보장합니다.
  - 각 층의 양 끝 노드는 다음 층의 양 끝 노드와 연결 ([L148](Scripts/Gameplay/Stage/StageMap/StageGenerator.cs#L148))
  - 중간 노드는 인접한 노드(i-1, i, i+1)와만 연결
  - 들어오는 간선이 없는 노드는 연결 후 따로 보정 ([L172](Scripts/Gameplay/Stage/StageMap/StageGenerator.cs#L172))
- **이벤트 순서**: 시나리오 시드로 이벤트 목록을 섞습니다 ([`SetMysteryListBySeed`](Scripts/App/Managers/MysteryManager/MysteryManager.cs#L32)).
- **이벤트의 랜덤 대상**(유물·카드 랜덤 삭제 등): `시드 + 지금까지의 선택 횟수`로 난수를 만듭니다 ([L27](Scripts/App/Managers/MysteryManager/MysteryResult.cs#L27)). 다시 접속해도 같은 결과가 나옵니다.

---

## 4. 서버 권한(Server-authoritative) 진행

- **수치는 서버가 결정**: 이벤트 결과를 적용할 때 클라이언트는 RPC로 `p_event_result_id`만 보냅니다 ([예시](Scripts/App/Managers/MysteryManager/MysteryResult.cs#L188-L215)). 얼마를 더하고 뺄지는 서버가 DB의 결과 데이터로 계산합니다. **로컬 상태는 서버가 성공을 응답한 뒤에만 바꿉니다.**
- **선택 기록**: 이벤트 선택과 클리어 횟수를 서버에 기록해서(`AddScenarioSelectList`, `IncreaseClearCount`) 같은 노드에 다시 들어가 다른 선택을 할 수 없습니다.
- **공통 호출 래퍼**: [`SupabaseWrap.ExecuteWithRefresh`](Scripts/Services/Supabase/Supabase/SupabaseWrap.cs#L10)
  JWT가 만료되면 토큰을 갱신하고 한 번 재시도합니다. 갱신에 실패하면 시작 화면으로 돌려보냅니다. 모든 RPC가 이 래퍼를 거치므로 세션 처리가 한곳에 모여 있습니다.

---

## 5. 트리거 기반 유물 · 적 패시브

- 유물 하나는 `(트리거, 효과 타입, 대상, 값, extra_data)` 데이터 조합입니다.
  트리거([`ArtifactTriggerType`](Scripts/Shared/Utils/Utils/EnumType.cs#L38) 18종)는 `on_battle_start`, `on_first_card_attack`, `on_get_damage_first`, `on_kill_enemy`, `on_shop_enter` 등이고, 효과([`ArtifaceEffectType`](Scripts/Shared/Utils/Utils/EnumType.cs#L44) 20종)와 조합합니다.
- 전투 코드에는 **"지금 이 시점이다"라는 트리거 호출만** 둡니다 (예: 공격 직전 [`ArtifactAttackEnemyFirst`](Scripts/Gameplay/CardSystem/CardFunction/CardFunction.cs#L229)).
  [`ProcessArtifactEffects`](Scripts/App/Managers/ArtifactFunction.cs#L134-L311)는 해당 트리거에 걸린 효과만 실행하고, 결과를 `DamageReceiveResult`(피해 무효, 배율, 부활, 저주 차단)로 돌려줍니다. **호출하는 쪽은 결과 객체만 해석하면 됩니다.**
- 적 패시브도 같은 방식입니다 ([`PassiveFunction`](Scripts/Gameplay/Battle/Battle/PassiveFunction.cs#L13), `EnemyPassiveTrigger` 6종).

---

## 6. 런타임 기반 구조

| 항목 | 내용 | 코드 |
|---|---|---|
| Entity / DTO 분리 | Supabase 어트리뷰트는 Entity에만 붙이고, 게임 로직은 DTO만 다룹니다. 백엔드를 바꿔도 DAO 계층만 바뀝니다. | [`Entities/`](Scripts/Data/Entities/Entities), [`DTOs/`](Scripts/Data/DTOs/DTOs), [`DAOs/`](Scripts/Data/DAOs/DAOs) |
| 병렬 로딩 | 기획 데이터 20개 테이블을 `Task.WhenAll`로 동시에 불러옵니다. | [`InGameDataLoadManager`](Scripts/Data/GameData/GameData/InGameDataLoadManager.cs) |
| 생명주기별 싱글톤 | `Singleton<T>`(순수 C#: DAO·서비스), `SceneSingleton<T>`(씬 한정 매니저), `MonoBehaviorSingleton<T>`(`DontDestroyOnLoad` 전역) | [`Generics/`](Scripts/Shared/Generics/Generics) |
| 오브젝트 풀 | 이펙트 타입별 큐를 두고, 로딩 시점에 미리 만들어 재사용합니다. | [`EffectPoolManager`](Scripts/App/Managers/EffectPoolManager.cs) |
| 커맨드 패턴 | 클릭과 드래그를 구분하는 로직(누른 위치와 뗀 위치 비교)은 베이스 클래스에 두고, 팝업 종류별로 `Execute`만 구현합니다. | [`ClickCommand`](Scripts/UI/Controls/ClickControl/ClickCommand.cs) |

---

## 7. 한계와 다음 단계

직접 짚어 본 개선점입니다.

- **기본 행동의 분기**: [`AbilityFunction`](Scripts/Gameplay/CardSystem/CardFunction/CardFunction.cs#L111-L221)은 기본 행동 12종을 아직 `switch`로 나눕니다. 조건과 수식어는 레지스트리로 옮겼으니, 행동 자체도 등록형으로 바꾸면 `CardFunction`에는 실행 순서 관리만 남습니다.
- **문자열 키의 위험**: `extra_data`가 `Dictionary<string, object>`이고 키가 문자열이라 오타가 런타임에야 드러납니다. 로딩 시점에 스키마를 검증하거나(모르는 키, 등록되지 않은 condition 키를 경고), 파싱 단계에서 타입이 있는 객체로 바꾸는 것이 다음 단계입니다.
- **수치 계산의 if 체인**: [`ValueChangeAction`](Scripts/Gameplay/CardSystem/CardFunction/Action/ValueChangeAction.cs) 안은 여전히 키별 if 체인입니다. 손패 수, 소환수 수, 방어도 같은 "기준값 공급자"를 별도 레지스트리로 뺄 수 있습니다.
- **단일 조건**: 조건은 지금 키 하나만 평가합니다(`Properties().First()`). AND/OR 조합이 필요해지면 복합 조건 평가기를 추가할 계획입니다.
- **결과 처리의 통일**: 이벤트와 적 대화의 결과 처리([`MysteryResult`](Scripts/App/Managers/MysteryManager/MysteryResult.cs), [`EnemyTextChoiceResult`](Scripts/Gameplay/Enemies/Enemy/EnemyTextChoiceResult.cs))는 아직 `switch`/`if` 방식입니다. 카드에서 검증한 레지스트리 구조로 맞출 수 있습니다.
- **테스트 격리**: 매니저끼리 싱글톤 `Instance`를 직접 참조해서 단위 테스트로 떼어내기 어렵습니다. 의존성 주입으로 바꿔 갈 계획입니다.
