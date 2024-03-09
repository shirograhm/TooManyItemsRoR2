using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class HereticSeal
    {
        public static ItemDef itemDef;

        // Gain 1% (+1% per stack) bonus damage for every 1% msising health.
        public static ConfigurableValue<float> damagePerMissing = new(
            "Item: Seal of the Heretic",
            "Damage Increase",
            1f,
            "Percent damage gained for each percentage of missing health.",
            new List<string>()
            {
                "ITEM_HERETICSEAL_DESC"
            }
        );

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

            itemDef.name = "HERETIC_SEAL";
            itemDef.nameToken = "HERETIC_SEAL_NAME";
            itemDef.pickupToken = "HERETIC_SEAL_PICKUP";
            itemDef.descriptionToken = "HERETIC_SEAL_DESCRIPTION";
            itemDef.loreToken = "HERETIC_SEAL_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier2;
            });

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("HereticSeal.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("HereticSeal.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        public static void Hooks()
        {
            On.RoR2.CharacterBody.FixedUpdate += (orig, self) =>
            {
                orig(self);

                if (self && self.inventory)
                {
                    int count = self.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        self.RecalculateStats();
                    }
                }
            };

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender == null || sender.inventory == null) return;

                int count = sender.inventory.GetItemCount(itemDef);
                if (count > 0)
                {
                    args.damageMultAdd += count * damagePerMissing.Value * (1 - sender.healthComponent.combinedHealthFraction);
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("HERETIC_SEAL", "Seal of the Heretic");
            LanguageAPI.Add("HERETIC_SEAL_NAME", "Seal of the Heretic");
            LanguageAPI.Add("HERETIC_SEAL_PICKUP", "Gain scaling damage based on missing health.");

            string desc = $"Gain <style=cIsUtility>{damagePerMissing.Value}%</style> <style=cStack>(+{damagePerMissing.Value}% per stack)</style> bonus damage for every 1% missing health.";
            LanguageAPI.Add("HERETIC_SEAL_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("HERETIC_SEAL_LORE", lore);
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