using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class BrokenMask
    {
        public static ItemDef itemDef;
        public static BuffDef burnDebuff;
        private static DotController.DotDef burnDotDef;
        private static DotController.DotIndex burnIndex;

        public static DamageAPI.ModdedDamageType damageType;
        public static DamageColorIndex maskDamageColor = DamageColorAPI.RegisterDamageColor(Utils.BROKEN_MASK_COLOR);

        // Dealing damage burns enemies for a portion of their max health.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Broken Mask",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_BROKENMASK_DESC"
            }
        );
        public static ConfigurableValue<float> burnDamage = new(
            "Item: Broken Mask",
            "Percent Burn",
            2f,
            "Burn damage dealt over the duration as a percentage of enemy max health.",
            new List<string>()
            {
                "ITEM_BROKENMASK_DESC"
            }
        );
        public static ConfigurableValue<int> burnDuration = new(
            "Item: Broken Mask",
            "Burn Duration",
            5,
            "Total duration of the burn in seconds.",
            new List<string>()
            {
                "ITEM_BROKENMASK_DESC"
            }
        );
        public static float burnDamagePercent = burnDamage.Value / 100f;
        public static float burnTickInterval = 0.5f;

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
            GenerateBuff();
            GenerateDot();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            ContentAddition.AddBuffDef(burnDebuff);
            NetworkingAPI.RegisterMessageType<Statistics.Sync>();

            burnIndex = DotAPI.RegisterDotDef(burnDotDef, BurnBehavior);
            damageType = DamageAPI.ReserveDamageType();

            Hooks();
        }

        private static void BurnBehavior(DotController self, DotController.DotStack dotStack)
        {
            if (dotStack.attackerObject)
            {
                CharacterBody attackerBody = dotStack.attackerObject.GetComponent<CharacterBody>();

                int count = 1;
                if (attackerBody && attackerBody.inventory)
                {
                    count = attackerBody.inventory.GetItemCount(itemDef);
                }

                float burnPercentPerTick = burnDamagePercent * burnTickInterval / burnDuration.Value;
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                dotStack.damage = self.victimBody.healthComponent.fullCombinedHealth * burnPercentPerTick * count;
#pragma warning restore Publicizer001 // Accessing a member that was not originally public

                // Damage calculation takes minions into account
                if (attackerBody && attackerBody.master && attackerBody.master.minionOwnership && attackerBody.master.minionOwnership.ownerMaster)
                {
                    if (attackerBody.master.minionOwnership.ownerMaster.GetBody())
                    {
                        attackerBody = attackerBody.master.minionOwnership.ownerMaster.GetBody();
                    }
                }

                var stats = attackerBody.inventory.GetComponent<Statistics>();
                stats.TotalDamageDealt += dotStack.damage;
            }
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "BROKENMASK";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier2);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("BrokenMask.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("BrokenMask.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage
            };
        }

        private static void GenerateBuff()
        {
            burnDebuff = ScriptableObject.CreateInstance<BuffDef>();

            burnDebuff.name = "Torment";
            burnDebuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("MaskDebuff.png");
            burnDebuff.canStack = false;
            burnDebuff.isHidden = false;
            burnDebuff.isDebuff = true;
            burnDebuff.isCooldown = false;
        }

        private static void GenerateDot()
        {
            burnDotDef = new DotController.DotDef
            {
                damageColorIndex = maskDamageColor,
                associatedBuff = burnDebuff,
                terminalTimedBuff = null,
                terminalTimedBuffDuration = 0,
                resetTimerOnAdd = true,
                interval = burnTickInterval
            };
        }

        public static void Hooks()
        {
            CharacterMaster.onStartGlobal += (obj) =>
            {
                obj.inventory?.gameObject.AddComponent<Statistics>();
            };

            GenericGameEvents.OnTakeDamage += (damageReport) =>
            {
                if (damageReport.attackerBody == null || damageReport.victimBody == null) return;

                CharacterBody vicBody = damageReport.victimBody;
                CharacterBody atkBody = damageReport.attackerBody;

                if (atkBody.inventory)
                {
                    int count = atkBody.inventory.GetItemCount(itemDef);
                    if (count > 0 && damageReport.dotType != burnIndex && vicBody != atkBody)
                    {
                        InflictDotInfo dotInfo = new()
                        {
                            victimObject = vicBody.gameObject,
                            attackerObject = atkBody.gameObject,
                            totalDamage = null,
                            dotIndex = burnIndex,
                            duration = burnDuration.Value,
                            maxStacksFromAttacker = 1,
                            damageMultiplier = 1f
                        };
                        DotController.InflictDot(ref dotInfo);
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