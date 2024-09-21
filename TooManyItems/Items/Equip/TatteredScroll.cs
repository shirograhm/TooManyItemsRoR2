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

        public static DamageColorIndex damageColor = DamageColorAPI.RegisterDamageColor(Utils.TATTERED_SCROLL_COLOR);

        // On activation, curse all enemies around you for a short duration. Killing cursed enemies grants additional gold.
        public static ConfigurableValue<bool> isEnabled = new(
            "Equipment: Tattered Scroll",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "EQUIPMENT_TATTEREDSCROLL_DESC"
            }
        );
        public static ConfigurableValue<int> curseDistance = new(
            "Equipment: Tattered Scroll",
            "Curse Distance",
            60,
            "Max distance that the curse can reach.",
            new List<string>()
            {
                "EQUIPMENT_TATTEREDSCROLL_DESC"
            }
        );
        public static ConfigurableValue<float> curseDuration = new(
            "Equipment: Tattered Scroll",
            "Curse Duration",
            10f,
            "Duration of the curse.",
            new List<string>()
            {
                "EQUIPMENT_TATTEREDSCROLL_DESC"
            }
        );
        public static ConfigurableValue<int> goldGranted = new(
            "Equipment: Tattered Scroll",
            "Gold Granted",
            20,
            "Gold gained for each cursed enemy killed.",
            new List<string>()
            {
                "EQUIPMENT_TATTEREDSCROLL_DESC"
            }
        );
        public static ConfigurableValue<int> equipCooldown = new(
            "Equipment: Tattered Scroll",
            "Cooldown",
            100,
            "Equipment cooldown.",
            new List<string>()
            {
                "EQUIPMENT_TATTEREDSCROLL_DESC"
            }
        );

        internal static void Init()
        {
            GenerateEquipment();
            GenerateBuff();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomEquipment(equipmentDef, displayRules));

            ContentAddition.AddBuffDef(curseDebuff);

            Hooks();
        }

        private static void GenerateEquipment()
        {
            equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();

            equipmentDef.name = "TATTEREDSCROLL";
            equipmentDef.AutoPopulateTokens();

            equipmentDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("TatteredScroll.png");
            equipmentDef.pickupModelPrefab = AssetHandler.bundle.LoadAsset<GameObject>("TatteredScroll.prefab");

            equipmentDef.appearsInMultiPlayer = true;
            equipmentDef.appearsInSinglePlayer = true;
            equipmentDef.canBeRandomlyTriggered = true;
            equipmentDef.enigmaCompatible = true;
            equipmentDef.canDrop = true;

            equipmentDef.cooldown = equipCooldown.Value;
        }

        private static void GenerateBuff()
        {
            curseDebuff = ScriptableObject.CreateInstance<BuffDef>();

            curseDebuff.name = "Siphon";
            curseDebuff.iconSprite = AssetHandler.bundle.LoadAsset<Sprite>("TatteredCurse.png");
            curseDebuff.canStack = false;
            curseDebuff.isHidden = false;
            curseDebuff.isDebuff = true;
            curseDebuff.isCooldown = false;
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

            GlobalEventManager.onCharacterDeathGlobal += (damageReport) =>
            {
                if (!NetworkServer.active) return;

                if (damageReport.victimBody && damageReport.victimBody.HasBuff(curseDebuff))
                {
                    if (damageReport.attackerMaster)
                    {
                        damageReport.attackerMaster.GiveMoney(Utils.ScaleGoldWithDifficulty(goldGranted.Value));
                    }
                }
            };
        }

        private static bool OnUse(EquipmentSlot slot)
        {
            HurtBox[] hurtboxes = new SphereSearch
            {
                mask = LayerIndex.entityPrecise.mask,
                origin = slot.characterBody.corePosition,
                queryTriggerInteraction = QueryTriggerInteraction.Collide,
                radius = curseDistance.Value
            }.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

            foreach (HurtBox hb in hurtboxes)
            {
                CharacterBody parent = hb.healthComponent.body;
                if (parent && parent != slot.characterBody && !parent.isPlayerControlled)
                {
                    parent.healthComponent.TakeDamage(new DamageInfo
                    {
                        damage = slot.characterBody.damage,
                        attacker = slot.characterBody.gameObject,
                        inflictor = slot.characterBody.gameObject,
                        procCoefficient = 0f,
                        position = parent.corePosition,
                        crit = false,
                        damageColorIndex = damageColor,
                        procChainMask = new ProcChainMask(),
                        damageType = DamageType.Silent
                    });

                    parent.AddTimedBuff(curseDebuff, curseDuration.Value);
                }
            }

            return true;
        }
    }
}
