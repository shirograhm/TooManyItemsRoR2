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

        // Killing an elite enemy grants 2% (+2% per stack) of your level experience cap as bonus experience.
        public static ConfigurableValue<float> experienceMultiplierPerStack = new(
            "Item: Holy Water",
            "XP Multiplier",
            2f,
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

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("HolyWater.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("HolyWater.prefab");
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

                if (atkBody && atkMaster && damageReport.victimBody.isElite)
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
                            float hyperbolicExperienceMultiplier = 1 - (1 / (1 + (experienceMultiplierAsPercent * count)));
                            float bonusXP = GetExperienceCap(atkBody.level) * hyperbolicExperienceMultiplier;

                            atkMaster.GiveExperience(Convert.ToUInt64(bonusXP));
                        }
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

            string desc = $"Killing an elite enemy grants <style=cIsUtility>{experienceMultiplierPerStack.Value}%</style> " +
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