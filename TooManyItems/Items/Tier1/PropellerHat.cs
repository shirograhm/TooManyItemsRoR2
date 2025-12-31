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
        public static float percentMovespeedBonus = movespeedBonus.Value / 100f;
        public static float percentMovespeedBonusExtraStacks = movespeedBonusExtraStacks.Value / 100f;
        public static float fallDamageTakenPercent = fallDamageTaken.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("PropellerHat", [ItemTag.Utility, ItemTag.CanBeTemporary], ItemTier.Tier1, [
                //new("mdlCommandoDualies", "Head", new Vector3(0f, 0.38f, 0.023f), new Vector3(0f, 40f, 10f), new Vector3(1.5f, 1.5f, 1.5f)),
                //new("mdlHuntress", "Head", new Vector3(0f, 0.3f, 0.013f), new Vector3(0f, 40f, 10f), new Vector3(1.2f, 1.2f, 1.2f)),
                //new("mdlBandit2", "Hat", new Vector3(0f, 0.25f, 0.023f), new Vector3(0f, 40f, 10f), new Vector3(1.2f, 1.2f, 1.2f)),
                //new("mdlToolbot", "Head", new Vector3(0f, 1.3f, 0.023f), new Vector3(0f, 50f, 10f), new Vector3(15f, 15f, 15f)),
                //new("mdlEngi", "HeadCenter", new Vector3(0f, 0.18f, 0.023f), new Vector3(0f, 50f, 10f), new Vector3(1.5f, 1.5f, 1.5f)),
                //new("mdlEngiTurret", "Head", new Vector3(0f, 1.8f, 0.53f), new Vector3(0f, 50f, 10f), new Vector3(3f, 3f, 3f)),
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
                        args.moveSpeedMultAdd += Utilities.GetLinearStacking(percentMovespeedBonus, movespeedBonusExtraStacks, count);
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
