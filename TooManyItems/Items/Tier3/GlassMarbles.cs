using R2API;
using RoR2;
using TooManyItems.Managers;

namespace TooManyItems.Items.Tier3
{
    internal class GlassMarbles
    {
        public static ItemDef itemDef;

        // Gain BASE damage per level.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Glass Marbles",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_GLASSMARBLES_DESC"]
        );
        public static ConfigurableValue<float> damagePerLevelPerStack = new(
            "Item: Glass Marbles",
            "Damage Increase",
            2f,
            "Amount of base damage gained per level per stack.",
            ["ITEM_GLASSMARBLES_DESC"]
        );

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("GlassMarbles", [ItemTag.Damage, ItemTag.CanBeTemporary], ItemTier.Tier3);

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
                        args.baseDamageAdd += count * damagePerLevelPerStack.Value;
                        args.levelDamageAdd += count * damagePerLevelPerStack.Value;
                    }
                }
            };
        }
    }
}
