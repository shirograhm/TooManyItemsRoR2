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

        // On kill, gain a stack of Mulch. Every 8 (-10% per stack) seconds, consume all stacks and heal for 12 HP per stack.
        public static ConfigurableValue<float> healingPerStack = new(
            "Item: Rusty Trowel",
            "Heal Per Stack",
            12f,
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
            10f,
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
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            ContentAddition.AddBuffDef(mulchBuff);
            ContentAddition.AddBuffDef(healingTimer);

            NetworkingAPI.RegisterMessageType<Statistics.Sync>();

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "RUSTED_TROWEL";
            itemDef.nameToken = "RUSTED_TROWEL_NAME";
            itemDef.pickupToken = "RUSTED_TROWEL_PICKUP";
            itemDef.descriptionToken = "RUSTED_TROWEL_DESCRIPTION";
            itemDef.loreToken = "RUSTED_TROWEL_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier3;
            });

            itemDef.pickupIconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("RustedTrowel.png");
            itemDef.pickupModelPrefab = TooManyItems.MainAssets.LoadAsset<GameObject>("RustyTrowel.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        private static void GenerateBuff()
        {
            mulchBuff = ScriptableObject.CreateInstance<BuffDef>();

            mulchBuff.name = "Mulch";
            mulchBuff.iconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("Mulch.png");
            mulchBuff.buffColor = new Color(0.192f, 0.435f, 0.22f, 1f);
            mulchBuff.canStack = true;
            mulchBuff.isDebuff = false;

            healingTimer = ScriptableObject.CreateInstance<BuffDef>();

            healingTimer.name = "Mulch Cooldown";
            healingTimer.iconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("MulchCooldown.png");
            healingTimer.buffColor = new Color(0.192f, 0.435f, 0.22f, 1f);
            healingTimer.isDebuff = false;
            healingTimer.canStack = false;
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

                        var stats = self.inventory.GetComponent<Statistics>();
                        stats.TotalHealingDone += healing;

#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                        self.SetBuffCount(mulchBuff.buffIndex, 0);
#pragma warning restore Publicizer001 // Accessing a member that was not originally public
                    }
                }
            };

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, eventManager, damageReport) =>
            {
                orig(eventManager, damageReport);
                if (!NetworkServer.active || damageReport.attackerBody == null) return;

                int itemCount = damageReport.attackerBody.inventory.GetItemCount(itemDef);
                if (itemCount > 0)
                {
                    damageReport.attackerBody.AddBuff(mulchBuff);
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("RUSTED_TROWEL", "Rusty Trowel");
            LanguageAPI.Add("RUSTED_TROWEL_NAME", "Rusty Trowel");
            LanguageAPI.Add("RUSTED_TROWEL_PICKUP", "Harvest Mulch from killing enemies. Heal periodically based on Mulch stacks.");

            string desc = $"On kill, gain a stack of Mulch. Every <style=cIsUtility>{rechargeTime.Value}</style> <style=cStack>(-{rechargeTimeReductionPerStack.Value}% per stack)</style> seconds, " +
                $"consume all Mulch to heal <style=cIsHealing>{healingPerStack.Value}</style> per stack.";
            LanguageAPI.Add("RUSTED_TROWEL_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("RUSTED_TROWEL_LORE", lore);
        }
    }
}

// Styles
// <style=cIsHealth>" + exampleValue + "</style>
// <style=cIsDamage>" + exampleValue + "</style>
// <style=cIsHealing>" + exampleValue + "</style>
// <style=cIsUtility>" + exampleValue + "</style>
// <style=cIsVoid>" + exampleValue + "</style>
// <style=cHumanObjective>" + exampleValue + "</style>
// <style=cLunarObjective>" + exampleValue + "</style>
// <style=cStack>" + exampleValue + "</style>
// <style=cWorldEvent>" + exampleValue + "</style>
// <style=cArtifact>" + exampleValue + "</style>
// <style=cUserSetting>" + exampleValue + "</style>
// <style=cDeath>" + exampleValue + "</style>
// <style=cSub>" + exampleValue + "</style>
// <style=cMono>" + exampleValue + "</style>
// <style=cShrine>" + exampleValue + "</style>
// <style=cEvent>" + exampleValue + "</style>