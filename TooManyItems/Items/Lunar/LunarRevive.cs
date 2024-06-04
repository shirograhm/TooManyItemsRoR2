using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class LunarRevive
    {
        public static ItemDef itemDef;

        // If you would take a fatal blow, instead revive with 45% (+45% per stack) max HP. Each revive, lose 2 (+2 per stack) items.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Sages Blessing",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_LUNARREVIVE_DESC"
            }
        );
        public static ConfigurableValue<float> reviveHealthPerStack = new(
            "Item: Sages Blessing",
            "Revive Health Per Stack",
            45f,
            "Percentage of health you spawn with when you revive.",
            new List<string>()
            {
                "ITEM_LUNARREVIVE_DESC"
            }
        );
        public static ConfigurableValue<int> itemsLostPerStack = new(
            "Item: Sages Blessing",
            "Items Lost Per Stack",
            2,
            "Items lost when you revive.",
            new List<string>()
            {
                "ITEM_LUNARREVIVE_DESC"
            }
        );
        public static float reviveHealthPercent = reviveHealthPerStack.Value / 100f;

        internal static void Init()
        {
            GenerateItem();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "LUNAR_REVIVE";
            itemDef.nameToken = "LUNAR_REVIVE_NAME";
            itemDef.pickupToken = "LUNAR_REVIVE_PICKUP";
            itemDef.descriptionToken = "LUNAR_REVIVE_DESCRIPTION";
            itemDef.loreToken = "LUNAR_REVIVE_LORE";

            Utils.SetItemTier(itemDef, ItemTier.Lunar);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("LunarRevive.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("LunarRevive.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        public static void Hooks()
        {
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
