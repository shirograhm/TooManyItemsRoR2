using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems.Items.Tier2
{
    internal class BrokenMask
    {
        public static ItemDef itemDef;
        public static BuffDef burnDebuff;
        private static DotController.DotDef burnDotDef;
        private static DotController.DotIndex burnIndex;

        public static DamageColorIndex maskDamageColor = DamageColorManager.RegisterDamageColor(Utilities.BROKEN_MASK_COLOR);

        // Dealing damage burns enemies for a portion of their max health.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Broken Mask",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_BROKENMASK_DESC"]
        );
        public static ConfigurableValue<float> burnDamage = new(
            "Item: Broken Mask",
            "Percent Burn",
            1.5f,
            "Burn damage dealt over the duration for the first stack as a percentage of enemy max health.",
            ["ITEM_BROKENMASK_DESC"]
        );
        public static ConfigurableValue<float> burnDamageExtraStacks = new(
            "Item: Broken Mask",
            "Percent Burn Extra Stacks",
            1.5f,
            "Burn damage dealt over the duration for extra stacks as a percentage of enemy max health.",
            ["ITEM_BROKENMASK_DESC"]
        );
        public static ConfigurableValue<int> burnDuration = new(
            "Item: Broken Mask",
            "Burn Duration",
            5,
            "Total duration of the burn in seconds.",
            ["ITEM_BROKENMASK_DESC"]
        );
        public static ConfigurableValue<float> burnTickInterval = new(
            "Item: Broken Mask",
            "Tick Interval",
            0.5f,
            "Keep this a clean divisor of Burn Duration to prevent unpredictable behaviour... or go wild with it, honestly, up to you ¯\\_(ツ)_/¯",
            ["ITEM_BROKENMASK_DESC"]
        );
        public static float percentBurnDamage = burnDamage.Value / 100f;
        public static float percentBurnDamageExtraStacks = burnDamageExtraStacks.Value / 100f;

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
            itemDef = ItemManager.GenerateItem("BrokenMask", [ItemTag.Damage, ItemTag.CanBeTemporary], ItemTier.Tier2);

            burnDebuff = ItemManager.GenerateBuff("Torment", AssetManager.bundle.LoadAsset<Sprite>("MaskDebuff.png"), isDebuff: true);
            ContentAddition.AddBuffDef(burnDebuff);

            NetworkingAPI.RegisterMessageType<Statistics.Sync>();

            GenerateDot();
            burnIndex = DotAPI.RegisterDotDef(burnDotDef, BurnBehavior);

            Hooks();
        }

        private static void BurnBehavior(DotController self, DotController.DotStack dotStack)
        {
            if (dotStack.attackerObject)
            {
                CharacterBody attackerBody = dotStack.attackerObject.GetComponent<CharacterBody>();
                if (attackerBody && attackerBody.inventory && self && self.victimHealthComponent)
                {
                    int count = attackerBody.inventory.GetItemCountEffective(itemDef);
                    float burnPercentPerTick = Utilities.GetLinearStacking(percentBurnDamage, percentBurnDamageExtraStacks, count) * burnTickInterval.Value / burnDuration.Value;
                    dotStack.damage = self.victimHealthComponent.fullCombinedHealth * burnPercentPerTick;

                    CharacterBody trackerBody = Utilities.GetMinionOwnershipParentBody(attackerBody);
                    Statistics stats = trackerBody.inventory.GetComponent<Statistics>();
                    stats.TotalDamageDealt += dotStack.damage;
                }
            }
        }

        private static void GenerateDot()
        {
            burnDotDef = new DotController.DotDef
            {
                damageCoefficient = 0f,
                damageColorIndex = maskDamageColor,
                associatedBuff = burnDebuff,
                terminalTimedBuff = null,
                terminalTimedBuffDuration = 0f,
                resetTimerOnAdd = true,
                interval = burnTickInterval.Value
            };
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
                if (vicBody && atkBody && atkBody.inventory)
                {
                    int count = atkBody.inventory.GetItemCountEffective(itemDef);
                    if (count > 0 && damageReport.dotType != burnIndex && !Utilities.OnSameTeam(vicBody, atkBody))
                    {
                        InflictDotInfo dotInfo = new()
                        {
                            victimObject = vicBody.gameObject,
                            attackerObject = atkBody.gameObject,
                            totalDamage = null,
                            dotIndex = burnIndex,
                            duration = burnDuration.Value,
                            maxStacksFromAttacker = 1,
                            damageMultiplier = 0f
                        };
                        DotController.InflictDot(ref dotInfo);
                    }
                }
            };
        }
    }
}
