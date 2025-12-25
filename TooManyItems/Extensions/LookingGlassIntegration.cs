using LookingGlass.ItemStatsNameSpace;
using RoR2;
using System.Collections.Generic;
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
                // Ancient Coin
                if (AncientCoin.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Gold Gain: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Gold);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Damage Taken: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Death);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            AncientCoin.goldMultiplierAsPercent * itemCount,
                            AncientCoin.damageMultiplierAsPercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)AncientCoin.itemDef.itemIndex, stats);
                }


                // Blood Dice
                if (BloodDice.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Permanent Health: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Health);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.FlatHealth);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };
                        if (master && master.inventory && master.inventory.GetComponent<BloodDice.Statistics>())
                        {
                            var component = master.inventory.GetComponent<BloodDice.Statistics>();
                            values.Add(component.PermanentHealth);
                        }
                        else
                        {
                            values.Add(0f);
                        }
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)BloodDice.itemDef.itemIndex, stats);
                }


                // Bottle Cap
                if (BottleCap.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Cooldown Reduction: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            Utilities.GetHyperbolicStacking(BottleCap.specialCDRPercent, itemCount)
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)BottleCap.itemDef.itemIndex, stats);
                }

                // Brass Knuckles
                if (BrassKnuckles.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Bonus Damage: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            BrassKnuckles.heavyHitBonusPercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)BrassKnuckles.itemDef.itemIndex, stats);
                }


                // Bread
                if (BreadLoaf.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Gold On-Kill: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Gold);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.descriptions.Add("Kills Remaining: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };
                        if (master && master.inventory && master.inventory.GetComponent<BreadLoaf.Statistics>())
                        {
                            var component = master.inventory.GetComponent<BreadLoaf.Statistics>();

                            values.Add(BreadLoaf.goldGainOnKill.Value * itemCount);
                            values.Add(BreadLoaf.killsNeededToScrap - component.KillsCounter);
                        }
                        else
                        {
                            values.Add(BreadLoaf.goldGainOnKill.Value * itemCount);
                            values.Add(0f);
                        }
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)BreadLoaf.itemDef.itemIndex, stats);
                }


                // Broken Mask
                if (BrokenMask.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Burn Damage: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Damage Dealt: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };
                        if (master && master.inventory && master.inventory.GetComponent<BrokenMask.Statistics>())
                        {
                            var component = master.inventory.GetComponent<BrokenMask.Statistics>();

                            values.Add(BrokenMask.burnDamagePercent * itemCount);
                            values.Add(component.TotalDamageDealt);
                        }
                        else
                        {
                            values.Add(BrokenMask.burnDamagePercent * itemCount);
                            values.Add(0f);
                        }
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)BrokenMask.itemDef.itemIndex, stats);
                }


                // Carving Blade
                if (CarvingBlade.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("On-Hit Damage Cap: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Damage Dealt: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };
                        if (master && master.inventory && master.inventory.GetComponent<CarvingBlade.Statistics>())
                        {
                            var component = master.inventory.GetComponent<CarvingBlade.Statistics>();

                            values.Add(CarvingBlade.CalculateDamageCapPercent(itemCount));
                            values.Add(Mathf.Max(component.TotalDamageDealt, 0));
                        }
                        else
                        {
                            values.Add(CarvingBlade.CalculateDamageCapPercent(itemCount));
                            values.Add(0f);
                        }
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)CarvingBlade.itemDef.itemIndex, stats);
                }


                // Crucifix
                if (Crucifix.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Burn Damage: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Health);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.PercentHealth);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            Utilities.GetReverseExponentialStacking(Crucifix.maxHealthBurnAmountPercent, Crucifix.maxHealthBurnAmountReductionPercent, itemCount)
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)Crucifix.itemDef.itemIndex, stats);
                }


                // Debit Card
                if (DebitCard.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Rebate: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            Utilities.GetHyperbolicStacking(DebitCard.rebatePercent, itemCount)
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)DebitCard.itemDef.itemIndex, stats);
                }

                // Double Down
                if (DoubleDown.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Total DoT Damage: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Death);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            Utilities.GetReverseExponentialStacking(DoubleDown.upFrontDamagePercent, DoubleDown.upFrontDamageReductionPercent, itemCount)
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)DoubleDown.itemDef.itemIndex, stats);
                }


                // Edible Glue
                if (EdibleGlue.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Slow Range: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Meters);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            EdibleGlue.GetSlowRadius(itemCount)
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)EdibleGlue.itemDef.itemIndex, stats);
                }


                // Epinephrine
                if (Epinephrine.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Buff Duration: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Seconds);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            Epinephrine.buffDuration * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)Epinephrine.itemDef.itemIndex, stats);
                }


                // Fleece Hoodie
                if (Hoodie.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Duration Bonus: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Cooldown: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Seconds);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            Hoodie.durationIncreasePercent * itemCount,
                            Hoodie.CalculateHoodieCooldown(itemCount)
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)Hoodie.itemDef.itemIndex, stats);
                }


                // Glass Marbles
                if (GlassMarbles.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Base Damage: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };
                        if (master && master.GetBody())
                        {
                            values.Add(GlassMarbles.damagePerLevelPerStack * itemCount * master.GetBody().level);
                        }
                        else
                        {
                            values.Add(0f);
                        }
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)GlassMarbles.itemDef.itemIndex, stats);
                }


                // Holy Water
                if (HolyWater.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Experience Gained: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Health);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.PercentHealth);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            HolyWater.CalculateExperienceMultiplier(itemCount)
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)HolyWater.itemDef.itemIndex, stats);
                }


                // Horseshoe
                if (Horseshoe.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Health: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Health);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.FlatHealth);
                    stats.descriptions.Add("Base Damage: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.descriptions.Add("Attack Speed: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Crit Chance: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Crit Damage: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Armor: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Event);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.descriptions.Add("Regeneration: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Healing);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.FlatHealing);
                    stats.descriptions.Add("Shield: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.FlatHealth);
                    stats.descriptions.Add("Movement Speed: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var empty = new List<float> { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };

                        if (!master || !master.inventory || !master.GetBody()) return empty;

                        var values = new List<float> { };
                        var component = master.inventory.GetComponent<HorseshoeStatisticsHandler>();
                        if (component)
                        {
                            values.Add(Horseshoe.GetScaledValue(component.MaxHealthBonus, master.GetBody().level, itemCount));
                            values.Add(Horseshoe.GetScaledValue(component.BaseDamageBonus, master.GetBody().level, itemCount));
                            values.Add(Horseshoe.GetScaledValue(component.AttackSpeedPercentBonus, master.GetBody().level, itemCount));
                            values.Add(Horseshoe.GetScaledValue(component.CritChanceBonus, master.GetBody().level, itemCount) / 100f);
                            values.Add(Horseshoe.GetScaledValue(component.CritDamageBonus, master.GetBody().level, itemCount));
                            values.Add(Horseshoe.GetScaledValue(component.ArmorBonus, master.GetBody().level, itemCount));
                            values.Add(Horseshoe.GetScaledValue(component.RegenerationBonus, master.GetBody().level, itemCount));
                            values.Add(Horseshoe.GetScaledValue(component.ShieldBonus, master.GetBody().level, itemCount));
                            values.Add(Horseshoe.GetScaledValue(component.MoveSpeedPercentBonus, master.GetBody().level, itemCount));
                        }
                        else
                        {
                            values = empty;
                        }
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)Horseshoe.itemDef.itemIndex, stats);
                }


                // Iron Heart
                if (IronHeart.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("On-Hit Damage: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Health);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.PercentHealth);
                    stats.descriptions.Add("Damage Dealt: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };
                        if (master && master.inventory && master.inventory.GetComponent<IronHeart.Statistics>())
                        {
                            var component = master.inventory.GetComponent<IronHeart.Statistics>();

                            values.Add(IronHeart.multiplierPerStack * itemCount);
                            values.Add(component.TotalDamageDealt);
                        }
                        else
                        {
                            values.Add(IronHeart.multiplierPerStack * itemCount);
                            values.Add(0f);
                        }
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)IronHeart.itemDef.itemIndex, stats);
                }


                // Milk Carton
                if (MilkCarton.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Damage Reduction: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            Utilities.GetHyperbolicStacking(MilkCarton.eliteDamageReductionPercent, itemCount)
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)MilkCarton.itemDef.itemIndex, stats);
                }


                // Magnifying Glass
                if (MagnifyingGlass.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Analyze Chance: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Damage Bonus: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };
                        // Check if we can use luck
                        if (master)
                            values.Add(Utilities.GetChanceAfterLuck(MagnifyingGlass.analyzeChancePercent, master.luck));
                        else
                            values.Add(MagnifyingGlass.analyzeChancePercent);
                        values.Add(MagnifyingGlass.damageTakenBonusPercent * itemCount);

                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)MagnifyingGlass.itemDef.itemIndex, stats);
                }


                // Paper Plane
                if (PaperPlane.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Bonus Damage: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            PaperPlane.damageBonusPercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)PaperPlane.itemDef.itemIndex, stats);
                }


                // Permafrost
                if (Permafrost.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Freeze Chance: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Bonus Damage: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };
                        // Check if we can calculate using luck
                        if (master)
                            values.Add(Utilities.GetChanceAfterLuck(Utilities.GetHyperbolicStacking(Permafrost.freezeChancePercent, itemCount), master.luck));
                        else
                            values.Add(Utilities.GetHyperbolicStacking(Permafrost.freezeChancePercent, itemCount));

                        values.Add(Permafrost.frozenDamageMultiplierPercent * itemCount);
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)Permafrost.itemDef.itemIndex, stats);
                }


                // Photodiode
                if (Photodiode.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Max Attack Speed: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            Photodiode.maxAttackSpeedAllowedPercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)Photodiode.itemDef.itemIndex, stats);
                }


                if (PropellerHat.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Movement Speed: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            PropellerHat.movespeedBonusPercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)PropellerHat.itemDef.itemIndex, stats);
                }


                // Red-Blue Glasses
                if (RedBlueGlasses.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Crit Chance: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Crit Damage: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            RedBlueGlasses.critChancePercent * itemCount,
                            RedBlueGlasses.critDamagePercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)RedBlueGlasses.itemDef.itemIndex, stats);
                }


                // Rubber Ducky
                if (RubberDucky.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Bonus Armor: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Armor);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            RubberDucky.armorPerStack * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)RubberDucky.itemDef.itemIndex, stats);
                }


                // Rusted Trowel
                if (RustedTrowel.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Cooldown: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Seconds);
                    stats.descriptions.Add("Healing Done: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Healing);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.FlatHealth);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };

                        if (master && master.inventory && master.inventory.GetComponent<RustedTrowel.Statistics>())
                        {
                            var component = master.inventory.GetComponent<RustedTrowel.Statistics>();

                            values.Add(RustedTrowel.CalculateCooldownInSec(itemCount));
                            values.Add(component.TotalHealingDone);
                        }
                        else
                        {
                            values.Add(RustedTrowel.CalculateCooldownInSec(itemCount));
                            values.Add(0f);
                        }
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)RustedTrowel.itemDef.itemIndex, stats);
                }


                // Shadow Crest
                if (ShadowCrest.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Missing Health Regen: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Healing);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            Utilities.GetHyperbolicStacking(ShadowCrest.regenPerSecondPercent, itemCount)
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ShadowCrest.itemDef.itemIndex, stats);
                }


                // Soul Ring
                if (SoulRing.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Bonus Regeneration: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Healing);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.FlatHealing);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };

                        if (master && master.inventory && master.inventory.GetComponent<SoulRing.Statistics>())
                        {
                            var component = master.inventory.GetComponent<SoulRing.Statistics>();
                            values.Add(component.HealthRegen);
                        }
                        else
                        {
                            values.Add(0f);
                        }
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)SoulRing.itemDef.itemIndex, stats);
                }


                // Spirit Stone
                if (SpiritStone.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Permanent Shield: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.FlatHealth);
                    stats.descriptions.Add("Health Penalty: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Health);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.PercentHealth);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };

                        if (master && master.inventory && master.inventory.GetComponent<SpiritStone.Statistics>())
                        {
                            var component = master.inventory.GetComponent<SpiritStone.Statistics>();
                            values.Add(component.PermanentShield);
                        }
                        else
                        {
                            values.Add(0f);
                        }
                        values.Add(Utilities.GetExponentialStacking(SpiritStone.maxHealthLostPercent, itemCount));

                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)SpiritStone.itemDef.itemIndex, stats);
                }


                // Thumbtack
                if (Thumbtack.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Bleed Chance: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Bonus Duration: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Seconds);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };

                        if (master)
                            values.Add(Utilities.GetChanceAfterLuck(Thumbtack.bleedChancePercent * itemCount, master.luck));
                        else
                            values.Add(Thumbtack.bleedChancePercent * itemCount);
                        values.Add(Thumbtack.bleedDurationBonus.Value * itemCount);

                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)Thumbtack.itemDef.itemIndex, stats);
                }


                // Void Heart
                if (IronHeartVoid.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Base Damage: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };
                        if (master && master.GetBody())
                        {
                            values.Add(IronHeartVoid.CalculateDamageBonus(master.GetBody(), itemCount));
                        }
                        else
                        {
                            values.Add(0f);
                        }
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)IronHeartVoid.itemDef.itemIndex, stats);
                }
            }
        }
    }
}
