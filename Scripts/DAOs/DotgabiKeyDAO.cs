using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DotgabiKeyDAO : Singleton<DotgabiKeyDAO>
{
    public async Task<List<DotgabiKeyDTO>> GetDotgabiKeyByIdAsync(EnumTypes.LanguageType language)
    {
        var response = await SupabaseClientProvider.Instance.ClientGameData
            .From<DotgabiKeyEntity>()
            .Select("*, dotgabi_key_locales(*)")
            .Filter("dotgabi_key_locales.lan_code", Supabase.Postgrest.Constants.Operator.Equals, language.ToString())
            .Get();

        var keyEntity = response.Models;

        if (keyEntity == null)
        {
            Debug.LogError($"DotgabiKey not found.");
            return null;
        }

        return keyEntity.ConvertAll(key => new DotgabiKeyDTO {
            KeyId = key.KeyId,
            KeyName = key.KeyLocales.Find(locale => locale.LanCode == language)?.KeyName ?? "No name available",
            KeyDescription = key.KeyLocales.Find(locale => locale.LanCode == language)?.KeyDescription ?? "No description available",
            FlavorText = key.KeyLocales.Find(locale => locale.LanCode == language)?.FlavorText ?? "No flavor text available",
            ImgPath = key.ImgPath
        });
    }
}
