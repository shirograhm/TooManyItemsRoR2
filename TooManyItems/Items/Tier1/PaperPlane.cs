using RoR2;
using TooManyItems.Managers;

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
            ["ITEM_PAPERPLANE_DESC"]
        );
        public static ConfigurableValue<float> damageBonus = new(
            "Item: Paper Plane",
            "Damage Increase",
            15f,
            "Percent bonus damage dealt per stack while airborne.",
            ["ITEM_PAPERPLANE_DESC"]
        );
        public static float damageBonusPercent = damageBonus.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("PaperPlane", [ItemTag.Damage, ItemTag.CanBeTemporary], ItemTier.Tier1);

            Hooks();
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
