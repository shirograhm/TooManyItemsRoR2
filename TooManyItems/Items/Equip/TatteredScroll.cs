using R2API;
using RoR2;
using System.Collections.Generic;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems.Items.Equip
{
    internal class TatteredScroll
    {
        public static EquipmentDef equipmentDef;
        public static BuffDef curseDebuff;

        public static DamageColorIndex damageColor = DamageColorManager.RegisterDamageColor(Utilities.TATTERED_SCROLL_COLOR);

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
            equipmentDef = ItemManager.GenerateEquipment("TatteredScroll", equipCooldown.Value);

            curseDebuff = ItemManager.GenerateBuff("Siphon", AssetManager.bundle.LoadAsset<Sprite>("TatteredCurse.png"), isDebuff: true);
            ContentAddition.AddBuffDef(curseDebuff);

            Hooks();
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
                        damageReport.attackerMaster.GiveMoney(Utilities.ScaleGoldWithDifficulty(goldGranted.Value));
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
