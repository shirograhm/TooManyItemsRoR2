using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class BottleCap
    {
        public static ItemDef itemDef;

        // Reduce your ultimate skill cooldown by 8% (+8% per stack).
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Bottle Cap",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_BOTTLECAP_DESC"
            }
        );
        public static ConfigurableValue<float> ultimateCDR = new(
            "Item: Bottle Cap",
            "Cooldown Reduction",
            8f,
            "Percent cooldown reduction on ultimate skill.",
            new List<string>()
            {
                "ITEM_BOTTLECAP_DESC"
            }
        );
        public static float ultimateCDRPercent = ultimateCDR.Value / 100f;

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

            itemDef.name = "BOTTLE_CAP";
            itemDef.nameToken = "BOTTLE_CAP_NAME";
            itemDef.pickupToken = "BOTTLE_CAP_PICKUP";
            itemDef.descriptionToken = "BOTTLE_CAP_DESCRIPTION";
            itemDef.loreToken = "BOTTLE_CAP_LORE";

            Utils.SetItemTier(itemDef, ItemTier.Tier1);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("BottleCap.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("BottleCap.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility
            };
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        float cdr = 1 - (1 / (1 + (ultimateCDRPercent * count)));
                        args.specialCooldownMultAdd -= cdr;
                    }
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("BOTTLE_CAP", "Bottle Cap");
            LanguageAPI.Add("BOTTLE_CAP_NAME", "Bottle Cap");
            LanguageAPI.Add("BOTTLE_CAP_PICKUP", "Reduce the cooldown of your ultimate skill.");

            string desc = $"Reduce your ultimate skill cooldown by <style=cIsUtility>{ultimateCDR.Value}%</style> " +
                $"<style=cStack>(+{ultimateCDR.Value}% per stack)</style>.";
            LanguageAPI.Add("BOTTLE_CAP_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("BOTTLE_CAP_LORE", lore);
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