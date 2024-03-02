﻿using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class BloodDice
    {
        public static ItemDef itemDef;

        // On kill, permanently gain 1 to 6 bonus health, up to a max of 500 (+500 per stack) health.
        public static ConfigurableValue<bool> affectedByLuck = new(
            "Item: Blood Dice",
            "Affected By Luck",
            true,
            "Whether or not the likelihood of a high roll is affected by luck.",
            new List<string>()
            {
                "ITEM_BLOODDICE_DESC"
            }
        );
        public static ConfigurableValue<float> maxHealthPerStack = new(
            "Item: Blood Dice",
            "Maximum Health Per Item",
            500f,
            "Maximum amount of permanent health allowed per stack.",
            new List<string>()
            {
                "ITEM_BLOODDICE_DESC"
            }
        );

        public class Statistics : MonoBehaviour
        {
            private float _permanentHealth;
            public float PermanentHealth
            {
                get { return _permanentHealth; }
                set
                {
                    _permanentHealth = value;
                    if (NetworkServer.active)
                    {
                        new Sync(gameObject.GetComponent<NetworkIdentity>().netId, value).Send(NetworkDestination.Clients);
                    }
                }
            }

            public class Sync : INetMessage
            {
                NetworkInstanceId objId;
                float permanentHealth;

                public Sync()
                {
                }

                public Sync(NetworkInstanceId objId, float health)
                {
                    this.objId = objId;
                    permanentHealth = health;
                }

                public void Deserialize(NetworkReader reader)
                {
                    objId = reader.ReadNetworkId();
                    permanentHealth = reader.ReadSingle();
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
                            component.PermanentHealth = permanentHealth;
                        }
                    }
                }

                public void Serialize(NetworkWriter writer)
                {
                    writer.Write(objId);
                    writer.Write(permanentHealth);
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

            itemDef.name = "BLOOD_DICE";
            itemDef.nameToken = "BLOOD_DICE_NAME";
            itemDef.pickupToken = "BLOOD_DICE_PICKUP";
            itemDef.descriptionToken = "BLOOD_DICE_DESCRIPTION";
            itemDef.loreToken = "BLOOD_DICE_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier3;
            });

            itemDef.pickupIconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("BloodDice.png");
            itemDef.pickupModelPrefab = TooManyItems.MainAssets.LoadAsset<GameObject>("BloodDice.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        public static void Hooks()
        {
            CharacterMaster.onStartGlobal += (obj) =>
            {
                obj.inventory?.gameObject.AddComponent<Statistics>();
            };

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender == null || sender.inventory == null) return;

                int count = sender.inventory.GetItemCount(itemDef);
                if (count > 0)
                {
                    var component = sender.inventory.GetComponent<Statistics>();
                    args.baseHealthAdd += component.PermanentHealth;
                }
            };

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, eventManager, damageReport) =>
            {
                orig(eventManager, damageReport);

                if (!NetworkServer.active) return;

                CharacterMaster atkMaster = damageReport.attackerMaster;
                CharacterBody atkBody = damageReport.attackerBody;

                if (atkMaster && atkBody)
                {
                    int itemCount = atkBody.inventory.GetItemCount(itemDef);
                    if (itemCount > 0)
                    {
                        float maxHealthAllowed = maxHealthPerStack.Value * itemCount;
                        int roll = GetDiceRoll(atkMaster);

                        var component = atkBody.inventory.GetComponent<Statistics>();

                        if (component.PermanentHealth + roll < maxHealthAllowed)
                        {
                            component.PermanentHealth += roll;
                        }
                        else
                        {
                            component.PermanentHealth = maxHealthAllowed;
                        }

                        atkBody.RecalculateStats();
                    }
                }
            };
        }

        private static int GetDiceRoll(CharacterMaster atkMaster)
        {
            int diceRoll = TooManyItems.rand.Next(1, 7);

            if (affectedByLuck)
            {
                int luckStat = (int)atkMaster.luck;

                for (int i = 0; i < Mathf.Abs(luckStat); i++)
                {
                    int newRoll = TooManyItems.rand.Next(1, 7);

                    if (luckStat > 0)
                    {
                        diceRoll = newRoll > diceRoll ? newRoll : diceRoll;
                    }
                    if (luckStat < 0)
                    {
                        diceRoll = newRoll < diceRoll ? newRoll : diceRoll;
                    }
                }
            }

            return diceRoll;
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("BLOOD_DICE", "Blood Dice");
            LanguageAPI.Add("BLOOD_DICE_NAME", "Blood Dice");
            LanguageAPI.Add("BLOOD_DICE_PICKUP", "Gain permanent health on kill.");

            string desc = $"On kill, gain <style=cIsHealth>1</style> to <style=cIsHealth>6</style> permanent max health, up to a maximum of " +
                $"<style=cIsHealth>{maxHealthPerStack.Value}</style> <style=cStack>(+{maxHealthPerStack.Value} per stack)</style>.";
            LanguageAPI.Add("BLOOD_DICE_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("BLOOD_DICE_LORE", lore);
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