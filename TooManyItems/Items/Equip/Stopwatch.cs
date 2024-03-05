using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class Stopwatch
    {
        public static EquipmentDef equipmentDef;
        public static BuffDef stopwatchEffect;

        public static DamageColorIndex damageColor = DamageColorAPI.RegisterDamageColor(new(0.93f, 0.95f, 0.89f, 1f));

        // On activation, enter Stasis for 6 seconds. During Stasis, you are invulnerable, but cannot move, attack, or use equipment.
        public static ConfigurableValue<float> stasisDuration = new(
            "Equipment: Stopwatch",
            "Stasis Duration",
            6f,
            "Duration of stopwatch effect.",
            new List<string>()
            {
                "ITEM_STOPWATCH_DESC"
            }
        );
        public static ConfigurableValue<int> equipCooldown = new(
            "Equipment: Stopwatch",
            "Cooldown",
            20,
            "Equipment cooldown.",
            new List<string>()
            {
                "ITEM_STOPWATCH_DESC"
            }
        );

        internal static void Init()
        {
            GenerateEquipment();
            GenerateBuff();
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomEquipment(equipmentDef, displayRules));

            ContentAddition.AddBuffDef(stopwatchEffect);

            Hooks();
        }

        private static void GenerateEquipment()
        {
            equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();

            equipmentDef.name = "STOPWATCH";
            equipmentDef.nameToken = "STOPWATCH_NAME";
            equipmentDef.pickupToken = "STOPWATCH_PICKUP";
            equipmentDef.descriptionToken = "STOPWATCH_DESCRIPTION";
            equipmentDef.loreToken = "STOPWATCH_LORE";

            equipmentDef.pickupIconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("Stopwatch.png");
            equipmentDef.pickupModelPrefab = TooManyItems.MainAssets.LoadAsset<GameObject>("Stopwatch.prefab");

            equipmentDef.appearsInMultiPlayer = true;
            equipmentDef.appearsInSinglePlayer = true;
            equipmentDef.canBeRandomlyTriggered = true;
            equipmentDef.enigmaCompatible = true;
            equipmentDef.canDrop = true;

            equipmentDef.cooldown = equipCooldown;
        }

        private static void GenerateBuff()
        {
            stopwatchEffect = ScriptableObject.CreateInstance<BuffDef>();

            stopwatchEffect.name = "Stasis";
            stopwatchEffect.iconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("Stasis.png");
            stopwatchEffect.canStack = false;
            stopwatchEffect.isDebuff = false;
            stopwatchEffect.isHidden = false;
        }

        public static void Hooks()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += (orig, self, equipDef) =>
            {
                if (NetworkServer.active && equipDef == equipmentDef)
                {
                    return OnUse(self);
                }
                return orig(self, equipDef);
            };

            On.RoR2.CharacterBody.OnBuffFirstStackGained += (orig, self, buffDef) =>
            {
                orig(self, buffDef);

                if (buffDef == stopwatchEffect)
                {
                    self.characterMotor.enabled = false;
                    self.characterDirection.enabled = false;
                }
            };

            On.RoR2.CharacterBody.OnBuffFinalStackLost += (orig, self, buffDef) =>
            {
                orig(self, buffDef);

                if (buffDef == stopwatchEffect)
                {
                    self.characterMotor.enabled = true;
                    self.characterDirection.enabled = true;
                }
            };

            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.body && victimInfo.body.HasBuff(stopwatchEffect))
                {
                    damageInfo.damage = 0f;
                }
            };
        }

        private static bool OnUse(EquipmentSlot slot)
        {
            CharacterBody body = slot.characterBody;
            body.AddTimedBuff(stopwatchEffect, stasisDuration);

            return true;
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("STOPWATCH", "Stopwatch");
            LanguageAPI.Add("STOPWATCH_NAME", "Stopwatch");
            LanguageAPI.Add("STOPWATCH_PICKUP", "On activation, become invulnerable. <style=cDeath>While invulnerable, you cannot move, attack, or use equipment.</style>");

            string desc = $"On activation, enter <style=cHumanObjective>Stasis</style> for <style=cIsUtility>{stasisDuration} seconds</style>. " +
                $"During <style=cHumanObjective>Stasis</style>, you are invulnerable, but <style=cDeath>cannot move, attack, or use equipment.</style>";
            LanguageAPI.Add("STOPWATCH_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("STOPWATCH_LORE", lore);
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
