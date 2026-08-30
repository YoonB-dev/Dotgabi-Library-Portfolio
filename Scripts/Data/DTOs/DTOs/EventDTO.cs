using System.Collections.Generic;
using UnityEngine;

public class EventDTOList
{
    public int EventNum { get; set; }
    public string Place { get; set; }
    public List<EventDTO> EventList { get; set; } = new List<EventDTO>();
}

public class EventDTO
{
    public string ImgPath { get; set; }
    public string EventText { get; set; }
    public List<EventChoiceDTO> EventChoices { get; set; } = new List<EventChoiceDTO>();
}
