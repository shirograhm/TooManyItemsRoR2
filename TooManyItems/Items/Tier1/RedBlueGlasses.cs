using R2API;
using RoR2;
using TooManyItems.Managers;

namespace TooManyItems.Items.Tier1
{
    internal class RedBlueGlasses
    {
        public static ItemDef itemDef;

        // Crit more and crit harder.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: 3D Glasses",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_REDBLUEGLASSES_DESC"]
        );
        public static ConfigurableValue<float> critChancePerStack = new(
            "Item: 3D Glasses",
            "Crit Chance",
            6f,
            "Amount of crit chance gained per stack.",
            ["ITEM_REDBLUEGLASSES_DESC"]
        );
        public static ConfigurableValue<float> critChancePerExtraStack = new(
            "Item: 3D Glasses",
            "Crit Chance Extra Stacks",
            6f,
            "Amount of crit chance gained for extra stacks.",
            ["ITEM_REDBLUEGLASSES_DESC"]
        );
        public static ConfigurableValue<float> critDamagePerStack = new(
            "Item: 3D Glasses",
            "Crit Damage",
            6f,
            "Amount of crit damage gained per stack.",
            ["ITEM_REDBLUEGLASSES_DESC"]
        );
        public static ConfigurableValue<float> critDamagePerExtraStack = new(
            "Item: 3D Glasses",
            "Crit Damage Extra Stacks",
            6f,
            "Amount of crit damage gained for extra stacks.",
            ["ITEM_REDBLUEGLASSES_DESC"]
        );
        public static float percentCritChance = critChancePerStack.Value / 100f;
        public static float percentCritChanceExtraStacks = critChancePerExtraStack.Value / 100f;
        public static float percentCritDamage = critDamagePerStack.Value / 100f;
        public static float percentCritDamageExtraStacks = critDamagePerExtraStack.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("RedBlueGlasses", [ItemTag.Damage, ItemTag.CanBeTemporary], ItemTier.Tier1);

            Hooks();
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        args.critAdd += Utilities.GetLinearStacking(critChancePerStack.Value, critChancePerExtraStack.Value, count);
                        args.critDamageMultAdd += Utilities.GetLinearStacking(percentCritDamage, percentCritDamageExtraStacks, count);
                    }
                }
            };
        }
    }
}
