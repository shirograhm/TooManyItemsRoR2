using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    public static class Utils
    {
        public static Color BROKEN_MASK_COLOR        = new(0.38f, 0.38f, 0.82f, 1f);
        public static Color CARVING_BLADE_COLOR      = new(0.09f, 0.67f, 0.62f, 1f);
        public static Color PERMAFROST_COLOR         = new(0.76f, 0.80f, 0.98f, 1f);
        public static Color IRON_HEART_COLOR         = new(0.44f, 0.44f, 0.44f, 1f);
        public static Color TATTERED_SCROLL_COLOR    = new(0.80f, 0.78f, 0.57f, 1f);
        public static Color VANITY_COLOR             = new(0.53f, 0.44f, 0.77f, 1f);

        internal static void Init()
        {
            NetworkingAPI.RegisterMessageType<SyncForceRecalculate>();
        }

        private class SyncForceRecalculate : INetMessage
        {
            NetworkInstanceId netID;

            public SyncForceRecalculate() { }
            public SyncForceRecalculate(NetworkInstanceId ID)
            {
                this.netID = ID;
            }

            public void Deserialize(NetworkReader reader)
            {
                netID = reader.ReadNetworkId();
            }

            public void OnReceived()
            {
                if (NetworkServer.active) return;

                GameObject obj = RoR2.Util.FindNetworkObject(netID);
                if (obj)
                {
                    CharacterBody body = obj.GetComponent<CharacterBody>();
                    if (body) body.RecalculateStats();
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(netID);
                writer.FinishMessage();
            }
        }

        public static void ForceRecalculate(CharacterBody body)
        {
            body.RecalculateStats();
            if (NetworkServer.active) new SyncForceRecalculate(body.netId);
        }

        public static void SetItemTier(ItemDef itemDef, ItemTier tier)
        {
            if (tier == ItemTier.NoTier)
            {
                try
                {
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
#pragma warning disable CS0618 // Type or member is obsolete
                    itemDef.deprecatedTier = tier;
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore Publicizer001 // Accessing a member that was not originally public
                }
                catch (Exception e)
                {
                    Log.Warning(String.Format("Error setting deprecatedTier for {0}: {1}", itemDef.name, e));
                }
            }

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = tier;
            });
        }

        public static CharacterBody GetMinionOwnershipParentBody(CharacterBody body)
        {
            if (body && body.master && body.master.minionOwnership && body.master.minionOwnership.ownerMaster && body.master.minionOwnership.ownerMaster.GetBody())
            {
                return body.master.minionOwnership.ownerMaster.GetBody();
            }
            return body;
        }

        public static uint ScaleGoldWithDifficulty(int goldGranted)
        {
            return Convert.ToUInt32(goldGranted * (1 + 50 * GetDifficultyAsPercentage()));
        }

        public static float GetChanceAfterLuck(float percent, float luckIn)
        {
            int luck = Mathf.CeilToInt(luckIn);

            if (luck > 0)
                return 1f - Mathf.Pow(1f - percent, luck + 1);
            if (luck < 0)
                return Mathf.Pow(percent, Mathf.Abs(luck) + 1);

            return percent;
        }

        public static float GetDifficultyAsPercentage()
        {
            return (Stage.instance.entryDifficultyCoefficient - 1f) / 98f;
        }

        public static float GetExponentialStacking(float percent, int count)
        {
            return 1f - Mathf.Pow(1f - percent, count);
        }

        public static float GetHyperbolicStacking(float percent, int count)
        {
            return 1f - 1f / (1f + percent * count);
        }

        public static ItemTier? GetLowestAvailableItemTier(Inventory inventory)
        {
            if (inventory.GetTotalItemCountOfTier(ItemTier.Tier1) > 0)
            {
                return ItemTier.Tier1;
            }
            else if (inventory.GetTotalItemCountOfTier(ItemTier.VoidTier1) > 0)
            {
                return ItemTier.VoidTier1;
            }
            else if (inventory.GetTotalItemCountOfTier(ItemTier.Tier2) > 0)
            {
                return ItemTier.Tier2;
            }
            else if (inventory.GetTotalItemCountOfTier(ItemTier.VoidTier2) > 0)
            {
                return ItemTier.VoidTier2;
            }
            else if (inventory.GetTotalItemCountOfTier(ItemTier.Tier3) > 0)
            {
                return ItemTier.Tier3;
            }
            else if (inventory.GetTotalItemCountOfTier(ItemTier.VoidTier3) > 0)
            {
                return ItemTier.VoidTier3;
            }
            else if (inventory.GetTotalItemCountOfTier(ItemTier.Boss) > 0)
            {
                return ItemTier.Boss;
            }
            else if (inventory.GetTotalItemCountOfTier(ItemTier.VoidBoss) > 0)
            {
                return ItemTier.VoidBoss;
            }
            else if (inventory.GetTotalItemCountOfTier(ItemTier.Lunar) > 0)
            {
                return ItemTier.Lunar;
            }
            
            return null;
        }

        public static ItemDef GetRandomItemOfTier(ItemTier tier)
        {
            ItemDef[] tierItems = ItemCatalog.allItemDefs.Where(itemDef => itemDef.tier == tier).ToArray();

            return tierItems[TooManyItems.RandGen.Next(0, tierItems.Length)];
        }
    }
}
