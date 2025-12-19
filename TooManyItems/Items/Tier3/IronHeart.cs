using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class IronHeart
    {
        public static ItemDef itemDef;

        public static DamageAPI.ModdedDamageType damageType;
        public static DamageColorIndex damageColor = DamageColorAPI.RegisterDamageColor(Utils.IRON_HEART_COLOR);

        // Gain HP. Deal bonus damage on-hit based on your max health.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Iron Heart",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_IRONHEART_DESC"
            }
        );
        public static ConfigurableValue<float> healthIncrease = new(
            "Item: Iron Heart",
            "Health Increase",
            200f,
            "Bonus health gained from this item. Does not increase with stacks.",
            new List<string>()
            {
                "ITEM_IRONHEART_DESC"
            }
        );
        public static ConfigurableValue<float> percentDamagePerStack = new(
            "Item: Iron Heart",
            "On-Hit Damage Scaling",
            1.5f,
            "Percent of maximum health dealt as bonus on-hit damage.",
            new List<string>()
            {
                "ITEM_IRONHEART_DESC"
            }
        );
        public static float multiplierPerStack = percentDamagePerStack.Value / 100.0f;

        public class Statistics : MonoBehaviour
        {
            private float _totalDamageDealt;
            public float TotalDamageDealt
            {
                get { return _totalDamageDealt; }
                set
                {
                    _totalDamageDealt = value;
                    if (NetworkServer.active)
                    {
                        new Sync(gameObject.GetComponent<NetworkIdentity>().netId, value).Send(NetworkDestination.Clients);
                    }
                }
            }

            public class Sync : INetMessage
            {
                NetworkInstanceId objId;
                float totalDamageDealt;

                public Sync()
                {
                }

                public Sync(NetworkInstanceId objId, float totalDamage)
                {
                    this.objId = objId;
                    totalDamageDealt = totalDamage;
                }

                public void Deserialize(NetworkReader reader)
                {
                    objId = reader.ReadNetworkId();
                    totalDamageDealt = reader.ReadSingle();
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
                            component.TotalDamageDealt = totalDamageDealt;
                        }
                    }
                }

                public void Serialize(NetworkWriter writer)
                {
                    writer.Write(objId);
                    writer.Write(totalDamageDealt);

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

            damageType = DamageAPI.ReserveDamageType();

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "IRONHEART";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier3);

            GameObject prefab = AssetHandler.bundle.LoadAsset<GameObject>("IronHeart.prefab");
            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("IronHeart.png");
            itemDef.pickupModelPrefab = prefab;
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,
                ItemTag.Healing,

                ItemTag.CanBeTemporary
            };
        }

        public static float CalculateDamageOnHit(CharacterBody sender, float itemCount)
        {
            return sender.healthComponent.fullHealth * itemCount * multiplierPerStack;
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
                    if (sender.inventory.GetItemCountEffective(itemDef) > 0)
                    {
                        args.baseHealthAdd += healthIncrease.Value;
                    }
                }
            };

            GenericGameEvents.OnHitEnemy += (damageInfo, attackerInfo, victimInfo) =>
            {
                CharacterBody attackerBody = attackerInfo.body;
                if (attackerBody && attackerInfo.inventory && attackerBody.healthComponent)
                {
                    int count = attackerInfo.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        float amount = CalculateDamageOnHit(attackerBody, count);

                        DamageInfo proc = new()
                        {
                            damage = amount,
                            attacker = attackerInfo.gameObject,
                            inflictor = attackerInfo.gameObject,
                            procCoefficient = 1f,
                            position = damageInfo.position,
                            crit = attackerBody.RollCrit(),
                            damageColorIndex = damageColor,
                            procChainMask = damageInfo.procChainMask,
                            damageType = DamageType.Silent
                        };
                        proc.AddModdedDamageType(damageType);

                        victimInfo.healthComponent.TakeDamage(proc);

                        // Damage calculation takes minions into account
                        CharacterBody trackerBody = Utils.GetMinionOwnershipParentBody(attackerBody);
                        Statistics stats = trackerBody.inventory.GetComponent<Statistics>();
                        stats.TotalDamageDealt += amount;
                    }
                }
            };
        }
    }
}
