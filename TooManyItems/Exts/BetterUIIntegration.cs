using BetterUI;
using RoR2;
using System;

namespace TooManyItems
{
    internal static class BetterUIIntegration
    {
        internal static void Init()
        {
            RoR2Application.onLoad += BetterUIItemStats.RegisterItemStats;
        }

        public static class BetterUIItemStats
        {
            public static void RegisterItemStats()
            {
                // Ancient Coin
                if (AncientCoin.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: AncientCoin.itemDef,
                        "Gold Gain",
                        AncientCoin.goldMultiplierAsPercent,
                        AncientCoin.goldMultiplierAsPercent,
                        stackingFormula: ItemStats.LinearStacking,
                        statFormatter: ItemStats.StatFormatter.Percent
                    );
                    ItemStats.RegisterStat(
                        itemDef: AncientCoin.itemDef,
                        "Damage Taken",
                        AncientCoin.damageMultiplierAsPercent,
                        AncientCoin.damageMultiplierAsPercent,
                        stackingFormula: ItemStats.LinearStacking,
                        statFormatter: ItemStats.StatFormatter.Percent
                    );
                }

                // Blood Dice
                if (BloodDice.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: BloodDice.itemDef,
                        "Health Gained",
                        1f,
                        1f,
                        statFormatter: BloodDiceHealthFormatter
                    );
                }

                // Bottle Cap
                if (BottleCap.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: BottleCap.itemDef,
                        "Cooldown Reduction",
                        BottleCap.ultimateCDRPercent,
                        BottleCap.ultimateCDRPercent,
                        stackingFormula: ItemStats.HyperbolicStacking,
                        statFormatter: ItemStats.StatFormatter.Percent
                    );
                }

                // Bread
                if (BreadLoaf.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: BreadLoaf.itemDef, 
                        "Healing On Kill", 
                        BreadLoaf.healthGainOnKillPercent, 
                        BreadLoaf.healthGainOnKillPercent, 
                        stackingFormula: ItemStats.LinearStacking, 
                        statFormatter: ItemStats.StatFormatter.Percent
                    );
                }

                // Broken Mask
                if (BrokenMask.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: BrokenMask.itemDef,
                        "Burn Damage",
                        BrokenMask.burnDamagePercent,
                        BrokenMask.burnDamagePercent,
                        stackingFormula: ItemStats.LinearStacking,
                        statFormatter: ItemStats.StatFormatter.Percent
                    );
                    ItemStats.RegisterStat(
                        itemDef: BrokenMask.itemDef,
                        "Damage Dealt",
                        1f,
                        1f,
                        statFormatter: BrokenMaskDamageFormatter
                    );
                }

                // Carving Blade
                if (CarvingBlade.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: CarvingBlade.itemDef,
                        "Damage On-Hit",
                        CarvingBlade.multiplierPerStack,
                        CarvingBlade.multiplierPerStack,
                        stackingFormula: ItemStats.HyperbolicStacking,
                        statFormatter: ItemStats.StatFormatter.Percent
                    );
                    ItemStats.RegisterStat(
                        itemDef: CarvingBlade.itemDef,
                        "Total Damage Dealt",
                        1f,
                        1f,
                        statFormatter: CarvingBladeDamageFormatter
                    );
                }

                // Debit Card
                if (DebitCard.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: DebitCard.itemDef,
                        "Rebate On Purchase",
                        DebitCard.rebatePercent,
                        DebitCard.rebatePercent,
                        stackingFormula: ItemStats.HyperbolicStacking,
                        statFormatter: ItemStats.StatFormatter.Percent
                    );
                }

                // Edible Glue
                if (EdibleGlue.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: EdibleGlue.itemDef,
                        "Slow Range",
                        EdibleGlue.slowRadiusPerStack.Value,
                        EdibleGlue.slowRadiusPerStack.Value,
                        stackingFormula: ItemStats.LinearStacking,
                        statFormatter: ItemStats.StatFormatter.Range
                    );
                }

                // Fleece Hoodie
                if (Hoodie.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: Hoodie.itemDef,
                        "Duration Bonus",
                        Hoodie.durationIncreasePercent,
                        Hoodie.durationIncreasePercent,
                        stackingFormula: ItemStats.LinearStacking,
                        statFormatter: ItemStats.StatFormatter.Percent
                    );
                    ItemStats.RegisterStat(
                        itemDef: Hoodie.itemDef,
                        "Cooldown",
                        Hoodie.rechargeTime.Value,
                        Hoodie.rechargeTimeReductionPercent,
                        stackingFormula: ItemStats.NegativeExponentialStacking,
                        statFormatter: ItemStats.StatFormatter.Seconds
                    );
                }

                // Glass Marble
                if (GlassMarble.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: GlassMarble.itemDef,
                        "Base Damage",
                        1f,
                        1f,
                        statFormatter: GlassMarbleBonusFormatter
                    );
                }

                // Holy Water
                if (HolyWater.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: HolyWater.itemDef,
                        "Experience Gained",
                        1,
                        1,
                        statFormatter: HolyWaterBonusFormatter
                    );
                }

                // Iron Heart
                if (IronHeart.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: IronHeart.itemDef,
                        "On-Hit Damage",
                        1f,
                        1f,
                        statFormatter: IronHeartOnHitFormatter
                    );
                    ItemStats.RegisterStat(
                        itemDef: IronHeart.itemDef,
                        "Total Damage Dealt",
                        1f,
                        1f,
                        statFormatter: IronHeartDamageFormatter
                    );
                }

                // Milk Carton
                if (MilkCarton.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: MilkCarton.itemDef,
                        "Damage Reduction",
                        MilkCarton.eliteDamageReductionPercent,
                        MilkCarton.eliteDamageReductionPercent,
                        stackingFormula: ItemStats.HyperbolicStacking,
                        statFormatter: ItemStats.StatFormatter.Percent
                    );
                }

                // Magnifying Glass
                if (MagnifyingGlass.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: MagnifyingGlass.itemDef,
                        "Analyze Chance",
                        MagnifyingGlass.analyzeChancePercent,
                        MagnifyingGlass.analyzeChancePercent,
                        stackingFormula: ItemStats.LinearStacking,
                        statFormatter: ItemStats.StatFormatter.Percent,
                        itemTag: ItemStats.ItemTag.Luck
                    );
                }

                // Paper Plane
                if (PaperPlane.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: PaperPlane.itemDef,
                        "Movement Speed",
                        PaperPlane.movespeedIncreasePercent,
                        PaperPlane.movespeedIncreasePercent,
                        stackingFormula: ItemStats.LinearStacking,
                        statFormatter: ItemStats.StatFormatter.Percent
                    );
                }

                // Photodiode
                if (Photodiode.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: Photodiode.itemDef,
                        "Max Attack Speed",
                        Photodiode.maxAttackSpeedAllowedPercent,
                        Photodiode.maxAttackSpeedAllowedPercent,
                        stackingFormula: ItemStats.LinearStacking,
                        statFormatter: ItemStats.StatFormatter.Percent
                    );
                }

                // Red-Blue Glasses
                if (RedBlueGlasses.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: RedBlueGlasses.itemDef,
                        "Crit Chance",
                        RedBlueGlasses.critChancePercent,
                        RedBlueGlasses.critChancePercent,
                        stackingFormula: ItemStats.LinearStacking,
                        statFormatter: ItemStats.StatFormatter.Percent
                    );
                    ItemStats.RegisterStat(
                        itemDef: RedBlueGlasses.itemDef,
                        "Crit Damage",
                        RedBlueGlasses.critDamagePercent,
                        RedBlueGlasses.critDamagePercent,
                        stackingFormula: ItemStats.LinearStacking,
                        statFormatter: ItemStats.StatFormatter.Percent
                    );
                }

                // Rubber Ducky
                if (RubberDucky.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: RubberDucky.itemDef,
                        "Bonus Armor",
                        RubberDucky.armorPerStack.Value,
                        RubberDucky.armorPerStack.Value,
                        stackingFormula: ItemStats.LinearStacking,
                        statFormatter: ItemStats.StatFormatter.Armor
                    );
                }

                // Rusty Trowel
                if (RustyTrowel.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: RustyTrowel.itemDef,
                        "Cooldown",
                        RustyTrowel.rechargeTime.Value,
                        RustyTrowel.rechargeTimeReductionPercent,
                        stackingFormula: ItemStats.NegativeExponentialStacking,
                        statFormatter: ItemStats.StatFormatter.Seconds
                    );
                    ItemStats.RegisterStat(
                        itemDef: RustyTrowel.itemDef,
                        "Healing Done",
                        1f,
                        1f,
                        statFormatter: RustedTrowelHealingFormatter
                    );
                }

                // Soul Ring
                if (SoulRing.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: SoulRing.itemDef,
                        "Bonus Regeneration",
                        1f,
                        1f,
                        statFormatter: SoulRingRegenFormatter
                    );
                }

                // Spirit Stone
                if (SpiritStone.isEnabled.Value)
                {
                    ItemStats.RegisterStat(
                        itemDef: SpiritStone.itemDef,
                        "Shield Gained",
                        1f,
                        1f,
                        statFormatter: SpiritStoneFormatter
                    );
                }
            }

            public static ItemStats.StatFormatter GlassMarbleBonusFormatter = new()
            {
                suffix = "",
                style = ItemStats.Styles.Damage,
                statFormatter = (sb, value, master) =>
                {
                    if (!master.inventory || !master.hasBody) return;

                    int count = master.inventory.GetItemCount(GlassMarble.itemDef);
                    if (count > 0)
                    {
                        string temp = String.Format("{0:#.#}", count * master.GetBody().level * GlassMarble.damagePerLevelPerStack.Value);
                        temp = temp == String.Empty ? "0.0" : temp;

                        sb.AppendFormat(temp);
                    }
                    else
                    {
                        sb.Append("0.0");
                    }
                }
            };

            public static ItemStats.StatFormatter HolyWaterBonusFormatter = new()
            {
                suffix = "",
                style = "cIsHealth",
                statFormatter = (sb, value, master) =>
                {
                    if (!master.inventory || !master.hasBody) return;

                    int count = master.inventory.GetItemCount(HolyWater.itemDef);
                    if (count > 0)
                    {
                        string temp = String.Format("{0:#.#}", HolyWater.CalculateExperienceMultiplier(count) * 100);
                        temp = temp == String.Empty ? "0.0" : temp;

                        sb.AppendFormat(temp);
                    }
                    else
                    {
                        sb.Append("0.0");
                    }

                    sb.Append("% enemy max HP");
                }
            };

            public static ItemStats.StatFormatter IronHeartOnHitFormatter = new()
            {
                suffix = "",
                style = ItemStats.Styles.Damage,
                statFormatter = (sb, value, master) =>
                {
                    if (!master.hasBody) return;

                    float onHitDamage = IronHeart.CalculateDamageOnHit(master.GetBody(), value);
                    string valueDamageText = onHitDamage == 0 ? "0" : String.Format("{0:#.#}", onHitDamage);

                    sb.AppendFormat(valueDamageText);
                }
            };

            public static ItemStats.StatFormatter IronHeartDamageFormatter = new()
            {
                suffix = "",
                style = ItemStats.Styles.Damage,
                statFormatter = (sb, value, master) =>
                {
                    if (!master.inventory) return;

                    var component = master.inventory.GetComponent<IronHeart.Statistics>();
                    if (component)
                    {
                        string temp = String.Format("{0:#}", component.TotalDamageDealt);
                        temp = temp == String.Empty ? "0" : temp;

                        sb.AppendFormat(temp);
                    }
                    else
                    {
                        sb.Append("0");
                    }
                }
            };

            private static ItemStats.StatFormatter CarvingBladeDamageFormatter = new()
            {
                suffix = "",
                style = ItemStats.Styles.Damage,
                statFormatter = (sb, value, master) =>
                {
                    if (!master.inventory) return;

                    var component = master.inventory.GetComponent<CarvingBlade.Statistics>();
                    if (component)
                    {
                        string temp = String.Format("{0:#}", component.TotalDamageDealt);
                        temp = temp == String.Empty ? "0" : temp;

                        sb.AppendFormat(temp);
                    }
                    else
                    {
                        sb.Append("0");
                    }
                }
            };

            private static ItemStats.StatFormatter RustedTrowelHealingFormatter = new()
            {
                suffix = "",
                style = ItemStats.Styles.Healing,
                statFormatter = (sb, value, master) =>
                {
                    if (!master.inventory) return;

                    var component = master.inventory.GetComponent<RustyTrowel.Statistics>();
                    if (component)
                    {
                        string temp = String.Format("{0:#}", component.TotalHealingDone);
                        temp = temp == String.Empty ? "0" : temp;

                        sb.AppendFormat(temp);
                    }
                    else
                    {
                        sb.Append("0");
                    }
                }
            };

            private static ItemStats.StatFormatter SoulRingRegenFormatter = new()
            {
                suffix = "",
                style = ItemStats.Styles.Healing,
                statFormatter = (sb, value, master) =>
                {
                    if (!master.inventory) return;

                    var component = master.inventory.GetComponent<SoulRing.Statistics>();
                    if (component)
                    {
                        string temp = String.Format("{0:#.#}", component.HealthRegen);
                        temp = temp == String.Empty ? "0.0" : temp;

                        sb.AppendFormat(temp);
                    }
                    else
                    {
                        sb.Append("0.0");
                    }

                    sb.Append(" HP/s");
                }
            };

            private static ItemStats.StatFormatter BloodDiceHealthFormatter = new()
            {
                suffix = "",
                style = ItemStats.Styles.Health,
                statFormatter = (sb, value, master) =>
                {
                    if (!master.inventory) return;

                    var component = master.inventory.GetComponent<BloodDice.Statistics>();
                    if (component)
                    {
                        string temp = String.Format("{0:#}", component.PermanentHealth);
                        temp = temp == String.Empty ? "0" : temp;

                        sb.AppendFormat(temp);
                    }
                    else
                    {
                        sb.Append("0");
                    }

                    sb.Append(" ");
                }
            };

            private static ItemStats.StatFormatter BrokenMaskDamageFormatter = new()
            {
                suffix = "",
                style = ItemStats.Styles.Damage,
                statFormatter = (sb, value, master) =>
                {
                    if (!master.inventory) return;

                    var component = master.inventory.GetComponent<BrokenMask.Statistics>();
                    if (component)
                    {
                        string temp = String.Format("{0:#}", component.TotalDamageDealt);
                        temp = temp == String.Empty ? "0" : temp;

                        sb.AppendFormat(temp);
                    }
                    else
                    {
                        sb.Append("0");
                    }
                }
            };

            private static ItemStats.StatFormatter SpiritStoneFormatter = new()
            {
                suffix = "",
                style = "cIsUtility",
                statFormatter = (sb, value, master) =>
                {
                    if (!master.inventory) return;

                    var component = master.inventory.GetComponent<SpiritStone.Statistics>();
                    if (component)
                    {
                        string temp = String.Format("{0:#}", component.PermanentShield);
                        temp = temp == String.Empty ? "0" : temp;

                        sb.AppendFormat(temp);
                    }
                    else
                    {
                        sb.Append("0");
                    }
                }
            };
        }
    }
}