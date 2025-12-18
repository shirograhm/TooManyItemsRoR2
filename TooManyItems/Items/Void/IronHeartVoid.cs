using R2API;
using RoR2;
using System.Collections.Generic;

namespace TooManyItems
{
    internal class IronHeartVoid : BaseItem
    {
        // Gain HP. Gain bonus damage based on your max health.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Defiled Heart",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_VOIDHEART_DESC"
            }
        );
        public static ConfigurableValue<float> healthIncrease = new(
            "Item: Defiled Heart",
            "Health Increase",
            300f,
            "Bonus health gained from this item. Does not increase with stacks.",
            new List<string>()
            {
                "ITEM_VOIDHEART_DESC"
            }
        );
        public static ConfigurableValue<float> percentDamagePerStack = new(
            "Item: Defiled Heart",
            "Bonus Damage Scaling",
            1.5f,
            "Percent of maximum health gained as base damage.",
            new List<string>()
            {
                "ITEM_VOIDHEART_DESC"
            }
        );
        public static float multiplierPerStack = percentDamagePerStack.Value / 100.0f;

        internal static void Init()
        {
            GenerateItem(
                "VOIDHEART",
                "IronHeartVoid.prefab",
                "IronHeartVoid.png",
                ItemTier.VoidTier3,
                [
                    ItemTag.Damage,
                    ItemTag.Healing,
                    ItemTag.CanBeTemporary
                ]
            );

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        public static float CalculateDamageBonus(CharacterBody sender, float itemCount)
        {
            return sender.healthComponent.fullCombinedHealth * itemCount * multiplierPerStack;
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
                        args.baseHealthAdd += healthIncrease.Value;
                        args.baseDamageAdd += CalculateDamageBonus(sender, count);
                    }
                }
            };
        }
    }
}
