﻿using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class SpiritStone
    {
        public static ItemDef itemDef;
        // Killing an enemy grants 1 (+1 per stack) permanent shield. Lose 50% (+50% per stack) max health.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Spirit Stone",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_SPIRITSTONE_DESC"
            }
        );
        public static ConfigurableValue<float> shieldPerKill = new(
            "Item: Spirit Stone",
            "Shield Amount",
            1f,
            "Health given as shield for every kill.",
            new List<string>()
            {
                "ITEM_SPIRITSTONE_DESC"
            }
        );
        public static ConfigurableValue<float> maxHealthLost = new(
            "Item: Spirit Stone",
            "Max Health Reduction",
            50f,
            "Max health lost as a penalty for holding this item.",
            new List<string>()
            {
                "ITEM_SPIRITSTONE_DESC"
            }
        );
        public static float maxHealthLostPercent = maxHealthLost.Value / 100f;

        public class Statistics : MonoBehaviour
        {
            private float _permanentShield;
            public float PermanentShield
            {
                get { return _permanentShield; }
                set
                {
                    _permanentShield = value;
                    if (NetworkServer.active)
                    {
                        new Sync(gameObject.GetComponent<NetworkIdentity>().netId, value).Send(NetworkDestination.Clients);
                    }
                }
            }

            public class Sync : INetMessage
            {
                NetworkInstanceId objId;
                float permanentShield;

                public Sync()
                {
                }

                public Sync(NetworkInstanceId objId, float shield)
                {
                    this.objId = objId;
                    permanentShield = shield;
                }

                public void Deserialize(NetworkReader reader)
                {
                    objId = reader.ReadNetworkId();
                    permanentShield = reader.ReadSingle();
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
                            component.PermanentShield = permanentShield;
                        }
                    }
                }

                public void Serialize(NetworkWriter writer)
                {
                    writer.Write(objId);
                    writer.Write(permanentShield);
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

            itemDef.name = "SPIRIT_STONE";
            itemDef.nameToken = "SPIRIT_STONE_NAME";
            itemDef.pickupToken = "SPIRIT_STONE_PICKUP";
            itemDef.descriptionToken = "SPIRIT_STONE_DESCRIPTION";
            itemDef.loreToken = "SPIRIT_STONE_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Lunar;
            });

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("SpiritStone.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("SpiritStone.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
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
                    int itemCount = sender.inventory.GetItemCount(itemDef);
                    if (itemCount > 0)
                    {
                        var component = sender.inventory.GetComponent<Statistics>();
                        args.baseShieldAdd += component.PermanentShield;

                        args.healthMultAdd -= 1 - Utils.GetExponentialStacking(1 - maxHealthLostPercent, itemCount);
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
                        component.PermanentShield += shieldPerKill * count;

                        Utils.ForceRecalculate(atkBody);
                    }

                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("SPIRIT_STONE", "Spirit Stone");
            LanguageAPI.Add("SPIRIT_STONE_NAME", "Spirit Stone");
            LanguageAPI.Add("SPIRIT_STONE_PICKUP", "Gain a permanent stacking shield when killing enemies. <style=cDeath>Lose a portion of your max health</style>.");

            string desc = $"Killing an enemy grants " +
                $"<style=cIsUtility>{shieldPerKill.Value} <style=cStack>(+{shieldPerKill.Value} per stack)</style> " +
                $"shield</style> permanently. " +
                $"<style=cDeath>Lose {maxHealthLost.Value}% <style=cStack>(+{maxHealthLost.Value}% per stack)</style> max health</style>.";
            LanguageAPI.Add("SPIRIT_STONE_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("SPIRIT_STONE_LORE", lore);
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