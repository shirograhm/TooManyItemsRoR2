using R2API;
using RoR2;
using TooManyItems.Managers;

namespace TooManyItems.Items.Void
{
    internal class VoidHeart
    {
        public static ItemDef itemDef;

        // Gain HP. Gain bonus damage based on your max health.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Defiled Heart",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_VOIDHEART_DESC"]
        );
        public static ConfigurableValue<float> healthIncrease = new(
            "Item: Defiled Heart",
            "Health Increase",
            200f,
            "Bonus health gained from this item. Does not increase with stacks.",
            ["ITEM_VOIDHEART_DESC"]
        );
        public static ConfigurableValue<float> percentDamagePerStack = new(
            "Item: Defiled Heart",
            "Bonus Damage Scaling",
            1.5f,
            "Percent of maximum health gained as base damage.",
            ["ITEM_VOIDHEART_DESC"]
        );
        public static ConfigurableValue<float> percentDamagePerExtraStack = new(
            "Item: Defiled Heart",
            "Bonus Damage Scaling Extra Stacks",
            1.5f,
            "Percent of maximum health gained as base damage with extra stacks.",
            ["ITEM_VOIDHEART_DESC"]
        );
        public static float multiplierPerStack = percentDamagePerStack.Value / 100.0f;
        public static float multiplierPerExtraStack = percentDamagePerExtraStack.Value / 100.0f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("VoidHeart", [ItemTag.Damage, ItemTag.Healing, ItemTag.CanBeTemporary], ItemTier.VoidTier3);

            Hooks();
        }

        public static float CalculateDamageBonus(CharacterBody sender, int itemCount)
        {
            return sender.healthComponent.fullCombinedHealth * Utilities.GetLinearStacking(multiplierPerStack, multiplierPerExtraStack, itemCount);
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory && sender.healthComponent)
                {
                    int count = sender.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        args.baseHealthAdd += healthIncrease.Value;
                        args.baseDamageAdd += CalculateDamageBonus(sender, count);
                    }
                }
            };
        }
    }
}
