using UnityEngine;

[System.Serializable]
public class StatusDTO
{
    public int Id { get; set; }
    public EnumTypes.Status StatusType { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImgPath { get; set; }
}
