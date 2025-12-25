using R2API;
using RoR2;
using System.Collections.Generic;
using TooManyItems.Managers;
using UnityEngine;

namespace TooManyItems.Items.Tier1
{
    internal class PaperPlane
    {
        public static ItemDef itemDef;

        // Increase damage while airborne.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Paper Plane",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_PAPERPLANE_DESC"
            }
        );
        public static ConfigurableValue<float> damageBonus = new(
            "Item: Paper Plane",
            "Damage Increase",
            15f,
            "Percent bonus damage dealt per stack while airborne.",
            new List<string>()
            {
                "ITEM_PAPERPLANE_DESC"
            }
        );
        public static float damageBonusPercent = damageBonus.Value / 100f;

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

            itemDef.name = "PAPERPLANE";
            itemDef.AutoPopulateTokens();

            Utilities.SetItemTier(itemDef, ItemTier.Tier1);

            GameObject prefab = AssetManager.bundle.LoadAsset<GameObject>("PaperPlane.prefab");
            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            itemDef.pickupIconSprite = AssetManager.bundle.LoadAsset<Sprite>("PaperPlane.png");
            itemDef.pickupModelPrefab = prefab;
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,

                ItemTag.CanBeTemporary
            };
        }

        public static void Hooks()
        {
            Utilities.AddRecalculateOnFrameHook(itemDef);

            GameEventManager.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (attackerInfo.inventory != null && attackerInfo.body != null)
                {
                    int itemCount = attackerInfo.inventory.GetItemCountEffective(itemDef);
                    if (itemCount > 0 && attackerInfo.body.characterMotor && !attackerInfo.body.characterMotor.isGrounded)
                    {
                        damageInfo.damage *= 1 + itemCount * damageBonusPercent;
                    }
                }
            };
        }
    }
}
