using R2API;
using RoR2;
using System.Collections.Generic;

namespace TooManyItems
{
    internal class BrassKnuckles : BaseItem
    {
        // Heavy hits deal more damage and stun enemies.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Brass Knuckles",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_BRASSKNUCKLES_DESC"
            }
        );
        public static ConfigurableValue<float> heavyHitCap = new(
            "Item: Brass Knuckles",
            "Heavy Hit Threshold",
            400f,
            "Minimum amount of damage dealt necessary to classify a hit as heavy.",
            new List<string>()
            {
                "ITEM_BRASSKNUCKLES_DESC"
            }
        );
        public static float heavyHitCapPercent = heavyHitCap.Value / 100f;

        public static ConfigurableValue<float> heavyHitBonus = new(
            "Item: Brass Knuckles",
            "Heavy Hit Bonus",
            25f,
            "Bonus percent damage dealt by heavy hits for each stack of this item.",
            new List<string>()
            {
                "ITEM_BRASSKNUCKLES_DESC"
            }
        );
        public static float heavyHitBonusPercent = heavyHitBonus.Value / 100f;

        internal static void Init()
        {
            GenerateItem(
                "BRASSKNUCKLES",
                "BrassKnuckles.prefab",
                "BrassKnuckles.png",
                ItemTier.Tier2,
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
            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                CharacterBody attackerBody = attackerInfo.body;
                CharacterBody victimBody = victimInfo.body;
                if (attackerBody && victimBody && attackerBody.inventory)
                {
                    int itemCount = attackerBody.inventory.GetItemCountEffective(itemDef);
                    if (itemCount > 0 && (attackerBody.damage * heavyHitCapPercent) <= damageInfo.damage)
                    {
                        damageInfo.damage *= 1 + (heavyHitBonusPercent * itemCount);
                        damageInfo.damageType |= DamageType.Stun1s;
                    }
                }
            };
        }
    }
}
