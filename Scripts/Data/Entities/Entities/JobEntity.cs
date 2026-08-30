using System.Collections.Generic;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("jobs")]
public class JobEntity : BaseModel
{
    [Column("id")] public int Id { get; set; }
    [Column("img_path")] public string ImgPath { get; set; }
    [Column("img_face_path")] public string ImageFacePath { get; set; }
    [Column("start_hp")] public int StartHP { get; set; }
    [Column("start_coin")] public int StartCoin { get; set; }
    // 조인된 데이터를 위한 속성
    [Reference(typeof(JobLocaleEntity), useInnerJoin: false)]
    public List<JobLocaleEntity> JobLocales { get; set; } = new();
}

[Table("job_locales")]
public class JobLocaleEntity : BaseModel
{
    [Column("id")] public int Id { get; set; }
    [Column("job_id")] public int JobId { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageCode { get; set; }
    [Column("job_name")] public string Name { get; set; }
    [Column("job_description")] public string Description { get; set; }
}