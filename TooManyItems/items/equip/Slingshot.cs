using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class Slingshot
    {
        public static EquipmentDef equipmentDef;
        public static BuffDef slingshotTimer;

        // On activation, mark your current position. After 8 seconds, blink back.
        public static ConfigurableValue<float> slingshotDuration = new(
            "Equipment: Slingshot",
            "Duration Before Blink",
            8f,
            "Time between using the equipment and blinking back.",
            new List<string>()
            {
                "ITEM_SLINGSHOT_DESC"
            }
        );

        public class Statistics : MonoBehaviour
        {
            private Vector3 _savedPosition;
            public Vector3 SavedPosition
            {
                get { return _savedPosition; }
                set
                {
                    _savedPosition = value;
                    if (NetworkServer.active)
                    {
                        new Sync(gameObject.GetComponent<NetworkIdentity>().netId, value).Send(NetworkDestination.Clients);
                    }
                }
            }

            public class Sync : INetMessage
            {
                NetworkInstanceId objId;
                Vector3 savedPosition;

                public Sync()
                {
                }

                public Sync(NetworkInstanceId objId, Vector3 position)
                {
                    this.objId = objId;
                    savedPosition = position;
                }

                public void Deserialize(NetworkReader reader)
                {
                    objId = reader.ReadNetworkId();
                    savedPosition.x = reader.ReadSingle();
                    savedPosition.y = reader.ReadSingle();
                    savedPosition.z = reader.ReadSingle();
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
                            component.SavedPosition = savedPosition;
                        }
                    }
                }

                public void Serialize(NetworkWriter writer)
                {
                    writer.Write(objId);
                    writer.Write(savedPosition.x);
                    writer.Write(savedPosition.y);
                    writer.Write(savedPosition.z);
                }
            }
        }

        internal static void Init()
        {
            GenerateEquipment();
            GenerateBuff();
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomEquipment(equipmentDef, displayRules));

            ContentAddition.AddBuffDef(slingshotTimer);
            NetworkingAPI.RegisterMessageType<Statistics.Sync>();

            Hooks();
        }

        private static void GenerateEquipment()
        {
            equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();

            equipmentDef.name = "SLINGSHOT";
            equipmentDef.nameToken = "SLINGSHOT_NAME";
            equipmentDef.pickupToken = "SLINGSHOT_PICKUP";
            equipmentDef.descriptionToken = "SLINGSHOT_DESCRIPTION";
            equipmentDef.loreToken = "SLINGSHOT_LORE";

            equipmentDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            equipmentDef.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();

            equipmentDef.appearsInMultiPlayer = true;
            equipmentDef.appearsInSinglePlayer = true;
            equipmentDef.canBeRandomlyTriggered = true;
            equipmentDef.enigmaCompatible = true;
            equipmentDef.canDrop = true;

            equipmentDef.cooldown = 30f;
        }

        private static void GenerateBuff()
        {
            slingshotTimer = ScriptableObject.CreateInstance<BuffDef>();

            slingshotTimer.name = "Slingshot Timer";
            slingshotTimer.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            slingshotTimer.canStack = false;
            slingshotTimer.isDebuff = false;
            slingshotTimer.isCooldown = true;
            slingshotTimer.isHidden = false;
        }

        public static void Hooks()
        {
            CharacterMaster.onStartGlobal += (obj) =>
            {
                obj.inventory?.gameObject.AddComponent<Statistics>();
            };

            On.RoR2.EquipmentSlot.PerformEquipmentAction += (orig, self, equipDef) =>
            {
                if (NetworkServer.active && equipDef == equipmentDef)
                {
                    return OnUse(self);
                }
                return orig(self, equipDef);
            };

            On.RoR2.CharacterBody.OnBuffFinalStackLost += (orig, self, buffDef) =>
            {
                orig(self, buffDef);

                if (buffDef == slingshotTimer)
                {
                    var stats = self.inventory.GetComponent<Statistics>();

                    // set new position
                    self.coreTransform.position = stats.SavedPosition;
                }
            };
        }

        private static bool OnUse(EquipmentSlot slot)
        {
            if (slot.characterBody.HasBuff(slingshotTimer)) return false;

            if (slot.characterBody.inventory)
            {
                var component = slot.characterBody.inventory.GetComponent<Statistics>();
                component.SavedPosition = slot.characterBody.footPosition;

                slot.characterBody.AddTimedBuff(slingshotTimer, slingshotDuration);
            }
            return true;
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("SLINGSHOT", "Slingshot");
            LanguageAPI.Add("SLINGSHOT_NAME", "Slingshot");
            LanguageAPI.Add("SLINGSHOT_PICKUP", "Mark your current location. After a duration, blink back.");

            string desc = $"On activation, mark your current location. After {slingshotDuration} seconds, blink back.";
            LanguageAPI.Add("SLINGSHOT_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("SLINGSHOT_LORE", lore);
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
