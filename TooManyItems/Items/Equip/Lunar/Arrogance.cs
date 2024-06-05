using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class Arrogance
    {
        public static EquipmentDef equipmentDef;
        public static BuffDef hubrisDebuff;

        // 125 second cooldown
        // While on cooldown, kills grant stacks of Hubris. Each stack causes you to take 0.2% bonus damage.
        // Activate this equipment to consume all Hubris stacks and deal 100% base damage per stack to a target enemy.
        public static ConfigurableValue<bool> isEnabled = new(
            "Equipment: Lunar Scepter",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_ARROGANCE_DESC"
            }
        );
        public static ConfigurableValue<float> damageTakenPerStack = new(
            "Equipment: Lunar Scepter",
            "Damage Taken Per Stack",
            0.2f,
            "Percent bonus damage taken for each stack of Hubris.",
            new List<string>()
            {
                "ITEM_ARROGANCE_DESC"
            }
        );
        public static ConfigurableValue<float> damageDealtPerStack = new(
            "Equipment: Lunar Scepter",
            "Damage Dealt Per Stack",
            100f,
            "Percent base damage dealt for each stack of Hubris accrued.",
            new List<string>()
            {
                "ITEM_ARROGANCE_DESC"
            }
        );
        public static ConfigurableValue<int> procCoefficient = new(
            "Equipment: Lunar Scepter",
            "Proc Coefficient",
            3,
            "Proc coefficient for the single damage instance on equipment use.",
            new List<string>()
            {
                "ITEM_ARROGANCE_DESC"
            }
        );
        public static ConfigurableValue<int> equipCooldown = new(
            "Equipment: Lunar Scepter",
            "Cooldown",
            125,
            "Equipment cooldown.",
            new List<string>()
            {
                "ITEM_ARROGANCE_DESC"
            }
        );
        public static float damageTakenPercentPerStack = damageTakenPerStack.Value / 100f;
        public static float damageDealtPercentPerStack = damageDealtPerStack.Value / 100f;

        internal static void Init()
        {
            GenerateEquipment();
            GenerateBuff();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomEquipment(equipmentDef, displayRules));

            ContentAddition.AddBuffDef(hubrisDebuff);

            Hooks();
        }

        private static void GenerateEquipment()
        {
            equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();

            equipmentDef.name = "ARROGANCE";
            equipmentDef.nameToken = "ARROGANCE_NAME";
            equipmentDef.pickupToken = "ARROGANCE_PICKUP";
            equipmentDef.descriptionToken = "ARROGANCE_DESCRIPTION";
            equipmentDef.loreToken = "ARROGANCE_LORE";

            equipmentDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Arrogance.png");
            equipmentDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Arrogance.prefab");

            equipmentDef.isLunar = true;
            equipmentDef.colorIndex = ColorCatalog.ColorIndex.LunarItem;

            equipmentDef.appearsInMultiPlayer = true;
            equipmentDef.appearsInSinglePlayer = true;
            equipmentDef.canBeRandomlyTriggered = false;
            equipmentDef.enigmaCompatible = false;
            equipmentDef.canDrop = true;

            equipmentDef.cooldown = equipCooldown.Value;
        }

        private static void GenerateBuff()
        {
            hubrisDebuff = ScriptableObject.CreateInstance<BuffDef>();

            hubrisDebuff.name = "Hubris";
            hubrisDebuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("HubrisDebuff.png");
            hubrisDebuff.canStack = true;
            hubrisDebuff.isHidden = false;
            hubrisDebuff.isDebuff = true;
            hubrisDebuff.isCooldown = false;
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

            GenericGameEvents.BeforeTakeDamage += (damageInfo, attacker, victim) =>
            {
                if (victim.body && victim.body.inventory)
                {
                    int stackCount = victim.body.GetBuffCount(hubrisDebuff);
                    if (stackCount > 0)
                    {
                        float multiplier = 1 + stackCount * damageTakenPercentPerStack;
                        damageInfo.damage *= multiplier;
                    }
                }
            };
        }

        private static bool OnUse(EquipmentSlot slot)
        {
            CharacterBody user = slot.characterBody;
            if (user)
            {
                // TODO: Somehow get target enemy

                CharacterBody targetEnemy = null;

                float damageAmount = user.damage * damageDealtPercentPerStack * user.GetBuffCount(hubrisDebuff);
                DamageInfo scepterDamage = new()
                {
                    damage = damageAmount,
                    attacker = user.gameObject,
                    inflictor = user.gameObject,
                    procCoefficient = procCoefficient.Value,
                    position = targetEnemy.corePosition,
                    crit = false,
                    damageColorIndex = DamageColorIndex.Default,
                    procChainMask = new ProcChainMask(),
                    damageType = DamageType.Silent
                };
                targetEnemy.healthComponent.TakeDamage(scepterDamage);
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                user.SetBuffCount(hubrisDebuff.buffIndex, 0);
#pragma warning restore Publicizer001 // Accessing a member that was not originally public

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
