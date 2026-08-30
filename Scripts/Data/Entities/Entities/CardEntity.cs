using System.Collections.Generic;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("card_view")]
public class CardEntity : BaseModel
{
    [Column("card_id")] public int Id { get; set; }
    [Column("card_type")] public EnumTypes.CardType CardType { get; set; }
    [Column("card_cost")] public int CardCost { get; set; }
    [Column("card_cost_2")] public int CardCost2 { get; set; }
    [Column("card_cost_3")] public int CardCost3 { get; set; }
    [Column("card_job")] public int[] CardJob { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageCode { get; set; }
    [Column("img_path")] public string ImgPath { get; set; }
    [Column("card_name")] public string CardName { get; set; }
    [Column("card_description")] public string CardDescription { get; set; }
    [Column("actions_data")] public List<JsonCardAction> CardActions { get; set; }
}