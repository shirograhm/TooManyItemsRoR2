using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class BreadLoaf
    {
        public static ItemDef itemDef;

        // While the teleporter is charging, killing enemies heals you for a portion of your missing health.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Loaf of Bread",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_BREADLOAF_DESC"
            }
        );
        public static ConfigurableValue<float> healthGainOnKill = new(
            "Item: Loaf of Bread",
            "Healing On Kill",
            3f,
            "Percent missing health gained after killing an enemy during the teleporter event.",
            new List<string>()
            {
                "ITEM_BREADLOAF_DESC"
            }
        );
        public static float healthGainOnKillPercent = healthGainOnKill.Value / 100f;

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

            itemDef.name = "BREADLOAF";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier1);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("BreadLoaf.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("BreadLoaf.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Healing,

                ItemTag.HoldoutZoneRelated,
                ItemTag.OnKillEffect
            };
        }

        public static void Hooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += (damageReport) =>
            {
                if (!NetworkServer.active) return;

                CharacterBody atkBody = damageReport.attackerBody;
                if (atkBody && atkBody.inventory)
                {
                    int itemCount = atkBody.inventory.GetItemCount(itemDef);
                    if (itemCount > 0)
                    {
                        foreach (HoldoutZoneController hzc in InstanceTracker.GetInstancesList<HoldoutZoneController>())
                        {
                            if (hzc.isActiveAndEnabled && hzc.IsBodyInChargingRadius(atkBody))
                            {
                                float healing = healthGainOnKillPercent * itemCount * atkBody.healthComponent.missingCombinedHealth;
                                atkBody.healthComponent.Heal(healing, new ProcChainMask());
                            }
                        }
                    }
                }
            };
        }
    }
}
