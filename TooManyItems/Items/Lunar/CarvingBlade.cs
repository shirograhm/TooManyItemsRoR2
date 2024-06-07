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
        public static DamageColorIndex damageColor = DamageColorAPI.RegisterDamageColor(Utils.CARVING_BLADE_COLOR);

        // Deal 1% (+1% per stack) enemy current health as bonus on-hit damage. You cannot crit.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Carving Blade",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_CARVINGBLADE_DESC"
            }
        );
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

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            NetworkingAPI.RegisterMessageType<Statistics.Sync>();

            damageType = DamageAPI.ReserveDamageType();

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "CARVINGBLADE";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Lunar);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("CarvingBlade.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("CarvingBlade.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        public static float CalculateDamageOnHit(CharacterBody sender, int itemCount)
        {
            return sender.healthComponent.health * Utils.GetHyperbolicStacking(multiplierPerStack, itemCount);
        }

        public static void Hooks()
        {
            CharacterMaster.onStartGlobal += (obj) =>
            {
                obj.inventory?.gameObject.AddComponent<Statistics>();
            };

            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (attackerInfo.inventory && attackerInfo.inventory.GetItemCount(itemDef) > 0)
                {
                    damageInfo.crit = false;
                }
            };

            GenericGameEvents.OnHitEnemy += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (attackerInfo.body && victimInfo.body && attackerInfo.inventory)
                {
                    int count = attackerInfo.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        // Minimum of 0.1 damage.
                        float amount = Mathf.Max(CalculateDamageOnHit(victimInfo.body, count), 0.1f);
                        if (damageCapMultiplier > 0) amount = Mathf.Min(amount, attackerInfo.body.damage * damageCapMultiplier);

                        DamageInfo proc = new()
                        {
                            damage = amount,
                            attacker = attackerInfo.gameObject,
                            inflictor = attackerInfo.gameObject,
                            procCoefficient = 0f,
                            position = damageInfo.position,
                            crit = false,
                            damageColorIndex = damageColor,
                            procChainMask = damageInfo.procChainMask,
                            damageType = DamageType.Silent
                        };
                        proc.AddModdedDamageType(damageType);

                        victimInfo.healthComponent.TakeDamage(proc);

                        // Damage calculation takes minions into account
                        CharacterBody trackerBody = Utils.GetMinionOwnershipParentBody(attackerInfo.body);
                        Statistics stats = trackerBody.inventory.GetComponent<Statistics>();
                        stats.TotalDamageDealt += amount;
                    }
                }
            };
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