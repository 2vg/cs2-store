# CS2 Store - Integrated Currency System

A flexible currency system is integrated into the CS2 Store plugin. This system allows you to create and manage multiple custom currencies alongside the traditional credit system, all within a unified store interface.

## Feature Overview

### Multiple Currency Support
- Create custom currencies for various purposes:
  - Special points during event periods
  - VIP tokens
  - Achievement reward coins
  - And any other currency you can imagine.

### Unified Store Experience
- All items, whether for credits or custom currencies, are displayed in the main store menu.
- The required currency and price are clearly shown for each item (e.g., "100 Credits" or "10 Event Coins").
- A single, powerful logic backend handles all types of transactions securely.

### API Integration
- Perform currency and item operations from external plugins.
- Grant currencies based on events, achievements, or any custom logic.
- Build complex gameplay systems on top of the store's economy.

## Database Structure

### New Tables

#### `store_currency_types`
Stores the definitions for your custom currency types.
```sql
CREATE TABLE store_currency_types (
    id INT AUTO_INCREMENT PRIMARY KEY,
    Type varchar(64) UNIQUE NOT NULL,
    DisplayName varchar(255) NOT NULL,
    Description TEXT,
    IsActive BOOLEAN DEFAULT TRUE,
    DateCreated DATETIME NOT NULL
);
```

#### `store_currencies`
Stores the balance of each custom currency for every player.
```sql
CREATE TABLE store_currencies (
    id INT AUTO_INCREMENT PRIMARY KEY,
    SteamID BIGINT UNSIGNED NOT NULL,
    CurrencyType varchar(64) NOT NULL,
    Amount INT DEFAULT 0,
    LastUpdated DATETIME NOT NULL,
    UNIQUE KEY (SteamID, CurrencyType)
);
```

## Configuration

### config.toml
```toml
[DatabaseConnection]
# Table names for the currency system
StoreCurrenciesName = "store_currencies"
StoreCurrencyTypesName = "store_currency_types"

[Commands]
# Commands for the currency system
Currency = [ "currency", "currencies", "balance" ]
GiveCurrency = [ "givecurrency" ]
RegisterCurrency = [ "registercurrency" ]
```

### Item Configuration (JSON)
To make an item require a custom currency, simply add the `currency_type` field to its definition. If omitted, it will default to "credits".

```json
{
  "Event Items": {
    "name": "Event Items",
    "Event Knife": {
      "name": "Event Knife",
      "type": "weapon",
      "uniqueid": "event_knife",
      "price": 100,
      "currency_type": "event_points",
      "weapon": "weapon_knife"
    },
    "Regular Grenade": {
      "name": "HE Grenade",
      "type": "equipment",
      "uniqueid": "hegrenade",
      "price": 300,
      "weapon": "weapon_hegrenade"
    }
  }
}
```

## Commands

### Player Commands
- `!currency` / `!currencies` / `!balance` - Displays your balances for all owned custom currencies.

### Admin Commands
- `!givecurrency <player> <currency_type> <amount>` - Gives a specified amount of a custom currency to a player.
- `!registercurrency <type> <display_name> [description]` - Registers a new currency type. `type` is the internal name used in JSON files, and `display_name` is what players see in the game.

## API Usage Examples

### Usage from External Plugins

```csharp
using StoreApi;

// Get the API instance
var storeApi = IStoreApi.Capability.Get();
if (storeApi == null) return;

// --- Currency Management ---

// Register a new currency type
var eventCurrency = new Store_CurrencyType
{
    Type = "event_points",
    DisplayName = "Event Points",
    Description = "Points earned during special events",
    IsActive = true,
    DateCreated = DateTime.Now
};
storeApi.RegisterCurrencyType(eventCurrency);

// Give currency to a player
storeApi.GivePlayerCurrency(player, "event_points", 50);

// Check a player's balance
int balance = storeApi.GetPlayerCurrency(player, "event_points");

// --- Item Transactions ---

// Purchase an item (works for both credits and custom currencies)
var item = storeApi.GetItem("event_knife");
if (item != null)
{
    // The backend automatically handles which currency to deduct based on the item's `currency_type`.
    storeApi.Item_Purchase(player, item);
}
```

### Event Handling
```csharp
// Utilize existing events for all purchases
storeApi.OnPlayerPurchaseItem += (player, item) =>
{
    // Check if the purchased item used a custom currency
    if (item.TryGetValue("currency_type", out string currencyType))
    {
        // It was a custom currency purchase
        Console.WriteLine($"Player {player.PlayerName} purchased {item["name"]} with {currencyType}.");
    }
    else
    {
        // It was a credit purchase
        Console.WriteLine($"Player {player.PlayerName} purchased {item["name"]} with credits.");
    }
};
```

## Menu System

The currency system is seamlessly integrated into the main store menu.
- There are no separate menus for different currencies.
- The store menu intelligently displays the price and the required currency for each item (e.g., "Price: 100 Credits" or "Price: 10 Event Coins").
- Players can see all their currency balances in the "Credit Info" section of the main menu.

## Important Notes

1.  **Database Migration**: The new tables (`store_currencies`, `store_currency_types`) are automatically created in your existing database.
2.  **Compatibility**: The custom currency system is fully integrated with the credit system. You can have items for credits and various custom currencies side-by-side in your store.
3.  **Performance**: Consider database index optimization if you plan to have a very large number of currency types or transactions.
