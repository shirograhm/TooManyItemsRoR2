using R2API;
using RoR2;
using System.Collections.Generic;

namespace TooManyItems
{
    internal class GlassMarbles : BaseItem
    {
        // Gain BASE damage per level.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Glass Marbles",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_GLASSMARBLES_DESC"
            }
        );
        public static ConfigurableValue<float> damagePerLevelPerStack = new(
            "Item: Glass Marbles",
            "Damage Increase",
            2f,
            "Amount of base damage gained per level per stack.",
            new List<string>()
            {
                "ITEM_GLASSMARBLES_DESC"
            }
        );

        internal static void Init()
        {
            GenerateItem(
                "GLASSMARBLES",
                "GlassMarbles.prefab",
                "GlassMarbles.png",
                ItemTier.Tier3,
                [
                    ItemTag.Damage,
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
                        args.baseDamageAdd += count * damagePerLevelPerStack.Value;
                        args.levelDamageAdd += count * damagePerLevelPerStack.Value;
                    }
                }
            };
        }
    }
}
