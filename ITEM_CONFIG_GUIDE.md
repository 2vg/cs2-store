# CS2 Store - Item Configuration Guide

This document explains how to configure items in the store's JSON configuration file (e.g., `cs2-store-example.json`).

## Basic Item Structure

Each item or category is a JSON object. Items that can be purchased must have a `type` and a `uniqueid`.

**Example of a Category with an Item:**
```json
{
  "Weapons": {
    "name": "*weapons",
    "AWP": {
      "name": "AWP",
      "type": "weapon",
      "uniqueid": "weapon_awp",
      "price": 5000,
      "weapon": "weapon_awp"
    }
  }
}
```

### Core Properties

-   `"name"`: The display name for the item or category. If the name is wrapped in `*`, it will be translated using the localization files (e.g., `*weapons` becomes "Weapons").
-   `"type"`: Defines the item's function (e.g., `weapon`, `playerskin`, `gravity`). This must correspond to a loaded item module.
-   `"uniqueid"`: A unique identifier for the item across the entire store. **Must not be duplicated.**
-   `"price"`: The cost of the item. The currency used depends on the `currency_type` field.

---

## Payment Options

You can specify whether an item costs credits or a custom currency.

### Using Credits (Default)
If the `currency_type` field is omitted, the `price` is treated as standard credits.

```json
"HE Grenade": {
  "name": "HE Grenade",
  "type": "equipment",
  "uniqueid": "hegrenade",
  "price": 300,
  "weapon": "weapon_hegrenade"
}
```

### Using Custom Currencies
Add the `"currency_type"` field to make an item require a custom currency. The value must match the **internal name** (`Type`) of a currency registered via the `!registercurrency` command.

```json
"EventCoinItem": {
  "name": "Special Event Item",
  "type": "trail",
  "uniqueid": "event_item_01",
  "price": 50,
  "currency_type": "event_coin"
}
```

---

## Free Access Conditions

These flags allow certain players to acquire and use items for free, bypassing the price. If a player meets any of these conditions, they can equip the item without purchase. If they later lose all qualifying conditions, the item will be unequipped (unless they had previously purchased it and its duration has not expired).

This system works for both credit-based and custom currency items.

### `flag` (Admin Flag)

Grants free access to players who have a specific permission flag defined in the admin framework.

-   **Value:** A string containing the required permission flag (e.g., `"@css/root"`).

**Example:** Only players with the `@css/root` flag can get this Godmode for free.
```json
"AdminOnlyGodmode": {
  "name": "Godmode (Admins)",
  "type": "godmode",
  "uniqueid": "admin_godmode",
  "price": 99999,
  "flag": "@css/root"
}
```

### `vip_only` (VIP Status)

Grants free access to any player recognized as a VIP by the **VipCore** plugin. This requires a working VipCore integration.

-   **Value:** The string `"true"`.

**Example:** All VIP players get this aura for free.
```json
"VipAura": {
  "name": "VIP Aura",
  "type": "trail",
  "uniqueid": "vip_aura_trail",
  "price": 1000,
  "vip_only": "true",
  "trail_color": "255 215 0 100"
}
```

### `vip_groups` (VIP Group Membership)

Grants free access to VIP players who belong to one of the specified VIP groups. This also requires VipCore integration.

-   **Value:** A comma-separated string of VIP group names (no spaces).

**Example:** Only VIPs in the "Gold" or "Platinum" group get this skin for free.
```json
"GoldVipSkin": {
  "name": "Gold VIP Player Skin",
  "type": "playerskin",
  "uniqueid": "skin_gold_vip",
  "price": 5000,
  "vip_groups": "Gold,Platinum"
}
```

**Note:** The conditions are checked with **OR** logic. A player only needs to meet one of the specified conditions (`flag` OR `vip_only` OR `vip_groups`) to gain free access.