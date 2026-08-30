using UnityEngine;

public class EventSmallDTO
{
    public int Id { get; set; }
    public int AmountMin { get; set; }
    public int AmountMax { get; set; }
    public EnumTypes.EventSmallType EventType { get; set; }
    public string Text { get; set; }
}
