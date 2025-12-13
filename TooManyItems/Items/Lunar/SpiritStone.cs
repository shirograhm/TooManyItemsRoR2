using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class SpiritStone
    {
        public static ItemDef itemDef;
        // Killing an enemy grants permanent shield. Lose a percentage of your max health.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Spirit Stone",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_SPIRITSTONE_DESC"
            }
        );
        public static ConfigurableValue<float> shieldPerKill = new(
            "Item: Spirit Stone",
            "Shield Amount",
            1f,
            "Health given as shield for every kill.",
            new List<string>()
            {
                "ITEM_SPIRITSTONE_DESC"
            }
        );
        public static ConfigurableValue<float> maxHealthLost = new(
            "Item: Spirit Stone",
            "Max Health Reduction",
            25f,
            "Max health lost as a penalty for holding the first stack of this item.",
            new List<string>()
            {
                "ITEM_SPIRITSTONE_DESC"
            }
        );
        public static ConfigurableValue<float> maxHealthLostExtraStack = new(
            "Item: Spirit Stone",
            "Max Health Reduction Extra Stacks",
            15f,
            "Max health lost as a penalty for holding extra stacks of this item.",
            new List<string>()
            {
                "ITEM_SPIRITSTONE_DESC"
            }
        );
        public static float maxHealthLostPercent = maxHealthLost.Value / 100f;
        public static float maxHealthLostExtraStackPercent = maxHealthLostExtraStack.Value / 100f;

        public class Statistics : MonoBehaviour
        {
            private float _permanentShield;
            public float PermanentShield
            {
                get { return _permanentShield; }
                set
                {
                    _permanentShield = value;
                    if (NetworkServer.active)
                    {
                        new Sync(gameObject.GetComponent<NetworkIdentity>().netId, value).Send(NetworkDestination.Clients);
                    }
                }
            }

            public class Sync : INetMessage
            {
                NetworkInstanceId objId;
                float permanentShield;

                public Sync()
                {
                }

                public Sync(NetworkInstanceId objId, float shield)
                {
                    this.objId = objId;
                    permanentShield = shield;
                }

                public void Deserialize(NetworkReader reader)
                {
                    objId = reader.ReadNetworkId();
                    permanentShield = reader.ReadSingle();
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
                            component.PermanentShield = permanentShield;
                        }
                    }
                }

                public void Serialize(NetworkWriter writer)
                {
                    writer.Write(objId);
                    writer.Write(permanentShield);

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

            itemDef.name = "SPIRITSTONE";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Lunar);

            GameObject prefab = AssetHandler.bundle.LoadAsset<GameObject>("SpiritStone.prefab");
            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("SpiritStone.png");
            itemDef.pickupModelPrefab = prefab;
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.OnKillEffect
            };
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
                    int itemCount = sender.inventory.GetItemCountPermanent(itemDef);
                    if (itemCount > 0)
                    {
                        Statistics component = sender.inventory.GetComponent<Statistics>();
                        args.baseShieldAdd += component.PermanentShield;

                        args.healthMultAdd -= Utils.GetExponentialStacking(maxHealthLostPercent, itemCount);
                    }
                }
            };

            On.RoR2.HealthComponent.GetHealthBarValues += (orig, self) =>
            {
                HealthComponent.HealthBarValues values = orig(self);
                if (self.body && self.body.inventory)
                {
                    int count = self.body.inventory.GetItemCountPermanent(itemDef);
                    if (count > 0)
                    {
                        values.curseFraction += (1f - values.curseFraction) * Utils.GetExponentialStacking(maxHealthLostPercent, maxHealthLostExtraStackPercent, count);
                        values.healthFraction = self.health * (1f - values.curseFraction) / self.fullCombinedHealth;
                        values.shieldFraction = self.shield * (1f - values.curseFraction) / self.fullCombinedHealth;
                    }
                }
                return values;
            };

            GlobalEventManager.onCharacterDeathGlobal += (damageReport) =>
            {
                if (!NetworkServer.active) return;

                CharacterBody atkBody = damageReport.attackerBody;
                if (atkBody && atkBody.inventory)
                {
                    int count = atkBody.inventory.GetItemCountPermanent(itemDef);
                    if (count > 0)
                    {
                        Statistics component = atkBody.inventory.GetComponent<Statistics>();
                        component.PermanentShield += shieldPerKill * count;

                        Utils.ForceRecalculate(atkBody);
                    }
                }
            };
        }
    }
}
