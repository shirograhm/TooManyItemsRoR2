using R2API;
using RoR2;
using System.Collections.Generic;

namespace TooManyItems
{
    internal class PaperPlane : BaseItem
    {
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
            GenerateItem(
                "PAPERPLANE",
                "PaperPlane.prefab",
                "PaperPlane.png",
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

            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
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
