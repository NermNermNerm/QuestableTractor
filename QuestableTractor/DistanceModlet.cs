using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Tools;
using static NermNermNerm.Stardew.LocalizeFromSource.SdvLocalize;

namespace NermNermNerm.Stardew.QuestableTractor
{
    /// <summary>
    ///   This class manages the ability of the player to increase the reach of tools.
    /// </summary>
    internal class DistanceModlet
    {
        private const string PerToolDistanceModDataKey = "QuestableTractor.PerToolDistance";

        private readonly ModEntry mod;
        private readonly Dictionary<string, int> perToolDistance = new Dictionary<string, int>();
        private DateTime lastTimeSpewedJankMessage = DateTime.MinValue;
        private string? lastHardStopMessage = null;
        private DateTime lastHardStopMessageTime = DateTime.MinValue;

        public DistanceModlet(ModEntry mod)
        {
            this.mod = mod;
        }

        public void Entry()
        {
            this.mod.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            this.mod.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this.mod.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            this.mod.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            this.mod.Helper.Events.Content.AssetRequested += this.ContentOnAssetRequested;
        }

        private void ContentOnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/AudioChanges"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, AudioCueData>().Data;

                    data[I("QTM_GearGrind")] = new()
                    {
                        Id = "QTM_GearGrind",
                        FilePaths = [ Path.Combine(this.mod.Helper.DirectoryPath, I("assets"), I("geargrind.wav")) ],
                        Category = I("Sound")
                    };
                });
            }
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            if (!Game1.player.hasOrWillReceiveMail(MailKeys.DistanceShifterMail) && this.mod.RestoreTractorQuestController.IsComplete)
            {
                Game1.player.mailForTomorrow.Add(MailKeys.DistanceShifterMail);
            }
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            this.perToolDistance.Clear();
            if (Game1.player.modData.TryGetValue(PerToolDistanceModDataKey, out string perToolDistances))
            {
                foreach (string pair in perToolDistances.Split("|", StringSplitOptions.RemoveEmptyEntries)) {
                    string[] nameValue = pair.Split("=", 2);
                    this.perToolDistance[nameValue[0]] = int.Parse(nameValue[1]);
                }
            }
        }

        private (int maxDistance, bool isJanky, string? limitReason) DistanceLimit
        {
            get
            {
                int heldToolUpgradeLevel = this.GetHeldToolUpgradeLevel();

                // See if it qualifies for normal radius and return if not.
                if (Game1.player.FarmingLevel < ModEntry.Config.FarmingLevelForNormalRadius)
                {
                    return (0, false, LF($"Farming skill of {ModEntry.Config.FarmingLevelForNormalRadius} is needed to make the tractor's area of effect normal size."));
                }

                // See if it qualifies for normal radius and return if not.
                if (heldToolUpgradeLevel < ModEntry.Config.QualityRequiredForNormalRadius)
                {
                    return (0, false, LF($"{ModConfig.LocalizedQualities[ModEntry.Config.QualityRequiredForNormalRadius]} tools are needed to make the tractor's area of effect normal sized."));
                }

                if (Game1.player.FarmingLevel >= ModEntry.Config.FarmingSkillRequiredForLargeRadius)
                {
                    return (
                        maxDistance: this.mod.TractorModConfig.DefaultDistance,
                        isJanky: (heldToolUpgradeLevel < Tool.iridium
                                  && heldToolUpgradeLevel == ModEntry.Config.QualityRequiredForNormalRadius),
                        limitReason: LF($"A farming skill of {ModEntry.Config.FarmingSkillRequiredForLargeRadius} is needed to make the tractor's area of effect large.")
                    );
                }

                if (heldToolUpgradeLevel < ModEntry.Config.QualityRequiredForLargeRadius)
                {
                    return (
                        maxDistance: this.mod.TractorModConfig.DefaultDistance,
                        isJanky: (heldToolUpgradeLevel < Tool.iridium
                                  && heldToolUpgradeLevel == ModEntry.Config.QualityRequiredForNormalRadius),
                        limitReason: LF($"{ModConfig.LocalizedQualities[ModEntry.Config.QualityRequiredForLargeRadius]} tools are needed to make the tractor's area of effect large.")
                    );
                }

                return (
                    maxDistance: this.mod.TractorModConfig.MaxDistance,
                    isJanky: (heldToolUpgradeLevel < Tool.iridium
                              && heldToolUpgradeLevel == ModEntry.Config.QualityRequiredForLargeRadius),
                    limitReason: null);
            }
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!this.IsPlayerRidingTractor())
            {
                return;
            }

            // This mod introduced the idea of keyboard-configurable distance, which TractorMod
            // later adopted also.  So what we want to do is transition people to using the
            // TractorMod setting.  So what we do here is that if the user has set the binding
            // in TractorMod, we use that and disregard any setting for QuestableTractorMod.
            int delta;
            if (ModEntry.Instance.TractorModConfig.IncreaseDistanceKeys.Keybinds.Any()
                ? ModEntry.Instance.TractorModConfig.IncreaseDistanceKeys.IsDown()
                : ModEntry.Config.ExpandToolEffectKeybind.IsDown())
            {
                delta = 1;
            }
            else if (ModEntry.Instance.TractorModConfig.ReduceDistanceKeys.Keybinds.Any()
                    ? ModEntry.Instance.TractorModConfig.ReduceDistanceKeys.IsDown()
                    : ModEntry.Config.ContractToolEffectKeybind.IsDown())
            {
                delta = -1;
            }
            else
            {
                return;
            }

            this.ChangeDistance(delta);
        }

        private void ChangeDistance(int delta)
        {
            string? heldTool = this.GetNameOfHeldTool();
            if (heldTool is null)
            {
                return;
            }
            if (!this.perToolDistance.TryGetValue(heldTool, out int modifiedDistance))
            {
                modifiedDistance = this.mod.TractorModConfig.DefaultDistance;
            }

            var limit = this.DistanceLimit;
            if (modifiedDistance == limit.maxDistance && delta > 0)
            {
                if (limit.limitReason is not null)
                {
                    Game1.playSound("QTM_GearGrind");
                    if (this.lastHardStopMessage != limit.limitReason || DateTime.Now - this.lastHardStopMessageTime > TimeSpan.FromSeconds(20))
                    {
                        Game1.addHUDMessage(new HUDMessage(limit.limitReason) { noIcon = true });
                        this.lastHardStopMessage = limit.limitReason;
                        this.lastHardStopMessageTime = DateTime.Now;
                    }
                }
                // Else the player is hitting the maximum size so it's probably just the user spamming the key.
                // It seems like no sound is the thing to do.
                return;
            }

            modifiedDistance = Math.Max(0, Math.Min(modifiedDistance + delta, this.DistanceLimit.maxDistance));
            this.mod.TractorModConfig.CurrentDistance = modifiedDistance;
            this.perToolDistance[heldTool] = modifiedDistance;
            Game1.player.modData[DistanceModlet.PerToolDistanceModDataKey] = string.Join("|",
                this.perToolDistance
                    .OrderBy(pair => pair.Key)
                    .Select(pair => $"{pair.Key}={pair.Value}"));
        }

        private bool IsPlayerRidingTractor()
            => Game1.player.mount?.modData.ContainsKey("Pathoschild.TractorMod") == true;


        private string? GetNameOfHeldTool()
        {
            if (Game1.player.CurrentTool is Axe)
                return I("axe");
            else if (Game1.player.CurrentTool is Pickaxe)
                return I("pickaxe");
            else if (Game1.player.CurrentTool is Hoe)
                return I("hoe");
            else if (Game1.player.CurrentTool is WateringCan)
                return I("wateringcan");
            else if (Game1.player.CurrentTool?.Name.EndsWith(I("Scythe")) == true)
                return I("scythe");
            else if (Game1.player.ActiveItem is not null &&
                     Game1.player.ActiveItem.Category == StardewValley.Object.SeedsCategory)
                return I("seeder");
            else if (Game1.player.ActiveItem is not null &&
                     Game1.player.ActiveItem.Category == StardewValley.Object.fertilizerCategory)
                return I("fertilizer");
            else
                return null;
        }

        private int GetHeldToolUpgradeLevel()
        {
            if (Game1.player.CurrentTool is null)
            {
                // seeder or fertilizer
                return Tool.iridium;
            }
            else if (Game1.player.CurrentTool.Name == I("Golden Scythe"))
            {
                return Tool.gold;
            }
            else if (Game1.player.CurrentTool.Name == I("Scythe"))
            {
                return Tool.steel;
            }
            else if (Game1.player.CurrentTool.Name.EndsWith(I("Scythe")))
            {
                // This is a catch-all in case some mod comes along with their own kind of scythe.
                // we're just calling all things named "*Scythe" (including "Iridium Scythe") to be
                // of the best quality.
                return Tool.iridium;
            }
            else
            {
                return Game1.player.CurrentTool.UpgradeLevel;
            }
        }

        [EventPriority(EventPriority.High)] // To get in front of TractorMod
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!this.IsPlayerRidingTractor())
            {
                return;
            }

            string? nameOfHeldTool = this.GetNameOfHeldTool();
            if (nameOfHeldTool is null)
            {
                return;
            }

            int newDistance = this.mod.TractorModConfig.DefaultDistance;
            var limits = this.DistanceLimit;
            if (this.perToolDistance.TryGetValue(nameOfHeldTool, out int savedDistance))
            {
                newDistance = Math.Min(limits.maxDistance, Math.Max(0, savedDistance));
            }

            if (limits.isJanky && Game1.random.Next(900) == 0 && newDistance == limits.maxDistance)
            {
                Game1.playSound("clank");

                if (DateTime.Now - this.lastTimeSpewedJankMessage > TimeSpan.FromMinutes(30))
                {
                    Game1.addHUDMessage(new HUDMessage(LF($"My old {Game1.player.CurrentTool.DisplayName} needs to be upgraded to work well with the tractor.")) { noIcon = true });
                    this.lastTimeSpewedJankMessage = DateTime.Now;
                }

                this.ChangeDistance(-1);
            }

            this.mod.TractorModConfig.CurrentDistance = newDistance;
        }
    }
}
