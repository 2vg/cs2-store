# CS2 Store - Currency System

A new currency system has been added to the CS2 Store plugin. This system allows you to manage multiple special currencies separately from the traditional credit system.

## Feature Overview

### Multiple Currency Support
- Special points during event periods
- VIP tokens
- Achievement reward coins
- Other custom currencies

### Flexible Currency Management
- Dynamic registration of currency types
- Balance management per currency
- Purchase and sale of currency-specific items

### API Integration
- Currency operations from external plugins
- Event-based currency granting
- Conditional currency systems

## Database Structure

### New Tables

#### `store_currency_types`
Stores currency type definitions
```sql
CREATE TABLE store_currency_types (
    id INT AUTO_INCREMENT PRIMARY KEY,
    Type varchar(64) UNIQUE NOT NULL,
    DisplayName varchar(255) NOT NULL,
    Description TEXT,
    Icon varchar(255),
    IsActive BOOLEAN DEFAULT TRUE,
    DateCreated DATETIME NOT NULL
);
```

#### `store_currencies`
Stores player currency balances
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

#### `store_currency_items`
Stores items purchased with currency
```sql
CREATE TABLE store_currency_items (
    id INT AUTO_INCREMENT PRIMARY KEY,
    SteamID BIGINT UNSIGNED NOT NULL,
    CurrencyType varchar(64) NOT NULL,
    Price INT UNSIGNED NOT NULL,
    Type varchar(16) NOT NULL,
    UniqueId varchar(256) NOT NULL,
    DateOfPurchase DATETIME NOT NULL,
    DateOfExpiration DATETIME NOT NULL
);
```

## Configuration

### config.toml
```toml
[DatabaseConnection]
# Table names for currency system
StoreCurrenciesName = "store_currencies"
StoreCurrencyTypesName = "store_currency_types"
StoreCurrencyItemsName = "store_currency_items"

[Commands]
# Commands for currency system
Currency = [ "currency", "currencies", "balance" ]
GiveCurrency = [ "givecurrency" ]
RegisterCurrency = [ "registercurrency" ]
```

### Item Configuration (JSON)
```json
{
  "Event Items": {
    "name": "Event Items",
    "currency_type": "event_points",
    "Event Knife": {
      "name": "Event Knife",
      "type": "weapon",
      "uniqueid": "event_knife",
      "price": 100,
      "currency_type": "event_points",
      "weapon": "weapon_knife"
    }
  }
}
```

## Commands

### Player Commands
- `!currency` - Display owned currency balances
- `!currencies` - Same as above
- `!balance` - Same as above

### Admin Commands
- `!givecurrency <player> <currency_type> <amount>` - Give currency to a player
- `!registercurrency <type> <display_name> [description] [icon]` - Register a new currency type

## API Usage Examples

### Usage from External Plugins

```csharp
using StoreApi;

// Get API instance
var storeApi = IStoreApi.Capability.Get();
if (storeApi == null) return;

// Register currency type
var eventCurrency = new Store_CurrencyType
{
    Type = "event_points",
    DisplayName = "Event Points",
    Description = "Points earned during special events",
    IsActive = true,
    DateCreated = DateTime.Now
};
storeApi.RegisterCurrencyType(eventCurrency);

// Give currency to player
storeApi.GivePlayerCurrency(player, "event_points", 50);

// Check currency balance
int balance = storeApi.GetPlayerCurrency(player, "event_points");

// Purchase with currency
var item = storeApi.GetItem("event_knife");
if (item != null)
{
    storeApi.CurrencyItem_Purchase(player, item, "event_points");
}
```

### Event Handling
```csharp
// Utilize existing events
storeApi.OnPlayerPurchaseItem += (player, item) =>
{
    // Handle currency purchases
    if (item.ContainsKey("currency_type"))
    {
        string currencyType = item["currency_type"];
        // Custom processing
    }
};
```

## Usage Scenarios

### 1. Special Currency During Events
```csharp
// At event start
storeApi.RegisterCurrencyType(new Store_CurrencyType
{
    Type = "halloween_tokens",
    DisplayName = "Halloween Tokens",
    Description = "Halloween event exclusive currency"
});

// On mission completion
storeApi.GivePlayerCurrency(player, "halloween_tokens", 10);
```

### 2. VIP Exclusive Currency
```csharp
// Daily grant to VIP players
if (IsVipPlayer(player))
{
    storeApi.GivePlayerCurrency(player, "vip_tokens", 5);
}
```

### 3. Achievement Reward System
```csharp
// On specific achievement
if (PlayerAchievedSomething(player))
{
    storeApi.GivePlayerCurrency(player, "achievement_coins", 25);
}
```

## Menu System

The currency system is integrated with the existing menu system, allowing players to:
- Access currency-specific stores through currency selection menu
- Purchase and sell currency-exclusive items
- Check currency balances

## Important Notes

1. **Database Migration**: New tables are automatically created in existing databases
2. **Compatibility**: Operates completely independently from the existing credit system
3. **Performance**: Consider database index optimization for large numbers of currency types or transactions

## Troubleshooting

### Common Issues
1. **Currency types not showing**: Check if `IsActive` flag is set to true
2. **Cannot purchase**: Verify that `currency_type` is correctly set on items
3. **Balance not updating**: Check database connection and table permissions

### Log Verification
```
[Store] Currency system initialized
[Store] Registered currency type: event_points
[Store] Player purchased item with currency: event_points
```

## Future Expansion Plans

- Currency exchange system
- Time-limited currencies
