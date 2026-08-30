using UnityEngine;

[System.Serializable]
public class StatusData
{
    public StatusDTO statusDTO;
    public int statusValue;
    public StatusData(StatusDTO statusDTO, int statusValue)
    {
        this.statusDTO = statusDTO;
        this.statusValue = statusValue;
    }
}
