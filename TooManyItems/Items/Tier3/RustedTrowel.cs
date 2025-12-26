using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems.Items.Tier3
{
    internal class RustedTrowel
    {
        public static ItemDef itemDef;
        public static BuffDef mulchBuff;
        public static BuffDef healingTimer;

        // Gain stacks of Mulch on-hit. Periodically heal based on the stacks accrued.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Rusted Trowel",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_RUSTYTROWEL_DESC"]
        );
        public static ConfigurableValue<float> healingPerStack = new(
            "Item: Rusted Trowel",
            "Heal Per Stack",
            3f,
            "Health recovered per stack of Mulch.",
            ["ITEM_RUSTEDTROWEL_DESC"]
        );
        public static ConfigurableValue<float> rechargeTime = new(
            "Item: Rusted Trowel",
            "Recharge Time",
            8f,
            "Time this item takes to recharge.",
            ["ITEM_RUSTEDTROWEL_DESC"]
        );
        public static ConfigurableValue<float> rechargeTimeReductionPerStack = new(
            "Item: Rusted Trowel",
            "Recharge Time Reduction",
            20f,
            "Percent of recharge time removed for every additional stack of this item.",
            ["ITEM_RUSTEDTROWEL_DESC"]
        );
        public static float rechargeTimeReductionPercent = rechargeTimeReductionPerStack.Value / 100f;

        public class Statistics : MonoBehaviour
        {
            private float _totalHealingDone;
            public float TotalHealingDone
            {
                get { return _totalHealingDone; }
                set
                {
                    _totalHealingDone = value;
                    if (NetworkServer.active)
                    {
                        new Sync(gameObject.GetComponent<NetworkIdentity>().netId, value).Send(NetworkDestination.Clients);
                    }
                }
            }

            public class Sync : INetMessage
            {
                NetworkInstanceId objId;
                float totalHealingDone;

                public Sync()
                {
                }

                public Sync(NetworkInstanceId objId, float totalHealing)
                {
                    this.objId = objId;
                    totalHealingDone = totalHealing;
                }

                public void Deserialize(NetworkReader reader)
                {
                    objId = reader.ReadNetworkId();
                    totalHealingDone = reader.ReadSingle();
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
                            component.TotalHealingDone = totalHealingDone;
                        }
                    }
                }

                public void Serialize(NetworkWriter writer)
                {
                    writer.Write(objId);
                    writer.Write(totalHealingDone);

                    writer.FinishMessage();
                }
            }
        }

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("RustedTrowel", [ItemTag.Healing, ItemTag.CanBeTemporary], ItemTier.Tier3);

            mulchBuff = ItemManager.GenerateBuff("Mulch", AssetManager.bundle.LoadAsset<Sprite>("Mulch.png"), canStack: true);
            ContentAddition.AddBuffDef(mulchBuff);
            healingTimer = ItemManager.GenerateBuff("Mulch Cooldown", AssetManager.bundle.LoadAsset<Sprite>("MulchCooldown.png"), isCooldown: true);
            ContentAddition.AddBuffDef(healingTimer);

            NetworkingAPI.RegisterMessageType<Statistics.Sync>();

            Hooks();
        }

        public static float CalculateCooldownInSec(int itemCount)
        {
            return rechargeTime.Value * Mathf.Pow(1 - rechargeTimeReductionPercent, itemCount - 1);
        }

        public static void Hooks()
        {
            CharacterMaster.onStartGlobal += (obj) =>
            {
                obj.inventory?.gameObject.AddComponent<Statistics>();
            };

            On.RoR2.CharacterBody.FixedUpdate += (orig, self) =>
            {
                orig(self);

                if (self && self.inventory)
                {
                    int itemCount = self.inventory.GetItemCountEffective(itemDef);
                    if (itemCount > 0 && self.GetBuffCount(mulchBuff) > 0 && !self.HasBuff(healingTimer))
                    {
                        self.AddTimedBuff(healingTimer, CalculateCooldownInSec(itemCount));
                    }
                }
            };

            On.RoR2.CharacterBody.OnBuffFinalStackLost += (orig, self, buffDef) =>
            {
                orig(self, buffDef);

                if (self && buffDef == healingTimer)
                {
                    int buffCount = self.GetBuffCount(mulchBuff);
                    if (buffCount > 0)
                    {
                        float healing = buffCount * healingPerStack.Value;
                        self.healthComponent.Heal(healing, new ProcChainMask());

                        Utilities.SpawnHealEffect(self);

                        Statistics stats = self.inventory.GetComponent<Statistics>();
                        stats.TotalHealingDone += healing;

#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                        self.SetBuffCount(mulchBuff.buffIndex, 0);
#pragma warning restore Publicizer001 // Accessing a member that was not originally public
                    }
                }
            };

            GameEventManager.OnHitEnemy += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (attackerInfo.body && attackerInfo.inventory)
                {
                    int itemCount = attackerInfo.inventory.GetItemCountEffective(itemDef);
                    if (itemCount > 0)
                    {
                        int newBuffCount = attackerInfo.body.GetBuffCount(mulchBuff) + itemCount;
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                        attackerInfo.body.SetBuffCount(mulchBuff.buffIndex, newBuffCount);
#pragma warning restore Publicizer001 // Accessing a member that was not originally public
                    }
                }
            };
        }
    }
}
