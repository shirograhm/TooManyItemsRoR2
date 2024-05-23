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

        public static float GetChanceAfterLuck(float percent, int luck)
        {
            return 1f - Mathf.Pow(1f - percent, luck + 1);
        }

        public static float GetExponentialStacking(float percent, int count)
        {
            return Mathf.Pow(percent, count);
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
