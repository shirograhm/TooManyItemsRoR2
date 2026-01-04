using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems.Items.Tier3
{
    internal class IronHeart
    {
        public static ItemDef itemDef;

        public static DamageAPI.ModdedDamageType damageType;
        public static DamageColorIndex damageColor = DamageColorManager.RegisterDamageColor(Utilities.IRON_HEART_COLOR);

        // Gain HP. Deal bonus damage on-hit based on your max health.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Iron Heart",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_IRONHEART_DESC"]
        );
        public static ConfigurableValue<float> healthIncrease = new(
            "Item: Iron Heart",
            "Health Increase",
            200f,
            "Bonus health gained from this item. Does not increase with stacks.",
            ["ITEM_IRONHEART_DESC"]
        );
        public static ConfigurableValue<float> percentDamagePerStack = new(
            "Item: Iron Heart",
            "On-Hit Damage Scaling",
            1.5f,
            "Percent of maximum health dealt as bonus on-hit damage.",
            ["ITEM_IRONHEART_DESC"]
        );
        public static ConfigurableValue<float> percentDamagePerExtraStack = new(
            "Item: Iron Heart",
            "On-Hit Damage Scaling Extra Stacks",
            1.5f,
            "Percent of maximum health dealt as bonus on-hit damage for extra stacks.",
            ["ITEM_IRONHEART_DESC"]
        );
        public static float multiplierPerStack = percentDamagePerStack.Value / 100.0f;
        public static float multiplierPerExtraStack = percentDamagePerExtraStack.Value / 100.0f;

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
            itemDef = ItemManager.GenerateItem("IronHeart", [ItemTag.Damage, ItemTag.Healing, ItemTag.CanBeTemporary], ItemTier.Tier3);

            NetworkingAPI.RegisterMessageType<Statistics.Sync>();

            damageType = DamageAPI.ReserveDamageType();

            Hooks();
        }

        public static float CalculateDamageOnHit(CharacterBody sender, int itemCount)
        {
            return sender.healthComponent.fullHealth * Utilities.GetLinearStacking(multiplierPerStack, multiplierPerExtraStack, itemCount);
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

            GameEventManager.OnHitEnemy += (damageInfo, attackerInfo, victimInfo) =>
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
                        CharacterBody trackerBody = Utilities.GetMinionOwnershipParentBody(attackerBody);
                        Statistics stats = trackerBody.inventory.GetComponent<Statistics>();
                        stats.TotalDamageDealt += amount *= proc.crit ? attackerBody.critMultiplier : 1f;
                    }
                }
            };
        }
    }
}
