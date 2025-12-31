using R2API;
using RoR2;
using TooManyItems.Managers;

namespace TooManyItems.Items.Tier1
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
            ["ITEM_PROPELLERHAT_DESC"]
        );
        public static ConfigurableValue<float> movespeedBonus = new(
            "Item: Propeller Hat",
            "Movement Speed",
            16f,
            "Percent bonus movement speed per stack while airborne.",
            ["ITEM_PROPELLERHAT_DESC"]
        );
        public static ConfigurableValue<float> movespeedBonusExtraStacks = new(
            "Item: Propeller Hat",
            "Movement Speed Extra Stacks",
            16f,
            "Percent bonus movement speed with extra stacks while airborne.",
            ["ITEM_PROPELLERHAT_DESC"]
        );
        public static ConfigurableValue<float> fallDamageTaken = new(
            "Item: Propeller Hat",
            "Fall Damage Taken",
            20f,
            "Percent fall damage taken while holding this item.",
            ["ITEM_PROPELLERHAT_DESC"]
        );
        public static float movespeedBonusPercent = movespeedBonus.Value / 100f;
        public static float movespeedBonusExtraStacksPercent = movespeedBonusExtraStacks.Value / 100f;
        public static float fallDamageTakenPercent = fallDamageTaken.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("PropellerHat", [ItemTag.Utility, ItemTag.CanBeTemporary], ItemTier.Tier1);

            Hooks();
        }

        public static void Hooks()
        {
            Utilities.AddRecalculateOnFrameHook(itemDef);

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCountEffective(itemDef);
                    if (count > 0 && sender.characterMotor && !sender.characterMotor.isGrounded)
                    {
                        args.moveSpeedMultAdd += Utilities.GetLinearStacking(movespeedBonusPercent, movespeedBonusExtraStacks, count);
                    }
                }
            };

            GameEventManager.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
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
