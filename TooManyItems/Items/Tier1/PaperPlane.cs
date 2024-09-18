using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class PaperPlane
    {
        public static ItemDef itemDef;

        // Increase damaage while airborne.
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

            Utils.SetItemTier(itemDef, ItemTier.Tier1);

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("PaperPlane.png");
            itemDef.pickupModelPrefab = AssetHandler.bundle.LoadAsset<GameObject>("PaperPlane.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage
            };
        }

        public static void Hooks()
        {
            On.RoR2.CharacterBody.FixedUpdate += (orig, self) =>
            {
                orig(self);

                if (self && self.inventory)
                {
                    if (self.inventory.GetItemCount(itemDef) > 0)
                    {
                        Utils.ForceRecalculate(self);
                    }
                }
            };

            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (attackerInfo.inventory != null && attackerInfo.body != null)
                {
                    int itemCount = attackerInfo.inventory.GetItemCount(itemDef);
                    if (itemCount > 0 && attackerInfo.body.characterMotor && !attackerInfo.body.characterMotor.isGrounded)
                    {
                        damageInfo.damage *= 1 + itemCount * damageBonusPercent;
                    }
                }
            };
        }
    }
}
