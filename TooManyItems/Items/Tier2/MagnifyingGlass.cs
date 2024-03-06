using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class MagnifyingGlass
    {
        public static ItemDef itemDef;
        public static BuffDef analyzedDebuff;

        // Gain 5% crit chance. Critical strikes have an 8% (+8% per stack) chance to Analyze the enemy, increasing their damage taken by 10% from all sources.
        public static ConfigurableValue<float> critBonus = new(
            "Item: Magnifying Glass",
            "Crit Chance",
            5f,
            "Crit chance increase so that the item isn't worthless without other crit items.",
            new List<string>()
            {
                "ITEM_MAGNIFYINGGLASS_DESC"
            }
        );
        public static ConfigurableValue<float> analyzeChance = new(
            "Item: Magnifying Glass",
            "Analyze Chance",
            8f,
            "Percent chance to Analyze an enemy on crit.",
            new List<string>()
            {
                "ITEM_MAGNIFYINGGLASS_DESC"
            }
        );
        public static ConfigurableValue<float> damageTakenBonus = new(
            "Item: Magnifying Glass",
            "Damage Taken Bonus",
            10f,
            "Percent damage taken bonus once Analyzed.",
            new List<string>()
            {
                "ITEM_MAGNIFYINGGLASS_DESC"
            }
        );
        public static float analyzeChancePercent = analyzeChance.Value / 100f;
        public static float damageTakenBonusPercent = damageTakenBonus.Value / 100f;

        internal static void Init()
        {
            GenerateItem();
            GenerateBuff();
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            ContentAddition.AddBuffDef(analyzedDebuff);

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "MAGNIFYING_GLASS";
            itemDef.nameToken = "MAGNIFYING_GLASS_NAME";
            itemDef.pickupToken = "MAGNIFYING_GLASS_PICKUP";
            itemDef.descriptionToken = "MAGNIFYING_GLASS_DESCRIPTION";
            itemDef.loreToken = "MAGNIFYING_GLASS_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier2;
            });

            itemDef.pickupIconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("MagnifyingGlass.png");
            itemDef.pickupModelPrefab = TooManyItems.MainAssets.LoadAsset<GameObject>("MagnifyingGlass.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        private static void GenerateBuff()
        {
            analyzedDebuff = ScriptableObject.CreateInstance<BuffDef>();

            analyzedDebuff.name = "Analyzed";
            analyzedDebuff.iconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("Analyzed.png");
            analyzedDebuff.canStack = false;
            analyzedDebuff.isHidden = false;
            analyzedDebuff.isDebuff = true;
            analyzedDebuff.isCooldown = false;
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender == null || sender.inventory == null) return;

                int count = sender.inventory.GetItemCount(itemDef);
                if (count > 0)
                {
                    args.critAdd += critBonus.Value;
                }
            };

            GenericGameEvents.OnTakeDamage += (damageReport) =>
            {
                if (damageReport.attackerBody && damageReport.victimBody)
                {
                    CharacterBody vicBody = damageReport.victimBody;
                    CharacterBody atkBody = damageReport.attackerBody;

                    int count = atkBody.inventory.GetItemCount(itemDef);
                    if (count > 0 && damageReport.damageInfo.crit)
                    {
                        if (RollForAnalyze(atkBody.master, count))
                        {
                            vicBody.AddBuff(analyzedDebuff);
                        }
                    }
                }
            };

            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.body && victimInfo.body.HasBuff(analyzedDebuff))
                {
                    damageInfo.damage *= 1 + damageTakenBonusPercent;
                }
            };
        }

        private static bool RollForAnalyze(CharacterMaster master, int itemCount)
        {
            double roll = TooManyItems.rand.NextDouble();

            int luck = (int)master.luck;
            for (int i = 0; i < Mathf.Abs(luck); i++)
            {
                double newRoll = TooManyItems.rand.NextDouble();

                if (luck > 0) roll = newRoll < roll ? newRoll : roll;   // Lower values are better for this roll
                if (luck < 0) roll = newRoll > roll ? newRoll : roll;
            }

            return roll <= analyzeChancePercent * itemCount;
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("MAGNIFYING_GLASS", "Magnifying Glass");
            LanguageAPI.Add("MAGNIFYING_GLASS_NAME", "Magnifying Glass");
            LanguageAPI.Add("MAGNIFYING_GLASS_PICKUP", "Critical strikes sometimes cause enemies to take more damage.");

            string desc = $"Gain <style=cIsUtility>{critBonus.Value}%</style> crit chance. " +
                $"Critical strikes have a <style=cIsUtility>{analyzeChance.Value}%</style> <style=cStack>(+{analyzeChance.Value}% per stack)</style> chance to Analyze the enemy, " +
                $"increasing their damage taken by <style=cIsDamage>{damageTakenBonus.Value}%</style> from all sources.";
            LanguageAPI.Add("MAGNIFYING_GLASS_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("MAGNIFYING_GLASS_LORE", lore);
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