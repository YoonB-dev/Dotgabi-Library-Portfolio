using System.Collections.Generic;
using UnityEngine;

public class EventResultDTO
{
    public int ResultId { get; set; }
    public string ResultType { get; set; }
    public string ResultAction { get; set; }
    public int Weight { get; set; }
    public string ResultText { get; set; }
    public Dictionary<string, object> ExtraData { get; set; } = new Dictionary<string, object>();
}

public class EventResultBundle
{
    public List<EventResultDTO> ResultDTOs { get; set; }
    public string ResultText { get; set; }
    public int Weight { get; set; }
}