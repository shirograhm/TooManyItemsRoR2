using R2API;
using RoR2;
using System.Collections.Generic;

namespace TooManyItems
{
    internal class PropellerHat : BaseItem
    {
        // Increase damage while airborne.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Propeller Hat",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_PROPELLERHAT_DESC"
            }
        );
        public static ConfigurableValue<float> movespeedBonus = new(
            "Item: Propeller Hat",
            "Movement Speed",
            16f,
            "Percent bonus movement speed per stack while airborne.",
            new List<string>()
            {
                "ITEM_PROPELLERHAT_DESC"
            }
        );
        public static ConfigurableValue<float> fallDamageTaken = new(
            "Item: Propeller Hat",
            "Fall Damage Taken",
            20f,
            "Percent fall damage taken while holding this item.",
            new List<string>()
            {
                "ITEM_PROPELLERHAT_DESC"
            }
        );
        public static float movespeedBonusPercent = movespeedBonus.Value / 100f;
        public static float fallDamageTakenPercent = fallDamageTaken.Value / 100f;

        internal static void Init()
        {
            GenerateItem(
                "PROPELLERHAT",
                "PropellerHat.prefab",
                "PropellerHat.png",
                ItemTier.Tier1,
                [
                    ItemTag.Utility,
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

            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.inventory == null || victimInfo.body == null) return;

                int count = victimInfo.inventory.GetItemCountEffective(itemDef);
                if (count > 0 && damageInfo.damageType == DamageType.FallDamage)
                {
                    damageInfo.damage *= fallDamageTakenPercent;
                }
            };
        }
    }
}
