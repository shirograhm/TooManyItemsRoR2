using R2API;
using RoR2;
using System.Collections.Generic;

namespace TooManyItems
{
    internal class RubberDucky : BaseItem
    {
        // Gain armor.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Rubber Ducky",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_RUBBERDUCKY_DESC"
            }
        );
        public static ConfigurableValue<int> armorPerStack = new(
            "Item: Rubber Ducky",
            "Armor",
            5,
            "Amount of flat armor gained per stack.",
            new List<string>()
            {
                "ITEM_RUBBERDUCKY_DESC"
            }
        );

        internal static void Init()
        {
            GenerateItem(
                "RUBBERDUCKY",
                "RubberDucky.prefab",
                "RubberDucky.png",
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
