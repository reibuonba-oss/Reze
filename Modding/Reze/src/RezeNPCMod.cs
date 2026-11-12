using System;
using System.Collections.Generic;
using UnityEngine;

namespace RezeNPC
{
    public sealed class RezeNPCMod : IMod
    {
        private const string ModId = "reze_arms_dealer_vi";
        private const string SellerId = "reze_arms_dealer_seller";
        private const string PortraitKey = "reze_portrait";
        private const string PortraitTextureKey = "reze";
        private const string LastVisitDayKey = "reze_last_visit_day";

        private static ModLog _log;
        private static Texture2D _portraitTexture;
        private static Sprite _portraitSprite;

        // All IDs are vanilla item IDs with positive base values in the game item catalogue.
        private static readonly string[] FirearmCandidates =
        {
            "printed_gun",
            "handmade_pistol",
            "heavy_handmade_pistol",
            "glock_receiver",
            "smg",
            "shotgun",
            "revolver",
            "stun_gun"
        };

        private static readonly string[] AmmunitionCandidates =
        {
            "small_pistol_ammo",
            "small_pistol_ammo_p",
            "small_pistol_ammo_nl",
            "heavy_pistol_ammo",
            "heavy_pistol_ammo_p",
            "heavy_pistol_ammo_nl"
        };

        private static readonly string[] ExplosiveCandidates =
        {
            "smoke_grenade",
            "flashbang_grenade",
            "c4",
            "c4_set"
        };

        private static readonly string[] MeleeCandidates =
        {
            "tazer",
            "kitchen_knife",
            "box_cutter",
            "combat_knife",
            "kitchen_cleaver",
            "combat_machete",
            "pipe_weapon",
            "stun_baton",
            "crowbar",
            "hatchet",
            "glass_shard_shiv",
            "skinning_knife"
        };

        public void Init(ModManifest manifest)
        {
            _log = new ModLog(manifest);
            _log.Log("Reze black-market arms dealer initialized for Probably Stolen 100-Rev6.");
        }

        public void OnEnable()
        {
            StoreClientListDict.storeClientDict[SellerId] = CreateSeller;
            ModHook.OnModAssetsLoaded += OnModAssetsLoaded;
            ModHook.OnGenerateCustomerVeryLate += OnGenerateCustomer;
            _log?.Log("Registered Reze's contraband weapons merchant.");
        }

        public void OnDisable()
        {
            StoreClientListDict.storeClientDict.Remove(SellerId);
            ModHook.OnModAssetsLoaded -= OnModAssetsLoaded;
            ModHook.OnGenerateCustomerVeryLate -= OnGenerateCustomer;
            _log?.Log("Reze NPC disabled.");
        }

        private static string L(string key) => ModHelper.GetLocalized(ModId, key);

        private static void OnModAssetsLoaded(LoadedMod loadedMod)
        {
            if (loadedMod == null || loadedMod.Manifest == null || loadedMod.Manifest.ID != ModId)
                return;

            try
            {
                if (loadedMod.Textures == null || !loadedMod.Textures.ContainsKey(PortraitTextureKey))
                {
                    _log?.Warning(L("log.portrait_missing"));
                    return;
                }

                _portraitTexture = loadedMod.Textures[PortraitTextureKey];
                _portraitTexture.filterMode = FilterMode.Bilinear;
                _portraitTexture.wrapMode = TextureWrapMode.Clamp;

                if (!EnsurePortraitRegistered())
                    _log?.Warning("Reze portrait will be retried when a customer is generated.");
                else
                    _log?.Log("Reze portrait registered.");
            }
            catch (Exception ex)
            {
                _log?.Error("Could not load Reze portrait: " + ex);
            }
        }

        private static bool EnsurePortraitRegistered()
        {
            if (_portraitTexture == null || SpriteDict.Instance == null ||
                SpriteDict.Instance.spriteDictionary == null)
                return false;

            if (_portraitSprite == null)
            {
                Sprite template = SpriteDict.Instance.GetSprite("mentor");
                if (template == null || template.rect.height <= 0f)
                    return false;

                Vector2 pivot = template.pivot / template.rect.size;
                float pixelsPerUnit = template.pixelsPerUnit *
                    (_portraitTexture.height / template.rect.height);

                _portraitSprite = Sprite.Create(
                    _portraitTexture,
                    new Rect(0f, 0f, _portraitTexture.width, _portraitTexture.height),
                    pivot,
                    pixelsPerUnit);

                _portraitSprite.name = PortraitKey;
            }

            SpriteDict.Instance.spriteDictionary[PortraitKey] = _portraitSprite;
            return true;
        }

        private static StoreClient CreateSeller()
        {
            var client = new StoreClient
            {
                identifier = SellerId,
                displayName = L("reze.name"),
                spriteName = PortraitKey,
                clientFaction = "FACTION_BLACK_MARKET",
                clientIntent = StoreClient.ClientIntent.SELL,
                reportDegradesFaction = false,
                cooldown = 5
            };

            // Complete client initialization first: it may rebuild main-dialogue actions.
            // Attach StockReze only after CompleteClientCreation so the end action survives.
            client.AddBasicDialog();
            client.CompleteClientCreation();
            ModHelper.SetClientSource(client, ModId);

            client.acceptDealDialogue.SetText(client.displayName, L("reze.accept"));
            client.allDoneDialogue.SetText(client.displayName, L("reze.goodbye"));
            client.mainDialogue
                .SetText(client.displayName, L("reze.seller.1"))
                .NextDialogue()
                .SetText(client.displayName, L("reze.seller.2"))
                .NextDialogue()
                .SetText(client.displayName, L("reze.seller.3"))
                .SetEndAction(StockReze);
            return client;
        }

        private static void OnGenerateCustomer(StoreClientManager manager)
        {
            try
            {
                if (manager == null)
                    return;

                int day = StoreStation.GetDayCounter();
                if (day < 2)
                    return;

                int lastVisitDay = ModHelper.GetModDataInt(ModId, LastVisitDayKey, -1000);
                if (day - lastVisitDay < 4)
                    return;

                // Reze is an occasional black-market visitor, not a guaranteed daily customer.
                if (UnityEngine.Random.Range(0, 100) >= 45)
                    return;

                if (!EnsurePortraitRegistered())
                {
                    _log?.Warning("Skipped Reze visit because her portrait sprite is not ready.");
                    return;
                }

                if (StoreClientManager.IsClientOnCooldown(SellerId))
                    return;

                manager.AddClient(CreateSeller());
                ModHelper.SetModDataInt(ModId, LastVisitDayKey, day);
            }
            catch (Exception ex)
            {
                _log?.Error("Reze customer scheduling failed: " + ex);
            }
        }

        private static void StockReze()
        {
            if (PlayerStore.Instance == null)
            {
                _log?.Warning("Reze stock was not created because PlayerStore is not ready.");
                return;
            }

            _log?.Log("Reze stock callback fired; creating counter stock.");
            int added = 0;
            added += AddRandomContrabandStock(FirearmCandidates, 2, 3);
            added += AddRandomContrabandStock(AmmunitionCandidates, 3, 3);
            added += AddRandomContrabandStock(ExplosiveCandidates, 2, 3);
            added += AddRandomContrabandStock(MeleeCandidates, 3, 3);

            // 50% chance per visit to offer one Kurumi exclusive firearm.
            // The two items are equally likely. If the separate item mod is disabled,
            // this optional stock entry is skipped and Reze's normal shop still works.
            if (UnityEngine.Random.Range(0, 100) < 50)
            {
                string kurumiWeaponId = UnityEngine.Random.Range(0, 2) == 0
                    ? "kurumi_clockwork_long_rifle"
                    : "kurumi_clockwork_pistol";

                if (TryAddKurumiWeapon(kurumiWeaponId))
                    added++;
            }

            _log?.Log(L("log.stock_added").Replace("{0}", added.ToString()));
        }

        private static bool TryAddKurumiWeapon(string itemId)
        {
            try
            {
                // Optional integration with the separate KurumiWeapons mod.
                GameItem item = ModHelper.SpawnItem(itemId);
                // unitBaseValue is only used by special power-source items. Ordinary
                // weapons, ammunition and melee items legitimately have unitBaseValue = 0.
                if (item == null || item.identifier != itemId ||
                    item.unitValue <= 0L)
                {
                    _log?.Warning("Kurumi weapon '" + itemId + "' is unavailable or has no valid price.");
                    return false;
                }

                ContrabandHelper.InitContrabandItem(item, 3);
                item.ApplyContrabandFeatureToChildren();

                // Reze is a black-market seller. Ignore BM_DISTRUSTED/LLDISTRUSTED
                // stock-gating perks so her advertised contraband is still offered.
                if (!PlayerStore.Instance.TryAddDirectSellingItemToTable(
                    item, isOwend: false, isStolen: false, ignorePerk: true, forcedHeat: 0))
                {
                    _log?.Warning("The store refused Kurumi weapon '" + itemId + "'.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _log?.Warning("Optional Kurumi weapon stock skipped ('" + itemId + "'): " + ex.Message);
                return false;
            }
        }

        private static int AddRandomContrabandStock(string[] candidateIds, int desiredCount, int contrabandLevel)
        {
            var pool = new List<string>(candidateIds);
            int added = 0;
            int attempts = pool.Count;

            while (pool.Count > 0 && added < desiredCount && attempts-- > 0)
            {
                int index = UnityEngine.Random.Range(0, pool.Count);
                string id = pool[index];
                pool.RemoveAt(index);

                if (TryAddContrabandItem(id, contrabandLevel))
                    added++;
            }

            if (added == 0 && candidateIds.Length > 0)
                _log?.Warning(L("log.stock_missing").Replace("{0}", candidateIds[0]));

            return added;
        }

        private static bool TryAddContrabandItem(string id, int contrabandLevel)
        {
            try
            {
                // ItemSpawner creates a fully initialized live item; DirectoryMaster.Item
                // can return a directory template or an unresolved placeholder.
                GameItem item = ItemSpawner.Spawn(id);

                // unitBaseValue is reserved for special power sources; standard stock
                // items such as guns, bullets, grenades and knives normally leave it at 0.
                if (item == null || item.identifier != id ||
                    item.unitValue <= 0L)
                {
                    _log?.Warning("Rejected Reze stock '" + id + "': invalid item or zero value.");
                    return false;
                }

                // Reze's entire catalogue is contraband, including ordinary melee weapons
                // and non-lethal ammunition that are not normally flagged as contraband.
                ContrabandHelper.InitContrabandItem(item, contrabandLevel);
                item.ApplyContrabandFeatureToChildren();

                // Skip the faction distrust gate: Reze is specifically the player's
                // black-market weapons supplier and must be able to offer contraband.
                if (!PlayerStore.Instance.TryAddDirectSellingItemToTable(
                    item, isOwend: false, isStolen: false, ignorePerk: true, forcedHeat: 0))
                {
                    _log?.Warning("The store refused Reze stock '" + id + "'.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _log?.Warning("Could not add Reze stock '" + id + "': " + ex.Message);
                return false;
            }
        }
    }
}
