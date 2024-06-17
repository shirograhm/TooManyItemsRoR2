using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class LunarReviveConsumed
    {
        public static ItemDef itemDef;

        // This item is given after a Lunar Revive. Reduces your max health by 20%. Each stage, lose 2 (+2 per stack) items at random. If you don't have 2 items to lose, die instead.
        public static ConfigurableValue<float> maxHealthLost = new(
            "Item: Sages Curse",
            "Health Lost",
            20f,
            "Percent max health lost while holding this item (after reviving).",
            new List<string>()
            {
                "ITEM_LUNARREVIVECONSUMED_DESC"
            }
        );
        public static float maxHealthLostPercent = maxHealthLost.Value / 100f;

        public static ConfigurableValue<int> itemsLostPerStage = new(
            "Item: Sages Curse",
            "Items Lost",
            2,
            "Number of items lost on new stage (after reviving).",
            new List<string>()
            {
                "ITEM_LUNARREVIVECONSUMED_DESC"
            }
        );

        internal static void Init()
        {
            GenerateItem();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "LUNARREVIVECONSUMED";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.NoTier);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("LunarReviveConsumed.png");
            itemDef.canRemove = false;
            itemDef.hidden = false;
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int itemCount = sender.inventory.GetItemCount(itemDef);
                    if (itemCount > 0)
                    {
                        args.healthMultAdd -= Utils.GetExponentialStacking(maxHealthLostPercent, itemCount);
                    }
                }
            };

            On.RoR2.HealthComponent.GetHealthBarValues += (orig, self) =>
            {
                HealthComponent.HealthBarValues values = orig(self);
                if (self.body && self.body.inventory)
                {
                    int count = self.body.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        values.curseFraction += (1f - values.curseFraction) * Utils.GetExponentialStacking(maxHealthLostPercent, count);
                        values.healthFraction = self.health * (1f - values.curseFraction) / self.fullCombinedHealth;
                        values.shieldFraction = self.shield * (1f - values.curseFraction) / self.fullCombinedHealth;
                    }
                }
                return values;
            };

            Stage.onStageStartGlobal += (stage) =>
            {
                foreach (NetworkUser user in NetworkUser.readOnlyInstancesList)
                {
                    CharacterMaster master = user.masterController.master ?? user.master;
                    if (master && master.inventory)
                    {
                        int itemCount = master.inventory.GetItemCount(itemDef);
                        if (itemCount > 0)
                        {
                            int itemsToLose = itemCount * itemsLostPerStage;
                            
                            List<ItemIndex> list = new(master.inventory.itemAcquisitionOrder);
                            Util.ShuffleList(list);

                            foreach (ItemIndex index in list)
                            {
                                if (itemsToLose == 0) break;

                                ItemDef def = ItemCatalog.GetItemDef(index);
                                if (def && def.tier != ItemTier.NoTier && def.itemIndex != LunarRevive.itemDef.itemIndex)
                                {
                                    master.inventory.RemoveItem(def);
                                    master.inventory.GiveItem(Debris.itemDef);
                                    CharacterMasterNotificationQueue.SendTransformNotification(
                                        master, def.itemIndex, Debris.itemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);

                                    itemsToLose -= 1;
                                }
                            }

                            if (itemsToLose > 0 && user.GetCurrentBody() && user.GetCurrentBody().healthComponent)
                            {
                                user.GetCurrentBody().healthComponent.Suicide(user.gameObject, user.gameObject);
                            }
                        }
                    }

                    if (user.GetCurrentBody())
                    {
                        EffectData effectData = new EffectData
                        {
                            origin = user.GetCurrentBody().corePosition
                        };
                        effectData.SetNetworkedObjectReference(user.gameObject);
                        EffectManager.SpawnEffect(HealthComponent.AssetReferences.fragileDamageBonusBreakEffectPrefab, effectData, transmit: true);
                    }
                }
            };
        }
    }
}
