using R2API;
using RoR2;
using System;
using System.Collections.Generic;

namespace TooManyItems
{
    internal class AncientCoin : BaseItem
    {
        // Gain more gold. Take more damage.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Ancient Coin",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_ANCIENTCOIN_DESC"
            }
        );
        public static ConfigurableValue<float> goldMultiplierPerStack = new(
            "Item: Ancient Coin",
            "Gold Multiplier",
            100f,
            "Gold generation increase as a percentage.",
            new List<string>()
            {
                "ITEM_ANCIENTCOIN_DESC"
            }
        );
        public static ConfigurableValue<float> damageMultiplierPerStack = new(
            "Item: Ancient Coin",
            "Damage Multiplier",
            25f,
            "Damage taken increase as a percentage.",
            new List<string>()
            {
                "ITEM_ANCIENTCOIN_DESC"
            }
        );
        public static float goldMultiplierAsPercent = goldMultiplierPerStack.Value / 100f;
        public static float damageMultiplierAsPercent = damageMultiplierPerStack.Value / 100f;

        internal static void Init()
        {
            GenerateItem(
                "ANCIENTCOIN",
                "AncientCoin.prefab",
                "AncientCoin.png",
                ItemTier.Lunar,
                null
            );

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        public static new void Hooks()
        {
            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.inventory == null || victimInfo.body == null) return;

                int count = victimInfo.inventory.GetItemCountPermanent(itemDef);
                if (count > 0)
                {
                    damageInfo.damage *= (1 + count * damageMultiplierAsPercent);
                }
            };

            On.RoR2.CharacterMaster.GiveMoney += (orig, self, amount) =>
            {
                if (self.inventory)
                {
                    int count = self.inventory.GetItemCountPermanent(itemDef);
                    if (count > 0)
                    {
                        float multiplier = 1 + count * goldMultiplierAsPercent;
                        amount = Convert.ToUInt32(amount * multiplier);
                    }
                }
                orig(self, amount);
            };
        }
    }
}

