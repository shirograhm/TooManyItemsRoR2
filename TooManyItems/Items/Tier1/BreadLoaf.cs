using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class BreadLoaf
    {
        public static ItemDef itemDef;

        // Reduce fall damage by 11% (+11% per stack).
        public static ConfigurableValue<float> fallDamageReduction = new(
            "Item: Loaf of Bread",
            "Fall Damage Reduction",
            11f,
            "Percentage of fall damage reduced per stack.",
            new List<string>()
            {
                "ITEM_BREADLOAF_DESC"
            }
        );
        public static float fallDamageReductionPercent = fallDamageReduction.Value / 100f;

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

            itemDef.name = "BREAD_LOAF";
            itemDef.nameToken = "BREAD_LOAF_NAME";
            itemDef.pickupToken = "BREAD_LOAF_PICKUP";
            itemDef.descriptionToken = "BREAD_LOAF_DESCRIPTION";
            itemDef.loreToken = "BREAD_LOAF_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier1;
            });

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("BreadLoaf.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("BreadLoaf.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        public static void Hooks()
        {
            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.inventory && damageInfo.damageType == RoR2.DamageType.FallDamage)
                {
                    int count = victimInfo.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        float damageReductionPercent = 1 - (1 / (1 + (fallDamageReductionPercent * count)));
                        damageInfo.damage *= 1 - damageReductionPercent;
                    }
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("BREAD_LOAF", "Loaf of Bread");
            LanguageAPI.Add("BREAD_LOAF_NAME", "Loaf of Bread");
            LanguageAPI.Add("BREAD_LOAF_PICKUP", "Reduce fall damage.");

            string desc = $"Reduce fall damage by <style=cIsUtility>{fallDamageReduction.Value}%</style> " +
                $"<style=cStack>(+{fallDamageReduction.Value}% per stack)</style>.";
            LanguageAPI.Add("BREAD_LOAF_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("BREAD_LOAF_LORE", lore);
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