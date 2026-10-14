using RoR2;
using System;
using TooManyItems.Managers;

namespace TooManyItems.Items.Lunar
{
    internal class AncientCoin
    {
        public static ItemDef itemDef;

        // Gain more gold. Take more damage.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Ancient Coin",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_ANCIENTCOIN_DESC"]
        );
        public static ConfigurableValue<float> goldMultiplierPerStack = new(
            "Item: Ancient Coin",
            "Gold Multiplier",
            100f,
            "Gold generation increase as a percentage.",
            ["ITEM_ANCIENTCOIN_DESC"]
        );
        public static ConfigurableValue<float> damageMultiplierPerStack = new(
            "Item: Ancient Coin",
            "Damage Multiplier",
            25f,
            "Damage taken increase as a percentage.",
            ["ITEM_ANCIENTCOIN_DESC"]
        );
        public static float goldMultiplierAsPercent = goldMultiplierPerStack.Value / 100f;
        public static float damageMultiplierAsPercent = damageMultiplierPerStack.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("AncientCoin", [ItemTag.AIBlacklist, ItemTag.Damage, ItemTag.Utility], ItemTier.Lunar);

            Hooks();
        }

        public static void Hooks()
        {
            GameEventManager.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.inventory == null || victimInfo.body == null) return;

                int count = victimInfo.inventory.GetItemCountPermanent(itemDef);
                if (count > 0)
                {
                    damageInfo.damage *= 1 + count * damageMultiplierAsPercent;
                }
            };

            // RoR2 1.5.0: GiveMoney(uint) is obsolete and forwards to GiveMoney(uint, bool bypassStatManager)
            On.RoR2.CharacterMaster.GiveMoney_uint_bool += (orig, self, amount, bypassStatManager) =>
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
                orig(self, amount, bypassStatManager);
            };
        }
    }
}

