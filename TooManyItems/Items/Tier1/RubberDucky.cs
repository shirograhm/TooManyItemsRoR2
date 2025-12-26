using R2API;
using RoR2;
using TooManyItems.Managers;

namespace TooManyItems.Items.Tier1
{
    internal class RubberDucky
    {
        public static ItemDef itemDef;

        // Gain armor.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Rubber Ducky",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_RUBBERDUCKY_DESC"]
        );
        public static ConfigurableValue<int> armorPerStack = new(
            "Item: Rubber Ducky",
            "Armor",
            5,
            "Amount of flat armor gained per stack.",
            ["ITEM_RUBBERDUCKY_DESC"]
        );

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("RubberDucky", [ItemTag.Utility, ItemTag.CanBeTemporary], ItemTier.Tier1);

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
                        args.armorAdd += count * armorPerStack.Value;
                    }
                }
            };
        }
    }
}
