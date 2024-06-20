using R2API;
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

        // Pay a portion of your current health to consecrate yourself and all allies for a short duration. Consecrated allies gain damage and attack speed.
        public static ConfigurableValue<bool> isEnabled = new(
            "Equipment: Chalice",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "EQUIPMENT_CHALICE_DESC"
            }
        );
        public static ConfigurableValue<float> consecrateDuration = new(
            "Equipment: Chalice",
            "Consecrate Duration",
            10f,
            "Duration of the Consecrate buff given.",
            new List<string>()
            {
                "EQUIPMENT_CHALICE_DESC"
            }
        );
        public static ConfigurableValue<float> consecrateDamageBonus = new(
            "Equipment: Chalice",
            "Consecrate Damage Bonus",
            30f,
            "Percent bonus damage dealt while Consecrated.",
            new List<string>()
            {
                "EQUIPMENT_CHALICE_DESC"
            }
        );
        public static ConfigurableValue<float> consecrateAttackSpeedBonus = new(
            "Equipment: Chalice",
            "Consecrate Attack Speed Bonus",
            90f,
            "Percent bonus attack speed gained while Consecrated.",
            new List<string>()
            {
                "EQUIPMENT_CHALICE_DESC"
            }
        );
        public static ConfigurableValue<float> currentHealthCost = new(
            "Equipment: Chalice",
            "Current Health Loss",
            90f,
            "Percent of current health lost as payment when Consecrated.",
            new List<string>()
            {
                "EQUIPMENT_CHALICE_DESC"
            }
        );
        public static ConfigurableValue<int> equipCooldown = new(
            "Equipment: Chalice",
            "Cooldown",
            70,
            "Equipment cooldown.",
            new List<string>()
            {
                "EQUIPMENT_CHALICE_DESC"
            }
        );
        public static float currentHealthCostPercent = currentHealthCost.Value / 100f;

        internal static void Init()
        {
            GenerateEquipment();
            GenerateBuff();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomEquipment(equipmentDef, displayRules));

            ContentAddition.AddBuffDef(consecratedBuff);

            Hooks();
        }

        private static void GenerateEquipment()
        {
            equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();

            equipmentDef.name = "CHALICE";
            equipmentDef.AutoPopulateTokens();

            equipmentDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("Chalice.png");
            equipmentDef.pickupModelPrefab = AssetHandler.bundle.LoadAsset<GameObject>("Chalice.prefab");

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
            consecratedBuff.iconSprite = AssetHandler.bundle.LoadAsset<Sprite>("ConsecratedBuff.png");
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
                    damage = damageToTake,
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
