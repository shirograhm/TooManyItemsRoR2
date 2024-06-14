using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
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
            new List<string>()
            {
                "ITEM_BLOODDICE_DESC"
            }
        );
        public static ConfigurableValue<bool> affectedByLuck = new(
            "Item: Blood Dice",
            "Affected By Luck",
            true,
            "Whether or not the likelihood of a high roll is affected by luck.",
            new List<string>()
            {
                "ITEM_BLOODDICE_DESC"
            }
        );
        public static ConfigurableValue<int> minHealthGain = new(
            "Item: Blood Dice",
            "Min Gain",
            2,
            "Minimum health that can be gained every kill.",
            new List<string>()
            {
                "ITEM_BLOODDICE_DESC"
            }
        );
        public static ConfigurableValue<int> maxHealthGain = new(
            "Item: Blood Dice",
            "Max Gain",
            12,
            "Maximum health that can be gained every kill.",
            new List<string>()
            {
                "ITEM_BLOODDICE_DESC"
            }
        );
        public static ConfigurableValue<float> maxHealthPerStack = new(
            "Item: Blood Dice",
            "Maximum Health Per Item",
            550f,
            "Maximum amount of permanent health allowed per stack.",
            new List<string>()
            {
                "ITEM_BLOODDICE_DESC"
            }
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

            itemDef.name = "BLOODDICE";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier3);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("BloodDice.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("BloodDice.prefab");
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

            On.RoR2.HealthComponent.GetHealthBarValues += (orig, self) =>
            {
                HealthComponent.HealthBarValues values = orig(self);
                if (self.body && self.body.inventory)
                {
                    if (self.body.inventory.GetItemCount(itemDef) > 0)
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
                    int count = sender.inventory.GetItemCount(itemDef);
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
                    int count = atkBody.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        float maxHealthAllowed = maxHealthPerStack.Value * count;
                        int roll = GetDiceRoll(atkMaster);

                        Statistics component = atkBody.inventory.GetComponent<Statistics>();

                        if (component.PermanentHealth + roll < maxHealthAllowed)
                        {
                            component.PermanentHealth += roll;
                        }
                        else
                        {
                            component.PermanentHealth = maxHealthAllowed;
                        }

                        Utils.ForceRecalculate(atkBody);
                    }
                }
            };
        }

        private static int GetDiceRoll(CharacterMaster master)
        {
            float mean = 7f, deviation = 2f;
            if (affectedByLuck.Value) mean += master.luck * deviation;
            
            return Mathf.Clamp(GenerateRollNormalDistribution(mean, deviation), minHealthGain.Value, maxHealthGain.Value);
        }

        private static int GenerateRollNormalDistribution(float mean, float deviation)
        {
            // Box-Mueller transform for normal distribution
            float x = (float) (1.0 - TooManyItems.rand.NextDouble());
            float y = (float) (1.0 - TooManyItems.rand.NextDouble());

            float randomSample = Mathf.Sqrt(-2f * Mathf.Log(x)) * Mathf.Sin(2f * Mathf.PI * y);
            float scaledSample = mean + deviation * randomSample;
            return Mathf.RoundToInt(scaledSample);
        }
    }
}
