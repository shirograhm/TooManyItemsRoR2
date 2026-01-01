using LookingGlass.ItemStatsNameSpace;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using TooManyItems.Handlers;
using TooManyItems.Items.Lunar;
using TooManyItems.Items.Tier1;
using TooManyItems.Items.Tier2;
using TooManyItems.Items.Tier3;
using TooManyItems.Items.Void;
using UnityEngine;

namespace TooManyItems.Extensions
{
    internal static class LookingGlassIntegration
    {
        internal static void Init()
        {
            RoR2Application.onLoad += LookingGlassStats.RegisterStats;
        }

        public static class LookingGlassStats
        {
            public static void RegisterStats()
            {
                if (Amnesia.isEnabled.Value)
                {
                    RegisterStatsForItem(Amnesia.itemDef, [
                        new("Lives Left: ", ItemStatsDef.ValueType.Healing, ItemStatsDef.MeasurementUnits.Number)
                        ], (master, count) =>
                        {
                            return [count];
                        });
                    RegisterStatsForItem(Amnesia.depletedDef, [
                        new("Lives Lived: ", ItemStatsDef.ValueType.Death, ItemStatsDef.MeasurementUnits.Number)
                        ], (master, count) =>
                        {
                            return [count];
                        });
                }

                if (AncientCoin.isEnabled.Value)
                    RegisterStatsForItem(AncientCoin.itemDef, [
                        new("Gold Gain: ", ItemStatsDef.ValueType.Gold, ItemStatsDef.MeasurementUnits.Percentage),
                        new("Damage Taken: ", ItemStatsDef.ValueType.Death, ItemStatsDef.MeasurementUnits.Percentage)
                        ], (master, itemCount) =>
                        {
                            return [AncientCoin.goldMultiplierAsPercent * itemCount, AncientCoin.damageMultiplierAsPercent * itemCount];
                        });
                if (BloodDice.isEnabled.Value)
                    RegisterStatsForItem(BloodDice.itemDef, [
                        new("Permanent Health: ", ItemStatsDef.ValueType.Health, ItemStatsDef.MeasurementUnits.FlatHealth)
                        ], (master, itemCount) =>
                        {
                            if (!master || !master.inventory || !master.inventory.GetComponent<BloodDice.Statistics>())
                                return [BloodDice.CalculateMaxHealthCap(itemCount)];

                            return [master.inventory.GetComponent<BloodDice.Statistics>().PermanentHealth];
                        });
                if (BottleCap.isEnabled.Value)
                    RegisterStatsForItem(BottleCap.itemDef, [
                        new("Cooldown Reduction: ", ItemStatsDef.ValueType.Utility, ItemStatsDef.MeasurementUnits.Percentage)
                        ], (master, itemCount) =>
                        {
                            return [Utilities.GetHyperbolicStacking(BottleCap.percentSpecialCDR, BottleCap.percentSpecialCDRExtraStacks, itemCount)];
                        });
                if (BrassKnuckles.isEnabled.Value)
                    RegisterStatsForItem(BrassKnuckles.itemDef, [
                        new("Bonus Damage: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Percentage)
                        ], (master, itemCount) =>
                        {
                            return [Utilities.GetLinearStacking(BrassKnuckles.percentHeavyHitBonus, BrassKnuckles.percentHeavyHitBonusExtraStacks, itemCount)];
                        });
                if (BreadLoaf.isEnabled.Value)
                    RegisterStatsForItem(BreadLoaf.itemDef, [
                        new("Gold On-Kill: ", ItemStatsDef.ValueType.Gold, ItemStatsDef.MeasurementUnits.Number),
                        new("Kills Remaining: ", ItemStatsDef.ValueType.Utility, ItemStatsDef.MeasurementUnits.Number)
                        ], (master, itemCount) =>
                        {
                            List<float> values = [Utilities.GetLinearStacking(BreadLoaf.goldGainOnKill.Value, itemCount)];

                            if (master && master.inventory && master.inventory.GetComponent<BreadLoaf.Statistics>())
                                values.Add(BreadLoaf.killsNeededToScrap - master.inventory.GetComponent<BreadLoaf.Statistics>().KillsCounter);
                            else
                                values.Add(0f);

                            return values;
                        });
                if (BrokenMask.isEnabled.Value)
                    RegisterStatsForItem(BrokenMask.itemDef, [
                        new("Burn Damage: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Percentage),
                        new("Damage Dealt: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Number)
                        ], (master, itemCount) =>
                        {
                            List<float> values = [Utilities.GetLinearStacking(BrokenMask.percentBurnDamage, BrokenMask.percentBurnDamageExtraStacks, itemCount)];

                            if (master && master.inventory && master.inventory.GetComponent<BrokenMask.Statistics>())
                                values.Add(master.inventory.GetComponent<BrokenMask.Statistics>().TotalDamageDealt);
                            else
                                values.Add(0f);

                            return values;
                        });
                if (CarvingBlade.isEnabled.Value)
                    RegisterStatsForItem(CarvingBlade.itemDef, [
                        new("On-Hit Damage Cap: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Percentage),
                        new("Damage Dealt: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Number)
                        ], (master, itemCount) =>
                        {
                            List<float> values = [CarvingBlade.CalculateDamageCapPercent(itemCount)];

                            if (master && master.inventory && master.inventory.GetComponent<CarvingBlade.Statistics>())
                                values.Add(Mathf.Max(master.inventory.GetComponent<CarvingBlade.Statistics>().TotalDamageDealt, 0));
                            else
                                values.Add(0f);

                            return values;
                        });
                if (Crucifix.isEnabled.Value)
                    RegisterStatsForItem(Crucifix.itemDef, [
                        new("Burn Damage: ", ItemStatsDef.ValueType.Health, ItemStatsDef.MeasurementUnits.PercentHealth)
                        ], (master, itemCount) =>
                        {
                            return [Utilities.GetReverseExponentialStacking(Crucifix.percentMaxHealthBurnAmount, Crucifix.percentMaxHealthBurnAmountReduction, itemCount)];
                        });
                if (DebitCard.isEnabled.Value)
                    RegisterStatsForItem(DebitCard.itemDef, [
                        new("Rebate: ", ItemStatsDef.ValueType.Utility, ItemStatsDef.MeasurementUnits.Percentage)
                        ], (master, itemCount) =>
                        {
                            return [Utilities.GetHyperbolicStacking(DebitCard.percentRebate, DebitCard.percentRebateExtraStacks, itemCount)];
                        });
                if (DoubleDown.isEnabled.Value)
                    RegisterStatsForItem(DoubleDown.itemDef, [
                        new("Total DoT Damage: ", ItemStatsDef.ValueType.Death, ItemStatsDef.MeasurementUnits.Percentage)
                        ], (master, itemCount) =>
                        {
                            return [Utilities.GetReverseExponentialStacking(DoubleDown.upFrontDamagePercent, DoubleDown.upFrontDamageReductionPercent, itemCount)];
                        });
                if (EdibleGlue.isEnabled.Value)
                    RegisterStatsForItem(EdibleGlue.itemDef, [
                        new("Slow Range: ", ItemStatsDef.ValueType.Utility, ItemStatsDef.MeasurementUnits.Meters)
                        ], (master, itemCount) =>
                        {
                            return [EdibleGlue.GetSlowRadius(itemCount)];
                        });
                if (Epinephrine.isEnabled.Value)
                    RegisterStatsForItem(Epinephrine.itemDef, [
                        new("Buff Duration: ", ItemStatsDef.ValueType.Utility, ItemStatsDef.MeasurementUnits.Seconds)
                        ], (master, itemCount) =>
                        {
                            return [Utilities.GetLinearStacking(Epinephrine.buffDuration.Value, itemCount)];
                        });
                if (Hoodie.isEnabled.Value)
                    RegisterStatsForItem(Hoodie.itemDef, [
                        new("Duration Bonus: ", ItemStatsDef.ValueType.Utility, ItemStatsDef.MeasurementUnits.Percentage),
                        new("Cooldown: ", ItemStatsDef.ValueType.Utility, ItemStatsDef.MeasurementUnits.Seconds)
                        ], (master, itemCount) =>
                        {
                            return [Utilities.GetLinearStacking(Hoodie.durationIncreasePercent, itemCount), Hoodie.CalculateHoodieCooldown(itemCount)];
                        });
                if (GlassMarbles.isEnabled.Value)
                    RegisterStatsForItem(GlassMarbles.itemDef, [
                        new("Base Damage: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Number)
                        ], (master, itemCount) =>
                        {
                            List<float> values = [];

                            if (master && master.GetBody())
                                values.Add(Utilities.GetLinearStacking(GlassMarbles.damagePerLevelPerStack.Value, GlassMarbles.damagePerLevelPerExtraStack.Value, itemCount) * master.GetBody().level);
                            else
                                values.Add(Utilities.GetLinearStacking(GlassMarbles.damagePerLevelPerStack.Value, GlassMarbles.damagePerLevelPerExtraStack.Value, itemCount));

                            return values;
                        });
                if (HolyWater.isEnabled.Value)
                    RegisterStatsForItem(HolyWater.itemDef, [
                        new("Experience Gained: ", ItemStatsDef.ValueType.Health, ItemStatsDef.MeasurementUnits.PercentHealth)
                        ], (master, itemCount) =>
                        {
                            return [HolyWater.CalculateExperienceMultiplier(itemCount)];
                        });
                if (Horseshoe.isEnabled.Value)
                    RegisterStatsForItem(Horseshoe.itemDef, [
                        new("Health: ", ItemStatsDef.ValueType.Health, ItemStatsDef.MeasurementUnits.FlatHealth),
                        new("Base Damage: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Number),
                        new("Attack Speed: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Percentage),
                        new("Crit Chance: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Percentage),
                        new("Crit Damage: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Percentage),
                        new("Armor: ", ItemStatsDef.ValueType.Event, ItemStatsDef.MeasurementUnits.Number),
                        new("Regeneration: ", ItemStatsDef.ValueType.Healing, ItemStatsDef.MeasurementUnits.FlatHealing),
                        new("Shield: ", ItemStatsDef.ValueType.Utility, ItemStatsDef.MeasurementUnits.FlatHealth),
                        new("Movement Speed: ", ItemStatsDef.ValueType.Utility, ItemStatsDef.MeasurementUnits.Percentage)
                        ], (master, itemCount) =>
                        {
                            List<float> values = [];
                            List<float> empty = [.. Enumerable.Repeat(0f, 9)];

                            if (!master || !master.inventory || !master.GetBody() || !master.inventory.GetComponent<HorseshoeStatisticsHandler>()) return empty;

                            HorseshoeStatisticsHandler component = master.inventory.GetComponent<HorseshoeStatisticsHandler>();
                            values.Add(Horseshoe.GetScaledValue(component.MaxHealthBonus, master.GetBody().level, itemCount));
                            values.Add(Horseshoe.GetScaledValue(component.BaseDamageBonus, master.GetBody().level, itemCount));
                            values.Add(Horseshoe.GetScaledValue(component.AttackSpeedPercentBonus, master.GetBody().level, itemCount));
                            values.Add(Horseshoe.GetScaledValue(component.CritChanceBonus, master.GetBody().level, itemCount) / 100f);
                            values.Add(Horseshoe.GetScaledValue(component.CritDamageBonus, master.GetBody().level, itemCount));
                            values.Add(Horseshoe.GetScaledValue(component.ArmorBonus, master.GetBody().level, itemCount));
                            values.Add(Horseshoe.GetScaledValue(component.RegenerationBonus, master.GetBody().level, itemCount));
                            values.Add(Horseshoe.GetScaledValue(component.ShieldBonus, master.GetBody().level, itemCount));
                            values.Add(Horseshoe.GetScaledValue(component.MoveSpeedPercentBonus, master.GetBody().level, itemCount));

                            return values;
                        });

                if (IronHeart.isEnabled.Value)
                    RegisterStatsForItem(IronHeart.itemDef, [
                        new("On-Hit Damage: ", ItemStatsDef.ValueType.Health, ItemStatsDef.MeasurementUnits.PercentHealth),
                        new("Damage Dealt: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Number)
                        ], (master, itemCount) =>
                        {
                            List<float> values = [Utilities.GetLinearStacking(IronHeart.multiplierPerStack, IronHeart.multiplierPerExtraStack, itemCount)];
                            if (master && master.inventory && master.inventory.GetComponent<IronHeart.Statistics>())
                                values.Add(master.inventory.GetComponent<IronHeart.Statistics>().TotalDamageDealt);
                            else
                                values.Add(0f);

                            return values;
                        });
                if (MilkCarton.isEnabled.Value)
                    RegisterStatsForItem(MilkCarton.itemDef, [
                        new("Damage Reduction: ", ItemStatsDef.ValueType.Utility, ItemStatsDef.MeasurementUnits.Percentage)
                        ], (master, itemCount) =>
                        {
                            return [Utilities.GetHyperbolicStacking(MilkCarton.percentEliteDamageReduction, MilkCarton.percentEliteDamageReductionExtraStacks, itemCount)];
                        });
                if (MagnifyingGlass.isEnabled.Value)
                    RegisterStatsForItem(MagnifyingGlass.itemDef, [
                        new("Analyze Chance: ", ItemStatsDef.ValueType.Utility, ItemStatsDef.MeasurementUnits.Percentage),
                        new("Damage Bonus: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Percentage)
                        ], (master, itemCount) =>
                        {
                            return
                            [
                                // Use luck from master if possible
                                master ? Utilities.GetChanceAfterLuck(MagnifyingGlass.percentAnalyzeChance, master.luck) : MagnifyingGlass.percentAnalyzeChance,
                                Utilities.GetLinearStacking(MagnifyingGlass.percentDamageTakenBonus, itemCount)
                            ];
                        });
                if (PaperPlane.isEnabled.Value)
                    RegisterStatsForItem(PaperPlane.itemDef, [
                        new("Bonus Damage: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Percentage)
                        ], (master, itemCount) =>
                        {
                            return [Utilities.GetLinearStacking(PaperPlane.percentDamageBonus, PaperPlane.percentDamageBonusExtraStacks, itemCount)];
                        });
                if (Permafrost.isEnabled.Value)
                    RegisterStatsForItem(Permafrost.itemDef, [
                        new("Freeze Chance: ", ItemStatsDef.ValueType.Utility, ItemStatsDef.MeasurementUnits.Percentage),
                        new("Bonus Damage: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Percentage)
                        ], (master, itemCount) =>
                        {
                            return
                            [
                                // Check if we can calculate using luck
                                master ? Utilities.GetChanceAfterLuck(Permafrost.freezeChancePercent, master.luck) : Permafrost.freezeChancePercent,
                                Utilities.GetLinearStacking(Permafrost.frozenDamageMultiplierPercent, Permafrost.frozenDamageMultiplierExtraStacksPercent, itemCount),
                            ];
                        });
                if (Photodiode.isEnabled.Value)
                    RegisterStatsForItem(Photodiode.itemDef, [
                        new("Max Attack Speed: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Percentage)
                        ], (master, itemCount) =>
                        {
                            return [Utilities.GetLinearStacking(Photodiode.maxAttackSpeedAllowedPercent, itemCount)];
                        });
                if (PropellerHat.isEnabled.Value)
                    RegisterStatsForItem(PropellerHat.itemDef, [
                        new("Movement Speed: ", ItemStatsDef.ValueType.Utility, ItemStatsDef.MeasurementUnits.Percentage)
                        ], (master, itemCount) =>
                        {
                            return [Utilities.GetLinearStacking(PropellerHat.percentMovespeedBonus, PropellerHat.percentMovespeedBonusExtraStacks, itemCount)];
                        });
                if (RedBlueGlasses.isEnabled.Value)
                    RegisterStatsForItem(RedBlueGlasses.itemDef, [
                        new("Crit Chance: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Percentage),
                        new("Crit Damage: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Percentage)
                        ], (master, itemCount) =>
                        {
                            return
                            [
                                Utilities.GetLinearStacking(RedBlueGlasses.percentCritChance, RedBlueGlasses.percentCritChanceExtraStacks, itemCount),
                                Utilities.GetLinearStacking(RedBlueGlasses.percentCritDamage, RedBlueGlasses.percentCritDamageExtraStacks, itemCount)
                            ];
                        });
                if (RubberDucky.isEnabled.Value)
                    RegisterStatsForItem(RubberDucky.itemDef, [
                        new("Bonus Armor: ", ItemStatsDef.ValueType.Armor, ItemStatsDef.MeasurementUnits.Number)
                        ], (master, itemCount) =>
                        {
                            return [Utilities.GetLinearStacking(RubberDucky.armorPerStack.Value, RubberDucky.armorPerExtraStack.Value, itemCount)];
                        });
                if (RustedTrowel.isEnabled.Value)
                    RegisterStatsForItem(RustedTrowel.itemDef, [
                        new("Cooldown: ", ItemStatsDef.ValueType.Utility, ItemStatsDef.MeasurementUnits.Seconds),
                        new("Healing Done: ", ItemStatsDef.ValueType.Healing, ItemStatsDef.MeasurementUnits.FlatHealth)
                        ], (master, itemCount) =>
                        {
                            List<float> values = [RustedTrowel.CalculateCooldownInSec(itemCount)];

                            if (master && master.inventory && master.inventory.GetComponent<RustedTrowel.Statistics>())
                                values.Add(master.inventory.GetComponent<RustedTrowel.Statistics>().TotalHealingDone);
                            else
                                values.Add(RustedTrowel.healingPerStack.Value);

                            return values;
                        });
                if (ShadowCrest.isEnabled.Value)
                    RegisterStatsForItem(ShadowCrest.itemDef, [
                        new("Missing Health Regen: ", ItemStatsDef.ValueType.Healing, ItemStatsDef.MeasurementUnits.Percentage)
                        ], (master, itemCount) =>
                        {
                            return [Utilities.GetHyperbolicStacking(ShadowCrest.percentRegenPerSecond, ShadowCrest.percentRegenPerSecondExtraStacks, itemCount)];
                        });
                if (SoulRing.isEnabled.Value)
                    RegisterStatsForItem(SoulRing.itemDef, [
                        new("Bonus Regen: ", ItemStatsDef.ValueType.Healing, ItemStatsDef.MeasurementUnits.FlatHealing)
                        ], (master, itemCount) =>
                        {
                            List<float> values = [];

                            if (master && master.inventory && master.inventory.GetComponent<SoulRing.Statistics>())
                                values.Add(master.inventory.GetComponent<SoulRing.Statistics>().HealthRegen);
                            else
                                values.Add(Utilities.GetLinearStacking(SoulRing.maxRegenOnFirstStack.Value, SoulRing.maxRegenForExtraStacks.Value, itemCount));

                            return values;
                        });
                if (SpiritStone.isEnabled.Value)
                    RegisterStatsForItem(SpiritStone.itemDef, [
                        new("Health Penalty: ", ItemStatsDef.ValueType.Health, ItemStatsDef.MeasurementUnits.PercentHealth),
                        new("Permanent Shield: ", ItemStatsDef.ValueType.Utility, ItemStatsDef.MeasurementUnits.FlatHealth)
                        ], (master, itemCount) =>
                        {
                            List<float> values = [Utilities.GetExponentialStacking(SpiritStone.maxHealthLostPercent, SpiritStone.maxHealthLostExtraStackPercent, itemCount)];

                            if (master && master.inventory && master.inventory.GetComponent<SpiritStone.Statistics>())
                                values.Add(master.inventory.GetComponent<SpiritStone.Statistics>().PermanentShield);
                            else
                                // Show current stack value if we can't get the actual value
                                values.Add(Utilities.GetLinearStacking(SpiritStone.shieldPerKill.Value, itemCount));

                            return values;
                        });
                if (Thumbtack.isEnabled.Value)
                    RegisterStatsForItem(Thumbtack.itemDef, [
                        new("Bleed Chance: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Percentage),
                        new("Bonus Duration: ", ItemStatsDef.ValueType.Utility, ItemStatsDef.MeasurementUnits.Seconds)
                        ], (master, itemCount) =>
                        {
                            float baseChance = Utilities.GetLinearStacking(Thumbtack.percentBleedChance, Thumbtack.percentBleedChanceExtraStacks, itemCount);
                            return
                            [
                                master ? Utilities.GetChanceAfterLuck(baseChance, master.luck) : baseChance,
                                Utilities.GetLinearStacking(Thumbtack.bleedDurationBonus.Value, Thumbtack.bleedDurationBonusExtraStacks.Value, itemCount),
                            ];
                        });
                if (VoidHeart.isEnabled.Value)
                    RegisterStatsForItem(VoidHeart.itemDef, [
                        new("Base Damage: ", ItemStatsDef.ValueType.Damage, ItemStatsDef.MeasurementUnits.Number)
                        ], (master, itemCount) =>
                        {
                            return [master && master.GetBody() ? VoidHeart.CalculateDamageBonus(master.GetBody(), itemCount) : 0f];
                        });
            }

            private static void RegisterStatsForItem(ItemDef itemDef, List<ItemStatLine> statLines, Func<CharacterMaster, int, List<float>> func)
            {
                if (!itemDef) throw new ArgumentNullException(nameof(itemDef));

                ItemStatsDef stats = new();
                foreach (ItemStatLine line in statLines)
                {
                    stats.descriptions.Add(line.Name);
                    stats.valueTypes.Add(line.ValueType);
                    stats.measurementUnits.Add(line.Units);
                }
                stats.calculateValues = func;
                ItemDefinitions.allItemDefinitions.Add((int)itemDef.itemIndex, stats);
            }

            private readonly struct ItemStatLine(string n, ItemStatsDef.ValueType v, ItemStatsDef.MeasurementUnits u)
            {
                public string Name { get; } = n;
                public ItemStatsDef.ValueType ValueType { get; } = v;
                public ItemStatsDef.MeasurementUnits Units { get; } = u;
            }
        }
    }
}
