using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyTextDTO
{
    public int Id { get; set; }
    public int EnemyId { get; set; }
    public EnumTypes.EnemyTextType TextType { get; set; }
    public string TextValue { get; set; }
    [field: SerializeField] public Dictionary<string, object> ExtraData { get; set; }
    public List<EnemyTextChoiceDTO> Choices { get; set; }
}

[System.Serializable]
public class EnemyTextChoiceDTO
{
    public int Id { get; set; }
    public int TextId { get; set; }
    public int ChoiceOrder { get; set; }
    public int NextIndex { get; set; }
    public string ChoiceText { get; set; }
    public Dictionary<string, object> ExtraData { get; set; }
}