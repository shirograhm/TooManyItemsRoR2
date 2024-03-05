using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class InstructionManual
    {
        public static ItemDef itemDef;

        // Reduce your equipment cooldown by 6% (+6% per stack).
        public static ConfigurableValue<float> equipmentCDR = new(
            "Item: Instruction Manual",
            "Cooldown Reduction",
            6f,
            "Cooldown reduction on equipment.",
            new List<string>()
            {
                "ITEM_INSTRUCTIONMANUAL_DESC"
            }
        );
        public static float equipmentCDRPercent = equipmentCDR.Value / 100f;

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

            itemDef.name = "INSTRUCTION_MANUAL";
            itemDef.nameToken = "INSTRUCTION_MANUAL_NAME";
            itemDef.pickupToken = "INSTRUCTION_MANUAL_PICKUP";
            itemDef.descriptionToken = "INSTRUCTION_MANUAL_DESCRIPTION";
            itemDef.loreToken = "INSTRUCTION_MANUAL_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier1;
            });

            itemDef.pickupIconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("InstructionManual.png");
            itemDef.pickupModelPrefab = TooManyItems.MainAssets.LoadAsset<GameObject>("InstructionManual.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender == null || sender.inventory == null) return;

                int count = sender.inventory.GetItemCount(itemDef);
                if (count > 0)
                {
                    float cdr = 1 - (1 / (1 + (equipmentCDRPercent * count)));
                    // Reduce equipment cooldown by cdr
                    // args.specialCooldownMultAdd -= cdr;
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("INSTRUCTION_MANUAL", "Instruction Manual");
            LanguageAPI.Add("INSTRUCTION_MANUAL_NAME", "Instruction Manual");
            LanguageAPI.Add("INSTRUCTION_MANUAL_PICKUP", "Reduce the cooldown of your equipment.");

            string desc = $"Reduce your equipment cooldown by <style=cIsUtility>{equipmentCDR.Value}%</style> " +
                $"<style=cStack>(+{equipmentCDR.Value}% per stack)</style>.";
            LanguageAPI.Add("INSTRUCTION_MANUAL_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("INSTRUCTION_MANUAL_LORE", lore);
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