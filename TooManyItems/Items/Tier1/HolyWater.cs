﻿using R2API;
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

        // Upon killing an elite enemy, gain 1% (+1% per stack) of your level experience cap as bonus experience.
        public static ConfigurableValue<float> experienceMultiplierPerStack = new(
            "Item: Holy Water",
            "XP Multiplier",
            1f,
            "Bonus experience gained on elite kill as a percentage of the level cap.",
            new List<string>()
            {
                "ITEM_HOLYWATER_DESC"
            }
        );
        public static float experienceMultiplierAsPercent = experienceMultiplierPerStack.Value / 100f;

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

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier1;
            });

            itemDef.pickupIconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("HolyWater.png");
            itemDef.pickupModelPrefab = TooManyItems.MainAssets.LoadAsset<GameObject>("HolyWater.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        public static void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, eventManager, damageReport) =>
            {
                orig(eventManager, damageReport);

                if (!NetworkServer.active) return;

                CharacterMaster atkMaster = damageReport.attackerMaster;
                CharacterBody atkBody = damageReport.attackerBody;

                if (atkBody && atkMaster && atkBody.inventory && damageReport.victimBody.isElite)
                {
                    int count = atkBody.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        float hyperbolicExperienceMultiplier = 1 - (1 / (1 + (experienceMultiplierAsPercent * count)));
                        float bonusXP = GetExperienceCap(atkBody.level) * hyperbolicExperienceMultiplier;   // XP scales with killer (minion or player) level bar

                        // Reroute XP from minions to players
                        if (atkMaster && atkMaster.minionOwnership && atkMaster.minionOwnership.ownerMaster)
                        {
                            atkMaster = atkMaster.minionOwnership.ownerMaster;
                        }
                        atkMaster.GiveExperience(Convert.ToUInt64(bonusXP));
                    }
                }
            };
        }

        private static float GetExperienceCap(float level)
        {
            return (-4f / 0.11f) * (1f - Mathf.Pow(1.55f, level));
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("HOLY_WATER", "Holy Water");
            LanguageAPI.Add("HOLY_WATER_NAME", "Holy Water");
            LanguageAPI.Add("HOLY_WATER_PICKUP", "Gain bonus experience upon killing elite enemies.");

            string desc = $"Upon killing an elite enemy, gain <style=cIsUtility>{experienceMultiplierPerStack.Value}%</style> " +
                $"<style=cStack>(+{experienceMultiplierPerStack.Value}% per stack)</style> of your current level experience cap as bonus experience.";
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