# Reze NPC — Probably Stolen 100-Rev6

A standalone Vietnamese NPC merchant mod using the Reze portrait already present in this repository.

## Stock

Reze is a black-market seller. Every item placed on her counter is explicitly initialized as contraband (contraband level 3), including items that are not normally contraband in the base game.

- **Firearms:** printed pistol, handmade pistols, receiver, SMG, shotgun, revolver and stun gun.
- **Ammunition:** .22 SR and 10 mm standard, AP and non-lethal ammunition.
- **Explosives:** smoke grenade, flashbang, C4 and activated C4.
- **Melee weapons:** taser, kitchen knife, box cutter, combat knife, cleaver, machete, pipe, stun baton, crowbar, hatchet, glass shiv and skinning knife.

Each visit stocks a random selection from all four groups: 2 firearms, 3 ammunition items, 2 explosives and 3 melee weapons.

## Safety against invalid stock

Items are created with `ItemSpawner.Spawn`, checked for matching ID and positive unit value/base value, marked contraband, and only added after `TryAddDirectSellingItemToTable` confirms acceptance. Invalid or unpriced items are skipped.

## NPC schedule

Reze starts appearing from day 2. Her visit is occasional (45% roll), with a four-day minimum spacing and the game's customer cooldown.

## Build

The GitHub Actions workflow uses the repository's root `Manager.zip` to compile against the game's managed assemblies and produces `Reze-NPC-Vietnamese.zip`.

The DLL build can be verified in CI. In-game appearance, stock generation and actual contraband inspection still require a live game test.
