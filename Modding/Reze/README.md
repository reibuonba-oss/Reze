# Reze NPC — Stock Fix 1.1.1

## Fixes
- Removed the invalid `unitBaseValue > 0` requirement. Ordinary weapons, ammunition, explosives and melee items can have `unitBaseValue = 0`; only `unitValue > 0` is required.
- Calls `TryAddDirectSellingItemToTable` with `ignorePerk: true`, so the black-market `BM_DISTRUSTED` reputation perk does not silently suppress Reze's contraband stock.
- The stock callback logs when it fires and logs each skipped/failed item.
- The optional Kurumi weapon stock uses the same bypass for the faction trust gate.

## Stock
- 2 random firearms
- 3 random ammunition types
- 2 random explosives/grenades
- 3 random melee weapons
- 50% chance for one additional Kurumi exclusive gun when KurumiWeapons is enabled

All Reze stock remains marked contraband.
