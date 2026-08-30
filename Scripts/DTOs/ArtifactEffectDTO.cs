using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[System.Serializable]
public class ArtifactEffectDTO
{
    [field: SerializeField] public EnumTypes.ArtifactTriggerType ItemTrigger { get; set; }
    [field: SerializeField] public EnumTypes.ArtifaceEffectType ItemEffectType { get; set; }
    [field: SerializeField] public EnumTypes.Target Target { get; set; }
    [field: SerializeField] public int Value { get; set; }
    [field: SerializeField] public string ValueType { get; set; }
    [field: SerializeField] public JObject ExtraData { get; set; } // Changed to object to allow mixed types
}
