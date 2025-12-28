using RoR2;
using TooManyItems.Handlers;
using TooManyItems.Managers;

namespace TooManyItems.Items.Tier3
{
    internal class Permafrost
    {
        public static ItemDef itemDef;

        public static DamageColorIndex damageColor = DamageColorManager.RegisterDamageColor(Utilities.PERMAFROST_COLOR);

        // Dealing damage has a chance to freeze enemies. You deal bonus damage to frozen enemies.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Permafrost",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_PERMAFROST_DESC"]
        );
        public static ConfigurableValue<float> freezeChance = new(
            "Item: Permafrost",
            "Freeze Chance",
            5f,
            "Chance to apply freeze when dealing damage.",
            ["ITEM_PERMAFROST_DESC"]
        );
        public static ConfigurableValue<float> frozenDamageMultiplier = new(
            "Item: Permafrost",
            "Bonus Frozen Damage",
            50f,
            "Percent bonus damage dealt to frozen enemies.",
            ["ITEM_PERMAFROST_DESC"]
        );
        public static ConfigurableValue<float> frozenDamageMultiplierExtraStacks = new(
            "Item: Permafrost",
            "Bonus Frozen Damage Extra Stacks",
            100f,
            "Percent bonus damage dealt to frozen enemies with extra stacks.",
            ["ITEM_PERMAFROST_DESC"]
        );
        public static float freezeChancePercent = freezeChance.Value / 100.0f;
        public static float frozenDamageMultiplierPercent = frozenDamageMultiplier.Value / 100.0f;
        public static float frozenDamageMultiplierExtraStacksPercent = frozenDamageMultiplierExtraStacks.Value / 100.0f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("Permafrost", [ItemTag.Damage, ItemTag.Utility, ItemTag.CanBeTemporary], ItemTier.Tier3);

            itemDef.pickupModelPrefab.AddComponent<PermafrostRotationHandler>();

            Hooks();
        }

        public static void Hooks()
        {
            GameEventManager.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                CharacterBody attackerBody = attackerInfo.body;
                CharacterBody victimBody = victimInfo.body;
                if (attackerBody && victimBody && attackerBody.inventory)
                {
                    int count = attackerBody.inventory.GetItemCountEffective(itemDef);
                    if (count > 0 && attackerBody.master)
                    {
                        // If damage is from skills, not on the same team, roll to freeze
                        if (!Utilities.OnSameTeam(attackerBody, victimBody) && Utilities.IsSkillDamage(damageInfo))
                        {
                            if (Util.CheckRoll0To1(freezeChancePercent * damageInfo.procCoefficient, attackerBody.master.luck, attackerBody.master))
                            {
                                damageInfo.damageType |= DamageType.Freeze2s;
                            }
                        }

                        if (victimBody.healthComponent.isInFrozenState)
                        {
                            damageInfo.damage *= 1 + Utilities.GetLinearStacking(frozenDamageMultiplierPercent, frozenDamageMultiplierExtraStacksPercent, count);
                            damageInfo.damageColorIndex = damageColor;
                        }
                    }
                }
            };
        }
    }
}
