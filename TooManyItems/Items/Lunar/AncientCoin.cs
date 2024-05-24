using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class AncientCoin
    {
        public static ItemDef itemDef;

        // Gain 100% (+100% per stack) more gold. Take 25% (+25% per stack) more damage.
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
            GenerateItem();
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "ANCIENT_COIN";
            itemDef.nameToken = "ANCIENT_COIN_NAME";
            itemDef.pickupToken = "ANCIENT_COIN_PICKUP";
            itemDef.descriptionToken = "ANCIENT_COIN_DESCRIPTION";
            itemDef.loreToken = "ANCIENT_COIN_LORE";

            Utils.SetItemTier(itemDef, ItemTier.Lunar);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("AncientCoin.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("AncientCoin.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        public static void Hooks()
        {
            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.inventory == null || victimInfo.body == null) return;

                int count = victimInfo.inventory.GetItemCount(itemDef);
                if (count > 0)
                {
                    damageInfo.damage *= (1 + count * damageMultiplierAsPercent);
                }
            };

            On.RoR2.CharacterMaster.GiveMoney += (orig, self, amount) =>
            {
                if (self.inventory == null) return;

                int count = self.inventory.GetItemCount(itemDef);
                if (count > 0)
                {
                    float multiplier = 1 + count * goldMultiplierAsPercent;
                    amount = Convert.ToUInt32(amount * multiplier);
                }

                orig(self, amount);
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("ANCIENT_COIN", "Ancient Coin");
            LanguageAPI.Add("ANCIENT_COIN_NAME", "Ancient Coin");
            LanguageAPI.Add("ANCIENT_COIN_PICKUP", "Gain more gold. <style=cDeath>Take more damage</style>.");

            string desc = $"Gain an additional <style=cHumanObjective>{goldMultiplierPerStack.Value}% <style=cStack>(+{goldMultiplierPerStack.Value}% per stack)</style> gold</style>. " +
                $"<style=cDeath>Take {damageMultiplierPerStack.Value}% <style=cStack>(+{damageMultiplierPerStack.Value}% per stack)</style> more damage</style>.";
            LanguageAPI.Add("ANCIENT_COIN_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("ANCIENT_COIN_LORE", lore);
        }
    }
}

// Styles
// <style=cIsHealth>" + exampleValue + "</style>
// <style=cIsDamage>" + exampleValue + "</style>
// <style=cIsHealing>" + exampleValue + "</style>
// <style=cIsUtility>" + exampleValue + "</style>
// <style=cIsVoid>" + exampleValue + "</style>
// <style=cHumanObjective>" + exampleValue + "</style>
// <style=cLunarObjective>" + exampleValue + "</style>
// <style=cStack>" + exampleValue + "</style>
// <style=cWorldEvent>" + exampleValue + "</style>
// <style=cArtifact>" + exampleValue + "</style>
// <style=cUserSetting>" + exampleValue + "</style>
// <style=cDeath>" + exampleValue + "</style>
// <style=cSub>" + exampleValue + "</style>
// <style=cMono>" + exampleValue + "</style>
// <style=cShrine>" + exampleValue + "</style>
// <style=cEvent>" + exampleValue + "</style>
