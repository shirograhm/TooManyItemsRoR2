using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
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
            new List<string>()
            {
                "ITEM_GLASSMARBLES_DESC"
            }
        );
        public static ConfigurableValue<float> damagePerLevelPerStack = new(
            "Item: Glass Marbles",
            "Damage Increase",
            2.5f,
            "Amount of base damage gained per level per stack.",
            new List<string>()
            {
                "ITEM_GLASSMARBLES_DESC"
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

            itemDef.name = "GLASSMARBLES";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier3);

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("GlassMarbles.png");
            itemDef.pickupModelPrefab = AssetHandler.bundle.LoadAsset<GameObject>("GlassMarbles.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage
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
                        args.baseDamageAdd += count * damagePerLevelPerStack.Value;
                        args.levelDamageAdd += count * damagePerLevelPerStack.Value;
                    }
                }
            };
        }
    }
}
