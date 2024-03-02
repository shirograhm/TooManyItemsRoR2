using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class TatteredScroll
    {
        public static EquipmentDef equipmentDef;
        public static BuffDef curseDebuff;

        public static DamageColorIndex damageColor = DamageColorAPI.RegisterDamageColor(new(0.93f, 0.95f, 0.89f, 1f));

        // On activation, curse all enemies in a 60m radius for 10 seconds. Killing cursed enemies grants 15 additional gold.
        public static ConfigurableValue<int> curseDistance = new(
            "Equipment: Tattered Scroll",
            "Curse Distance",
            60,
            "Max distance that the curse can reach.",
            new List<string>()
            {
                "ITEM_TATTEREDSCROLL_DESC"
            }
        );
        public static ConfigurableValue<float> curseDuration = new(
            "Equipment: Tattered Scroll",
            "Curse Duration",
            10f,
            "Duration of the curse.",
            new List<string>()
            {
                "ITEM_TATTEREDSCROLL_DESC"
            }
        );
        public static ConfigurableValue<int> goldGranted = new(
            "Equipment: Tattered Scroll",
            "Gold Granted",
            15,
            "Gold gained for each cursed enemy killed.",
            new List<string>()
            {
                "ITEM_TATTEREDSCROLL_DESC"
            }
        );
        public static ConfigurableValue<int> equipCooldown = new(
            "Equipment: Tattered Scroll",
            "Cooldown",
            80,
            "Equipment cooldown.",
            new List<string>()
            {
                "ITEM_TATTEREDSCROLL_DESC"
            }
        );

        internal static void Init()
        {
            GenerateEquipment();
            GenerateBuff();
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomEquipment(equipmentDef, displayRules));

            ContentAddition.AddBuffDef(curseDebuff);

            Hooks();
        }

        private static void GenerateEquipment()
        {
            equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();

            equipmentDef.name = "TATTERED_SCROLL";
            equipmentDef.nameToken = "TATTERED_SCROLL_NAME";
            equipmentDef.pickupToken = "TATTERED_SCROLL_PICKUP";
            equipmentDef.descriptionToken = "TATTERED_SCROLL_DESCRIPTION";
            equipmentDef.loreToken = "TATTERED_SCROLL_LORE";

            equipmentDef.pickupIconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("TatteredScroll.png");
            equipmentDef.pickupModelPrefab = TooManyItems.MainAssets.LoadAsset<GameObject>("TatteredScroll.prefab");

            equipmentDef.appearsInMultiPlayer = true;
            equipmentDef.appearsInSinglePlayer = true;
            equipmentDef.canBeRandomlyTriggered = true;
            equipmentDef.enigmaCompatible = true;
            equipmentDef.canDrop = true;

            equipmentDef.cooldown = equipCooldown;
        }

        private static void GenerateBuff()
        {
            curseDebuff = ScriptableObject.CreateInstance<BuffDef>();

            curseDebuff.name = "Siphon";
            curseDebuff.iconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("TatteredCurse.png");
            curseDebuff.canStack = false;
            curseDebuff.isDebuff = true;
            curseDebuff.isHidden = false;
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

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, eventManager, damageReport) =>
            {
                orig(eventManager, damageReport);

                if (!NetworkServer.active || damageReport.victimBody == null || damageReport.attackerMaster == null) return;

                if (damageReport.victimBody.HasBuff(curseDebuff))
                {
                    damageReport.attackerMaster.GiveMoney(Convert.ToUInt32(goldGranted.Value));
                }
            };
        }

        private static bool OnUse(EquipmentSlot slot)
        {
            HurtBox[] hurtboxes = new SphereSearch
            {
                mask = LayerIndex.enemyBody.mask,
                origin = slot.characterBody.corePosition,
                queryTriggerInteraction = QueryTriggerInteraction.Collide,
                radius = curseDistance.Value
            }.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

            foreach (HurtBox hb in hurtboxes)
            {
                CharacterBody parent = hb.healthComponent.body;
                if (parent && parent != slot.characterBody)
                {
                    DamageInfo info = new()
                    {
                        damage = 1f,
                        attacker = slot.characterBody.gameObject,
                        inflictor = slot.characterBody.gameObject,
                        procCoefficient = 0f,
                        position = parent.corePosition,
                        crit = false,
                        damageColorIndex = damageColor,
                        procChainMask = new ProcChainMask(),
                        damageType = DamageType.Silent
                    };
                    parent.healthComponent.TakeDamage(info);

                    parent.AddTimedBuff(curseDebuff, curseDuration.Value);
                }
            }

            return true;
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("TATTERED_SCROLL", "Tattered Scroll");
            LanguageAPI.Add("TATTERED_SCROLL_NAME", "Tattered Scroll");
            LanguageAPI.Add("TATTERED_SCROLL_PICKUP", "Curse nearby enemies when activated. Killing cursed enemies grants extra gold.");

            string desc = $"On activation, curse enemies within a <style=cIsUtility>{curseDistance.Value}m</style> radius for {curseDuration.Value} seconds. " +
                $"Killing cursed enemies grants <style=cIsUtility>{goldGranted.Value}</style> extra gold.";
            LanguageAPI.Add("TATTERED_SCROLL_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("TATTERED_SCROLL_LORE", lore);
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
