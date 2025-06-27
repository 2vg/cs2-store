using CounterStrikeSharp.API.Core;
using static Store.Store;
using static StoreApi.Store;
using static Store.Config_Config;
using Store.Extension;

namespace Store;

public static class CurrencyItem
{
    public static bool Purchase(CCSPlayerController player, Dictionary<string, string> item, string currencyType)
    {
        if (!item.TryGetValue("price", out string? priceStr) || !int.TryParse(priceStr, out int price))
        {
            return false;
        }

        if (!item.TryGetValue("uniqueid", out string? uniqueId) || string.IsNullOrEmpty(uniqueId))
        {
            return false;
        }

        if (!item.TryGetValue("type", out string? type) || string.IsNullOrEmpty(type))
        {
            return false;
        }

        int currentAmount = Currency.Get(player, currencyType);
        if (currentAmount < price)
        {
            return false;
        }

        if (!Currency.Spend(player, currencyType, price))
        {
            return false;
        }

        Store_CurrencyItem currencyItem = new()
        {
            SteamID = player.SteamID,
            CurrencyType = currencyType,
            Price = price,
            Type = type,
            UniqueId = uniqueId,
            DateOfPurchase = DateTime.Now,
            DateOfExpiration = item.TryGetValue("duration", out string? durationStr) && int.TryParse(durationStr, out int duration) && duration > 0
                ? DateTime.Now.AddSeconds(duration)
                : DateTime.MinValue
        };

        Instance.GlobalStoreCurrencyItems.Add(currencyItem);

        Database.SaveCurrencyItem(player, currencyItem);

        Store_Item regularItem = new()
        {
            SteamID = player.SteamID,
            Price = price,
            Type = type,
            UniqueId = uniqueId,
            DateOfPurchase = currencyItem.DateOfPurchase,
            DateOfExpiration = currencyItem.DateOfExpiration
        };

        Instance.GlobalStorePlayerItems.Add(regularItem);
        Database.SavePlayerItem(player, regularItem);

        return true;
    }

    public static bool Sell(CCSPlayerController player, Dictionary<string, string> item, string currencyType)
    {
        if (!item.TryGetValue("uniqueid", out string? uniqueId) || string.IsNullOrEmpty(uniqueId))
        {
            return false;
        }

        Store_CurrencyItem? currencyItem = Instance.GlobalStoreCurrencyItems
            .FirstOrDefault(i => i.SteamID == player.SteamID && i.UniqueId == uniqueId && i.CurrencyType == currencyType);

        if (currencyItem == null)
        {
            return false;
        }

        int sellPrice = (int)(currencyItem.Price * Config.Settings.SellRatio);

        Currency.Give(player, currencyType, sellPrice);

        Instance.GlobalStoreCurrencyItems.Remove(currencyItem);

        Database.RemoveCurrencyItem(player, currencyItem);

        Store_Item? regularItem = Instance.GlobalStorePlayerItems
            .FirstOrDefault(i => i.SteamID == player.SteamID && i.UniqueId == uniqueId);

        if (regularItem != null)
        {
            Instance.GlobalStorePlayerItems.Remove(regularItem);
            Database.RemovePlayerItem(player, regularItem);
        }

        return true;
    }

    public static List<Store_CurrencyItem> GetPlayerItems(CCSPlayerController player, string? currencyType = null)
    {
        var query = Instance.GlobalStoreCurrencyItems.Where(i => i.SteamID == player.SteamID);

        if (!string.IsNullOrEmpty(currencyType))
        {
            query = query.Where(i => i.CurrencyType == currencyType);
        }

        return query.ToList();
    }

    public static bool PlayerHas(CCSPlayerController player, string currencyType, string uniqueId)
    {
        return Instance.GlobalStoreCurrencyItems
            .Any(i => i.SteamID == player.SteamID && i.CurrencyType == currencyType && i.UniqueId == uniqueId);
    }

    public static bool Give(CCSPlayerController player, Dictionary<string, string> item, string currencyType)
    {
        if (!item.TryGetValue("uniqueid", out string? uniqueId) || string.IsNullOrEmpty(uniqueId))
        {
            return false;
        }

        if (!item.TryGetValue("type", out string? type) || string.IsNullOrEmpty(type))
        {
            return false;
        }

        int price = item.TryGetValue("price", out string? priceStr) && int.TryParse(priceStr, out int p) ? p : 0;

        Store_CurrencyItem currencyItem = new()
        {
            SteamID = player.SteamID,
            CurrencyType = currencyType,
            Price = price,
            Type = type,
            UniqueId = uniqueId,
            DateOfPurchase = DateTime.Now,
            DateOfExpiration = item.TryGetValue("duration", out string? durationStr) && int.TryParse(durationStr, out int duration) && duration > 0
                ? DateTime.Now.AddSeconds(duration)
                : DateTime.MinValue
        };

        Instance.GlobalStoreCurrencyItems.Add(currencyItem);

        Database.SaveCurrencyItem(player, currencyItem);

        Store_Item regularItem = new()
        {
            SteamID = player.SteamID,
            Price = price,
            Type = type,
            UniqueId = uniqueId,
            DateOfPurchase = currencyItem.DateOfPurchase,
            DateOfExpiration = currencyItem.DateOfExpiration
        };

        Instance.GlobalStorePlayerItems.Add(regularItem);
        Database.SavePlayerItem(player, regularItem);

        return true;
    }
}