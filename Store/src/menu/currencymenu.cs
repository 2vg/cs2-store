using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Interface;
using Store.Extension;
using System.Text.Json;
using static Store.Config_Config;
using static Store.MenuBase;
using static Store.Store;
using static StoreApi.Store;

namespace Store;

public static class CurrencyMenu
{
    public static void DisplayCurrencySelection(CCSPlayerController player, Dictionary<string, string> item)
    {
        BaseMenu menu = CreateMenuByType(Instance.Localizer["currency_select_title"]);
        menu.ScreenMenu_ShowResolutionsOption = true;

        List<Store_CurrencyType> availableCurrencies = Currency.GetAvailableCurrencyTypes();
        
        if (availableCurrencies.Count == 0)
        {
            player.PrintToChatMessage("currency_no_available");
            return;
        }

        foreach (Store_CurrencyType currencyType in availableCurrencies)
        {
            if (!currencyType.IsActive) continue;

            int playerBalance = Currency.Get(player, currencyType.Type);
            string itemName = $"{currencyType.DisplayName} (残高: {playerBalance})";
            
            menu.AddItem(itemName, (p, o) => 
            {
                DisplayCurrencyStore(p, currencyType.Type, false);
            });
        }

        menu.Display(player, 0);
    }

    public static void DisplayCurrencyStore(CCSPlayerController player, string currencyType, bool inventory)
    {
        Store_CurrencyType? currencyInfo = Currency.GetCurrencyType(currencyType);
        if (currencyInfo == null)
        {
            player.PrintToChatMessage("currency_type_not_found", currencyType);
            return;
        }

        int balance = Currency.Get(player, currencyType);
        string title = Instance.Localizer["currency_store_title", currencyInfo.DisplayName, balance];
        
        OpenCurrencyMenu(player, title, Instance.Config.Items, inventory, null, currencyType);
    }

    public static void OpenCurrencyMenu(CCSPlayerController player, string title, JsonElement elementData, bool inventory, IMenu? prevMenu, string currencyType)
    {
        BaseMenu menu = CreateMenuByType(title);
        menu.ScreenMenu_ShowResolutionsOption = prevMenu == null;
        menu.PrevMenu = prevMenu;

        List<JsonProperty> items = elementData.GetElementJsonProperty(["flag", "team", "currency_type"]);
        foreach (JsonProperty item in items)
        {
            if (item.Value.TryGetProperty("flag", out JsonElement flagElement) && !CheckFlag(player, flagElement.ToString(), true))
                continue;

            if (item.Value.TryGetProperty("team", out JsonElement teamElement) && player.Team.ToString() != teamElement.ToString())
                continue;

            if (item.Value.TryGetProperty("currency_type", out JsonElement currencyTypeElement))
            {
                string itemCurrencyType = currencyTypeElement.ToString();
                if (itemCurrencyType != currencyType)
                    continue;
            }
            else
            {
                continue;
            }

            if (item.Value.TryGetProperty("uniqueid", out JsonElement uniqueIdElement))
            {
                AddCurrencyItems(menu, player, uniqueIdElement, inventory, currencyType);
                continue;
            }

            if (inventory && !CurrencyItem.PlayerHas(player, currencyType, item.Name))
                continue;

            string categoryName = GetCategoryName(player, item);
            menu.AddItem(categoryName, (p, o) => OpenCurrencyMenu(p, categoryName, item.Value, inventory, menu, currencyType));
        }

        menu.Display(player, 0);
    }

    private static void AddCurrencyItems(BaseMenu menu, CCSPlayerController player, JsonElement uniqueIdElement, bool inventory, string currencyType)
    {
        Dictionary<string, string>? item = Item.GetItem(uniqueIdElement.ToString());
        if (item == null) return;

        item["currency_type"] = currencyType;

        if (inventory)
        {
            if (!CurrencyItem.PlayerHas(player, currencyType, item["uniqueid"]))
                return;

            AddCurrencyInventoryItem(menu, player, item, currencyType);
        }
        else
        {
            AddCurrencyStoreItem(menu, player, item, currencyType);
        }
    }

    private static void AddCurrencyStoreItem(BaseMenu menu, CCSPlayerController player, Dictionary<string, string> item, string currencyType)
    {
        if (!item.TryGetValue("price", out string? priceStr) || !int.TryParse(priceStr, out int price))
            return;

        Store_CurrencyType? currencyInfo = Currency.GetCurrencyType(currencyType);
        string currencySymbol = currencyInfo?.DisplayName ?? currencyType;

        int playerBalance = Currency.Get(player, currencyType);
        bool canAfford = playerBalance >= price;

        string itemName = $"{item.GetValueOrDefault("name", "Unknown")} - {price} {currencySymbol}";
        if (!canAfford)
            itemName += Instance.Localizer["currency_insufficient_suffix"];

        menu.AddItem(itemName, (p, o) =>
        {
            if (!canAfford)
            {
                p.PrintToChatMessage("currency_insufficient_funds", currencySymbol);
                return;
            }

            if (CurrencyItem.Purchase(p, item, currencyType))
            {
                p.PrintToChatMessage("currency_purchase_success", item["name"], price, currencySymbol);
                Api.PlayerPurchaseItem(p, item);
            }
            else
            {
                p.PrintToChatMessage("currency_purchase_failed");
            }
        });
    }

    private static void AddCurrencyInventoryItem(BaseMenu menu, CCSPlayerController player, Dictionary<string, string> item, string currencyType)
    {
        string itemName = item.GetValueOrDefault("name", "Unknown");
        
        menu.AddItem(itemName, (p, o) =>
        {
            DisplayCurrencyItemActions(p, item, currencyType);
        });
    }

    private static void DisplayCurrencyItemActions(CCSPlayerController player, Dictionary<string, string> item, string currencyType)
    {
        BaseMenu menu = CreateMenuByType(Instance.Localizer["currency_item_actions", item.GetValueOrDefault("name", "Unknown")]);

        bool isEquipped = Item.PlayerUsing(player, item["type"], item["uniqueid"]);
        string equipText = isEquipped ? Instance.Localizer["currency_unequip"] : Instance.Localizer["currency_equip"];
        
        menu.AddItem(equipText, (p, o) =>
        {
            if (isEquipped)
            {
                if (Item.Unequip(p, item, true))
                {
                    p.PrintToChatMessage("currency_unequipped", item["name"]);
                    Api.PlayerUnequipItem(p, item);
                }
            }
            else
            {
                if (Item.Equip(p, item))
                {
                    p.PrintToChatMessage("currency_equipped", item["name"]);
                    Api.PlayerEquipItem(p, item);
                }
            }
        });

        if (Config.Menu.EnableSelling)
        {
            menu.AddItem(Instance.Localizer["currency_sell"], (p, o) =>
            {
                if (CurrencyItem.Sell(p, item, currencyType))
                {
                    Store_CurrencyType? currencyInfo = Currency.GetCurrencyType(currencyType);
                    string currencySymbol = currencyInfo?.DisplayName ?? currencyType;
                    int sellPrice = (int)(int.Parse(item.GetValueOrDefault("price", "0")) * Config.Settings.SellRatio);
                    
                    p.PrintToChatMessage("currency_sell_success", item["name"], sellPrice, currencySymbol);
                    Api.PlayerSellItem(p, item);
                }
                else
                {
                    p.PrintToChatMessage("currency_sell_failed");
                }
            });
        }

        menu.Display(player, 0);
    }

    private static string GetCategoryName(CCSPlayerController player, JsonProperty item)
    {
        if (item.Value.TryGetProperty("name", out JsonElement nameElement))
        {
            return Instance.Localizer.ForPlayer(player, nameElement.ToString());
        }
        return item.Name;
    }
}