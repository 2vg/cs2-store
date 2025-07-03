using CounterStrikeSharp.API.Core;
using static Store.Store;
using static StoreApi.Store;

namespace Store;

public static class Credits
{
    public static Store_Player? GetStorePlayer(CCSPlayerController player)
    {
        return Instance.GlobalStorePlayers.FirstOrDefault(p => p.SteamID == player.SteamID);
    }

    public static int Get(CCSPlayerController player)
    {
        return GetStorePlayer(player)?.Credits ?? -1;
    }

    public static int GetOriginal(CCSPlayerController player)
    {
        return GetStorePlayer(player)?.OriginalCredits ?? -1;
    }

    public static int SetOriginal(CCSPlayerController player, int credits)
    {
        Store_Player? storePlayer = GetStorePlayer(player);
        if (storePlayer == null) return -1;

        storePlayer.OriginalCredits = credits;
        return storePlayer.OriginalCredits;
    }

    public static int Set(CCSPlayerController player, int credits)
    {
        Store_Player? storePlayer = GetStorePlayer(player);
        if (storePlayer == null) return -1;

        int oldCredits = storePlayer.Credits;
        storePlayer.Credits = credits;
        
        // Save credits change to database immediately
        if (storePlayer.Credits != oldCredits)
        {
            Database.ExecuteAsync($@"
                UPDATE {Config_Config.Config.DatabaseConnection.StorePlayersName}
                SET Credits = @Credits, DateOfLastJoin = @DateOfLastJoin
                WHERE SteamID = @SteamID;
            ",
            new
            {
                Credits = storePlayer.Credits,
                DateOfLastJoin = DateTime.Now,
                SteamID = player.SteamID
            });
        }
        
        return storePlayer.Credits;
    }

    public static int Give(CCSPlayerController player, int credits)
    {
        Store_Player? storePlayer = GetStorePlayer(player);
        if (storePlayer == null) return -1;

        int oldCredits = storePlayer.Credits;
        storePlayer.Credits = Math.Max(storePlayer.Credits + credits, 0);
        
        // Save credits change to database immediately
        if (storePlayer.Credits != oldCredits)
        {
            Database.ExecuteAsync($@"
                UPDATE {Config_Config.Config.DatabaseConnection.StorePlayersName}
                SET Credits = @Credits, DateOfLastJoin = @DateOfLastJoin
                WHERE SteamID = @SteamID;
            ",
            new
            {
                Credits = storePlayer.Credits,
                DateOfLastJoin = DateTime.Now,
                SteamID = player.SteamID
            });
        }
        
        return storePlayer.Credits;
    }
}