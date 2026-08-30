using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("summons")]
public class SummonEntity : BaseModel
{
    [PrimaryKey("id")] [Column("id")] public int SummonId { get; set; }
    [Column("img_path")] public string ImgPath { get; set; }
}

[Table("summon_locales")]
public class SummonLocalesEntity : BaseModel
{
    [Column("id")] public int SummonId { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageCode { get; set; }
    [Column("name")] public string Name { get; set; }
    [Column("description")] public string Description { get; set; }
}
