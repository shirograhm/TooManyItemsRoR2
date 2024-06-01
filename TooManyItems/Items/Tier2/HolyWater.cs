using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class HolyWater
    {
        public static ItemDef itemDef;

        // Killing an elite enemy grants all allies 1% to 100% (+50% per stack) of its max health as bonus experience, scaling with difficulty.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Holy Water",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_HOLYWATER_DESC"
            }
        );
        public static ConfigurableValue<float> minExperienceMultiplierPerStack = new(
            "Item: Holy Water",
            "Minimum XP Multiplier",
            1f,
            "Minimum enemy max health converted to bonus experience when killing an elite.",
            new List<string>()
            {
                "ITEM_HOLYWATER_DESC"
            }
        );
        public static float minExperienceMultiplierAsPercent = minExperienceMultiplierPerStack.Value / 100f;

        public static ConfigurableValue<float> maxExperienceMultiplierPerStack = new(
            "Item: Holy Water",
            "Maximum XP Multiplier",
            100f,
            "Maximum enemy max health converted to bonus experience when killing an elite.",
            new List<string>()
            {
                "ITEM_HOLYWATER_DESC"
            }
        );
        public static float maxExperienceMultiplierAsPercent = maxExperienceMultiplierPerStack.Value / 100f;

        public static ConfigurableValue<float> extraStacksMultiplier = new(
            "Item: Holy Water",
            "Extra Stack Scaling",
            50f,
            "Experience bonus for additional stacks.",
            new List<string>()
            {
                "ITEM_HOLYWATER_DESC"
            }
        );
        public static float extraStacksMultiplierPercent = extraStacksMultiplier.Value / 100f;

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

            itemDef.name = "HOLY_WATER";
            itemDef.nameToken = "HOLY_WATER_NAME";
            itemDef.pickupToken = "HOLY_WATER_PICKUP";
            itemDef.descriptionToken = "HOLY_WATER_DESCRIPTION";
            itemDef.loreToken = "HOLY_WATER_LORE";

            Utils.SetItemTier(itemDef, ItemTier.Tier2);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("HolyWater.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("HolyWater.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility
            };
        }

        public static void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, eventManager, damageReport) =>
            {
                orig(eventManager, damageReport);

                if (!NetworkServer.active) return;

                CharacterMaster atkMaster = damageReport.attackerMaster;
                CharacterBody atkBody = damageReport.attackerBody;
                CharacterBody vicBody = damageReport.victimBody;

                if (atkBody && atkMaster && vicBody && vicBody.isElite)
                {
                    // Check if attacker is minion and if we can switch to player
                    if (atkMaster.minionOwnership && atkMaster.minionOwnership.ownerMaster && atkMaster.minionOwnership.ownerMaster.hasBody)
                    {
                        atkMaster = atkMaster.minionOwnership.ownerMaster;
                        atkBody = atkMaster.GetBody();
                    }
                    if (atkBody.inventory)
                    {
                        int count = atkBody.inventory.GetItemCount(itemDef);
                        if (count > 0)
                        {
                            float bonusXP = vicBody.healthComponent.fullCombinedHealth * CalculateExperienceMultiplier(count);

                            atkMaster.GiveExperience(Convert.ToUInt64(bonusXP));
                        }
                    }
                }
            };
        }

        public static float CalculateExperienceMultiplier(int itemCount)
        {
            float difference = maxExperienceMultiplierAsPercent - minExperienceMultiplierAsPercent;
            float multiplier = minExperienceMultiplierAsPercent + difference * Utils.getDifficultyAsPercentage();

            return multiplier * (1 + extraStacksMultiplierPercent * (itemCount - 1));
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("HOLY_WATER", "Holy Water");
            LanguageAPI.Add("HOLY_WATER_NAME", "Holy Water");
            LanguageAPI.Add("HOLY_WATER_PICKUP", "Killing elite enemies grants all allies a percentage of the enemy's max HP as bonus experience.");

            string desc = $"Killing an elite enemy grants all allies <style=cIsHealth>{minExperienceMultiplierPerStack.Value}-{maxExperienceMultiplierPerStack.Value}%</style> " +
                $"<style=cStack>(+{extraStacksMultiplier.Value}% per stack)</style> of its <style=cIsHealth>max HP</style> as bonus experience. " +
                $"<style=cIsUtility>Scales over time.</style>.";
            LanguageAPI.Add("HOLY_WATER_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("HOLY_WATER_LORE", lore);
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