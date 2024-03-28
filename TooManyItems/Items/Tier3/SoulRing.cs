using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class SoulRing
    {
        public static ItemDef itemDef;

        // On kill, permanently increase your health regeneration by 0.2 health, up to a maximum of 25 (+25 per stack) HP/s.
        public static ConfigurableValue<float> healthRegenOnKill = new(
            "Item: Soul Ring",
            "Health Regen On Kill",
            0.2f,
            "Amount of permanent health regeneration gained on kill.",
            new List<string>()
            {
                "ITEM_SOULRING_DESC"
            }
        );
        public static ConfigurableValue<float> maxRegenPerStack = new(
            "Item: Soul Ring",
            "Maximum Regen Per Stack",
            25f,
            "Maximum amount of permanent health regeneration allowed per stack.",
            new List<string>()
            {
                "ITEM_SOULRING_DESC"
            }
        );

        public class Statistics : MonoBehaviour
        {
            private float _healthRegen;
            public float HealthRegen
            {
                get { return _healthRegen; }
                set
                {
                    _healthRegen = value;
                    if (NetworkServer.active)
                    {
                        new Sync(gameObject.GetComponent<NetworkIdentity>().netId, value).Send(NetworkDestination.Clients);
                    }
                }
            }

            public class Sync : INetMessage
            {
                NetworkInstanceId objId;
                float healthRegen;

                public Sync()
                {
                }

                public Sync(NetworkInstanceId objId, float regen)
                {
                    this.objId = objId;
                    healthRegen = regen;
                }

                public void Deserialize(NetworkReader reader)
                {
                    objId = reader.ReadNetworkId();
                    healthRegen = reader.ReadSingle();
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
                            component.HealthRegen = healthRegen;
                        }
                    }
                }

                public void Serialize(NetworkWriter writer)
                {
                    writer.Write(objId);
                    writer.Write(healthRegen);
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

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "SOUL_RING";
            itemDef.nameToken = "SOUL_RING_NAME";
            itemDef.pickupToken = "SOUL_RING_PICKUP";
            itemDef.descriptionToken = "SOUL_RING_DESCRIPTION";
            itemDef.loreToken = "SOUL_RING_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier3;
            });

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("SoulRing.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("SoulRing.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Healing,

                ItemTag.OnKillEffect
            };
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
                    int count = sender.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        var component = sender.inventory.GetComponent<Statistics>();

                        args.baseRegenAdd += component.HealthRegen;
                    }
                }
            };

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, eventManager, damageReport) =>
            {
                orig(eventManager, damageReport);

                if (!NetworkServer.active) return;

                CharacterBody atkBody = damageReport.attackerBody;
                if (atkBody && atkBody.inventory)
                {
                    int count = atkBody.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        var component = atkBody.inventory.GetComponent<Statistics>();
                        float maxRegenAllowed = maxRegenPerStack.Value * count;

                        if (component.HealthRegen + healthRegenOnKill.Value <= maxRegenAllowed)
                        {
                            component.HealthRegen += healthRegenOnKill.Value;
                        }
                        else
                        {
                            component.HealthRegen = maxRegenAllowed;
                        }
                    }

                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("SOUL_RING", "Soul Ring");
            LanguageAPI.Add("SOUL_RING_NAME", "Soul Ring");
            LanguageAPI.Add("SOUL_RING_PICKUP", "Gain permanent health regen on kill.");

            string desc = $"On kill, permanently increase your health regeneration by <style=cIsHealing>{healthRegenOnKill.Value} HP/s</style>, " +
                $"up to a maximum of <style=cIsHealing>{maxRegenPerStack.Value} <style=cStack>(+{maxRegenPerStack.Value} per stack)</style> HP/s</style>.";
            LanguageAPI.Add("SOUL_RING_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("SOUL_RING_LORE", lore);
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