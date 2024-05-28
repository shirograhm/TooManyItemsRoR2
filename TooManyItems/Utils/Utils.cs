using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    public static class Utils
    {
        public static Color BROKEN_MASK_COLOR        = new(0.38f, 0.38f, 0.82f, 1f);
        public static Color CARVING_BLADE_COLOR      = new(0.09f, 0.67f, 0.62f, 1f);
        public static Color PERMAFROST_COLOR         = new(0.76f, 0.80f, 0.98f, 1f);
        public static Color IRON_HEART_COLOR         = new(0.85f, 0.20f, 0.22f, 1f);
        public static Color TATTERED_SCROLL_COLOR    = new(0.80f, 0.78f, 0.57f, 1f);

        internal static void Init()
        {
            NetworkingAPI.RegisterMessageType<SyncForceRecalculate>();
        }

        public static void SetItemTier(ItemDef itemDef, ItemTier tier)
        {
            try
            {
#pragma warning disable Publicizer001
                itemDef._itemTierDef.tier = tier;
#pragma warning restore Publicizer001
            }
            catch
            {
                Log.Debug("Unable to set deprecatedTier.");
            }

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = tier;
            });
        }

        public static float getDifficultyAsPercentage()
        {
            return (Stage.instance.entryDifficultyCoefficient - 1f) / 98f;
        }

        public static float GetChanceAfterLuckDiscrete(float percent, float luck)
        {
            Mathf.CeilToInt(luck);

            if (luck > 0)
                return 1f - Mathf.Pow(1f - percent, luck + 1);
            if (luck < 0)
                return Mathf.Pow(percent, Mathf.Abs(luck) + 1);

            return percent;
        }

        public static float GetExponentialStacking(float percent, int count)
        {
            return 1f - Mathf.Pow(1f - percent, count);
        }

        public static float GetHyperbolicStacking(float percent, int count)
        {
            return 1f - 1f / (1f + percent * count);
        }

        public static void ForceRecalculate(CharacterBody body)
        {
            body.RecalculateStats();
            if (NetworkServer.active) new SyncForceRecalculate(body.netId);
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
            }
        }
    }
}
