using UnityEngine;

public class UserClearAchieveDTO
{
    public int AchieveId { get; set; }
}

public class UserAchieveCurrDataDTO
{
    public int MoveForwardCount { get; set; }
    public int BattleCount { get; set; }
    public int ShopPurchaseCount { get; set; }
    public int RestCount { get; set; }
    public int ShowAdCount { get; set; }
    public int TotalUseCard { get; set; }
    public int TotalCoinUse { get; set; } // 사용한 코인의 총합
}

public class UserAchievePriceGetDTO
{
    public bool BigPrice1 { get; set; }
    public bool BigPrice2 { get; set; }
    public bool BigPrice3 { get; set; }
    public bool BigPrice4 { get; set; }
}