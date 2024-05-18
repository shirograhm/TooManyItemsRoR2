using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class RedBlueGlasses
    {
        public static ItemDef itemDef;

        // Gain 6% (+6%) crit chance and 6% (+6%) crit damage.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: 3D Glasses",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_REDBLUEGLASSES_DESC"
            }
        );
        public static ConfigurableValue<float> critChancePerStack = new(
            "Item: 3D Glasses",
            "Crit Chance Increase",
            6f,
            "Amount of crit chance gained per stack.",
            new List<string>()
            {
                "ITEM_REDBLUEGLASSES_DESC"
            }
        );
        public static ConfigurableValue<float> critDamagePerStack = new(
            "Item: 3D Glasses",
            "Crit Damage Increase",
            6f,
            "Amount of crit damage gained per stack.",
            new List<string>()
            {
                "ITEM_REDBLUEGLASSES_DESC"
            }
        );
        public static float critChancePercent = critChancePerStack.Value / 100f;
        public static float critDamagePercent = critDamagePerStack.Value / 100f;

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

            itemDef.name = "REDBLUE_GLASSES";
            itemDef.nameToken = "REDBLUE_GLASSES_NAME";
            itemDef.pickupToken = "REDBLUE_GLASSES_PICKUP";
            itemDef.descriptionToken = "REDBLUE_GLASSES_DESCRIPTION";
            itemDef.loreToken = "REDBLUE_GLASSES_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier1;
            });

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("3DGlasses.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("3DGlasses.prefab");
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
                        args.critAdd += count * critChancePerStack.Value;
                        args.critDamageMultAdd += count * critDamagePercent;
                    }
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("REDBLUE_GLASSES", "3D Glasses");
            LanguageAPI.Add("REDBLUE_GLASSES_NAME", "3D Glasses");
            LanguageAPI.Add("REDBLUE_GLASSES_PICKUP", "Gain a small amount of crit chance and crit damage.");

            string desc = $"Gain <style=cIsDamage>{critChancePerStack.Value}% <style=cStack>(+{critChancePerStack.Value}% per stack)</style> crit chance</style> and " +
                $"<style=cIsDamage>{critDamagePerStack.Value}% <style=cStack>(+{critDamagePerStack.Value}% per stack)</style> crit damage</style>.";
            LanguageAPI.Add("REDBLUE_GLASSES_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("REDBLUE_GLASSES_LORE", lore);
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