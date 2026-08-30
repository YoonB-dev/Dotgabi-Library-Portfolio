using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


[System.Serializable]
public class EnemyPassiveDTO
{
    [JsonProperty("passive_id")] public int PassiveId { get; set; }
    [JsonProperty("passive_text")] public string PassiveText { get; set; }
    [JsonProperty("passive_img_path")] public string PassiveImgPath { get; set; }
    [JsonProperty("abilities")] public List<EnemyPassiveAbilityDTO> PassiveAbilities { get; set; }
}


[System.Serializable]
public class EnemyPassiveAbilityDTO
{
    [JsonProperty("passive_trigger")] public EnumTypes.EnemyPassiveTrigger PassiveTrigger { get; set; }
    [JsonProperty("action")] public EnumTypes.Action Action { get; set; }
    [JsonProperty("target")] public EnumTypes.Target Target { get; set; }
    [JsonProperty("value")] public int? Value { get; set; }
    [JsonProperty("value2")] public int? Value2 { get; set; }
    [JsonProperty("value3")] public int? Value3 { get; set; }
    [JsonProperty("extra_data")] public Dictionary<string, object> ExtraData { get; set; }
}
