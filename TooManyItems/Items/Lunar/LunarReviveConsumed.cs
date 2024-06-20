using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class LunarReviveConsumed
    {
        public static ItemDef itemDef;

        // This item is given after a Lunar Revive. Randomly deletes 2 (+2 per stack) items upon entering a new stage, prioritizing lower tiered items. If you don't have items to lose, cleanse all stacks of this item and die instead.
        public static ConfigurableValue<int> itemsLostPerStage = new(
            "Item: Sages Curse",
            "Items Lost",
            2,
            "Items lost per stack of this item upon entering a new stage.",
            new List<string>()
            {
                "ITEM_LUNARREVIVECONSUMED_DESC"
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

            itemDef.name = "LUNARREVIVECONSUMED";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.NoTier);

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("LunarReviveConsumed.png");
            itemDef.canRemove = false;
            itemDef.hidden = false;
        }

        private static async Task RunLunarReviveCurseAsync(CharacterMaster master, int itemCount)
        {
            await Task.Delay(1500);

            int itemsToLose = itemsLostPerStage * itemCount;
            while (itemsToLose > 0)
            {
                ItemTier? tierToLose = Utils.GetLowestAvailableItemTier(master.inventory);
                if (tierToLose != null)
                {
                    ItemDef defToLose = Utils.GetRandomItemOfTier((ItemTier)tierToLose);
                    if (master.inventory.GetItemCount(defToLose) > 0)
                    {
                        await Task.Delay(500);
                        ScrapperController.CreateItemTakenOrb(master.GetBody().corePosition, master.GetBody().gameObject, defToLose.itemIndex);
                        master.inventory.RemoveItem(defToLose);

                        CharacterMasterNotificationQueue.SendTransformNotification(
                            master, defToLose.itemIndex, itemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);

                        itemsToLose -= 1;
                    }
                }
                else
                {
                    break;
                }
            }

            if (itemsToLose > 0)
            {
                AkSoundEngine.PostEvent(AssetHandler.LUNAR_REVIVE_TICKING_ID, master.GetBody().gameObject);
                await Task.Delay(2000);
                master.GetBody().healthComponent.Suicide();
            }
        }

        public static void Hooks()
        {
            Stage.onStageStartGlobal += (stage) =>
            {
                foreach (NetworkUser user in NetworkUser.readOnlyInstancesList)
                {
                    CharacterMaster master = user.masterController.master ?? user.master;
                    if (master && master.inventory)
                    {
                        int itemCount = master.inventory.GetItemCount(itemDef);
                        if (itemCount > 0)
                        {
                            // Run this method async for all players with the revive item.
                            RunLunarReviveCurseAsync(master, itemCount);
                        }
                    }
                }
            };
        }
    }
}
