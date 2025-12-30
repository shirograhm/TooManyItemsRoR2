using R2API;
using RoR2;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems.Items.Equip.Lunar
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
            ["EQUIPMENT_CHALICE_DESC"]
        );
        public static ConfigurableValue<float> consecrateDuration = new(
            "Equipment: Chalice",
            "Consecrate Duration",
            10f,
            "Duration of the Consecrate buff given.",
            ["EQUIPMENT_CHALICE_DESC"]
        );
        public static ConfigurableValue<float> consecrateDamageBonus = new(
            "Equipment: Chalice",
            "Consecrate Damage Bonus",
            30f,
            "Percent bonus damage dealt while Consecrated.",
            ["EQUIPMENT_CHALICE_DESC"]
        );
        public static ConfigurableValue<float> consecrateAttackSpeedBonus = new(
            "Equipment: Chalice",
            "Consecrate Attack Speed Bonus",
            90f,
            "Percent bonus attack speed gained while Consecrated.",
            ["EQUIPMENT_CHALICE_DESC"]
        );
        public static ConfigurableValue<float> currentHealthCost = new(
            "Equipment: Chalice",
            "Current Health Loss",
            90f,
            "Percent of current health lost as payment when Consecrated.",
            ["EQUIPMENT_CHALICE_DESC"]
        );
        public static ConfigurableValue<int> equipCooldown = new(
            "Equipment: Chalice",
            "Cooldown",
            70,
            "Equipment cooldown.",
            ["EQUIPMENT_CHALICE_DESC"]
        );
        public static float currentHealthCostPercent = currentHealthCost.Value / 100f;

        internal static void Init()
        {
            equipmentDef = ItemManager.GenerateEquipment("Chalice", equipCooldown.Value, isLunar: true, canBeRandomlyTriggered: false);

            consecratedBuff = ItemManager.GenerateBuff("Consecrated", AssetManager.bundle.LoadAsset<Sprite>("ConsecratedBuff.png"));
            ContentAddition.AddBuffDef(consecratedBuff);

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
                    damageColorIndex = DamageColorIndex.Fragile,
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
