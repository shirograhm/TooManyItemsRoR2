using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class PropellerHat
    {
        public static ItemDef itemDef;

        // Increase damage while airborne.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Propeller Hat",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_PROPELLER_HAT_DESC"
            }
        );
        public static ConfigurableValue<float> movespeedBonus = new(
            "Item: Propeller Hat",
            "Movement Increase",
            20f,
            "Percent bonus movement speed per stack while airborne.",
            new List<string>()
            {
                "ITEM_PROPELLER_HAT_DESC"
            }
        );
        public static float movespeedBonusPercent = movespeedBonus.Value / 100f;

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

            itemDef.name = "PROPELLERHAT";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier1);

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("PropellerHat.png");
            itemDef.pickupModelPrefab = AssetHandler.bundle.LoadAsset<GameObject>("PropellerHat.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility,

                ItemTag.CanBeTemporary
            };
        }

        public static void Hooks()
        {
            On.RoR2.CharacterBody.FixedUpdate += (orig, self) =>
            {
                orig(self);

                if (self && self.inventory)
                {
                    if (self.inventory.GetItemCountEffective(itemDef) > 0)
                    {
                        Utils.ForceRecalculate(self);
                    }
                }
            };

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCountEffective(itemDef);
                    if (count > 0 && sender.characterMotor && !sender.characterMotor.isGrounded)
                    {
                        args.moveSpeedMultAdd += movespeedBonusPercent * count;
                    }
                }
            };
        }
    }
}
