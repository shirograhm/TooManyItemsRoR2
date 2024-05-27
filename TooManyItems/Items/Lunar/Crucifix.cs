using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class Crucifix
    {
        public static ItemDef itemDef;

        // Reduce damage taken by 40%. Taking damage set you on fire for 5% max health over 3 seconds.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Crucifix",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_CRUCIFIX_DESC"
            }
        );
        public static ConfigurableValue<float> damageReduction = new(
            "Item: Crucifix",
            "Damage Reduction",
            40f,
            "Percentage of damage reduced.",
            new List<string>()
            {
                "ITEM_CRUCIFIX_DESC"
            }
        );
        public static ConfigurableValue<float> maxHealthBurnAmount = new(
            "Item: Crucifix",
            "Burn Amount",
            5f,
            "Percentage of max health taken over the duration of the burn.",
            new List<string>()
            {
                "ITEM_CRUCIFIX_DESC"
            }
        );
        public static ConfigurableValue<int> fireDuration = new(
            "Item: Crucifix",
            "Duration of Fire",
            3,
            "Duration of fire debuff after taking damage.",
            new List<string>()
            {
                "ITEM_CRUCIFIX_DESC"
            }
        );
        public static float damageReductionPercent = damageReduction.Value / 100f;

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

            itemDef.name = "CRUCIFIX";
            itemDef.nameToken = "CRUCIFIX_NAME";
            itemDef.pickupToken = "CRUCIFIX_PICKUP";
            itemDef.descriptionToken = "CRUCIFIX_DESCRIPTION";
            itemDef.loreToken = "CRUCIFIX_LORE";

            Utils.SetItemTier(itemDef, ItemTier.Lunar);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Crucifix.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Crucifix.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        public static void Hooks()
        {
            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.inventory == null || victimInfo.body == null || attackerInfo.body == null) return;

                int count = victimInfo.inventory.GetItemCount(itemDef);
                if (count > 0 && attackerInfo.body != victimInfo.body)
                {
                    damageInfo.damage *= (1 - damageReductionPercent);

                    InflictDotInfo dotInfo = new()
                    {
                        victimObject = victimInfo.body.gameObject,
                        attackerObject = victimInfo.body.gameObject,
                        totalDamage = victimInfo.body.healthComponent.fullCombinedHealth * maxHealthBurnAmount.Value / 100f,
                        dotIndex = DotController.DotIndex.Burn,
                        duration = fireDuration.Value * count,
                        damageMultiplier = 1f
                    };
                    DotController.InflictDot(ref dotInfo);
                }
            };
        }
        private static void AddTokens()
        {
            LanguageAPI.Add("CRUCIFIX", "Crucifix");
            LanguageAPI.Add("CRUCIFIX_NAME", "Crucifix");
            LanguageAPI.Add("CRUCIFIX_PICKUP", "Reduce damage taken. <style=cDeath>Taking damage sets you on fire</style>.");

            string desc = $"Reduce damage taken by <style=cIsUtility>{damageReduction.Value}%</style>. " +
                $"<style=cDeath>Taking damage burns you for {maxHealthBurnAmount.Value}% max health over " +
                $"<style=cIsUtility>{fireDuration.Value} <style=cStack>(+{fireDuration.Value} per stack)</style> seconds</style></style>.";
            LanguageAPI.Add("CRUCIFIX_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("CRUCIFIX_LORE", lore);
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