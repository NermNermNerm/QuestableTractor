using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Force.DeepCloner;
using HarmonyLib;
using NermNermNerm.Stardew.LocalizeFromSource;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.Buildings;

namespace NermNermNerm.Stardew.QuestableTractor
{
    /// <summary>
    ///   This class manages the relationship between this mod and PathosChild.TractorMod.
    /// </summary>
    [NoStrict]
    internal class TractorModConfig
    {
        private readonly ModEntry mod;

        public const string GarageBuildingId = "Pathoschild.TractorMod_Stable";
        public const string TractorModId = "Pathoschild.TractorMod";

        private Action tractorModUpdateConfig = null!; // all these are set in Entry
        private Mod tractorModEntry = null!;
        private Func<object> getTractorModConfig = null!;
        private Action<object> setTractorModConfig = null!;
        private PropertyInfo distanceProperty = null!;

        /// <summary>
        ///   This is the copy of the config that TractorMod will use during gameplay; it has sections of it
        ///   disabled depending on the unlock state of the tractor.
        /// </summary>
        private object tractorModConfigForInGame = null!;

        /// <summary>
        ///   This is the copy of the config that gets saved permanently and is the copy that the user sees
        ///   when editing the settings.
        /// </summary>
        private object tractorModConfigForStorage = null!;

        public TractorModConfig(ModEntry mod)
        {
            this.mod = mod;
        }

        public void Entry()
        {
            // TODO: Skip all this hooliganism if GenericModConfigMenu is not installed.

            var modInfo = this.mod.Helper.ModRegistry.Get(TractorModId);
            if (modInfo is null)
            {
                this.mod.LogError($"QuestableTractorMod requires {TractorModId}, which is missing from your installation");
                return;
            }

            this.tractorModEntry = (Mod)modInfo.GetType().GetProperty("Mod")!.GetValue(modInfo)!;
            var tractorModConfigField = this.tractorModEntry.GetType().GetField("Config", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetField)!;
            var tractorModUpdateConfigMethod = this.tractorModEntry.GetType().GetMethod("UpdateConfig", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;
            this.tractorModUpdateConfig = () => { tractorModUpdateConfigMethod.Invoke(this.tractorModEntry, Array.Empty<object>()); };
            this.getTractorModConfig = () => tractorModConfigField.GetValue(this.tractorModEntry)!;
            this.setTractorModConfig = config => tractorModConfigField.SetValue(this.tractorModEntry, config);

            if (this.mod.Helper.ModRegistry.Get("spacechase0.GenericModConfigMenu") is null)
            {
                this.mod.LogInfo($"Generic Mod Config Menu is not installed, so not dealing with run-time changes to configuration");
            }
            else
            {
                var apiType = this.tractorModEntry.GetType().Assembly.GetType("Pathoschild.Stardew.TractorMod.Framework.GenericModConfigMenuIntegrationForTractor", true)!;
                instance = this;
                var constructors = apiType.GetConstructors();
                ModEntry.Instance.Harmony.Patch(
                    original: AccessTools.Constructor(apiType, constructors.First().GetParameters().Select(pi => pi.ParameterType).ToArray()),
                    prefix: new HarmonyMethod(typeof(TractorModConfig), nameof(GenericModConfigMenuIntegrationForTractor_Ctor_PrefixStatic)));
            }
        }

        private static TractorModConfig instance = null!;
        private static bool GenericModConfigMenuIntegrationForTractor_Ctor_PrefixStatic(ref object getConfig, ref Action reset, ref Action saveAndApply)
            => instance.GenericModConfigMenuIntegrationForTractor_Ctor_Prefix(ref getConfig, ref reset, ref saveAndApply);

        private bool GenericModConfigMenuIntegrationForTractor_Ctor_Prefix(ref object getConfig, ref Action reset, ref Action saveAndApply)
        {
            // This block is conditionally mirrored in OnDayStart for the case when Generic Mod Config Menu is not installed.
            this.tractorModConfigForInGame = this.getTractorModConfig();
            this.tractorModConfigForStorage = this.tractorModConfigForInGame.DeepClone()!;
            this.distanceProperty = this.tractorModConfigForInGame.GetType().GetProperty("Distance")!;

            // This is a fancy (but necessary) way of writing:  getConfig = ()=>this.tractorModConfigForStorage.
            // Necessary because that produces a Func<Object>, but getConfig is expected to be a Func<ModConfig>
            // where ModConfig is TractorMod's ModConfig, which we have no compile-time access to.
            var lambdaBody = Expression.Convert(
                Expression.Invoke(Expression.Constant(() => this.tractorModConfigForStorage)),
                this.tractorModConfigForInGame.GetType());
            getConfig = Expression.Lambda(getConfig.GetType(), lambdaBody).Compile();

            var tractorModReset = reset;
            reset = () =>
            {
                tractorModReset(); // This replaces TractorMod's Config with a new one!
                this.tractorModConfigForStorage = this.getTractorModConfig(); // So get the new one
                this.tractorModConfigForInGame = this.tractorModConfigForStorage.DeepClone()!; // Copy all the settings
                this.mod.UpdateTractorModConfig(); // Sync-up with our current quest state
                this.setTractorModConfig(this.tractorModConfigForInGame); // Put our runtime one back
                this.tractorModUpdateConfig(); // and tell TractorMod to deal with the merged world.
            };
            var tractorModSaveAndApply = saveAndApply;
            saveAndApply = () =>
            {
                this.setTractorModConfig(this.tractorModConfigForStorage); // Let tractormod see the storage world
                tractorModSaveAndApply(); // ...so it can save that view 
                this.tractorModConfigForInGame = this.tractorModConfigForStorage.DeepClone()!; // Copy all the saved settings
                this.mod.UpdateTractorModConfig(); // Merge whatever changes the user made with the quest state
                this.setTractorModConfig(this.tractorModConfigForInGame); // Now put our reduced state out to TractorMod
                this.tractorModUpdateConfig(); // and tell TractorMod to deal with the merged world.
            };

            return true; // run the constructor as normal with our substituted hooks
        }

        public int CurrentDistance
        {
            get => (int)this.distanceProperty.GetValue(this.tractorModConfigForInGame)!;
            set => this.distanceProperty.SetValue(this.tractorModConfigForInGame, value);
        }

        public int DefaultDistance => (int)this.distanceProperty.GetValue(this.tractorModConfigForStorage)!;

        public int MaxDistance => 1 + (int)this.distanceProperty.GetValue(this.tractorModConfigForStorage)!;

        public void TractorGarageBuildingCostChanged()
        {
            this.mod.Helper.GameContent.InvalidateCache("Data/Buildings");
            this.mod.LogTrace($"Invalidating asset 'Data/Buildings'.");
        }

        public void SetConfig(bool isHoeUnlocked, bool isLoaderUnlocked, bool isHarvesterUnlocked, bool isWatererUnlocked, bool isSpreaderUnlocked)
        {
            this.SetupTool("Axe", isLoaderUnlocked);
            this.SetupTool("Fertilizer", isSpreaderUnlocked);
            this.SetupTool("GrassStarter", isSpreaderUnlocked);
            this.SetupTool("Hoe", isHoeUnlocked);
            this.SetupTool("MilkPail", false);
            this.SetupTool("MeleeBlunt", false);
            this.SetupTool("MeleeDagger", false);
            this.SetupTool("MeleeSword", false);
            this.SetupTool("PickAxe", isLoaderUnlocked);
            this.SetupTool("Scythe", isHarvesterUnlocked);
            this.SetupTool("Seeds", isSpreaderUnlocked);
            this.SetupTool("Shears", false);
            this.SetupTool("Slingshot", false);
            this.SetupTool("WateringCan", isWatererUnlocked);
            this.SetupTool("SeedBagMod", isSpreaderUnlocked);

            this.TractorGarageBuildingCostChanged();
        }

        private void SetupTool(string toolName, bool isEnabled)
        {
            var stdAttachProp = this.tractorModConfigForInGame.GetType().GetProperty("StandardAttachments")!;
            object stdAttachInGame = stdAttachProp.GetValue(this.tractorModConfigForInGame)!;
            object stdAttachInSettings = stdAttachProp.GetValue(this.tractorModConfigForStorage)!;

            var toolProp = stdAttachInGame.GetType().GetProperty(toolName)!;
            object toolInGame = toolProp.GetValue(stdAttachInGame)!;
            object toolInSettings = toolProp.GetValue(stdAttachInSettings)!;

            foreach (var prop in toolInGame.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty).Where(p => p.PropertyType == typeof(bool)))
            {
                prop.SetValue(toolInGame, isEnabled && (bool)prop.GetValue(toolInSettings)!);
            }
        }

        [EventPriority(EventPriority.Low)] // Causes us to come after TractorMod's, which does not set EventPriority
        internal void OnDayStarted()
        {
            if (this.tractorModConfigForInGame is null)
            {
                this.tractorModConfigForInGame = this.getTractorModConfig();
                this.tractorModConfigForStorage = this.tractorModConfigForInGame.DeepClone()!;
                this.distanceProperty = this.tractorModConfigForInGame.GetType().GetProperty("Distance")!;
            }

            // TractorMod creates a tractor on day start.  We remove it if it's not configured.  Otherwise, doing nothing is the right thing.
            if (Game1.IsMasterGame && !this.mod.RestoreTractorQuestController.IsCompletedByMasterPlayer)
            {
                Farm farm = Game1.getFarm();
                var tractorIds = farm.buildings.OfType<Stable>().Where(s => s.buildingType.Value == GarageBuildingId).Select(s => s.HorseId).ToHashSet();
                var horses = farm.characters.OfType<Horse>().Where(h => tractorIds.Contains(h.HorseId)).ToList();
                foreach (var tractor in horses)
                {
                    farm.characters.Remove(tractor);
                }
            }
        }

        internal void EditBuildings(IDictionary<string, BuildingData> buildingData)
        {
            if (Game1.MasterPlayer is null)
            {
                this.mod.LogTrace($"Skipping building updates -- we were asked for it before the game was loaded or we're multiplayer.");
                // Leave it alone if we're being called before on game start.
                return;
            }

            if (!buildingData.TryGetValue(GarageBuildingId, out BuildingData? value))
            {
                this.mod.LogError($"It looks like TractorMod is not loaded - {GarageBuildingId} does not exist");
                return;
            }

            if (!this.mod.RestoreTractorQuestController.IsStartedByMasterPlayer
                || (!this.mod.RestoreTractorQuestController.IsCompletedByMasterPlayer && this.mod.RestoreTractorQuestController.GetState(Game1.MasterPlayer) < RestorationState.BuildTractorGarage))
            {
                this.mod.LogTrace($"Disabled the ability to buy a tractor garage at Robin's.");
                value.Builder = null;
            }
            else if (!this.mod.RestoreTractorQuestController.IsCompletedByMasterPlayer && this.mod.RestoreTractorQuestController.GetState(Game1.MasterPlayer) < RestorationState.WaitingForSebastianDay1)
            {
                this.mod.LogTrace($"Discounted garage price at Robin's.");
                value.BuildCost = 350;
                value.BuildMaterials = new List<BuildingMaterial>
                {
                    new BuildingMaterial() { ItemId = "(O)388", Amount = 300 }, // Wood
                    new BuildingMaterial() { ItemId = "(O)390", Amount = 200 }, // Stone
                    new BuildingMaterial() { ItemId = "(O)395", Amount = 1 }, // 1 cup of coffee
                };
            }
            else
            {
                // Normal cost after first build
                this.mod.LogTrace($"Reverted the tractor garage price to normal at Robin's.");
            }
        }
    }
}
