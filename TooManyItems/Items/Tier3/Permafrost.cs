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
            itemDef = ItemManager.GenerateItem("Permafrost", [ItemTag.Damage, ItemTag.Utility, ItemTag.CanBeTemporary], ItemTier.Tier3, [
                //new("mdlCommandoDualies", "Head", new Vector3(0f, 0.38f, 0.023f), new Vector3(0f, 40f, 10f), new Vector3(1.5f, 1.5f, 1.5f)),
                //new("mdlHuntress", "Head", new Vector3(0f, 0.3f, 0.013f), new Vector3(0f, 40f, 10f), new Vector3(1.2f, 1.2f, 1.2f)),
                //new("mdlBandit2", "Hat", new Vector3(0f, 0.25f, 0.023f), new Vector3(0f, 40f, 10f), new Vector3(1.2f, 1.2f, 1.2f)),
                //new("mdlToolbot", "Head", new Vector3(0f, 1.3f, 0.023f), new Vector3(0f, 50f, 10f), new Vector3(15f, 15f, 15f)),
                //new("mdlEngi", "HeadCenter", new Vector3(0f, 0.18f, 0.023f), new Vector3(0f, 50f, 10f), new Vector3(1.5f, 1.5f, 1.5f)),
                //new("mdlEngiTurret", "Head", new Vector3(0f, 0.5f, 0.023f), new Vector3(0f, 50f, 10f), new Vector3(3f, 3f, 3f)),
                //new("mdlMage", "Head", new Vector3(0f, 0.17f, 0.004f), new Vector3(0f, -20f, 10f), new Vector3(1.2f, 1.2f, 1.2f)),
                //new("mdlMerc", "Head", new Vector3(0f, 0.25f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1.5f, 1.5f, 1.5f)),
                //new("mdlTreebot", "PlatformBase", new Vector3(0f, 2.2f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)),
                //new("mdlLoader", "Head", new Vector3(0f, 0.2f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1.5f, 1.5f, 1.5f)),
                //new("mdlCroco", "Head", new Vector3(0f, 1.5f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)),
                //new("mdlCaptain", "Hat", new Vector3(0f, 0.5f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)),
                //new("mdlRailGunner", "Head", new Vector3(0f, 0.2f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)),
                //new("mdlVoidSurvivor", "Neck", new Vector3(0f, 0.28f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1.5f, 1.5f, 1.5f)),
                //new("mdlSeeker", "Head", new Vector3(0f, 0.5f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)),
                //new("mdlChef", "Head", new Vector3(0f, 0.5f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)),
                //new("mdlFalseSon", "Head", new Vector3(0f, 0.5f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)),
                //new("mdlDroneTech", "Head", new Vector3(0f, 0.5f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)),
                //new("mdlDrifter", "Head", new Vector3(0f, 0.5f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)),
                //new("mdlScav", "Head", new Vector3(0f, 0.5f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f))
            ]);
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
                        // If damage is from skills and not on the same team, apply freeze
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
