using RoR2;
using TooManyItems.Items.Lunar;
using TooManyItems.Items.Tier1;
using TooManyItems.Items.Tier2;
using TooManyItems.Items.Tier3;

namespace TooManyItems.Extensions
{
    public static class ProperSaveIntegration
    {
        public static readonly string PROPER_SAVE_KEY = "TOOMANYITEMS_SAVE_DATA";

        public static void Init()
        {
            ProperSave.SaveFile.OnGatherSaveData += (dict) =>
            {
                foreach (PlayerCharacterMasterController controller in PlayerCharacterMasterController.instances)
                {
                    if (controller && controller.master && controller.master.inventory)
                    {
                        Inventory inventory = controller.master.inventory;

                        float bloodDiceHealth = inventory.GetComponent<BloodDice.Statistics>() ? inventory.GetComponent<BloodDice.Statistics>().PermanentHealth : 0f;
                        int breadLoafKills = inventory.GetComponent<BreadLoaf.Statistics>() ? inventory.GetComponent<BreadLoaf.Statistics>().KillsCounter : 0;
                        float brokenMaskDamage = inventory.GetComponent<BrokenMask.Statistics>() ? inventory.GetComponent<BrokenMask.Statistics>().TotalDamageDealt : 0f;
                        float carvingBladeDamage = inventory.GetComponent<CarvingBlade.Statistics>() ? inventory.GetComponent<CarvingBlade.Statistics>().TotalDamageDealt : 0f;
                        float ironHeartDamage = inventory.GetComponent<IronHeart.Statistics>() ? inventory.GetComponent<IronHeart.Statistics>().TotalDamageDealt : 0f;
                        float soulRingRegen = inventory.GetComponent<SoulRing.Statistics>() ? inventory.GetComponent<SoulRing.Statistics>().HealthRegen : 0f;
                        float spiritStoneShield = inventory.GetComponent<SpiritStone.Statistics>() ? inventory.GetComponent<SpiritStone.Statistics>().PermanentShield : 0f;
                        float rustedTrowelHealing = inventory.GetComponent<RustedTrowel.Statistics>() ? inventory.GetComponent<RustedTrowel.Statistics>().TotalHealingDone : 0f;

                        if (dict.TryGetValue(PROPER_SAVE_KEY, out object obj) && obj is ProperSaveDataCollection collection)
                        {
                            collection.BloodDiceHealth = bloodDiceHealth;
                            collection.BreadLoafKills = breadLoafKills;
                            collection.BrokenMaskDamage = brokenMaskDamage;
                            collection.CarvingBladeDamage = carvingBladeDamage;
                            collection.IronHeartDamage = ironHeartDamage;
                            collection.SoulRingRegen = soulRingRegen;
                            collection.SpiritStoneShield = spiritStoneShield;
                            collection.RustedTrowelHealing = rustedTrowelHealing;

                            Log.Debug("Overwriting existing values in ProperSave's data block.");
                        }
                        else
                        {
                            dict.Add(PROPER_SAVE_KEY, new ProperSaveDataCollection(
                                bloodDiceHealth: bloodDiceHealth,
                                breadLoafKills: breadLoafKills,
                                brokenMaskDamage: brokenMaskDamage,
                                carvingBladeDamage: carvingBladeDamage,
                                ironHeartDamage: ironHeartDamage,
                                soulRingRegen: soulRingRegen,
                                spiritStoneShield: spiritStoneShield,
                                rustedTrowelHealing: rustedTrowelHealing
                            ));

                            Log.Debug("Saving a new ProperSave data block.");
                        }
                    }
                }
            };

            CharacterMaster.onStartGlobal += (obj) =>
            {
                // TODO: Add a flag to check if the CharacterMaster is the player
                if (obj && obj.inventory && obj.inventory.gameObject && !ProperSave.Loading.IsLoading)
                {
                    ProperSaveDataCollection saveData = ProperSave.Loading.CurrentSave.GetModdedData<ProperSaveDataCollection>(PROPER_SAVE_KEY);
                    Inventory inventory = obj.inventory;

                    BloodDice.Statistics bloodDiceStats = inventory.gameObject.GetComponent<BloodDice.Statistics>();
                    if (bloodDiceStats) bloodDiceStats.PermanentHealth = saveData.BloodDiceHealth;

                    BreadLoaf.Statistics breadLoafStats = inventory.gameObject.GetComponent<BreadLoaf.Statistics>();
                    if (breadLoafStats) breadLoafStats.KillsCounter = saveData.BreadLoafKills;

                    BrokenMask.Statistics brokenMaskStats = inventory.gameObject.GetComponent<BrokenMask.Statistics>();
                    if (brokenMaskStats) brokenMaskStats.TotalDamageDealt = saveData.BrokenMaskDamage;

                    CarvingBlade.Statistics carvingBladeStats = inventory.gameObject.GetComponent<CarvingBlade.Statistics>();
                    if (carvingBladeStats) carvingBladeStats.TotalDamageDealt = saveData.CarvingBladeDamage;

                    IronHeart.Statistics ironHeartStats = inventory.gameObject.GetComponent<IronHeart.Statistics>();
                    if (ironHeartStats) ironHeartStats.TotalDamageDealt = saveData.IronHeartDamage;

                    SoulRing.Statistics soulRingStats = inventory.gameObject.GetComponent<SoulRing.Statistics>();
                    if (soulRingStats) soulRingStats.HealthRegen = saveData.SoulRingRegen;

                    SpiritStone.Statistics spiritStoneStats = inventory.gameObject.GetComponent<SpiritStone.Statistics>();
                    if (spiritStoneStats) spiritStoneStats.PermanentShield = saveData.SpiritStoneShield;

                    RustedTrowel.Statistics rustedTrowelStats = inventory.gameObject.GetComponent<RustedTrowel.Statistics>();
                    if (rustedTrowelStats) rustedTrowelStats.TotalHealingDone = saveData.RustedTrowelHealing;
                }
            };
        }

        public class ProperSaveDataCollection(float bloodDiceHealth, int breadLoafKills, float brokenMaskDamage, float carvingBladeDamage, float ironHeartDamage, float soulRingRegen, float spiritStoneShield, float rustedTrowelHealing)
        {
            public float BloodDiceHealth = bloodDiceHealth;
            public int BreadLoafKills = breadLoafKills;
            public float BrokenMaskDamage = brokenMaskDamage;
            public float CarvingBladeDamage = carvingBladeDamage;
            public float IronHeartDamage = ironHeartDamage;
            public float SoulRingRegen = soulRingRegen;
            public float SpiritStoneShield = spiritStoneShield;
            public float RustedTrowelHealing = rustedTrowelHealing;
        }
    }
}
