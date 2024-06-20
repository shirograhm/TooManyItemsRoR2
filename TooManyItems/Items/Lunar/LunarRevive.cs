using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class LunarRevive
    {
        public static ItemDef itemDef;

        // Upon death, consume this blessing to revive with 3 seconds of invulnerability.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Sages Blessing",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_LUNARREVIVE_DESC"
            }
        );

        internal static void Init()
        {
            GenerateItem();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "LUNARREVIVE";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Lunar);

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("LunarRevive.png");
            itemDef.pickupModelPrefab = AssetHandler.bundle.LoadAsset<GameObject>("LunarRevive.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        public static void Hooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += (damageReport) =>
            {
                if (!NetworkServer.active) return; 
                
                CharacterMaster master = damageReport.victimMaster;
                if (master && master.inventory)
                {
                    int itemCount = master.inventory.GetItemCount(itemDef);
                    if (itemCount > 0 && master.GetBody())
                    {
                        master.inventory.RemoveItem(itemDef);
                        master.inventory.GiveItem(LunarReviveConsumed.itemDef);

                        CharacterMasterNotificationQueue.SendTransformNotification(
                            master, itemDef.itemIndex, LunarReviveConsumed.itemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);

                        master.Respawn(master.GetBody().footPosition, Quaternion.identity);
                    }
                }
            };
        }
    }
}
