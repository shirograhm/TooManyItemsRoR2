using R2API;
using RoR2;
using System.Collections.Generic;

namespace TooManyItems
{
    internal class RedBlueGlasses : BaseItem
    {
        // Crit more and crit harder.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: 3D Glasses",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_REDBLUEGLASSES_DESC"
            }
        );
        public static ConfigurableValue<float> critChancePerStack = new(
            "Item: 3D Glasses",
            "Crit Chance",
            6f,
            "Amount of crit chance gained per stack.",
            new List<string>()
            {
                "ITEM_REDBLUEGLASSES_DESC"
            }
        );
        public static ConfigurableValue<float> critDamagePerStack = new(
            "Item: 3D Glasses",
            "Crit Damage",
            6f,
            "Amount of crit damage gained per stack.",
            new List<string>()
            {
                "ITEM_REDBLUEGLASSES_DESC"
            }
        );
        public static float critChancePercent = critChancePerStack.Value / 100f;
        public static float critDamagePercent = critDamagePerStack.Value / 100f;

        internal static void Init()
        {
            GenerateItem(
                "REDBLUEGLASSES",
                "3DGlasses.prefab",
                "3dGlasses.png",
                ItemTier.Tier1,
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
                        args.critAdd += count * critChancePerStack.Value;
                        args.critDamageMultAdd += count * critDamagePercent;
                    }
                }
            };
        }
    }
}
