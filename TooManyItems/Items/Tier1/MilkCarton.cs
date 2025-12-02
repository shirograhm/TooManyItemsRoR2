using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
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
            "Percent damage reduction agains elite enemies.",
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

            Utils.SetItemTier(itemDef, ItemTier.Tier1);

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("MilkCarton.png");
            itemDef.pickupModelPrefab = AssetHandler.bundle.LoadAsset<GameObject>("MilkCarton.prefab");
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
            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.inventory)
                {
                    int count = victimInfo.inventory.GetItemCountEffective(itemDef);
                    if (attackerInfo.body && attackerInfo.body.isElite && count > 0)
                    {
                        float damageReductionPercent = Utils.GetHyperbolicStacking(eliteDamageReductionPercent, count);
                        damageInfo.damage *= 1 - damageReductionPercent;
                    }
                }
            };
        }
    }
}
