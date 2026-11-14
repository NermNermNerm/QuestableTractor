using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;

using static NermNermNerm.Stardew.LocalizeFromSource.SdvLocalize;

namespace NermNermNerm.Stardew.QuestableTractor;

public class ModConfigMenu
{
    private ModEntry mod = null!;

    public void Entry(ModEntry mod)
    {
        this.mod = mod;

        mod.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }


    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        IManifest modManifest = this.mod.ModManifest;
        ModConfig config = ModEntry.Config;

        // get Generic Mod Config Menu's API (if it's installed)
        var configMenu =
            this.mod.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
            return;

        // register mod configs
        configMenu.Register(
            mod: modManifest,
            reset: () => config = new ModConfig(),
            save: () => this.mod.Helper.WriteConfig(config)
        );
        configMenu.AddParagraph(modManifest,() => L("Looking for the expand/contract tractor effect keybinds?  These are now in TractorMod's config."));

        configMenu.AddSectionTitle(
            mod:modManifest,
            text:() => L("Requirements for normal (3x3 grid) use:"));
        configMenu.AddTextOption(
            mod: modManifest,
            name: () => L("Tool quality:"),
            getValue: () => ModConfig.LocalizedQualities[Math.Clamp(config.QualityRequiredForNormalRadius, 0, 2)],
            setValue: (newValueAsLocalizedString) => { config.QualityRequiredForNormalRadius = Math.Max(0, ModConfig.LocalizedQualities.IndexOf(newValueAsLocalizedString)); },
            allowedValues: [ModConfig.LocalizedQualities[0], ModConfig.LocalizedQualities[1], ModConfig.LocalizedQualities[2]],
            tooltip: () => L("If the tool used with the tractor isn't at this upgrade level, the tractor will only work on the smallest action radius (usually 1x1)."));
        configMenu.AddNumberOption(
            mod: modManifest,
            name: () => L("Farming skill level:"),
            getValue: () => config.FarmingLevelForNormalRadius,
            setValue: (newLevel) => config.FarmingLevelForNormalRadius = newLevel,
            min: 0,
            max: 10,
            interval: 1
            );
        configMenu.AddBoolOption(
            mod: modManifest,
            name: () => L("Janky if at minimum"),
            getValue: () => config.JankyIfAtMinimumForNormalRadius,
            setValue: (newValue) => config.JankyIfAtMinimumForNormalRadius = newValue,
            tooltip: () => L("If checked and the farmer only barely meets the requirements above, the tractor will occasionally slip into a smaller action radius."));
        configMenu.AddSectionTitle(
            mod:modManifest,
            text:() => L("Requirements for expanded (5x5 grid) use:"));
        configMenu.AddTextOption(
            mod: modManifest,
            name: () => L("Tool quality:"),
            getValue: () => ModConfig.LocalizedQualities[Math.Clamp(config.QualityRequiredForLargeRadius, 2, 4)],
            setValue: (newValueAsLocalizedString) => { config.QualityRequiredForLargeRadius = Math.Max(0, ModConfig.LocalizedQualities.IndexOf(newValueAsLocalizedString)); },
            allowedValues: ModConfig.LocalizedQualities,
            tooltip: () => L("If the tool used with the tractor isn't at this upgrade level, the tractor will not be able to do the 5x5 grid."));
        configMenu.AddNumberOption(
            mod: modManifest,
            name: () => L("Farming skill level:"),
            getValue: () => config.FarmingSkillRequiredForLargeRadius,
            setValue: (newLevel) => config.FarmingSkillRequiredForLargeRadius = newLevel,
            min: 0,
            max: 10,
            interval: 1
        );
        configMenu.AddBoolOption(
            mod: modManifest,
            name: () => L("Janky if at minimum"),
            getValue: () => config.JankyIfAtMinimumForLargeRadius,
            setValue: (newValue) => config.JankyIfAtMinimumForLargeRadius = newValue,
            tooltip: () => L("If checked and the farmer only barely meets the requirements above, the tractor will occasionally slip into a smaller action radius."));
    }
}
