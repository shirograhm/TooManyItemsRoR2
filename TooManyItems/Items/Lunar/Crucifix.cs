using RoR2;
using TooManyItems.Managers;

namespace TooManyItems.Items.Lunar
{
    internal class Crucifix
    {
        public static ItemDef itemDef;

        // Reduce damage taken. Taking damage set you on fire.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Crucifix",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_CRUCIFIX_DESC"]
        );
        public static ConfigurableValue<float> damageReduction = new(
            "Item: Crucifix",
            "Damage Reduction",
            100f,
            "Percentage of damage reduced.",
            ["ITEM_CRUCIFIX_DESC"]
        );
        public static ConfigurableValue<float> maxHealthBurnAmount = new(
            "Item: Crucifix",
            "Burn Amount",
            30f,
            "Percentage of max health taken over the duration of the burn.",
            ["ITEM_CRUCIFIX_DESC"]
        );
        public static ConfigurableValue<float> maxHealthBurnAmountReduction = new(
            "Item: Crucifix",
            "Burn Amount Reduction",
            15f,
            "Percentage of burn damage reduced per stack of this item. This scales hyperbolically.",
            ["ITEM_CRUCIFIX_DESC"]
        );
        public static ConfigurableValue<bool> isCrucifixBurnStackable = new(
            "Item: Crucifix",
            "Is Burn Stackable",
            false,
            "Whether or not the burn caused by Crucifix is stackable.",
            ["ITEM_CRUCIFIX_DESC"]
        );
        public static float percentDamageReduction = damageReduction.Value / 100f;
        public static float percentMaxHealthBurnAmount = maxHealthBurnAmount.Value / 100f;
        public static float percentMaxHealthBurnAmountReduction = maxHealthBurnAmountReduction.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("Crucifix", [ItemTag.Utility], ItemTier.Lunar);

            Hooks();
        }

        public static void Hooks()
        {
            GameEventManager.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.inventory == null || victimInfo.body == null || victimInfo.body.healthComponent == null || attackerInfo.body == null) return;

                // Not immune to void death
                if (damageInfo.damageType == DamageType.VoidDeath) return;

                int count = victimInfo.inventory.GetItemCountPermanent(itemDef);
                if (count > 0 && attackerInfo.body != victimInfo.body)
                {
                    damageInfo.damage *= 1 - percentDamageReduction;
                    float stackedPercentage = Utilities.GetReverseExponentialStacking(percentMaxHealthBurnAmount, percentMaxHealthBurnAmountReduction, count);

                    InflictDotInfo dotInfo = new()
                    {
                        victimObject = victimInfo.gameObject,
                        attackerObject = victimInfo.gameObject,
                        totalDamage = victimInfo.body.healthComponent.fullCombinedHealth * stackedPercentage,
                        dotIndex = DotController.DotIndex.Burn,
                        duration = 0f,
                        damageMultiplier = 1f
                    };
                    if (!isCrucifixBurnStackable.Value) dotInfo.maxStacksFromAttacker = 1;

                    DotController.InflictDot(ref dotInfo);
                }
            };
        }
    }
}
