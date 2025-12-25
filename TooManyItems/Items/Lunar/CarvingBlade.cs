using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems.Items.Lunar
{
    internal class CarvingBlade
    {
        public static ItemDef itemDef;

        public static DamageAPI.ModdedDamageType damageType;
        public static DamageColorIndex damageColor = DamageColorManager.RegisterDamageColor(Utilities.CARVING_BLADE_COLOR);

        // Deal a percentage of enemy current health as bonus on-hit damage. You cannot crit.
        // On-hit, deal 2% of the enemy's current HP. Per-hit damage is capped at 2000% (+1000% per stack) of your BASE damage. You cannot crit.
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
        public static ConfigurableValue<float> currentHPDamage = new(
            "Item: Carving Blade",
            "On-Hit Damage Scaling",
            2f,
            "Percent of enemy's current health dealt as bonus on-hit damage.",
            new List<string>()
            {
                "ITEM_CARVINGBLADE_DESC"
            }
        );
        public static ConfigurableValue<float> damageCapMultiplier = new(
            "Item: Carving Blade",
            "Damage Cap",
            2000f,
            "Maximum damage on-hit. This value is displayed as a percentage of the user's base damage (100 = 1x your base damage).",
            new List<string>()
            {
                "ITEM_CARVINGBLADE_DESC"
            }
        );
        public static ConfigurableValue<float> damageCapMultiplierExtraStacks = new(
            "Item: Carving Blade",
            "Damage Cap Extra Stacks",
            1000f,
            "Maximum damage on-hit with extra stacks. This value is displayed as a percentage of the user's base damage (100 = 1x your base damage).",
            new List<string>()
            {
                "ITEM_CARVINGBLADE_DESC"
            }
        );
        public static float currentHPDamageAsPercent = currentHPDamage.Value / 100.0f;
        public static float damageCapMultAsPercent = damageCapMultiplier.Value / 100.0f;
        public static float damageCapMultExtraStacksAsPercent = damageCapMultiplierExtraStacks.Value / 100.0f;

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

            itemDef.name = "CARVINGBLADE";
            itemDef.AutoPopulateTokens();

            Utilities.SetItemTier(itemDef, ItemTier.Lunar);

            GameObject prefab = AssetManager.bundle.LoadAsset<GameObject>("CarvingBlade.prefab");
            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            itemDef.pickupIconSprite = AssetManager.bundle.LoadAsset<Sprite>("CarvingBlade.png");
            itemDef.pickupModelPrefab = prefab;
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        public static float CalculateDamageCapPercent(int itemCount)
        {
            return damageCapMultAsPercent + damageCapMultExtraStacksAsPercent * (itemCount - 1);
        }

        public static void Hooks()
        {
            CharacterMaster.onStartGlobal += (obj) =>
            {
                obj.inventory?.gameObject.AddComponent<Statistics>();
            };

            GameEventManager.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (attackerInfo.inventory && attackerInfo.inventory.GetItemCountPermanent(itemDef) > 0)
                {
                    damageInfo.crit = false;
                }
            };

            GameEventManager.OnHitEnemy += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (attackerInfo.body && victimInfo.body && attackerInfo.inventory)
                {
                    int itemCount = attackerInfo.inventory.GetItemCountPermanent(itemDef);
                    if (itemCount > 0 && attackerInfo.teamComponent.teamIndex != victimInfo.teamComponent.teamIndex)
                    {
                        // Minimum of 0.01 damage to prevent negative values in LookingGlass
                        float amount = Mathf.Max(victimInfo.body.healthComponent.health * currentHPDamageAsPercent, 0.01f);
                        // Cap the damage. If the damage cap was set to -1 to remove it, set it to default value instead.
                        if (damageCapMultiplier.Value < 0) damageCapMultAsPercent = 40f;
                        amount = Mathf.Min(amount, attackerInfo.body.damage * CalculateDamageCapPercent(itemCount));

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
                        CharacterBody trackerBody = Utilities.GetMinionOwnershipParentBody(attackerInfo.body);
                        Statistics stats = trackerBody.inventory.GetComponent<Statistics>();
                        stats.TotalDamageDealt += amount;
                    }
                }
            };
        }
    }
}
