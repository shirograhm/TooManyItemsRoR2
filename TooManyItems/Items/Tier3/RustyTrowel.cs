using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class RustyTrowel
    {
        public static ItemDef itemDef;
        public static BuffDef mulchBuff;
        public static BuffDef healingTimer;

        // Gain stacks of Mulch on-hit. Periodically heal based on the stacks accrued.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Rusty Trowel",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_RUSTYTROWEL_DESC"
            }
        );
        public static ConfigurableValue<float> healingPerStack = new(
            "Item: Rusty Trowel",
            "Heal Per Stack",
            3f,
            "Health recovered per stack of Mulch.",
            new List<string>()
            {
                "ITEM_RUSTEDTROWEL_DESC"
            }
        );
        public static ConfigurableValue<float> rechargeTime = new(
            "Item: Rusty Trowel",
            "Recharge Time",
            8f,
            "Time this item takes to recharge.",
            new List<string>()
            {
                "ITEM_RUSTEDTROWEL_DESC"
            }
        );
        public static ConfigurableValue<float> rechargeTimeReductionPerStack = new(
            "Item: Rusty Trowel",
            "Recharge Time Reduction",
            30f,
            "Percent of recharge time removed for every additional stack of this item.",
            new List<string>()
            {
                "ITEM_RUSTEDTROWEL_DESC"
            }
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
                }
            }
        }

        internal static void Init()
        {
            GenerateItem();
            GenerateBuff();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            ContentAddition.AddBuffDef(mulchBuff);
            ContentAddition.AddBuffDef(healingTimer);

            NetworkingAPI.RegisterMessageType<Statistics.Sync>();

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "RUSTEDTROWEL";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier3);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("RustedTrowel.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("RustyTrowel.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Healing
            };
        }

        private static void GenerateBuff()
        {
            mulchBuff = ScriptableObject.CreateInstance<BuffDef>();

            mulchBuff.name = "Mulch";
            mulchBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Mulch.png");
            mulchBuff.canStack = true;
            mulchBuff.isHidden = false;
            mulchBuff.isDebuff = false;
            mulchBuff.isCooldown = false;

            healingTimer = ScriptableObject.CreateInstance<BuffDef>();

            healingTimer.name = "Mulch Cooldown";
            healingTimer.iconSprite = Assets.bundle.LoadAsset<Sprite>("MulchCooldown.png");
            healingTimer.canStack = false;
            healingTimer.isHidden = false;
            healingTimer.isDebuff = false;
            healingTimer.isCooldown = true;
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
                    int itemCount = self.inventory.GetItemCount(itemDef);
                    if (itemCount > 0 && self.GetBuffCount(mulchBuff) > 0 && !self.HasBuff(healingTimer))
                    {
                        self.AddTimedBuff(healingTimer, CalculateCooldownInSec(itemCount));
                    }
                }
            };

            On.RoR2.CharacterBody.OnBuffFinalStackLost += (orig, self, buffDef) =>
            {
                orig(self, buffDef);

                if (buffDef == healingTimer)
                {
                    int buffCount = self.GetBuffCount(mulchBuff);
                    if (buffCount > 0)
                    {
                        float healing = buffCount * healingPerStack.Value;
                        self.healthComponent.Heal(healing, new ProcChainMask());

                        Statistics stats = self.inventory.GetComponent<Statistics>();
                        stats.TotalHealingDone += healing;

#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                        self.SetBuffCount(mulchBuff.buffIndex, 0);
#pragma warning restore Publicizer001 // Accessing a member that was not originally public
                    }
                }
            };

            GenericGameEvents.OnHitEnemy += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (attackerInfo.body && attackerInfo.inventory)
                {
                    int itemCount = attackerInfo.inventory.GetItemCount(itemDef);
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
