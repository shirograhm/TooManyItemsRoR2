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

        public static GameObject vanityTargetIndicatorPrefab;

        // 30 second cooldown
        // Gain stacks of Hubris when killing enemies. Each stack reduces your BASE damage by 3%.
        // Activate this equipment to consume all Hubris stacks and deal 300% BASE damage per stack to a target enemy.
        public static ConfigurableValue<bool> isEnabled = new(
            "Equipment: Crown of Vanity",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "EQUIPMENT_VANITY_DESC"
            }
        );
        public static ConfigurableValue<float> damageLostPerStack = new(
            "Equipment: Crown of Vanity",
            "Base Damage Lost",
            3f,
            "Percent base damage lost for each stack of Hubris.",
            new List<string>()
            {
                "EQUIPMENT_VANITY_DESC"
            }
        );
        public static float damageLostPercentPerStack = damageLostPerStack.Value / 100f;

        public static ConfigurableValue<float> damageDealtPerStack = new(
            "Equipment: Crown of Vanity",
            "Damage Dealt",
            300f,
            "Percent damage dealt for each stack of Hubris accrued.",
            new List<string>()
            {
                "EQUIPMENT_VANITY_DESC"
            }
        );
        public static float damageDealtPercentPerStack = damageDealtPerStack.Value / 100f;

        public static ConfigurableValue<int> procCoefficient = new(
            "Equipment: Crown of Vanity",
            "Proc Coefficient",
            3,
            "Proc coefficient for the single damage instance on equipment use.",
            new List<string>()
            {
                "EQUIPMENT_VANITY_DESC"
            }
        );
        public static ConfigurableValue<int> equipCooldown = new(
            "Equipment: Crown of Vanity",
            "Cooldown",
            30,
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

            vanityTargetIndicatorPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/WoodSpriteIndicator"), "TooManyItems_vanityTargetIndicator", false);
            vanityTargetIndicatorPrefab.GetComponentInChildren<SpriteRenderer>().sprite = Assets.bundle.LoadAsset<Sprite>("VanityTargeter.png");
            vanityTargetIndicatorPrefab.GetComponentInChildren<SpriteRenderer>().color = Utils.VANITY_COLOR;
            vanityTargetIndicatorPrefab.GetComponentInChildren<TMPro.TextMeshPro>().color = Utils.VANITY_COLOR;

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
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
            hubrisDebuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Hubris.png");
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
                                targeter.indicator.visualizerPrefab = vanityTargetIndicatorPrefab;
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
                else if (self.characterBody.GetBuffCount(hubrisDebuff) > 0)
                {
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                    self.characterBody.SetBuffCount(hubrisDebuff.buffIndex, 0);
#pragma warning restore Publicizer001 // Accessing a member that was not originally public
                }

                return orig(self, equipDef);
            };

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    args.damageMultAdd -= Utils.GetExponentialStacking(damageLostPercentPerStack, sender.GetBuffCount(hubrisDebuff));
                }
            };

            GenericGameEvents.BeforeTakeDamage += (damageInfo, attacker, victim) =>
            {
                if (victim.body && victim.body.inventory)
                {
                    int stackCount = victim.body.GetBuffCount(hubrisDebuff);
                    if (stackCount > 0)
                    {
                        float multiplier = 1 + stackCount * damageLostPercentPerStack;
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

