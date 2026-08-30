using System.Threading.Tasks;

public class GameData : Singleton<GameData>
{
    public EnumMainType.ScenarioType CurrScenarioType;

    public async Task UpdateUserAndGetScenarioType()
    {
        var user = await SupabaseClientProvider.Instance.Client
            .From<UserEntity>()
            .Where(x => x.AuthId == UserManager.Instance.AuthId)
            .Single();

        if (user != null)
        {
            //CurrScenarioType = user.CurrScenarioType;
        }
    }
}

