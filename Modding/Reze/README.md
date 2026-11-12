# Reze NPC — Vietnamese Portrait Update 1.1.3

## Appearance and dialogue
- Replaces the NPC portrait at `Textures/reze.png` with the new transparent portrait supplied by the user.
- All Reze dialogue and mod log strings are Vietnamese.
- The game currently does not list Vietnamese (`vi`) as a supported locale code. To ensure Vietnamese is actually displayed, the same Vietnamese strings are placed in `en.csv` as the fallback language, and in `vi.csv` for completeness.

## Stock and behavior
- Firearms, ammunition, explosives/grenades and melee weapons.
- Optional Kurumi weapon: 50% chance per Reze visit to add one of the two custom Kurumi guns.
- Each item is initialized as contraband and added through `TryAddDirectSellingItemToTable` with `ignorePerk: true`, bypassing faction distrust suppression.
- Ordinary firearms/ammunition/melee stock is validated using item ID and `unitValue` only; `unitBaseValue` is not required.
- Seller dialogue callback remains attached after `CompleteClientCreation()` so the game does not replace it while initializing Reze.

## Install
Replace the previous Reze folder with this build. Keep the separate `KurumiWeapons` mod enabled to receive Kurumi's exclusive firearms.
