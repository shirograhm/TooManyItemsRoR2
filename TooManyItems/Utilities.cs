using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Orbs;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    public static class Utilities
    {
        public static Color BROKEN_MASK_COLOR = new(0.38f, 0.38f, 0.82f, 1f);
        public static Color CARVING_BLADE_COLOR = new(0.09f, 0.67f, 0.62f, 1f);
        public static Color PERMAFROST_COLOR = new(0.76f, 0.80f, 0.98f, 1f);
        public static Color IRON_HEART_COLOR = new(0.44f, 0.44f, 0.44f, 1f);
        public static Color TATTERED_SCROLL_COLOR = new(0.80f, 0.78f, 0.57f, 1f);
        public static Color VANITY_COLOR = new(0.53f, 0.44f, 0.77f, 1f);

        internal static void Init()
        {
            NetworkingAPI.RegisterMessageType<SyncForceRecalculate>();
        }

        private class SyncForceRecalculate : INetMessage
        {
            NetworkInstanceId netID;

            public SyncForceRecalculate() { }
            public SyncForceRecalculate(NetworkInstanceId ID)
            {
                this.netID = ID;
            }

            public void Deserialize(NetworkReader reader)
            {
                netID = reader.ReadNetworkId();
            }

            public void OnReceived()
            {
                if (NetworkServer.active) return;

                GameObject obj = RoR2.Util.FindNetworkObject(netID);
                if (obj)
                {
                    CharacterBody body = obj.GetComponent<CharacterBody>();
                    if (body) body.RecalculateStats();
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(netID);
                writer.FinishMessage();
            }
        }

        public static void ForceRecalculate(CharacterBody body)
        {
            body.RecalculateStats();
            if (NetworkServer.active) new SyncForceRecalculate(body.netId);
        }

        public static void AddRecalculateOnFrameHook(ItemDef def)
        {
            On.RoR2.CharacterBody.FixedUpdate += (orig, self) =>
            {
                orig(self);

                if (self && self.inventory)
                {
                    int count = self.inventory.GetItemCountEffective(def);
                    if (count > 0)
                    {
                        ForceRecalculate(self);
                    }
                }
            };
        }

        public static CharacterBody GetMinionOwnershipParentBody(CharacterBody body)
        {
            if (body && body.master && body.master.minionOwnership && body.master.minionOwnership.ownerMaster && body.master.minionOwnership.ownerMaster.GetBody())
            {
                return body.master.minionOwnership.ownerMaster.GetBody();
            }
            return body;
        }

        public static bool OnSameTeam(CharacterBody body1, CharacterBody body2)
        {
            if (body1 == null) throw new ArgumentNullException("body1");
            if (body2 == null) throw new ArgumentNullException("body2");
            return body1.teamComponent && body2.teamComponent && body1.teamComponent.teamIndex == body2.teamComponent.teamIndex;
        }

        public static bool IsSkillDamage(DamageInfo damageInfo)
        {
            return damageInfo.damageType == DamageTypeCombo.GenericPrimary || damageInfo.damageType == DamageTypeCombo.GenericSecondary || damageInfo.damageType == DamageTypeCombo.GenericUtility || damageInfo.damageType == DamageTypeCombo.GenericSpecial;
        }

        public static uint ScaleGoldWithDifficulty(int goldGranted)
        {
            return Convert.ToUInt32(goldGranted * (1 + 50 * GetDifficultyAsPercentage()));
        }

        public static float GetChanceAfterLuck(float percent, float luckIn)
        {
            int luck = Mathf.CeilToInt(luckIn);

            if (luck > 0)
                return 1f - Mathf.Pow(1f - percent, luck + 1);
            if (luck < 0)
                return Mathf.Pow(percent, Mathf.Abs(luck) + 1);

            return percent;
        }

        public static float GetDifficultyAsPercentage()
        {
            return (Stage.instance.entryDifficultyCoefficient - 1f) / 98f;
        }

        public static float GetDifficultyAsMultiplier()
        {
            return Stage.instance.entryDifficultyCoefficient;
        }

        public static float GetLinearStacking(float baseValue, int count)
        {
            return GetLinearStacking(baseValue, baseValue, count);
        }

        public static float GetLinearStacking(float baseValue, float extraValue, int count)
        {
            return baseValue + extraValue * (count - 1);
        }

        public static float GetExponentialStacking(float percent, int count)
        {
            return GetExponentialStacking(percent, percent, count);
        }

        public static float GetExponentialStacking(float percent, float stackPercent, int count)
        {
            return 1f - (1 - percent) * Mathf.Pow(1f - stackPercent, count - 1);
        }

        public static float GetReverseExponentialStacking(float baseValue, float reducePercent, int count)
        {
            return baseValue * Mathf.Pow(1 - reducePercent, count - 1);
        }

        public static float GetHyperbolicStacking(float percent, int count)
        {
            return GetHyperbolicStacking(percent, percent, count);
        }

        public static float GetHyperbolicStacking(float percent, float extraPercent, int count)
        {
            float denominator = (1f + percent) * (1 + extraPercent * (count - 1));
            return 1f - 1f / denominator;
        }

        public static void SpawnHealEffect(CharacterBody self)
        {
            EffectData effectData = new()
            {
                origin = self.transform.position,
                rootObject = self.gameObject
            };
            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/MedkitHealEffect"), effectData, transmit: true);
        }

        public static void SendGoldOrbAndEffect(uint goldGain, Vector3 origin, HurtBox target)
        {
            OrbManager.instance.AddOrb(new GoldOrb()
            {
                goldAmount = goldGain,
                origin = origin,
                target = target,
            });
            EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/CoinImpact"), origin, Vector3.up, transmit: true);
        }

        public static bool IsItemTierRandomizable(ItemTier tier)
        {
            return tier == ItemTier.Tier1 || tier == ItemTier.Tier2 || tier == ItemTier.Tier3 || tier == ItemTier.Lunar;
        }

        public static ItemIndex GetRandomItemOfTier(ItemTier tier)
        {
            if (!IsItemTierRandomizable(tier)) throw new Exception("Invalid tier called for random item.");

            switch (tier)
            {
                case ItemTier.Tier1:
                    var arrayNoScrap = ItemCatalog.tier1ItemList.Where(index => index != RoR2Content.Items.ScrapWhite.itemIndex).ToArray();
                    int randomIndex = UnityEngine.Random.Range(0, arrayNoScrap.Count());
                    return arrayNoScrap[randomIndex];
                case ItemTier.Tier2:
                    var arrayNoScrap2 = ItemCatalog.tier2ItemList.Where(index => index != RoR2Content.Items.ScrapGreen.itemIndex).ToArray();
                    int randomIndex2 = UnityEngine.Random.Range(0, arrayNoScrap2.Count());
                    return arrayNoScrap2[randomIndex2];
                case ItemTier.Tier3:
                    var arrayNoScrap3 = ItemCatalog.tier3ItemList.Where(index => index != RoR2Content.Items.ScrapRed.itemIndex).ToArray();
                    int randomIndex3 = UnityEngine.Random.Range(0, arrayNoScrap3.Count());
                    return arrayNoScrap3[randomIndex3];
                case ItemTier.Lunar:
                    int randomIndexLunar = UnityEngine.Random.Range(0, ItemCatalog.lunarItemList.Count);
                    return ItemCatalog.lunarItemList[randomIndexLunar];
                default:
                    Log.Error("Invalid tier called for random item.");
                    return ItemIndex.None;
            }
        }
    }
}
