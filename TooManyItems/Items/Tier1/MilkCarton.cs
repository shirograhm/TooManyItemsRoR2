using R2API;
using RoR2;
using System.Collections.Generic;
using TooManyItems.Managers;
using UnityEngine;

namespace TooManyItems.Items.Tier1
{
    internal class MilkCarton
    {
        public static ItemDef itemDef;

        // Reduce damage from elite enemies.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Milk Carton",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_MILKCARTON_DESC"
            }
        );
        public static ConfigurableValue<float> eliteDamageReduction = new(
            "Item: Milk Carton",
            "Damage Reduction",
            10f,
            "Percent damage reduction against elite enemies.",
            new List<string>()
            {
                "ITEM_MILKCARTON_DESC"
            }
        );
        public static float eliteDamageReductionPercent = eliteDamageReduction.Value / 100f;

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

            itemDef.name = "MILKCARTON";
            itemDef.AutoPopulateTokens();

            Utilities.SetItemTier(itemDef, ItemTier.Tier1);

            GameObject prefab = AssetManager.bundle.LoadAsset<GameObject>("MilkCarton.prefab");
            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            itemDef.pickupIconSprite = AssetManager.bundle.LoadAsset<Sprite>("MilkCarton.png");
            itemDef.pickupModelPrefab = prefab;
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility,

                ItemTag.CanBeTemporary
            };
        }

        public static void Hooks()
        {
            GameEventManager.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.inventory)
                {
                    int count = victimInfo.inventory.GetItemCountEffective(itemDef);
                    if (attackerInfo.body && attackerInfo.body.isElite && count > 0 && damageInfo.damageColorIndex != DamageColorIndex.DelayedDamage)
                    {
                        float damageReductionPercent = Utilities.GetHyperbolicStacking(eliteDamageReductionPercent, count);
                        damageInfo.damage *= 1 - damageReductionPercent;
                    }
                }
            };
        }
    }
}
