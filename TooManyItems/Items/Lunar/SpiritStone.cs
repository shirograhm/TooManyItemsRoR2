using R2API;
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
        // On-kill, gain 1% (+1% per stack) of your max health as permanent shield. All your cooldowns are increased by 4 seconds.
        public static ConfigurableValue<float> maxHealthShield = new(
            "Item: Spirit Stone",
            "Shield Amount",
            1f,
            "Percent of max health given as shield for each lunar item in inventory.",
            new List<string>()
            {
                "ITEM_SPIRITSTONE_DESC"
            }
        );
        public static float maxHealthShieldPercent = maxHealthShield.Value / 100f;

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
                    }
                }
            };

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, eventManager, damageReport) =>
            {
                orig(eventManager, damageReport);

                if (!NetworkServer.active) return;

                if (damageReport.attackerBody)
                {
                    CharacterBody atkBody = damageReport.attackerBody;
                    if (atkBody.inventory)
                    {
                        int count = atkBody.inventory.GetItemCount(itemDef);
                        if (count > 0)
                        {
                            float incrementedShieldAmount = atkBody.healthComponent.fullCombinedHealth * maxHealthShieldPercent * count;

                            var component = atkBody.inventory.GetComponent<Statistics>();
                            component.PermanentShield += incrementedShieldAmount;

                            atkBody.RecalculateStats();
                        }
                    }
                }
            };

            GenericGameEvents.OnTakeDamage += (damageReport) =>
            {
                if (damageReport.victimBody)
                {
                    CharacterBody vicBody = damageReport.victimBody;
                    if (vicBody.inventory && vicBody.inventory.GetItemCount(itemDef) > 0)
                    {
                        vicBody.AddBuff(RoR2Content.Buffs.PermanentCurse);
                    }
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("SPIRIT_STONE", "Spirit Stone");
            LanguageAPI.Add("SPIRIT_STONE_NAME", "Spirit Stone");
            LanguageAPI.Add("SPIRIT_STONE_PICKUP", "Gain a permanent stacking shield when killing enemies. <style=cDeath>Taking damage reduces your max health.</style>");

            string desc = $"On-kill, gain <style=cIsUtility>{maxHealthShield.Value}%</style> " +
                $"(+<style=cIsUtility>{maxHealthShield.Value}%</style> per stack) of your max health as permanent shield. " +
                $"<style=cDeath>Taking damage applies a stacking curse, which lowers your max health.</style>";
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