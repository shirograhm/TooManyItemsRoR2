using R2API;
using RoR2;
using System.Linq;
using TooManyItems.Helpers;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace TooManyItems.Items.Equip.Lunar
{
    internal class Vanity
    {
        public static EquipmentDef equipmentDef;
        public static BuffDef hubrisDebuff;

        public static GameObject vanityTargetIndicatorPrefab;
        public static GameObject implosionEffectObject;

        public static DamageAPI.ModdedDamageType damageType;
        public static DamageColorIndex damageColor = DamageColorManager.RegisterDamageColor(Utilities.VANITY_COLOR);

        // Gain stacks of Hubris when killing enemies. Activate to cleanse all stacks and damage a target enemy. This damage scales with stacks cleansed.
        public static ConfigurableValue<bool> isEnabled = new(
            "Equipment: Crown of Vanity",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["EQUIPMENT_VANITY_DESC"]
        );
        public static ConfigurableValue<float> damageLostPerStack = new(
            "Equipment: Crown of Vanity",
            "Base Damage Lost",
            3f,
            "Percent base damage lost for each stack of Hubris.",
            ["EQUIPMENT_VANITY_DESC"]
        );
        public static float damageLostPercentPerStack = damageLostPerStack.Value / 100f;

        public static ConfigurableValue<float> damageDealtPerStack = new(
            "Equipment: Crown of Vanity",
            "Damage Dealt",
            150f,
            "Percent damage dealt for each stack of Hubris accrued.",
            ["EQUIPMENT_VANITY_DESC"]
        );
        public static ConfigurableValue<float> coefficient = new(
            "Equipment: Crown of Vanity",
            "Proc Coefficient",
            1.5f,
            "Proc coefficient for the single damage instance on equipment use.",
            ["EQUIPMENT_VANITY_DESC"]
        );
        public static ConfigurableValue<int> equipCooldown = new(
            "Equipment: Crown of Vanity",
            "Cooldown",
            70,
            "Equipment cooldown.",
            ["EQUIPMENT_VANITY_DESC"]
        );
        public static float damageDealtPercentPerStack = damageDealtPerStack.Value / 100f;

        internal static void Init()
        {
            equipmentDef = ItemManager.GenerateEquipment("Vanity", equipCooldown.Value, isLunar: true, canBeRandomlyTriggered: false, enigmaCompatible: false);

            hubrisDebuff = ItemManager.GenerateBuff("Hubris", AssetManager.bundle.LoadAsset<Sprite>("Hubris.png"), canStack: true, isDebuff: true);
            ContentAddition.AddBuffDef(hubrisDebuff);

            vanityTargetIndicatorPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/WoodSpriteIndicator").InstantiateClone("TooManyItems_vanityTargetIndicator", false);
            vanityTargetIndicatorPrefab.GetComponentInChildren<SpriteRenderer>().color = Utilities.VANITY_COLOR;
            vanityTargetIndicatorPrefab.GetComponentInChildren<TMPro.TextMeshPro>().color = Utilities.VANITY_COLOR;

            implosionEffectObject = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteIce/AffixWhiteExplosion.prefab").WaitForCompletion();
            // implosionEffectObject = Assets.bundle.LoadAsset<GameObject>("VanityImplosionEffect.prefab");
            ContentAddition.AddEffect(implosionEffectObject);

            damageType = DamageAPI.ReserveDamageType();

            Hooks();
        }

        public static void Hooks()
        {
            On.RoR2.EquipmentSlot.Start += (orig, self) =>
            {
                orig(self);
                self.gameObject.AddComponent<EquipmentTargetHandler>();
            };

            On.RoR2.EquipmentSlot.Update += (orig, self) =>
            {
                orig(self);

                EquipmentTargetHandler targeter = self.gameObject.GetComponent<EquipmentTargetHandler>();
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

                return orig(self, equipDef);
            };

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    args.damageMultAdd -= Utilities.GetExponentialStacking(damageLostPercentPerStack, sender.GetBuffCount(hubrisDebuff));
                }
            };

            GlobalEventManager.onCharacterDeathGlobal += (damageReport) =>
            {
                if (!NetworkServer.active) return;

                CharacterBody atkBody = damageReport.attackerBody;
                if (atkBody && atkBody.equipmentSlot && atkBody.equipmentSlot.equipmentIndex == equipmentDef.equipmentIndex)
                {
                    if (!damageReport.damageInfo.HasModdedDamageType(damageType))
                    {
                        atkBody.AddBuff(hubrisDebuff);
                    }
                }
            };
        }

        private static bool OnUse(EquipmentSlot slot)
        {
            EquipmentTargetHandler targeter = slot.GetComponent<EquipmentTargetHandler>();
            CharacterBody targetEnemy = targeter && targeter.obj ? targeter.obj.GetComponent<CharacterBody>() : null;

            CharacterBody user = slot.characterBody;
            if (user && targetEnemy && targetEnemy.healthComponent)
            {
                int buffCount = user.GetBuffCount(hubrisDebuff);

#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                user.SetBuffCount(hubrisDebuff.buffIndex, 0);
#pragma warning restore Publicizer001 // Accessing a member that was not originally public

                EffectManager.SpawnEffect(implosionEffectObject, new EffectData
                {
                    origin = targetEnemy.corePosition,
                    scale = 0.2f * buffCount + targetEnemy.radius,
                    color = Utilities.VANITY_COLOR
                },
                true);

                float damageAmount = user.damage * damageDealtPercentPerStack * buffCount;
                DamageInfo damageInfo = new()
                {
                    damage = damageAmount,
                    attacker = user.gameObject,
                    inflictor = user.gameObject,
                    procCoefficient = coefficient.Value,
                    position = targetEnemy.corePosition,
                    crit = user.RollCrit(),
                    damageColorIndex = damageColor,
                    procChainMask = new ProcChainMask(),
                    damageType = DamageType.BypassOneShotProtection | DamageType.BypassArmor | DamageType.BypassBlock | DamageType.Silent
                };
                damageInfo.AddModdedDamageType(damageType);
                targetEnemy.healthComponent.TakeDamage(damageInfo);

                return true;
            }

            return false;
        }
    }
}

