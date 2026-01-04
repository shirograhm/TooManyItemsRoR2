using RoR2;
using TooManyItems.Managers;
using UnityEngine;

namespace TooManyItems.Items.Tier2
{
    internal class BrassKnuckles
    {
        public static ItemDef itemDef;

        // Heavy hits deal more damage and stun enemies.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Brass Knuckles",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_BRASSKNUCKLES_DESC"]
        );
        public static ConfigurableValue<float> heavyHitCap = new(
            "Item: Brass Knuckles",
            "Heavy Hit Threshold",
            400f,
            "Minimum amount of damage dealt necessary to classify a hit as heavy.",
            ["ITEM_BRASSKNUCKLES_DESC"]
        );
        public static float percentHeavyHitCap = heavyHitCap.Value / 100f;

        public static ConfigurableValue<float> heavyHitBonus = new(
            "Item: Brass Knuckles",
            "Heavy Hit Bonus",
            25f,
            "Bonus percent damage dealt by heavy hits for the first stack of this item.",
            ["ITEM_BRASSKNUCKLES_DESC"]
        );
        public static ConfigurableValue<float> heavyHitBonusExtraStacks = new(
            "Item: Brass Knuckles",
            "Heavy Hit Bonus Extra Stacks",
            25f,
            "Bonus percent damage dealt by heavy hits for each extra stack of this item.",
            ["ITEM_BRASSKNUCKLES_DESC"]
        );
        public static float percentHeavyHitBonus = heavyHitBonus.Value / 100f;
        public static float percentHeavyHitBonusExtraStacks = heavyHitBonusExtraStacks.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("BrassKnuckles", [ItemTag.Damage, ItemTag.Utility, ItemTag.CanBeTemporary], ItemTier.Tier2);

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
                    int itemCount = attackerBody.inventory.GetItemCountEffective(itemDef);
                    if (itemCount > 0 && attackerBody.damage * percentHeavyHitCap <= damageInfo.damage)
                    {
                        damageInfo.damage *= 1 + Utilities.GetLinearStacking(percentHeavyHitBonus, percentHeavyHitBonusExtraStacks, itemCount);
                        damageInfo.damageType |= DamageType.Stun1s;
                        damageInfo.damageColorIndex = DamageColorIndex.Luminous;

                        EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade"), damageInfo.position, -damageInfo.force, transmit: true);
                    }
                }
            };
        }
    }
}
