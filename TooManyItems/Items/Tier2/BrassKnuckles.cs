using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
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
            GenerateItem();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "BRASSKNUCKLES";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier2);

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("BrassKnuckles.png");
            itemDef.pickupModelPrefab = AssetHandler.bundle.LoadAsset<GameObject>("BrassKnuckles.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage
            };
        }

        public static void Hooks()
        {
            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                CharacterBody attackerBody = attackerInfo.body;
                CharacterBody victimBody = victimInfo.body;
                if (attackerBody && victimBody && attackerBody.inventory)
                {
                    int itemCount = attackerBody.inventory.GetItemCount(itemDef);
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
