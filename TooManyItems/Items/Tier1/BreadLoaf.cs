using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems.Items.Tier1
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
            ["ITEM_BREADLOAF_DESC"]
        );
        public static ConfigurableValue<int> goldGainOnKill = new(
            "Item: Loaf of Bread",
            "Gold On Kill",
            1,
            "Gold gained after killing an enemy, up to a set number of times (see next config).",
            ["ITEM_BREADLOAF_DESC"]
        );
        public static ConfigurableValue<int> killsNeededToScrap = new(
            "Item: Loaf of Bread",
            "Kills Needed",
            25,
            "How many kills are required per item stack to scrap the item and gain reward gold.",
            ["ITEM_BREADLOAF_DESC"]
        );
        public static ConfigurableValue<int> goldGainOnScrap = new(
            "Item: Loaf of Bread",
            "Gold On Scrap",
            25,
            "Gold gained after killing enough enemies (scales with difficulty). This item turns into scrap once this happens.",
            ["ITEM_BREADLOAF_DESC"]
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
            itemDef = ItemManager.GenerateItem("BreadLoaf", [ItemTag.AIBlacklist, ItemTag.Utility, ItemTag.OnKillEffect], ItemTier.Tier1);

            NetworkingAPI.RegisterMessageType<Statistics.Sync>();

            Hooks();
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
                    int itemCount = atkBody.inventory.GetItemCountPermanent(itemDef);
                    if (itemCount > 0)
                    {
                        Statistics stats = atkBody.inventory.GetComponent<Statistics>();
                        if (stats)
                        {
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
                                atkBody.inventory.RemoveItemPermanent(itemDef);
                                atkBody.inventory.GiveItemPermanent(RoR2Content.Items.ScrapWhite);

                                Utilities.SendGoldOrbAndEffect(Utilities.ScaleGoldWithDifficulty(goldGainOnScrap.Value), atkBody.corePosition, atkBody.mainHurtBox);

                                // Reset the kills counter
                                stats.KillsCounter = 0;
                            }
                            else
                            {
                                // Gain gold on kill
                                Utilities.SendGoldOrbAndEffect(Convert.ToUInt32(goldGainOnKill.Value * itemCount), damageReport.damageInfo.position, atkBody.mainHurtBox);
                            }
                        }
                    }
                }
            };
        }
    }
}
