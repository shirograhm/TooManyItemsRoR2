using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class BreadLoaf
    {
        public static ItemDef itemDef;

        // Gain gold on kill. After enough kills, gain bonus gold and convert a stack of this item to scrap.
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
        public static ConfigurableValue<int> goldGainOnKill = new(
            "Item: Loaf of Bread",
            "Gold On Kill",
            1,
            "Gold gained after killing an enemy, up to a set number of times (see next config).",
            new List<string>()
            {
                "ITEM_BREADLOAF_DESC"
            }
        );
        public static ConfigurableValue<int> killsNeededToScrap = new(
            "Item: Loaf of Bread",
            "Kills Needed",
            100,
            "How many kills are required per item stack to scrap the item and gain reward gold.",
            new List<string>()
            {
                "ITEM_BREADLOAF_DESC"
            }
        );
        public static ConfigurableValue<int> goldGainOnScrap = new(
            "Item: Loaf of Bread",
            "Gold On Scrap",
            25,
            "Gold gained after killing enough enemies (scales with difficulty). This item turns into scrap once this happens.",
            new List<string>()
            {
                "ITEM_BREADLOAF_DESC"
            }
        );

        public class Statistics : MonoBehaviour
        {
            private int _killsCounter;
            public int KillsCounter
            {
                get { return _killsCounter; }
                set
                {
                    _killsCounter = value;
                    if (NetworkServer.active)
                    {
                        new Sync(gameObject.GetComponent<NetworkIdentity>().netId, value).Send(NetworkDestination.Clients);
                    }
                }
            }

            public class Sync : INetMessage
            {
                NetworkInstanceId objId;
                int killsCounter;

                public Sync()
                {
                }

                public Sync(NetworkInstanceId objId, int kills)
                {
                    this.objId = objId;
                    killsCounter = kills;
                }

                public void Deserialize(NetworkReader reader)
                {
                    objId = reader.ReadNetworkId();
                    killsCounter = reader.ReadInt32();
                }

                public void OnReceived()
                {
                    if (NetworkServer.active) return;

                    GameObject obj = Util.FindNetworkObject(objId);
                    if (obj != null)
                    {
                        Statistics component = obj.GetComponent<Statistics>();
                        if (component != null)
                        {
                            component.KillsCounter = killsCounter;
                        }
                    }
                }

                public void Serialize(NetworkWriter writer)
                {
                    writer.Write(objId);
                    writer.Write(killsCounter);

                    writer.FinishMessage();
                }
            }
        }

        internal static void Init()
        {
            GenerateItem();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            NetworkingAPI.RegisterMessageType<Statistics.Sync>();

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "BREADLOAF";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier1);

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("BreadLoaf.png");
            itemDef.pickupModelPrefab = AssetHandler.bundle.LoadAsset<GameObject>("BreadLoaf.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility,

                ItemTag.OnKillEffect
            };
        }

        public static void Hooks()
        {
            CharacterMaster.onStartGlobal += (obj) =>
            {
                obj.inventory?.gameObject.AddComponent<Statistics>();
            };

            GlobalEventManager.onCharacterDeathGlobal += (damageReport) =>
            {
                if (!NetworkServer.active) return;

                CharacterBody atkBody = damageReport.attackerBody;
                if (atkBody && atkBody.inventory)
                {
                    int itemCount = atkBody.inventory.GetItemCount(itemDef);
                    if (itemCount > 0)
                    {
                        Statistics stats = atkBody.inventory.GetComponent<Statistics>();
                        if (stats) {
                            // Increase kill counter
                            stats.KillsCounter += 1;
                            // If kill limit is reached, scrap 1 stack and grant reward gold
                            if (stats.KillsCounter >= killsNeededToScrap.Value)
                            {
                                CharacterMasterNotificationQueue.PushItemTransformNotification(
                                    atkBody.master,
                                    itemDef.itemIndex,
                                    RoR2Content.Items.ScrapWhite.itemIndex,
                                    CharacterMasterNotificationQueue.TransformationType.Default
                                ); 
                                atkBody.inventory.RemoveItem(itemDef);
                                atkBody.inventory.GiveItem(RoR2Content.Items.ScrapWhite);
                                atkBody.master.GiveMoney(Utils.ScaleGoldWithDifficulty(goldGainOnScrap.Value));
                                // Reset the kills counter
                                stats.KillsCounter = 0;
                            }
                            else
                            {
                                // Gain gold on kill
                                atkBody.master.GiveMoney(Convert.ToUInt32(goldGainOnKill.Value * itemCount));
                            }
                        }
                    }
                }
            };
        }
    }
}
