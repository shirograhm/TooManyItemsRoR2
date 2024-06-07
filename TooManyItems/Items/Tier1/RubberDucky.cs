using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
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
            GenerateItem();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "RUBBERDUCKY";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier1);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("RubberDucky.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("RubberDucky.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility
            };
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        args.armorAdd += count * armorPerStack.Value;
                    }
                }
            };
        }
    }
}
