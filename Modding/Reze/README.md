# Reze NPC — Callback Stock Fix 1.1.2

## Root fix
- Build the base customer first with `AddBasicDialog()` and `CompleteClientCreation()`.
- Attach the custom main dialogue and `SetEndAction(StockReze)` afterwards, so the game cannot replace the stock callback while completing client initialization.
- Set the mod source with `ModHelper.SetClientSource(client, ModId)`.

## Stock
- Firearms, ammunition, explosives/grenades and melee weapons.
- Optional Kurumi weapon: 50% chance per Reze visit to add one of the two custom Kurumi guns.
- Each item is initialized as contraband and added through `TryAddDirectSellingItemToTable` with `ignorePerk: true`, bypassing faction distrust suppression.
- Ordinary firearms/ammunition/melee stock is validated using item ID and `unitValue` only; `unitBaseValue` is not required.

The callback writes a diagnostic log when it fires and how many items were added.
