using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class GlassMarble
    {
        public static ItemDef itemDef;

        // Gain 1 (+1) damage per level.
        public static ConfigurableValue<int> damagePerLevelPerStack = new(
            "Item: Glass Marbles",
            "Damage Increase",
            1,
            "Amount of base damage gained per level per stack.",
            new List<string>()
            {
                "ITEM_GLASSMARBLES_DESC"
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

            itemDef.name = "GLASS_MARBLES";
            itemDef.nameToken = "GLASS_MARBLES_NAME";
            itemDef.pickupToken = "GLASS_MARBLES_PICKUP";
            itemDef.descriptionToken = "GLASS_MARBLES_DESCRIPTION";
            itemDef.loreToken = "GLASS_MARBLES_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier2;
            });

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("GlassMarbles.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("GlassMarbles.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage
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
                        args.levelDamageAdd += count * damagePerLevelPerStack.Value;
                    }
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("GLASS_MARBLES", "Glass Marbles");
            LanguageAPI.Add("GLASS_MARBLES_NAME", "Glass Marbles");
            LanguageAPI.Add("GLASS_MARBLES_PICKUP", "Gain base damage per level.");

            string desc = $"Gain <style=cIsUtility>{damagePerLevelPerStack.Value}</style> <style=cStack>(+{damagePerLevelPerStack.Value} per stack)</style> base damage per level.";
            LanguageAPI.Add("GLASS_MARBLES_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("GLASS_MARBLES_LORE", lore);
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