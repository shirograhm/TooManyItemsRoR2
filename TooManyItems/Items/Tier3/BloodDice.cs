using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Orbs;
using System;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems.Items.Tier3
{
    internal class BloodDice
    {
        public static ItemDef itemDef;

        // Gain permanent health on-kill.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Blood Dice",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_BLOODDICE_DESC"]
        );
        public static ConfigurableValue<bool> affectedByLuck = new(
            "Item: Blood Dice",
            "Affected By Luck",
            true,
            "Whether or not the likelihood of a high roll is affected by luck.",
            ["ITEM_BLOODDICE_DESC"]
        );
        public static ConfigurableValue<int> minHealthGain = new(
            "Item: Blood Dice",
            "Min Gain",
            2,
            "Minimum health that can be gained every kill.",
            ["ITEM_BLOODDICE_DESC"]
        );
        public static ConfigurableValue<int> maxHealthGain = new(
            "Item: Blood Dice",
            "Max Gain",
            12,
            "Maximum health that can be gained every kill.",
            ["ITEM_BLOODDICE_DESC"]
        );
        public static ConfigurableValue<float> maxHealthPerStack = new(
            "Item: Blood Dice",
            "Maximum Health Per Item",
            550f,
            "Maximum amount of permanent health allowed per stack.",
            ["ITEM_BLOODDICE_DESC"]
        );

        public class Statistics : MonoBehaviour
        {
            private float _permanentHealth;
            public float PermanentHealth
            {
                get { return _permanentHealth; }
                set
                {
                    _permanentHealth = value;
                    if (NetworkServer.active)
                    {
                        new Sync(gameObject.GetComponent<NetworkIdentity>().netId, value).Send(NetworkDestination.Clients);
                    }
                }
            }

            public class Sync : INetMessage
            {
                NetworkInstanceId objId;
                float permanentHealth;

                public Sync()
                {
                }

                public Sync(NetworkInstanceId objId, float health)
                {
                    this.objId = objId;
                    permanentHealth = health;
                }

                public void Deserialize(NetworkReader reader)
                {
                    objId = reader.ReadNetworkId();
                    permanentHealth = reader.ReadSingle();
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
                            component.PermanentHealth = permanentHealth;
                        }
                    }
                }

                public void Serialize(NetworkWriter writer)
                {
                    writer.Write(objId);
                    writer.Write(permanentHealth);

                    writer.FinishMessage();
                }
            }
        }

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("BloodDice", [ItemTag.AIBlacklist, ItemTag.Utility, ItemTag.OnKillEffect, ItemTag.CanBeTemporary], ItemTier.Tier3);

            NetworkingAPI.RegisterMessageType<Statistics.Sync>();

            Hooks();
        }

        public static void Hooks()
        {
            CharacterMaster.onStartGlobal += (obj) =>
            {
                obj.inventory?.gameObject.AddComponent<Statistics>();
            };

            On.RoR2.HealthComponent.GetHealthBarValues += (orig, self) =>
            {
                HealthComponent.HealthBarValues values = orig(self);
                if (self.body && self.body.inventory)
                {
                    if (self.body.inventory.GetItemCountEffective(itemDef) > 0)
                    {
                        values.hasInfusion = true;
                    }
                }
                return values;
            };

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        Statistics component = sender.inventory.GetComponent<Statistics>();
                        // Take Math.min incase item was dropped or removed from inventory
                        component.PermanentHealth = Mathf.Min(component.PermanentHealth, maxHealthPerStack.Value * count);
                        args.baseHealthAdd += component.PermanentHealth;
                    }
                }
            };

            GlobalEventManager.onCharacterDeathGlobal += (damageReport) =>
            {
                if (!NetworkServer.active) return;

                CharacterMaster atkMaster = damageReport.attackerMaster;
                CharacterBody atkBody = damageReport.attackerBody;
                if (atkMaster && atkBody && atkBody.inventory)
                {
                    int count = atkBody.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        Statistics component = atkBody.inventory.GetComponent<Statistics>();

                        float maxHealthAllowed = Utilities.GetLinearStacking(maxHealthPerStack.Value, count);
                        // Use math.min for health cap
                        int healthToGain = Math.Min(GetDiceRoll(atkMaster), Mathf.RoundToInt(maxHealthAllowed - component.PermanentHealth));
                        // Only send orbs if item is not fully stacked
                        if (healthToGain > 0)
                        {
                            for (int i = 0; i < healthToGain; i++)
                            {
                                OrbManager.instance.AddOrb(new BloodDiceOrb(damageReport, i));
                            }
                        }
                    }
                }
            };
        }

        public static int GetDiceRoll(CharacterMaster master)
        {
            float mean = 7f, deviation = 4f;
            if (affectedByLuck.Value) mean += master.luck * deviation;

            return Mathf.Clamp(GenerateRollNormalDistribution(mean, deviation), minHealthGain.Value, maxHealthGain.Value);
        }

        private static int GenerateRollNormalDistribution(float mean, float deviation)
        {
            // Box-Mueller transform for normal distribution
            float x = (float)(1.0 - TooManyItems.RandGen.NextDouble());
            float y = (float)(1.0 - TooManyItems.RandGen.NextDouble());

            float randomSample = Mathf.Sqrt(-2f * Mathf.Log(x)) * Mathf.Sin(2f * Mathf.PI * y);
            float scaledSample = mean + deviation * randomSample;
            return Mathf.RoundToInt(scaledSample);
        }
    }

    public class BloodDiceOrb : Orb
    {
        private readonly float speed = 25f;

        private readonly CharacterBody targetBody;
        private Inventory targetInventory;

        public BloodDiceOrb(DamageReport report, int sequence)
        {
            if (report.victimBody && report.attackerBody)
            {
                this.targetBody = report.attackerBody ?? null;
                this.origin = report.victimBody ? report.victimBody.corePosition : Vector3.zero;

                if (targetBody) this.target = targetBody.mainHurtBox;
            }
            this.speed += sequence;
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
            EffectManager.SpawnEffect(OrbStorageUtility.Get("Prefabs/Effects/OrbEffects/InfusionOrbEffect"), effectData, transmit: true);

            targetInventory = targetBody.inventory;
        }

        public override void OnArrival()
        {
            if (targetInventory)
            {
                int count = targetInventory.GetItemCountEffective(BloodDice.itemDef);

                float maxHealthAllowed = Utilities.GetLinearStacking(BloodDice.maxHealthPerStack.Value, count);
                BloodDice.Statistics component = targetInventory.GetComponent<BloodDice.Statistics>();

                if (component)
                {
                    // Each orb grants 1 HP
                    component.PermanentHealth += 1;
                    if (component.PermanentHealth > maxHealthAllowed) component.PermanentHealth = maxHealthAllowed;
                }

                if (targetBody) Utilities.ForceRecalculate(targetBody);
            }
        }
    }
}
