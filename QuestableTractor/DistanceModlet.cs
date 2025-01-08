using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

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

        private int MaxDistanceForThisFarmer
            => Game1.player.FarmingLevel < 10 ? this.mod.TractorModConfig.DefaultDistance : this.mod.TractorModConfig.MaxDistance; 

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!this.IsPlayerRidingTractor())
            {
                return;
            }

            string? newToolName = this.GetNameOfHeldTool();
            if (newToolName is null)
            {
                return;
            }

            if (!this.perToolDistance.TryGetValue(newToolName, out int newDistance))
            {
                newDistance = this.mod.TractorModConfig.DefaultDistance;
            }
            if (e.Button == SButton.Add)
            {
                ++newDistance;
            }
            else if (e.Button == SButton.Subtract)
            {
                --newDistance;
            }
            else
            {
                return;
            }

            newDistance = Math.Min(this.MaxDistanceForThisFarmer, Math.Max(0, newDistance));
            this.mod.TractorModConfig.CurrentDistance = newDistance;
            this.perToolDistance[newToolName] = newDistance;
            Game1.player.modData[PerToolDistanceModDataKey] = string.Join("|",
                this.perToolDistance
                    .OrderBy(pair => pair.Key)
                    .Select(pair => $"{pair.Key}={pair.Value}"));
        }

        private bool IsPlayerRidingTractor()
            => Game1.player.mount?.modData.ContainsKey("Pathoschild.TractorMod") == true;


        private string? GetNameOfHeldTool()
        {
            if (Game1.player.CurrentTool is not null)
                return Game1.player.CurrentTool.Name;
            else if (Game1.player.ActiveItem is not null && Game1.player.ActiveItem.Category == StardewValley.Object.SeedsCategory)
                return I("seeder");
            else if (Game1.player.ActiveItem is not null && Game1.player.ActiveItem.Category == StardewValley.Object.fertilizerCategory)
                return I("fertilizer");
            else
                return null;
        }

        private Dictionary<string, int> perToolDistance = new Dictionary<string, int>();

        [EventPriority(EventPriority.High)] // To get in front of TractorMod
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!this.IsPlayerRidingTractor())
            {
                return;
            }

            string? newToolName = this.GetNameOfHeldTool();
            int newDistance = this.mod.TractorModConfig.DefaultDistance;
            if (newToolName is not null && this.perToolDistance.TryGetValue(newToolName, out int savedDistance))
            {
                newDistance = Math.Min(this.MaxDistanceForThisFarmer, Math.Max(0, savedDistance));
            }

            this.mod.TractorModConfig.CurrentDistance = newDistance;
        }
    }
}
