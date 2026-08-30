using System.Collections.Generic;
using UnityEngine;

public class InGameData : Singleton<InGameData>
{
    public List<CardDTO> Cards;
    public List<JobDTO> Jobs;
    public List<ArtifactDTO> Artifacts;
    public List<EnemyDTO> Enemys;
    public List<EnemyTextDTO> EnemyTexts; // 적 대화 정보 추가
    public List<StoryDTO> Stories;
    public List<EventDTOList> Events;
    public List<EventSmallDTO> EventSmalls; // 작은 이벤트 정보 추가
    public List<ShopItemDTO> ShopItems;
    public List<ShopItemDTO> FrameShopItems;
    public List<ShopItemDTO> DecoShopItems;
    public List<ShopItemDTO> CharacterShopItems;
    public List<StatusDTO> Buffs;
    public List<StatusDTO> Debuffs;
    public List<LogDTO> Logs; // 로그 데이터 추가
    public List<AchieveDTOList> AchieveDTOLists; // 업적 데이터 추가

    // 도깨비 키 데이터
    public List<DotgabiKeyDTO> DotgabiKeys;

    // 도사 전용
    public List<SummonDTO> Summons;

    // 탈춤꾼 전용 (마스크)
    public List<MaskDTO> Masks;

    // 메인 스토리 텍스트
    public List<MainStoryDTO> MainStoryTexts;
    public List<MainStoryItemDTO> MainStoryItems; // 메인 스토리 아이템 데이터 추가
}
