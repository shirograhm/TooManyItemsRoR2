using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class RubberDucky
    {
        public static ItemDef itemDef;

        // Gain 5 (+5) armor.
        public static ConfigurableValue<int> armorPerStack = new(
            "Item: Rubber Ducky",
            "Armor Increase",
            5,
            "Amount of flat armor gained per stack.",
            new List<string>()
            {
                "ITEM_ARMORITEM_DESC"
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

            itemDef.name = "RUBBER_DUCKY";
            itemDef.nameToken = "RUBBER_DUCKY_NAME";
            itemDef.pickupToken = "RUBBER_DUCKY_PICKUP";
            itemDef.descriptionToken = "RUBBER_DUCKY_DESCRIPTION";
            itemDef.loreToken = "RUBBER_DUCKY_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier1;
            });

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("RubberDucky.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("RubberDucky.prefab");
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
                        args.armorAdd += count * armorPerStack.Value;
                    }
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("RUBBER_DUCKY", "Rubber Ducky");
            LanguageAPI.Add("RUBBER_DUCKY_NAME", "Rubber Ducky");
            LanguageAPI.Add("RUBBER_DUCKY_PICKUP", "Gain flat armor.");

            string desc = $"Gain <style=cIsUtility>{armorPerStack.Value}</style> <style=cStack>(+{armorPerStack.Value} per stack)</style> armor.";
            LanguageAPI.Add("RUBBER_DUCKY_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("RUBBER_DUCKY_LORE", lore);
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