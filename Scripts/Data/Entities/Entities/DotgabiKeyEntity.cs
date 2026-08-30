using System.Collections.Generic;
using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("dotgabi_key")]
public class DotgabiKeyEntity : BaseModel
{
    [PrimaryKey("key_id")]
    [Column("key_id")] public int KeyId { get; set; }
    [Column("img_path")] public string ImgPath { get; set; }
    [JsonProperty("dotgabi_key_locales")] public List<DotgabiKeyLocaleEntity> KeyLocales { get; set; } // 로컬라이즈된 텍스트 정보
}

[Table("dotgabi_key_locales")]
public class DotgabiKeyLocaleEntity : BaseModel
{
    [Column("key_id")] public int KeyId { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanCode { get; set; }
    [Column("name")] public string KeyName { get; set; }
    [Column("description")] public string KeyDescription { get; set; }
    [Column("flavor_text")] public string FlavorText { get; set; }
}