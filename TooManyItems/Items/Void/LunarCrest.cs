using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class LunarCrest
    {
        public static ItemDef itemDef;

        // Gain health regen based on missing health.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Lunar Crest",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_LUNARCREST_DESC"
            }
        );
        public static ConfigurableValue<float> regenPerSecond = new(
            "Item: Lunar Crest",
            "Regen Per Second",
            2f,
            "Percentage of missing health regenerated per second.",
            new List<string>()
            {
                "ITEM_LUNARCREST_DESC"
            }
        );
        public static float regenPerSecondPercent = regenPerSecond.Value / 100f;

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

            itemDef.name = "LUNARCREST";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.VoidTier2);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("LunarCrest.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("LunarCrest.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Healing
            };
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
                        Utils.ForceRecalculate(self);
                    }
                }
            };

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        // Make sure this calculation only runs when healthFraction is below 1, not above 1
                        if (sender.healthComponent.combinedHealthFraction < 1f)
                        {
                            args.baseRegenAdd += Utils.GetHyperbolicStacking(regenPerSecondPercent, count) * sender.healthComponent.missingCombinedHealth;
                        }
                    }
                }
            };
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