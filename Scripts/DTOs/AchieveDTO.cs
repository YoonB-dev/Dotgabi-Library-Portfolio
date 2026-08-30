using System.Collections.Generic;
using UnityEngine;

public class AchieveDTO
{
    public int Id { get; set; }
    public int Level { get; set; }
    public int TargetValue { get; set; }
    public EnumMainType.CurrencyType PriceType { get; set; }
    public int PriceAmount { get; set; }
    public string Description { get; set; }
    public EnumTypes.AchieveType AchieveType { get; set; }
}

public class AchieveDTOList
{
    public List<AchieveDTO> Achieves { get; set; }
    public EnumTypes.AchieveType AchieveType { get; set; }
}