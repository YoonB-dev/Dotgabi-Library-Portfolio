using Newtonsoft.Json.Linq;
using UnityEngine;

public class StoryDTO
{
    public string StoryId { get; set; }
    public string ImgPath { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class MainStoryItemDTO
{
    public int Id { get; set; }
    public EnumMainType.ProductType ItemType { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImgPath { get; set; }
    public JObject ExtraData { get; set; }
}