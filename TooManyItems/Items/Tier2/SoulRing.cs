using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems.Items.Tier2
{
    internal class SoulRing
    {
        public static ItemDef itemDef;

        // Gain permanent health regeneration on-kill.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Soul Ring",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_SOULRING_DESC"
            }
        );
        public static ConfigurableValue<float> healthRegenOnKill = new(
            "Item: Soul Ring",
            "Regen On Kill",
            0.1f,
            "Amount of permanent health regeneration gained on kill.",
            new List<string>()
            {
                "ITEM_SOULRING_DESC"
            }
        );
        public static ConfigurableValue<float> maxRegenOnFirstStack = new(
            "Item: Soul Ring",
            "Maximum Regen On First Stack",
            5f,
            "Maximum amount of permanent health regeneration granted on first stack.",
            new List<string>()
            {
                "ITEM_SOULRING_DESC"
            }
        );
        public static ConfigurableValue<float> maxRegenForExtraStacks = new(
            "Item: Soul Ring",
            "Maximum Regen On Extra Stacks",
            3f,
            "Maximum amount of permanent health regeneration granted on additional stacks.",
            new List<string>()
            {
                "ITEM_SOULRING_DESC"
            }
        );

        public class Statistics : MonoBehaviour
        {
            private float _healthRegen;
            public float HealthRegen
            {
                get { return _healthRegen; }
                set
                {
                    _healthRegen = value;
                    if (NetworkServer.active)
                    {
                        new Sync(gameObject.GetComponent<NetworkIdentity>().netId, value).Send(NetworkDestination.Clients);
                    }
                }
            }

            public class Sync : INetMessage
            {
                NetworkInstanceId objId;
                float healthRegen;

                public Sync()
                {
                }

                public Sync(NetworkInstanceId objId, float regen)
                {
                    this.objId = objId;
                    healthRegen = regen;
                }

                public void Deserialize(NetworkReader reader)
                {
                    objId = reader.ReadNetworkId();
                    healthRegen = reader.ReadSingle();
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
                            component.HealthRegen = healthRegen;
                        }
                    }
                }

                public void Serialize(NetworkWriter writer)
                {
                    writer.Write(objId);
                    writer.Write(healthRegen);

                    writer.FinishMessage();
                }
            }
        }

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("SoulRing", [ItemTag.AIBlacklist, ItemTag.Healing, ItemTag.OnKillEffect, ItemTag.CanBeTemporary], ItemTier.Tier2);

            NetworkingAPI.RegisterMessageType<Statistics.Sync>();

            Hooks();
        }

        public static float CalculateMaxRegenCap(int count)
        {
            return maxRegenOnFirstStack.Value + maxRegenForExtraStacks.Value * (count - 1);
        }

        public static void Hooks()
        {
            CharacterMaster.onStartGlobal += (obj) =>
            {
                obj.inventory?.gameObject.AddComponent<Statistics>();
            };

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        Statistics component = sender.inventory.GetComponent<Statistics>();

                        args.baseRegenAdd += component.HealthRegen;
                    }
                }
            };

            GlobalEventManager.onCharacterDeathGlobal += (damageReport) =>
            {
                if (!NetworkServer.active) return;

                CharacterBody atkBody = damageReport.attackerBody;
                if (atkBody && atkBody.inventory)
                {
                    int count = atkBody.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        Statistics component = atkBody.inventory.GetComponent<Statistics>();
                        float maxRegenAllowed = CalculateMaxRegenCap(count);

                        if (component.HealthRegen + healthRegenOnKill.Value <= maxRegenAllowed)
                        {
                            component.HealthRegen += healthRegenOnKill.Value;
                        }
                        else
                        {
                            component.HealthRegen = maxRegenAllowed;
                        }
                    }
                }
            };
        }
    }
}
