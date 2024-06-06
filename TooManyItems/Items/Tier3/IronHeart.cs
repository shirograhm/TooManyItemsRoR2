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
                "ITEM_IRON_HEART_DESC"
            }
        );
        public static ConfigurableValue<float> healthIncrease = new(
            "Item: Iron Heart",
            "Health Increase",
            300f,
            "Bonus health gained from this item. Does not increase with stacks.",
            new List<string>()
            {
                "ITEM_IRON_HEART_DESC"
            }
        );
        public static ConfigurableValue<float> percentDamagePerStack = new(
            "Item: Iron Heart",
            "On-Hit Damage Scaling",
            2f,
            "Percent of maximum health dealt as bonus on-hit damage.",
            new List<string>()
            {
                "ITEM_IRON_HEART_DESC"
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
                }
            }
        }

        internal static void Init()
        {
            GenerateItem();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            NetworkingAPI.RegisterMessageType<Statistics.Sync>();

            damageType = DamageAPI.ReserveDamageType();

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "IRON_HEART";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier3);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("IronHeart.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("IronHeart.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,
                ItemTag.Utility
            };
        }

        public static float CalculateDamageOnHit(CharacterBody sender, float itemCount)
        {
            return sender.healthComponent.fullCombinedHealth * itemCount * multiplierPerStack;
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
                    if (sender.inventory.GetItemCount(itemDef) > 0)
                    {
                        args.baseHealthAdd += healthIncrease.Value;
                    }
                }
            };

            GenericGameEvents.OnHitEnemy += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (attackerInfo.body && attackerInfo.inventory)
                {
                    int count = attackerInfo.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        float amount = CalculateDamageOnHit(attackerInfo.body, count);

                        DamageInfo proc = new()
                        {
                            damage = amount,
                            attacker = attackerInfo.gameObject,
                            inflictor = attackerInfo.gameObject,
                            procCoefficient = 1f,
                            position = damageInfo.position,
                            crit = attackerInfo.body.RollCrit(),
                            damageColorIndex = damageColor,
                            procChainMask = damageInfo.procChainMask,
                            damageType = DamageType.Silent
                        };
                        proc.AddModdedDamageType(damageType);

                        victimInfo.healthComponent.TakeDamage(proc);

                        // Damage calculation takes minions into account
                        CharacterBody trackerBody = Utils.GetMinionOwnershipParentBody(attackerInfo.body);
                        var stats = trackerBody.inventory.GetComponent<Statistics>();
                        stats.TotalDamageDealt += amount;
                    }
                }
            };

            //On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
            //{
            //    orig(self, damageInfo, victim);

            //    if (!NetworkServer.active) return;
            //    if (damageInfo.attacker == null || victim == null) return;

            //    CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            //    CharacterBody victimBody = victim.GetComponent<CharacterBody>();

            //    if (attackerBody != null && attackerBody.inventory != null)
            //    {
            //        int count = attackerBody.inventory.GetItemCount(itemDef);

            //        if (count > 0)
            //        {
            //            float damageAmount = CalculateDamageOnHit(attackerBody, count);

            //            DamageInfo damageProc = new()
            //            {
            //                damage = damageAmount,
            //                attacker = damageInfo.attacker,
            //                inflictor = damageInfo.attacker,
            //                procCoefficient = 1f,
            //                position = damageInfo.position,
            //                crit = attackerBody.RollCrit(),
            //                damageColorIndex = damageColor,
            //                procChainMask = damageInfo.procChainMask,
            //                damageType = DamageType.Silent
            //            };
            //            damageProc.AddModdedDamageType(damageType);

            //            victimBody.healthComponent.TakeDamage(damageProc);

            //            // Damage calculation takes minions into account
            //            if (attackerBody && attackerBody.master && attackerBody.master.minionOwnership && attackerBody.master.minionOwnership.ownerMaster)
            //            {
            //                if (attackerBody.master.minionOwnership.ownerMaster.GetBody())
            //                {
            //                    attackerBody = attackerBody.master.minionOwnership.ownerMaster.GetBody();
            //                }
            //            }
            //            var stats = attackerBody.inventory.GetComponent<Statistics>();
            //            stats.TotalDamageDealt += damageAmount;
            //        }
            //    }
            //};
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