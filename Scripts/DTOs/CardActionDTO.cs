using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardActionDTO
{
    [field: SerializeField] public int OrderIndex { get; set; }
    [field: SerializeField] public EnumTypes.Action ActionType { get; set; }
    [field: SerializeField] public EnumTypes.Target Target { get; set; }
    [field: SerializeField] public int[] Value { get; set; }
    [field: SerializeField] public EnumTypes.EffectType Effect { get; set; }
    [field: SerializeField] public Dictionary<string, object> ExtraData { get; set; }
    public CardActionDTO DeepCopy()
    {
        return new CardActionDTO {
            OrderIndex = this.OrderIndex,
            ActionType = this.ActionType,
            Target = this.Target,
            Value = this.Value != null ? (int[])this.Value.Clone() : null, // 배열 복사
            Effect = this.Effect,
            ExtraData = this.ExtraData != null ? new Dictionary<string, object>(this.ExtraData) : null
        };
    }
}

// 중간 매핑을 위한 클래스
public class JsonCardAction
{
    public int card_id { get; set; }
    public int order_index { get; set; }
    public string action { get; set; }
    public string target { get; set; }
    public int value { get; set; }
    public int value_upgrade { get; set; }
    public int value_upgrade2 { get; set; }
    public string effect { get; set; }
    public Dictionary<string, object> extra_data { get; set; }
}
