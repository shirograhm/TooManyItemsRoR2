using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class CarvingBlade
    {
        public static ItemDef itemDef;

        public static DamageAPI.ModdedDamageType damageType;
        public static DamageColorIndex damageColor = DamageColorAPI.RegisterDamageColor(new(0.09f, 0.67f, 0.42f, 1f));

        // Deal 1% (+1% per stack) enemy current health as bonus on-hit damage. You cannot crit.
        public static ConfigurableValue<float> percentDamagePerStack = new(
            "Item: Carving Blade",
            "On-Hit Damage Scaling",
            1f,
            "Percent of enemy's current health dealt as bonus on-hit damage.",
            new List<string>()
            {
                "ITEM_CARVINGBLADE_DESC"
            }
        );
        // This damage is capped at 20000% of the player's base damage.
        public static ConfigurableValue<float> damageCapMultiplier = new(
            "Item: Carving Blade",
            "Damage Cap",
            200f,
            "Maximum damage on-hit. This value is multiplied by the user's base damage.\nSet this value to -1 to remove the cap.",
            new List<string>()
            {
                "ITEM_CARVINGBLADE_DESC"
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
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            NetworkingAPI.RegisterMessageType<Statistics.Sync>();

            damageType = DamageAPI.ReserveDamageType();

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "CARVING_BLADE";
            itemDef.nameToken = "CARVING_BLADE_NAME";
            itemDef.pickupToken = "CARVING_BLADE_PICKUP";
            itemDef.descriptionToken = "CARVING_BLADE_DESCRIPTION";
            itemDef.loreToken = "CARVING_BLADE_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Lunar;
            });

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("CarvingBlade.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("CarvingBlade.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        public static float CalculateDamageOnHit(CharacterBody sender, float itemCount)
        {
            float hyperbolicMultiplier = 1 - 1 / (1 + multiplierPerStack * itemCount);
            return sender.healthComponent.health * hyperbolicMultiplier;
        }

        public static void Hooks()
        {
            CharacterMaster.onStartGlobal += (obj) =>
            {
                obj.inventory?.gameObject.AddComponent<Statistics>();
            };

            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (attackerInfo.inventory == null) return;

                if (attackerInfo.inventory.GetItemCount(itemDef) > 0)
                {
                    damageInfo.crit = false;
                }
            };

            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
            {
                orig(self, damageInfo, victim);

                if (!NetworkServer.active) return;
                if (damageInfo.attacker == null || victim == null) return;

                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                CharacterBody victimBody = victim.GetComponent<CharacterBody>();

                if (attackerBody != null && attackerBody.inventory != null)
                {
                    int count = attackerBody.inventory.GetItemCount(itemDef);

                    if (count > 0)
                    {
                        float damageAmount = CalculateDamageOnHit(victimBody, count);
                        // Cap damage based on config
                        if (damageCapMultiplier > 0) damageAmount = Mathf.Min(damageAmount, attackerBody.damage * damageCapMultiplier);

                        DamageInfo damageProc = new()
                        {
                            damage = damageAmount,
                            attacker = damageInfo.attacker.gameObject,
                            inflictor = damageInfo.attacker.gameObject,
                            procCoefficient = 0f,
                            position = damageInfo.position,
                            crit = false,
                            damageColorIndex = damageColor,
                            procChainMask = damageInfo.procChainMask,
                            damageType = DamageType.Silent
                        };
                        damageProc.AddModdedDamageType(damageType);

                        victimBody.healthComponent.TakeDamage(damageProc);

                        // Damage calculation takes minions into account
                        if (attackerBody && attackerBody.master && attackerBody.master.minionOwnership && attackerBody.master.minionOwnership.ownerMaster)
                        {
                            if (attackerBody.master.minionOwnership.ownerMaster.GetBody())
                            {
                                attackerBody = attackerBody.master.minionOwnership.ownerMaster.GetBody();
                            }
                        }
                        var stats = attackerBody.inventory.GetComponent<Statistics>();
                        stats.TotalDamageDealt += damageAmount;
                    }
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("CARVING_BLADE", "Carving Blade");
            LanguageAPI.Add("CARVING_BLADE_NAME", "Carving Blade");
            LanguageAPI.Add("CARVING_BLADE_PICKUP", "Deal bonus damage on-hit based on the enemy's current health. <style=cDeath>You cannot critically strike.</style>");

            string desc = $"Deal <style=cIsDamage>{percentDamagePerStack.Value}%</style> <style=cStack>(+{percentDamagePerStack.Value}% per stack)</style> of enemy current health as bonus on-hit damage. " +
                $"<style=cDeath>You cannot critically strike.</style>";
            LanguageAPI.Add("CARVING_BLADE_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("CARVING_BLADE_LORE", lore);
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