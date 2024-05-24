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
                    stats.valueTypes.Add(ItemStatsDef.ValueType.HumanObjective);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("Damage Taken: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            AncientCoin.goldMultiplierAsPercent * itemCount,
                            AncientCoin.damageMultiplierAsPercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("ANCIENT_COIN"), stats);
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
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("BLOOD_DICE"), stats);
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
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("BOTTLE_CAP"), stats);
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
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("BREAD_LOAF"), stats);
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
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("BROKEN_MASK"), stats);
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
                                values.Add(component.TotalDamageDealt);
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
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("CARVING_BLADE"), stats);
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
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("DEBIT_CARD"), stats);
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
                    stats.calculateValues = (master, itemCount) =>
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
                    stats.descriptions.Add("Bonus Base Damage: ");
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
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("GLASS_MARBLES"), stats);
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
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("HOLY_WATER"), stats);
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
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("IRON_HEART"), stats);
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
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("MILK_CARTON"), stats);
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
                            values.Add(Utils.GetChanceAfterLuck(MagnifyingGlass.analyzeChancePercent * itemCount, (int)master.luck));
                        else
                            values.Add(MagnifyingGlass.analyzeChancePercent * itemCount);
                        
                        return values;
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
                    stats.calculateValues = (master, itemCount) =>
                    {
                        return new List<float> {
                            PaperPlane.movespeedIncreasePercent * itemCount
                        };
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("PAPER_PLANE"), stats);
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
                            values.Add(Utils.GetChanceAfterLuck(Utils.GetHyperbolicStacking(Permafrost.freezeChancePercent, itemCount), (int) master.luck));
                        else
                            values.Add(Utils.GetHyperbolicStacking(Permafrost.freezeChancePercent, itemCount));
                        
                        values.Add(Permafrost.frozenDamageMultiplierPercent * itemCount);
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("PERMAFROST"), stats);
                }

                // Photodiode
                if (Photodiode.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("Max Bonus Attack Speed: ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
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
                    stats.calculateValues = (master, itemCount) =>
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
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Event);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.calculateValues = (master, itemCount) =>
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
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("RUSTED_TROWEL"), stats);
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
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("SOUL_RING"), stats);
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
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("SPIRIT_STONE"), stats);
                }

                // Horseshoe
                if (Horseshoe.isEnabled.Value)
                {
                    ItemStatsDef stats = new ItemStatsDef();
                    stats.descriptions.Add("<style=cIsDamage>Base Damage:</style> ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.descriptions.Add("<style=cIsDamage>Attack Speed:</style> ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("<style=cIsDamage>Crit Chance:</style> ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("<style=cIsDamage>Crit Damage:</style> ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("<style=cEvent>Armor:</style> ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Event);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                    stats.descriptions.Add("<style=cIsHealing>Regeneration:</style> ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Healing);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Healing);
                    stats.descriptions.Add("<style=cIsHealth>Max Health:</style> ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Health);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Health);
                    stats.descriptions.Add("<style=cIsUtility>Shield:</style> ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Health);
                    stats.descriptions.Add("<style=cIsUtility>Movement Speed:</style> ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.descriptions.Add("<style=cIsUtility>Cooldown Reduction:</style> ");
                    stats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                    stats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                    stats.calculateValues = (master, itemCount) =>
                    {
                        if (!master || !master.inventory) return new List<float> { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
                        
                        var values = new List<float> { };
                        var component = master.inventory.GetComponent<HorseshoeStatistics>();
                        if (component)
                        {
                            values.Add(component.BaseDamageBonus);
                            values.Add(component.AttackSpeedPercentBonus);
                            values.Add(component.CritChanceBonus / 100f);
                            values.Add(component.CritDamageBonus);
                            values.Add(component.ArmorBonus);
                            values.Add(component.RegenerationBonus);
                            values.Add(component.MaxHealthBonus);
                            values.Add(component.ShieldBonus);
                            values.Add(component.MoveSpeedPercentBonus);
                            values.Add(component.CooldownReductionBonus);
                        }
                        else
                        {
                            values.Add(0f);
                            values.Add(0f);
                            values.Add(0f);
                            values.Add(0f);
                            values.Add(0f);
                            values.Add(0f);
                            values.Add(0f);
                            values.Add(0f);
                            values.Add(0f);
                            values.Add(0f);
                        }
                        return values;
                    };
                    ItemDefinitions.allItemDefinitions.Add((int)ItemCatalog.FindItemIndex("HORSESHOE_ITEM"), stats);
                }
            }
        }
    }
}
