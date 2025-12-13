using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
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
            new List<string>()
            {
                "ITEM_DOUBLEDOWN_DESC"
            }
        );
        public static ConfigurableValue<float> upFrontDamage = new(
            "Item: Double Down",
            "Total Up Front",
            200f,
            "Percentage of the total DoT damage taken up front.",
            new List<string>()
            {
                "ITEM_DOUBLEDOWN_DESC"
            }
        );
        public static ConfigurableValue<float> upFrontDamageReduction = new(
            "Item: Double Down",
            "Damage Reduced Per Stack",
            12f,
            "Percentage of the up front damage reduced with stacks.",
            new List<string>()
            {
                "ITEM_DOUBLEDOWN_DESC"
            }
        );
        public static float upFrontDamagePercent = upFrontDamage.Value / 100f;
        public static float upFrontDamageReductionPercent = upFrontDamageReduction.Value / 100f;

        internal static void Init()
        {
            GenerateItem();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "DOUBLEDOWN";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Lunar);

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("DoubleDown.png");
            itemDef.pickupModelPrefab = AssetHandler.bundle.LoadAsset<GameObject>("DoubleDown.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        public static void Hooks()
        {
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
                            float stackedDamage = Utils.GetReverseExponentialStacking(upFrontDamagePercent, upFrontDamageReductionPercent, itemCount);
                            float totalDamageCalc = dotDamage * stackedDamage;

                            vicBody.healthComponent.TakeDamage(new DamageInfo
                            {
                                damage = totalDamageCalc,
                                damageType = DamageType.Generic,
                                attacker = info.attackerObject,
                                inflictor = info.attackerObject,
                                position = vicBody.corePosition,
                                force = Vector3.zero,
                                crit = false,
                                // Cannot proc items/effects
                                procCoefficient = 0f,
                                procChainMask = new ProcChainMask()
                            });

                            return; // Skip applying the DoT
                        }
                    }
                }
            }

            orig(ref info);
        }
    }
}
