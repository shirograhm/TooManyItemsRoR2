using R2API;
using RoR2;
using System.Collections.Generic;
using TooManyItems.Managers;
using UnityEngine;

namespace TooManyItems.Items.Void
{
    internal class IronHeartVoid
    {
        public static ItemDef itemDef;

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
            GenerateItem();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "VOIDHEART";
            itemDef.AutoPopulateTokens();

            Utilities.SetItemTier(itemDef, ItemTier.VoidTier3);

            GameObject prefab = AssetManager.bundle.LoadAsset<GameObject>("IronHeartVoid.prefab");
            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            itemDef.pickupIconSprite = AssetManager.bundle.LoadAsset<Sprite>("IronHeartVoid.png");
            itemDef.pickupModelPrefab = prefab;
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.requiredExpansion = TooManyItems.sotvDLC;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,
                ItemTag.Healing,

                ItemTag.CanBeTemporary
            };
        }

        public static float CalculateDamageBonus(CharacterBody sender, float itemCount)
        {
            return sender.healthComponent.fullCombinedHealth * itemCount * multiplierPerStack;
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
