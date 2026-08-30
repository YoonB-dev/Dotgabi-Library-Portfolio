using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class MainStoryDTO
{
    public int TextId { get; set; }
    public EnumTypes.MainStoryTrigger TextTrigger { get; set; }
    public EnumTypes.Target TextTarget { get; set; }
    public int? NextTextId { get; set; }
    public string StoryText { get; set; } // 로컬라이즈된 텍스트
    public JObject ExtraData { get; set; } // 추가 정보 (JSON 형태로 저장)
    public List<MainStoryChooseDTO> ChooseList { get; set; } // 선택지 목록
}

public class MainStoryChooseDTO
{
    public int Id { get; set; }
    public string ChooseText { get; set; } // 로컬라이즈된 텍스트
    public int? NextTextId { get; set; } // 어떤 선택지를 고르면 나오는지
    public List<MainStoryResultDTO> ResultList { get; set; } // 선택지 결과 목록
    public JObject ExtraData { get; set; } // 추가 정보 (JSON 형태로 저장)
}

public class MainStoryResultDTO
{
    public int Id { get; set; }
    public string ResultText { get; set; } // 로컬라이즈된 텍스트
    public int? NextTextId { get; set; } // 어떤 선택지를 고르면 나오는지
    public JObject ExtraData { get; set; } // 추가 정보 (JSON 형태로 저장)
}
