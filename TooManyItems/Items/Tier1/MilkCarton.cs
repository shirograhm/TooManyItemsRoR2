using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Orbs;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems.Items.Tier1
{
    internal class MilkCarton
    {
        public static ItemDef itemDef;

        public static DamageColorIndex damageColor = DamageColorManager.RegisterDamageColor(Utilities.MILK_CARTON_DAMAGE_COLOR);

        // Reflect some of the damage taken from elite enemies.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Milk Carton",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_MILKCARTON_DESC"]
        );
        public static ConfigurableValue<float> eliteDamageReflection = new(
            "Item: Milk Carton",
            "Damage Retaliation",
            50f,
            "Percent damage retaliated back to elite enemies.",
            ["ITEM_MILKCARTON_DESC"]
        );
        public static ConfigurableValue<float> eliteDamageReflectionExtraStacks = new(
            "Item: Milk Carton",
            "Damage Retaliation Extra Stacks",
            50f,
            "Percent damage retaliated back to elite enemies with extra stacks.",
            ["ITEM_MILKCARTON_DESC"]
        );
        public static ConfigurableValue<float> reflectProcCoefficient = new(
            "Item: Milk Carton",
            "Proc Coefficient",
            0.5f,
            "Proc coefficient for the damage retaliation.",
            ["ITEM_MILKCARTON_DESC"]
        );
        public static float percentEliteDamageReflection = eliteDamageReflection.Value / 100f;
        public static float percentEliteDamageReflectionExtraStacks = eliteDamageReflectionExtraStacks.Value / 100f;

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
                        component?.TotalDamageDealt = totalDamageDealt;
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
            itemDef = ItemManager.GenerateItem("MilkCarton", [ItemTag.Damage, ItemTag.Utility, ItemTag.CanBeTemporary], ItemTier.Tier1);

            NetworkingAPI.RegisterMessageType<Statistics.Sync>();

            Hooks();
        }

        public static void Hooks()
        {
            CharacterMaster.onStartGlobal += (obj) =>
            {
                obj.inventory?.gameObject.AddComponent<Statistics>();
            };

            GameEventManager.OnTakeDamage += (damageReport) =>
            {
                CharacterBody vicBody = damageReport.victimBody;
                CharacterBody atkBody = damageReport.attackerBody;
                if (vicBody && atkBody && vicBody.inventory)
                {
                    int count = vicBody.inventory.GetItemCountEffective(itemDef);
                    if (atkBody && atkBody.isElite && count > 0 && damageReport.damageInfo.damageColorIndex != DamageColorIndex.DelayedDamage)
                    {
                        OrbManager.instance.AddOrb(new MilkCartonOrb(damageReport));
                    }
                }
            };
        }

        public class MilkCartonOrb : Orb
        {
            private readonly float speed = 60f;
            private readonly DamageReport damageReport;

            public MilkCartonOrb(DamageReport report)
            {
                damageReport = report;

                if (report.victimBody && report.attackerBody)
                {
                    origin = report.victimBody ? report.victimBody.corePosition : Vector3.zero;
                    if (report.attackerBody) target = report.attackerBody.mainHurtBox;
                }
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
                EffectManager.SpawnEffect(OrbStorageUtility.Get("Prefabs/Effects/OrbEffects/ClayGooOrbEffect"), effectData, transmit: true);
            }

            public override void OnArrival()
            {
                if (damageReport.victimBody && damageReport.victimBody.inventory)
                {
                    int count = damageReport.victimBody.inventory.GetItemCountEffective(MilkCarton.itemDef);

                    float amount = damageReport.damageInfo.damage * Utilities.GetLinearStacking(percentEliteDamageReflection, percentEliteDamageReflectionExtraStacks, count);
                    DamageInfo proc = new()
                    {
                        damage = amount,
                        attacker = damageReport.victimBody.gameObject,
                        inflictor = damageReport.attackerBody.gameObject,
                        procCoefficient = reflectProcCoefficient.Value,
                        position = damageReport.attackerBody.corePosition,
                        crit = damageReport.victimBody.RollCrit(),
                        damageColorIndex = MilkCarton.damageColor,
                        procChainMask = new ProcChainMask(),
                        damageType = DamageType.BypassBlock
                    };
                    damageReport.attackerBody.healthComponent.TakeDamage(proc);

                    // Damage calculation takes minions into account
                    CharacterBody trackerBody = Utilities.GetMinionOwnershipParentBody(damageReport.victimBody);
                    Statistics stats = trackerBody.inventory.GetComponent<Statistics>();
                    if (stats) stats.TotalDamageDealt += amount;
                }
            }
        }
    }
}
