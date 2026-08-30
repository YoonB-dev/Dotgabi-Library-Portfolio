using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("statuses")]
public class StatusEntity : BaseModel
{
    [Column("id")] public int Id { get; set; }
    [Column("status_type")] public EnumTypes.Status StatusType { get; set; }
    [Column("img_path")] public string ImgPath { get; set; }
}

[Table("statuses_locales")]
public class StatusLocaleEntity : BaseModel
{
    [Column("id")] public int Id { get; set; }
    [Column("status_type")] public EnumTypes.Status StatusType { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageCode { get; set; }
    [Column("status_name")] public string Name { get; set; }
    [Column("status_description")] public string Description { get; set; }
}
