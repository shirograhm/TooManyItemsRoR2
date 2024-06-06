using R2API;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class Vanity
    {
        public static EquipmentDef equipmentDef;
        public static BuffDef hubrisDebuff;

        public static GameObject targeterVisualizerPrefab;

        // 125 second cooldown
        // While on cooldown, kills grant stacks of Hubris. Each stack causes you to take 0.4% bonus damage.
        // Activate this equipment to consume all Hubris stacks and deal 100% base damage per stack to a target enemy.
        public static ConfigurableValue<bool> isEnabled = new(
            "Equipment: Vanity Mirror",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "EQUIPMENT_VANITY_DESC"
            }
        );
        public static ConfigurableValue<float> damageTakenPerStack = new(
            "Equipment: Vanity Mirror",
            "Damage Amp",
            0.4f,
            "Percent bonus damage taken for each stack of Hubris.",
            new List<string>()
            {
                "EQUIPMENT_VANITY_DESC"
            }
        );
        public static float damageTakenPercentPerStack = damageTakenPerStack.Value / 100f;

        public static ConfigurableValue<float> damageDealtPerStack = new(
            "Equipment: Vanity Mirror",
            "Damage Dealt",
            100f,
            "Percent base damage dealt for each stack of Hubris accrued.",
            new List<string>()
            {
                "EQUIPMENT_VANITY_DESC"
            }
        );
        public static float damageDealtPercentPerStack = damageDealtPerStack.Value / 100f;

        public static ConfigurableValue<int> procCoefficient = new(
            "Equipment: Vanity Mirror",
            "Proc Coefficient",
            3,
            "Proc coefficient for the single damage instance on equipment use.",
            new List<string>()
            {
                "EQUIPMENT_VANITY_DESC"
            }
        );
        public static ConfigurableValue<int> equipCooldown = new(
            "Equipment: Vanity Mirror",
            "Cooldown",
            125,
            "Equipment cooldown.",
            new List<string>()
            {
                "EQUIPMENT_VANITY_DESC"
            }
        );

        internal static void Init()
        {
            GenerateEquipment();
            GenerateBuff();

            targeterVisualizerPrefab = Assets.bundle.LoadAsset<GameObject>("VanityTargeter.prefab");

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomEquipment(equipmentDef, displayRules));

            ContentAddition.AddBuffDef(hubrisDebuff);

            Hooks();
        }

        private static void GenerateEquipment()
        {
            equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();

            equipmentDef.name = "VANITY";
            equipmentDef.AutoPopulateTokens();

            equipmentDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Vanity.png");
            equipmentDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Vanity.prefab");

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
            On.RoR2.EquipmentSlot.Start += (orig, self) =>
            {
                orig(self);
                self.gameObject.AddComponent<EquipmentTargeter>();
            };

            On.RoR2.EquipmentSlot.Update += (orig, self) =>
            {
                orig(self);

                EquipmentTargeter targeter = self.gameObject.GetComponent<EquipmentTargeter>();
                if (targeter)
                {
                    if (equipmentDef.equipmentIndex == self.equipmentIndex)
                    {
                        if (self.stock > 0)
                        {
                            targeter.ConfigureTargetFinderForEnemies(self);

                            HurtBox hurtBox = targeter.search.GetResults().FirstOrDefault();
                            if (hurtBox)
                            {
                                targeter.obj = hurtBox.healthComponent.gameObject;
                                targeter.indicator.visualizerPrefab = targeterVisualizerPrefab;
                                targeter.indicator.targetTransform = hurtBox.transform;
                            }
                            else
                            {
                                targeter.Invalidate();
                            }
                            targeter.indicator.active = hurtBox;
                        }
                        else
                        {
                            targeter.Invalidate();
                            targeter.indicator.active = false;
                        }
                    }
                    else
                    {
                        targeter.Invalidate();
                        targeter.indicator.active = false;
                    }
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

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, eventManager, damageReport) =>
            {
                orig(eventManager, damageReport);
                if (!NetworkServer.active) return;

                CharacterBody atkBody = damageReport.attackerBody;
                if (atkBody && atkBody.equipmentSlot.equipmentIndex == equipmentDef.equipmentIndex)
                {
                    atkBody.AddBuff(hubrisDebuff);
                }
            };
        }

        private static bool OnUse(EquipmentSlot slot)
        {
            CharacterBody user = slot.characterBody;
            if (user)
            {
                EquipmentTargeter targeter = slot.GetComponent<EquipmentTargeter>();
                CharacterBody targetEnemy = (targeter) ? targeter.obj.GetComponent<CharacterBody>() : null;

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
