using LookingGlass.ItemStatsNameSpace;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
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
                    stats.valueTypes.Add(ItemStatsDef.ValueType.HumanObjective);
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
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Health);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };
                        if (master && master.inventory)
                        {
                            var component = master.inventory.GetComponent<BloodDice.Statistics>();
                            if (component)
                            {
                                values.Add(component.PermanentHealth);
                            }
                            else
                            {
                                values.Add(0f);
                            }
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
                            Utils.GetHyperbolicStacking(BottleCap.ultimateCDRPercent, itemCount)
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)BottleCap.itemDef.itemIndex, stats);
                }

                // Bread
                if (BreadLoaf.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Healing On-Kill: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Healing);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            BreadLoaf.healthGainOnKillPercent * itemCount
                        };
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
                        if (master && master.inventory)
                        {
                            values.Add(BrokenMask.burnDamagePercent * itemCount);

                            var component = master.inventory.GetComponent<BrokenMask.Statistics>();
                            if (component)
                            {
                                values.Add(component.TotalDamageDealt);
                            }
                            else
                            {
                                values.Add(0f);
                            }
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
                    stats.descriptions.Add("Damage On-Hit: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Damage Dealt: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> {  };
                        if (master && master.inventory)
                        {
                            values.Add(Utils.GetHyperbolicStacking(CarvingBlade.multiplierPerStack, itemCount));

                            var component = master.inventory.GetComponent<CarvingBlade.Statistics>();
                            if (component)
                            {
                                values.Add(Mathf.Max(component.TotalDamageDealt, 0));
                            }
                            else
                            {
                                values.Add(0f);
                            }
                        }
                        else
                        {
                            values.Add(Utils.GetHyperbolicStacking(CarvingBlade.multiplierPerStack, itemCount));
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
                    stats.descriptions.Add("Burn Duration: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Seconds);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            Crucifix.fireDuration * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)Crucifix.itemDef.itemIndex, stats);
                }

                // Debit Card
                if (DebitCard.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Rebate on Purchase: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            DebitCard.rebatePercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)DebitCard.itemDef.itemIndex, stats);
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
                            EdibleGlue.slowRadiusPerStack * itemCount
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
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
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
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Health);
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
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Healing);
                    stats.descriptions.Add("Shield: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Health);
                    stats.descriptions.Add("Movement Speed: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var empty = new List<float> { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };

                        if (!master || !master.inventory || !master.GetBody()) return empty;

                        var values = new List<float> { };
                        var component = master.inventory.GetComponent<HorseshoeStatistics>();
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
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Damage Dealt: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };
                        if (master && master.inventory)
                        {
                            values.Add(IronHeart.multiplierPerStack * itemCount);

                            var component = master.inventory.GetComponent<IronHeart.Statistics>();
                            if (component)
                            {
                                values.Add(component.TotalDamageDealt);
                            }
                            else
                            {
                                values.Add(0f);
                            }
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

                // Lunar Revive Consumed
                if (LunarRevive.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Max Health Sacrificed: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Health);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Items Lost Per Stage: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            Utils.GetExponentialStacking(LunarReviveConsumed.maxHealthLostPercent, itemCount),
                            LunarReviveConsumed.itemsLostPerStage * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)LunarReviveConsumed.itemDef.itemIndex, stats);
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
                            Utils.GetHyperbolicStacking(MilkCarton.eliteDamageReductionPercent, itemCount)
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
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };
                        // Check if we can use luck
                        if (master)
                            values.Add(Utils.GetChanceAfterLuck(MagnifyingGlass.analyzeChancePercent * itemCount, master.luck));
                        else
                            values.Add(MagnifyingGlass.analyzeChancePercent * itemCount);

                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)MagnifyingGlass.itemDef.itemIndex, stats);
                }

                // Paper Plane
                if (PaperPlane.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Movement Speed: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            PaperPlane.movespeedIncreasePercent * itemCount
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
                            values.Add(Utils.GetChanceAfterLuck(Utils.GetHyperbolicStacking(Permafrost.freezeChancePercent, itemCount), master.luck));
                        else
                            values.Add(Utils.GetHyperbolicStacking(Permafrost.freezeChancePercent, itemCount));
                        
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
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Event);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            RubberDucky.armorPerStack * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)RubberDucky.itemDef.itemIndex, stats);
                }

                // Rusty Trowel
                if (RustyTrowel.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Cooldown: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Seconds);
                    stats.descriptions.Add("Health Recovered: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Healing);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };

                        if (master && master.inventory)
                        {
                            values.Add(RustyTrowel.CalculateCooldownInSec(itemCount));

                            var component = master.inventory.GetComponent<RustyTrowel.Statistics>();
                            if (component)
                            {
                                values.Add(component.TotalHealingDone);
                            }
                            else
                            {
                                values.Add(0f);
                            }
                        }
                        else
                        {
                            values.Add(RustyTrowel.CalculateCooldownInSec(itemCount));
                            values.Add(0f);
                        }
                        return values;
                    }; 
                    ItemDefinitions.allItemDefinitions.Add((int)RustyTrowel.itemDef.itemIndex, stats);
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
                            Utils.GetHyperbolicStacking(ShadowCrest.regenPerSecondPercent, itemCount)
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
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Healing);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };

                        if (master && master.inventory)
                        {
                            var component = master.inventory.GetComponent<SoulRing.Statistics>();
                            if (component)
                            {
                                values.Add(component.HealthRegen);
                            }
                            else
                            {
                                values.Add(0f);
                            }
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
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Health);
                    stats.descriptions.Add("Max Health Sacrificed: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Health);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage); 
                    stats.calculateValues = (master, itemCount) =>
                    {
                        var values = new List<float> { };

                        if (master && master.inventory)
                        {
                            var component = master.inventory.GetComponent<SpiritStone.Statistics>();
                            if (component)
                                values.Add(component.PermanentShield);
                            else
                                values.Add(0f);
                        }
                        else
                        {
                            values.Add(0f);
                        }
                        values.Add(Utils.GetExponentialStacking(SpiritStone.maxHealthLostPercent, itemCount));

                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)SpiritStone.itemDef.itemIndex, stats);
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
