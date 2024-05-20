using LookingGlass.ItemStatsNameSpace;
using LookingGlass.StatsDisplay;
using RoR2;
using System;
using System.Collections.Generic;

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
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Damage Taken: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            AncientCoin.goldMultiplierAsPercent * itemCount,
                            AncientCoin.damageMultiplierAsPercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("ANCIENT_COIN"), stats);
                }

                // Bottle Cap
                if (BottleCap.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Cooldown Reduction: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            Utils.GetHyperbolicStacking(BottleCap.ultimateCDRPercent, itemCount)
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("BOTTLE_CAP"), stats);
                }

                // Bread
                if (BreadLoaf.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Healing On Kill: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Healing);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            BreadLoaf.healthGainOnKillPercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("BREAD_LOAF"), stats);
                }

                // Broken Mask
                if (BrokenMask.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Burn Damage: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            BrokenMask.burnDamagePercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("BROKEN_MASK"), stats);
                }

                // Carving Blade
                if (CarvingBlade.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Damage On-Hit: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            Utils.GetHyperbolicStacking(CarvingBlade.multiplierPerStack, itemCount)
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("CARVING_BLADE"), stats);
                }

                // Debit Card
                if (DebitCard.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Rebate on Purchase: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            DebitCard.rebatePercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("DEBIT_CARD"), stats);
                }

                // Edible Glue
                if (BreadLoaf.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Slow Range: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Meters);
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            EdibleGlue.slowRadiusPerStack * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("EDIBLE_GLUE"), stats);
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
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            Hoodie.durationIncreasePercent * itemCount,
                            Hoodie.CalculateHoodieCooldown(itemCount)
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("HOODIE"), stats);
                }

                // Glass Marbles
                if (GlassMarbles.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Base Damage per Level: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            GlassMarbles.damagePerLevelPerStack * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("GLASS_MARBLES"), stats);
                }

                // Holy Water
                if (HolyWater.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Experience Gained: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Health);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            HolyWater.CalculateExperienceMultiplier(itemCount)
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("HOLY_WATER"), stats);
                }

                // Iron Heart
                if (IronHeart.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("On-Hit Damage: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Health);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            IronHeart.multiplierPerStack * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("IRON_HEART"), stats);
                }

                // Milk Carton
                if (MilkCarton.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Damage Reduction: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            Utils.GetHyperbolicStacking(MilkCarton.eliteDamageReductionPercent, itemCount)
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("MILK_CARTON"), stats);
                }

                // Magnifying Glass
                if (MagnifyingGlass.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Analyze Chance: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            MagnifyingGlass.analyzeChancePercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("MAGNIFYING_GLASS"), stats);
                }

                // Paper Plane
                if (PaperPlane.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Movement Speed: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            PaperPlane.movespeedIncreasePercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("PAPER_PLANE"), stats);
                }

                // Photodiode
                if (Photodiode.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Max Attack Speed: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            Photodiode.maxAttackSpeedAllowedPercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("PHOTODIODE"), stats);
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
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            RedBlueGlasses.critChancePercent * itemCount,
                            RedBlueGlasses.critDamagePercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("REDBLUE_GLASSES"), stats);
                }

                // Rubber Ducky
                if (RubberDucky.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Bonus Armor: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            RubberDucky.armorPerStack * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("RUBBER_DUCKY"), stats);
                }

                // Rusty Trowel
                if (RustyTrowel.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Stacks On-Hit: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number); 
                    stats.descriptions.Add("Cooldown: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Seconds);
                    stats.calculateValues = (itemCount) =>
                    {
                        return new List<float> {
                            itemCount,
                            RustyTrowel.CalculateCooldownInSec(itemCount)
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("RUSTED_TROWEL"), stats);
                }
            }
        }
    }
}
