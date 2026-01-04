using RoR2;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems.Items.Lunar
{
    internal class DoubleDown
    {
        public static ItemDef itemDef;

        // Take all DoT damage up front.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Double Down",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_DOUBLEDOWN_DESC"]
        );
        public static ConfigurableValue<float> upFrontDamage = new(
            "Item: Double Down",
            "Total Up Front",
            200f,
            "Percentage of the total DoT damage taken up front.",
            ["ITEM_DOUBLEDOWN_DESC"]
        );
        public static ConfigurableValue<float> upFrontDamageReduction = new(
            "Item: Double Down",
            "Damage Reduced Per Stack",
            12f,
            "Percentage of the up front damage reduced with stacks.",
            ["ITEM_DOUBLEDOWN_DESC"]
        );
        public static float percentUpFrontDamage = upFrontDamage.Value / 100f;
        public static float percentUpFrontDamageReductionPerStack = upFrontDamageReduction.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("DoubleDown", [ItemTag.Utility], ItemTier.Lunar);

            On.RoR2.DotController.InflictDot_refInflictDotInfo += DotController_InflictDot_refInflictDotInfo;
        }

        private static void DotController_InflictDot_refInflictDotInfo(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo info)
        {
            if (NetworkServer.active)
            {
                // Cannot affect Lunar Ruin DoT
                if (info.victimObject && info.dotIndex != DotController.DotIndex.LunarRuin)
                {
                    CharacterBody vicBody = info.victimObject.GetComponent<CharacterBody>();
                    if (vicBody && vicBody.inventory && vicBody.healthComponent)
                    {
                        int itemCount = vicBody.inventory.GetItemCountEffective(itemDef);
                        if (itemCount > 0)
                        {
                            float dotDamage = info.totalDamage ?? 0f;
                            float stackedDamage = Utilities.GetReverseExponentialStacking(percentUpFrontDamage, percentUpFrontDamageReductionPerStack, itemCount);
                            float totalDamageCalc = dotDamage * stackedDamage;

                            // Roll for crit if the attacker body exists
                            bool isCrit = false;
                            if (info.attackerObject && info.attackerObject.GetComponent<CharacterBody>())
                                isCrit = info.attackerObject.GetComponent<CharacterBody>().RollCrit();

                            // Spawn a cleanse effect to indicate DoT removal
                            EffectManager.SpawnEffect(
                                LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/CleanseEffect"), new EffectData
                                {
                                    origin = vicBody.corePosition,
                                    rootObject = vicBody.gameObject
                                }, transmit: true);

                            vicBody.healthComponent.TakeDamage(new DamageInfo
                            {
                                damage = totalDamageCalc,
                                damageType = DamageType.Generic,
                                attacker = info.attackerObject,
                                inflictor = info.attackerObject,
                                position = vicBody.corePosition,
                                force = Vector3.zero,
                                crit = isCrit,
                                // Cannot proc items/effects
                                procCoefficient = 0f,
                                procChainMask = new ProcChainMask()
                            });

                            return; // Skip applying the DoT (return before orig call)
                        }
                    }
                }
            }

            orig(ref info);
        }
    }
}
