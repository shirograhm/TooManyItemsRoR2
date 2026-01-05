using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Orbs;
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
            ["ITEM_SOULRING_DESC"]
        );
        public static ConfigurableValue<float> healthRegenOnKill = new(
            "Item: Soul Ring",
            "Regen On Kill",
            0.1f,
            "Amount of permanent health regeneration gained on kill.",
            ["ITEM_SOULRING_DESC"]
        );
        public static ConfigurableValue<float> maxRegenOnFirstStack = new(
            "Item: Soul Ring",
            "Maximum Regen On First Stack",
            4f,
            "Maximum amount of permanent health regeneration granted on first stack.",
            ["ITEM_SOULRING_DESC"]
        );
        public static ConfigurableValue<float> maxRegenForExtraStacks = new(
            "Item: Soul Ring",
            "Maximum Regen On Extra Stacks",
            1f,
            "Maximum amount of permanent health regeneration granted on additional stacks.",
            ["ITEM_SOULRING_DESC"]
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
                        if (component)
                        {
                            float maxRegenAllowed = Utilities.GetLinearStacking(maxRegenOnFirstStack.Value, maxRegenForExtraStacks.Value, count);
                            float healthRegenToGain = Mathf.Min(healthRegenOnKill.Value, maxRegenAllowed - component.HealthRegen);
                            // Only send orb if item is not fully stacked
                            if (healthRegenToGain > 0)
                            {
                                OrbManager.instance.AddOrb(new SoulRingOrb(damageReport, healthRegenToGain));
                            }
                        }
                    }
                }
            };
        }

        public class SoulRingOrb : Orb
        {
            private const float speed = 30f;

            private readonly float healthRegenOnKilled;

            private readonly CharacterBody targetBody;
            private Inventory targetInventory;

            public SoulRingOrb(DamageReport report, float regenOnKill)
            {
                if (report.attackerMaster && report.victimBody && report.attackerBody)
                {
                    this.targetBody = report.attackerBody ?? null;
                    this.origin = report.victimBody ? report.victimBody.corePosition : Vector3.zero;

                    if (targetBody) this.target = targetBody.mainHurtBox;
                }

                this.healthRegenOnKilled = regenOnKill;
            }

            public override void Begin()
            {
                base.duration = base.distanceToTarget / speed;
                EffectData effectData = new()
                {
                    origin = origin,
                    genericFloat = base.duration
                };
                effectData.SetHurtBoxReference(target);
                EffectManager.SpawnEffect(OrbStorageUtility.Get("Prefabs/Effects/OrbEffects/HealthOrbEffect"), effectData, transmit: true);

                targetInventory = targetBody.inventory;
            }

            public override void OnArrival()
            {
                if (targetInventory)
                {
                    float maxRegenAllowed =
                        Utilities.GetLinearStacking(maxRegenOnFirstStack.Value, maxRegenForExtraStacks.Value, targetInventory.GetItemCountEffective(SoulRing.itemDef));

                    Statistics component = targetInventory.GetComponent<Statistics>();
                    if (component)
                    {
                        component.HealthRegen += healthRegenOnKilled;
                        if (component.HealthRegen > maxRegenAllowed) component.HealthRegen = maxRegenAllowed;
                    }

                    if (targetBody) Utilities.ForceRecalculate(targetBody);
                }
            }
        }
    }
}
