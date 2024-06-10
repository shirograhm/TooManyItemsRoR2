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

        // This item is given after a Lunar Revive. Lose 50% max HP and 25% BASE damage, exponentially.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Sages Curse",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_LUNARREVIVECONSUMED_DESC"
            }
        );
        public static ConfigurableValue<float> baseDamageLost = new(
            "Item: Sages Curse",
            "Damage Lost",
            25f,
            "Percent base damage reduced while holding this item (after reviving).",
            new List<string>()
            {
                "ITEM_LUNARREVIVECONSUMED_DESC"
            }
        );
        public static float baseDamageLostPercent = baseDamageLost.Value / 100f;
        
        public static ConfigurableValue<float> maxHealthLost = new(
            "Item: Sages Curse",
            "Health Lost",
            50f,
            "Percent max health lost while holding this item (after reviving).",
            new List<string>()
            {
                "ITEM_LUNARREVIVECONSUMED_DESC"
            }
        );
        public static float maxHealthLostPercent = maxHealthLost.Value / 100f;


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
                        args.damageMultAdd -= Utils.GetExponentialStacking(baseDamageLostPercent, itemCount);
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
        }
    }
}
