using System.Threading.Tasks;
using Supabase;

public class SupabaseClientProvider : Singleton<SupabaseClientProvider>
{
    public Client Client { get; private set; }
    public Client ClientGameData { get; private set; }

    public async Task InitializeAsync()
    {
        if (Client != null)
        {
            return; // Already initialized
        }

        var options = new SupabaseOptions {
            AutoRefreshToken = true,
            AutoConnectRealtime = false,
        };


        Client = new Client(Config.SUPABASE_URL, Config.SUPABASE_KEY, options);
        await Client.InitializeAsync();
    }

    public async Task InitializeGameDataAsync()
    {
        if (ClientGameData != null)
        {
            return; // Already initialized
        }

        var options = new SupabaseOptions {
            Schema = "game_data",
        };

        ClientGameData = new Client(Config.SUPABASE_URL, Config.SUPABASE_KEY, options);
        await ClientGameData.InitializeAsync();
    }
}
