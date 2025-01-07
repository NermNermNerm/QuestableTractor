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
            this.toolDistanceOverrides.Clear();
            if (Game1.player.modData.TryGetValue(ToolDistanceModDataKey, out string toolWidthModifiers))
            {
                foreach (string pair in toolWidthModifiers.Split("|", StringSplitOptions.RemoveEmptyEntries)) {
                    string[] nameValue = pair.Split("=", 2);
                    this.toolDistanceOverrides[nameValue[0]] = int.Parse(nameValue[1]);
                }
            }
        }

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

            int modifier = 0;
            if (e.Button == SButton.Add)
            {
                modifier = 1;
            }
            else if (e.Button == SButton.Subtract)
            {
                modifier = -1;
            }
            else
            {
                return;
            }

            this.toolDistanceOverrides.TryGetValue(newToolName, out int oldModifier);
            modifier = Math.Min(MaxDistanceModifierForCurrentFarmer, Math.Max(-1, oldModifier + modifier));
            if (modifier == oldModifier)
            {
                return;
            }

            this.toolDistanceOverrides[newToolName] = modifier;
            Game1.player.modData[ToolDistanceModDataKey] = string.Join("|",
                this.toolDistanceOverrides
                    .OrderBy(pair => pair.Key)
                    .Select(pair => $"{pair.Key}={pair.Value}"));

            this.mod.TractorModConfig.SetDistanceModifier(modifier);
        }

        private static int MaxDistanceModifierForCurrentFarmer => Game1.player.GetSkillLevel(Farmer.farmingSkill) == 10 ? 1 : 0;

        private const string ToolDistanceModDataKey = "QuestableTractor.ToolDistanceModifier";
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

        private Dictionary<string, int> toolDistanceOverrides = new Dictionary<string, int>();

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!this.IsPlayerRidingTractor())
            {
                return;
            }

            string? newToolName = this.GetNameOfHeldTool();
            int newDistanceModifier = 0;
            if (newToolName is not null)
            {
                this.toolDistanceOverrides.TryGetValue(newToolName, out newDistanceModifier);
                {
                    newDistanceModifier = Math.Min(MaxDistanceModifierForCurrentFarmer, Math.Max(-1, newDistanceModifier));
                }
            }

            this.mod.TractorModConfig.SetDistanceModifier(newDistanceModifier);
        }
    }
}
