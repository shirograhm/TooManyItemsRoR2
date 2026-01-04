using RoR2;
using TooManyItems.Managers;

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
            ["ITEM_MILKCARTON_DESC"]
        );
        public static ConfigurableValue<float> eliteDamageReduction = new(
            "Item: Milk Carton",
            "Damage Reduction",
            10f,
            "Percent damage reduction against elite enemies.",
            ["ITEM_MILKCARTON_DESC"]
        );
        public static ConfigurableValue<float> eliteDamageReductionExtraStacks = new(
            "Item: Milk Carton",
            "Damage Reduction Extra Stacks",
            10f,
            "Percent damage reduction against elite enemies with extra stacks.",
            ["ITEM_MILKCARTON_DESC"]
        );
        public static float percentEliteDamageReduction = eliteDamageReduction.Value / 100f;
        public static float percentEliteDamageReductionExtraStacks = eliteDamageReductionExtraStacks.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("MilkCarton", [ItemTag.Utility, ItemTag.CanBeTemporary], ItemTier.Tier1);

            Hooks();
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
                        float damageReductionPercent = Utilities.GetHyperbolicStacking(percentEliteDamageReduction, percentEliteDamageReductionExtraStacks, count);
                        damageInfo.damage *= 1 - damageReductionPercent;
                    }
                }
            };
        }
    }
}
