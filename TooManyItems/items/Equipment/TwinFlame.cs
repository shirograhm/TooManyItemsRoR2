using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class TwinFlame
    {
        public static EquipmentDef equipmentDef;
        public static BuffDef twinFlameDebuff;

        // On activation, target an enemy. For the next 8 seconds, you and the enemy take 300% damage.
        public static ConfigurableValue<float> twinMultiplier = new(
            "Equipment: Twin Flame",
            "Damage Multiplier",
            300f,
            "Percentage damage multiplier for the twin effect.",
            new List<string>()
            {
                "ITEM_TWINFLAME_DESC"
            }
        );
        public static ConfigurableValue<float> twinDuration = new(
            "Equipment: Twin Flame",
            "Twin Duration",
            8f,
            "Duration of the twin effect.",
            new List<string>()
            {
                "ITEM_TWINFLAME_DESC"
            }
        );
        public static ConfigurableValue<int> equipCooldown = new(
            "Equipment: Twin Flame",
            "Cooldown",
            100,
            "Equipment cooldown.",
            new List<string>()
            {
                "ITEM_TWINFLAME_DESC"
            }
        );

        internal static void Init()
        {
            GenerateEquipment();
            GenerateBuff();
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomEquipment(equipmentDef, displayRules));

            ContentAddition.AddBuffDef(twinFlameDebuff);

            Hooks();
        }

        private static void GenerateEquipment()
        {
            equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();

            equipmentDef.name = "TWIN_FLAME";
            equipmentDef.nameToken = "TWIN_FLAME_NAME";
            equipmentDef.pickupToken = "TWIN_FLAME_PICKUP";
            equipmentDef.descriptionToken = "TWIN_FLAME_DESCRIPTION";
            equipmentDef.loreToken = "TWIN_FLAME_LORE";

            equipmentDef.pickupIconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("TwinFlame.png");
            equipmentDef.pickupModelPrefab = TooManyItems.MainAssets.LoadAsset<GameObject>("TwinFlame.prefab");

            equipmentDef.appearsInMultiPlayer = true;
            equipmentDef.appearsInSinglePlayer = true;
            equipmentDef.canBeRandomlyTriggered = true;
            equipmentDef.enigmaCompatible = true;
            equipmentDef.canDrop = true;

            equipmentDef.cooldown = equipCooldown;
        }

        private static void GenerateBuff()
        {
            twinFlameDebuff = ScriptableObject.CreateInstance<BuffDef>();

            twinFlameDebuff.name = "Flame Link";
            twinFlameDebuff.iconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("TwinFlameDebuff.png");
            twinFlameDebuff.canStack = false;
            twinFlameDebuff.isDebuff = true;
            twinFlameDebuff.isHidden = false;
        }

        public static void Hooks()
        {
            On.RoR2.EquipmentSlot.Update += (orig, self) =>
            {
                orig(self);

                if (self.equipmentIndex == equipmentDef.equipmentIndex)
                {
                    // Raycast to find & set target
                }
            };

            On.RoR2.EquipmentSlot.PerformEquipmentAction += (orig, self, equipDef) =>
            {
                if (NetworkServer.active && equipDef == equipmentDef)
                {
                    return OnUse(self);
                }
                return orig(self, equipDef);
            };
        }

        private static bool OnUse(EquipmentSlot slot)
        {
            // apply debuff to yourself and targeted enemy
            return true;
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("TWIN_FLAME", "Twin Flame");
            LanguageAPI.Add("TWIN_FLAME_NAME", "Twin Flame");
            LanguageAPI.Add("TWIN_FLAME_PICKUP", "Increase damage taken by an enemy and <style=cDeath>by yourself.</style>");

            string desc = $"On activation, increase damage taken by your target <style=cDeath>and yourself</style> by <style=cIsUtility>{twinMultiplier.Value}%</style>.";
            LanguageAPI.Add("TWIN_FLAME_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("TWIN_FLAME_LORE", lore);
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
