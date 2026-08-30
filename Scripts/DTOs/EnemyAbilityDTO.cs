using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class EnemyAbilityDTO
{
    [JsonProperty("text")] public string Text { get; set; }
    [JsonProperty("abilities")] public List<EnemyAbilityDetailDTO> Abilities { get; set; }
}

[System.Serializable]
public class EnemyAbilityDetailDTO
{
    [JsonProperty("ability_type")] public EnumTypes.EnemyActionType Type { get; set; }
    [JsonProperty("target")] public EnumTypes.Target Target { get; set; }
    [JsonProperty("value")] public int Value { get; set; }
    [JsonProperty("extra_data")] public Dictionary<string, object> ExtraData { get; set; }
}
