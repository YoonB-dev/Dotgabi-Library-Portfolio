using UnityEngine;

[System.Serializable]
public class JobDTO
{
    [field: SerializeField] public int Id { get; set; }
    [field: SerializeField] public string Name { get; set; }
    [field: SerializeField] public string Description { get; set; }
    [field: SerializeField] public string ImgPath { get; set; }
    [field: SerializeField] public string ImgFacePath { get; set; }
    [field: SerializeField] public int StartHP { get; set; }
    [field: SerializeField] public int StartCoin { get; set; }
}
