using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("masks")]
public class MaskEntity : BaseModel
{
    [Column("id")] public int Id { get; set; }
    [Column("img_path")] public string ImgPath { get; set; }
}

[Table("mask_locales")]
public class MaskLocaleEntity : BaseModel
{
    [Column("id")] public int Id { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageCode { get; set; }
    [Column("name")] public string Name { get; set; }
    [Column("description")] public string Description { get; set; }
}
