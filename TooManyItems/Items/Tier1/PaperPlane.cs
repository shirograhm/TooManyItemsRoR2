using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class PaperPlane
    {
        public static ItemDef itemDef;

        // Increase movement speed by 18% while airborne.
        public static ConfigurableValue<float> movespeedIncrease = new(
            "Item: Paper Plane",
            "Movement Speed Increase",
            18f,
            "Percent movement speed gained per stack while airborne.",
            new List<string>()
            {
                "ITEM_PAPERPLANE_DESC"
            }
        );
        public static float movespeedIncreasePercent = movespeedIncrease.Value / 100f;

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

            itemDef.name = "PAPER_PLANE";
            itemDef.nameToken = "PAPER_PLANE_NAME";
            itemDef.pickupToken = "PAPER_PLANE_PICKUP";
            itemDef.descriptionToken = "PAPER_PLANE_DESCRIPTION";
            itemDef.loreToken = "PAPER_PLANE_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier1;
            });

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("PaperPlane.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("PaperPlane.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility
            };
        }

        public static void Hooks()
        {
            On.RoR2.CharacterBody.FixedUpdate += (orig, self) =>
            {
                orig(self);

                if (self && self.inventory)
                {
                    if (self.inventory.GetItemCount(itemDef) > 0)
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
                    if (count > 0 && sender.characterMotor && !sender.characterMotor.isGrounded)
                    {
                        args.moveSpeedMultAdd += movespeedIncreasePercent * count;
                    }
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("PAPER_PLANE", "Paper Plane");
            LanguageAPI.Add("PAPER_PLANE_NAME", "Paper Plane");
            LanguageAPI.Add("PAPER_PLANE_PICKUP", "Gain movement speed while airborne.");

            string desc = $"Gain <style=cIsUtility>{movespeedIncrease.Value}%</style> " +
                $"<style=cStack>(+{movespeedIncrease.Value}% per stack)</style> movement speed while airborne.";
            LanguageAPI.Add("PAPER_PLANE_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("PAPER_PLANE_LORE", lore);
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