﻿using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class Chalice
    {
        public static EquipmentDef equipmentDef;
        public static BuffDef consecratedBuff;

        // Pay 90% of your current health to consecrate yourself and all allies. Consecrated allies gain 30% damage and 90% attack speed for 10 seconds. (60 sec)
        public static ConfigurableValue<bool> isEnabled = new(
            "Equipment: Chalice",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_CHALICE_DESC"
            }
        );
        public static ConfigurableValue<float> consecrateDuration = new(
            "Equipment: Chalice",
            "Consecrate Duration",
            10f,
            "Duration of the Consecrate buff given.",
            new List<string>()
            {
                "ITEM_CHALICE_DESC"
            }
        );
        public static ConfigurableValue<float> consecrateDamageBonus = new(
            "Equipment: Chalice",
            "Consecrate Damage Bonus",
            30f,
            "Percent bonus damage dealt while Consecrated.",
            new List<string>()
            {
                "ITEM_CHALICE_DESC"
            }
        );
        public static ConfigurableValue<float> consecrateAttackSpeedBonus = new(
            "Equipment: Chalice",
            "Consecrate Attack Speed Bonus",
            90f,
            "Percent bonus attack speed gained while Consecrated.",
            new List<string>()
            {
                "ITEM_CHALICE_DESC"
            }
        );
        public static ConfigurableValue<float> currentHealthCost = new(
            "Equipment: Chalice",
            "Current Health Loss",
            90f,
            "Percent of current health lost as payment when Consecrated.",
            new List<string>()
            {
                "ITEM_CHALICE_DESC"
            }
        );
        public static ConfigurableValue<int> equipCooldown = new(
            "Equipment: Chalice",
            "Cooldown",
            60,
            "Equipment cooldown.",
            new List<string>()
            {
                "ITEM_CHALICE_DESC"
            }
        );
        public static float currentHealthCostPercent = currentHealthCost.Value / 100f;

        internal static void Init()
        {
            GenerateEquipment();
            GenerateBuff();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomEquipment(equipmentDef, displayRules));

            ContentAddition.AddBuffDef(consecratedBuff);

            Hooks();
        }

        private static void GenerateEquipment()
        {
            equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();

            equipmentDef.name = "CHALICE";
            equipmentDef.nameToken = "CHALICE_NAME";
            equipmentDef.pickupToken = "CHALICE_PICKUP";
            equipmentDef.descriptionToken = "CHALICE_DESCRIPTION";
            equipmentDef.loreToken = "CHALICE_LORE";

            equipmentDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Chalice.png");
            equipmentDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Chalice.prefab");

            equipmentDef.isLunar = true;
            equipmentDef.colorIndex = ColorCatalog.ColorIndex.LunarItem;

            equipmentDef.appearsInMultiPlayer = true;
            equipmentDef.appearsInSinglePlayer = true;
            equipmentDef.canBeRandomlyTriggered = false;
            equipmentDef.enigmaCompatible = true;
            equipmentDef.canDrop = true;

            equipmentDef.cooldown = equipCooldown.Value;
        }

        private static void GenerateBuff()
        {
            consecratedBuff = ScriptableObject.CreateInstance<BuffDef>();

            consecratedBuff.name = "Consecrated";
            consecratedBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("ConsecratedBuff.png");
            consecratedBuff.canStack = false;
            consecratedBuff.isHidden = false;
            consecratedBuff.isDebuff = false;
            consecratedBuff.isCooldown = false;
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

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    if (sender.HasBuff(consecratedBuff))
                    {
                        args.attackSpeedMultAdd += consecrateAttackSpeedBonus.Value / 100f;
                        args.damageMultAdd += consecrateDamageBonus.Value / 100f;
                    }
                }
            };
        }

        private static bool OnUse(EquipmentSlot slot)
        {
            CharacterBody body = slot.characterBody;
            if (body)
            {
                float damageToTake = body.healthComponent.health * currentHealthCostPercent;
                // Escape the method if the activation would kill ourself
                if (body.healthComponent.combinedHealth <= damageToTake) return false;

                DamageInfo useCost = new()
                {
                    damage = body.healthComponent.combinedHealth * currentHealthCostPercent,
                    attacker = null,
                    inflictor = null,
                    procCoefficient = 0f,
                    position = body.corePosition,
                    crit = false,
                    damageColorIndex = DamageColorIndex.Default,
                    procChainMask = new ProcChainMask(),
                    damageType = DamageType.Silent
                };
                body.healthComponent.TakeDamage(useCost);
                
                foreach (TeamComponent component in TeamComponent.GetTeamMembers(slot.characterBody.teamComponent.teamIndex))
                {
                    component.body.AddTimedBuff(consecratedBuff, consecrateDuration.Value);
                }
                return true;
            }
            return false;
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
