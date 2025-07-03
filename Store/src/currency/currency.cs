using CounterStrikeSharp.API.Core;
using static Store.Store;
using static StoreApi.Store;

namespace Store;

public static class Currency
{
    public static Store_Player? GetStorePlayer(CCSPlayerController player)
    {
        return Instance.GlobalStorePlayers.FirstOrDefault(p => p.SteamID == player.SteamID);
    }

    public static int Get(CCSPlayerController player, string currencyType)
    {
        Store_Player? storePlayer = GetStorePlayer(player);
        if (storePlayer == null) return -1;

        Store_PlayerCurrency? currency = Instance.GlobalStorePlayerCurrencies
            .FirstOrDefault(c => c.SteamID == player.SteamID && c.CurrencyType == currencyType);

        return currency?.Amount ?? 0;
    }

    public static int GetOriginal(CCSPlayerController player, string currencyType)
    {
        Store_Player? storePlayer = GetStorePlayer(player);
        if (storePlayer == null) return -1;

        Store_PlayerCurrency? currency = Instance.GlobalStorePlayerCurrencies
            .FirstOrDefault(c => c.SteamID == player.SteamID && c.CurrencyType == currencyType);

        return currency?.OriginalAmount ?? 0;
    }

    public static int SetOriginal(CCSPlayerController player, string currencyType, int amount)
    {
        Store_Player? storePlayer = GetStorePlayer(player);
        if (storePlayer == null) return -1;

        Store_PlayerCurrency? currency = Instance.GlobalStorePlayerCurrencies
            .FirstOrDefault(c => c.SteamID == player.SteamID && c.CurrencyType == currencyType);

        if (currency == null)
        {
            currency = new Store_PlayerCurrency
            {
                SteamID = player.SteamID,
                CurrencyType = currencyType,
                Amount = 0,
                OriginalAmount = amount
            };
            Instance.GlobalStorePlayerCurrencies.Add(currency);
        }
        else
        {
            currency.OriginalAmount = amount;
        }

        return currency.OriginalAmount;
    }

    public static int Set(CCSPlayerController player, string currencyType, int amount)
    {
        Store_Player? storePlayer = GetStorePlayer(player);
        if (storePlayer == null) return -1;

        Store_PlayerCurrency? currency = Instance.GlobalStorePlayerCurrencies
            .FirstOrDefault(c => c.SteamID == player.SteamID && c.CurrencyType == currencyType);

        if (currency == null)
        {
            currency = new Store_PlayerCurrency
            {
                SteamID = player.SteamID,
                CurrencyType = currencyType,
                Amount = amount,
                OriginalAmount = 0
            };
            Instance.GlobalStorePlayerCurrencies.Add(currency);
        }
        else
        {
            currency.Amount = amount;
        }

        return currency.Amount;
    }

    public static int Give(CCSPlayerController player, string currencyType, int amount)
    {
        Store_Player? storePlayer = GetStorePlayer(player);
        if (storePlayer == null) return -1;

        Store_PlayerCurrency? currency = Instance.GlobalStorePlayerCurrencies
            .FirstOrDefault(c => c.SteamID == player.SteamID && c.CurrencyType == currencyType);

        if (currency == null)
        {
            currency = new Store_PlayerCurrency
            {
                SteamID = player.SteamID,
                CurrencyType = currencyType,
                Amount = Math.Max(amount, 0),
                OriginalAmount = 0
            };
            Instance.GlobalStorePlayerCurrencies.Add(currency);
        }
        else
        {
            currency.Amount = Math.Max(currency.Amount + amount, 0);
        }

        return currency.Amount;
    }

    public static bool Spend(CCSPlayerController player, string currencyType, int amount)
    {
        if (amount <= 0) return false;

        int currentAmount = Get(player, currencyType);
        if (currentAmount < amount) return false;

        Set(player, currencyType, currentAmount - amount);
        return true;
    }

    public static List<Store_PlayerCurrency> GetPlayerCurrencies(CCSPlayerController player)
    {
        return Instance.GlobalStorePlayerCurrencies
            .Where(c => c.SteamID == player.SteamID)
            .ToList();
    }

    public static List<Store_CurrencyType> GetAvailableCurrencyTypes()
    {
        return Instance.GlobalStoreCurrencyTypes.ToList();
    }

    public static bool RegisterCurrencyType(Store_CurrencyType currencyType)
    {
        if (Instance.GlobalStoreCurrencyTypes.Any(c => c.Type == currencyType.Type))
            return false;

        Instance.GlobalStoreCurrencyTypes.Add(currencyType);
        return true;
    }

    public static Store_CurrencyType? GetCurrencyType(string currencyType)
    {
        return Instance.GlobalStoreCurrencyTypes.FirstOrDefault(c => c.Type == currencyType);
    }
}