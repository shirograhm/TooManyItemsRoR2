using R2API;
using RoR2;
using System.Collections.Generic;

namespace TooManyItems
{
    internal class MilkCarton : BaseItem
    {
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
            GenerateItem(
                "MILKCARTON",
                "MilkCarton.prefab",
                "MilkCarton.png",
                ItemTier.Tier1,
                [
                    ItemTag.Utility,
                    ItemTag.CanBeTemporary
                ]
            );

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        public static new void Hooks()
        {
            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.inventory)
                {
                    int count = victimInfo.inventory.GetItemCountEffective(itemDef);
                    if (attackerInfo.body && attackerInfo.body.isElite && count > 0 && damageInfo.damageColorIndex != DamageColorIndex.DelayedDamage)
                    {
                        float damageReductionPercent = Utils.GetHyperbolicStacking(eliteDamageReductionPercent, count);
                        damageInfo.damage *= 1 - damageReductionPercent;
                    }
                }
            };
        }
    }
}
