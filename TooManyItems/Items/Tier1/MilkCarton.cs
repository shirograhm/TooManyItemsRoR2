using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class MilkCarton
    {
        public static ItemDef itemDef;

        // Reduce damage from elite enemies by 8% (+8% per stack).
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Milk Carton",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_MILKCARTON_DESC"
            }
        );
        public static ConfigurableValue<float> eliteDamageReduction = new(
            "Item: Milk Carton",
            "Percent Damage Reduction",
            8f,
            "Percent damage reduction agains elite enemies.",
            new List<string>()
            {
                "ITEM_MILKCARTON_DESC"
            }
        );
        public static float eliteDamageReductionPercent = eliteDamageReduction.Value / 100f;

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

            itemDef.name = "MILK_CARTON";
            itemDef.nameToken = "MILK_CARTON_NAME";
            itemDef.pickupToken = "MILK_CARTON_PICKUP";
            itemDef.descriptionToken = "MILK_CARTON_DESCRIPTION";
            itemDef.loreToken = "MILK_CARTON_LORE";

            Utils.SetItemTier(itemDef, ItemTier.Tier1);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("MilkCarton.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("MilkCarton.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility
            };
        }

        public static void Hooks()
        {
            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.inventory)
                {
                    int count = victimInfo.inventory.GetItemCount(itemDef);
                    if (attackerInfo.body && attackerInfo.body.isElite && count > 0)
                    {
                        float damageReductionPercent = 1 - (1 / (1 + (eliteDamageReductionPercent * count)));
                        damageInfo.damage *= 1 - damageReductionPercent;
                    }
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("MILK_CARTON", "Milk Carton");
            LanguageAPI.Add("MILK_CARTON_NAME", "Milk Carton");
            LanguageAPI.Add("MILK_CARTON_PICKUP", "Reduce damage taken from elites.");

            string desc = $"Reduce damage taken from elite enemies by <style=cIsUtility>{eliteDamageReduction.Value}%</style> <style=cStack>(+{eliteDamageReduction.Value}% per stack)</style>.";
            LanguageAPI.Add("MILK_CARTON_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("MILK_CARTON_LORE", lore);
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