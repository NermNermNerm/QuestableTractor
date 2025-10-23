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

        configMenu.AddKeybindList(
            mod: modManifest,
            name: () => L("Expand Tractor Effect"),
            tooltip: () => L("Press this key to increase the action radius of tools when riding on the tractor.  Capped at +1 tile from the normal size configured in TractorMod."),
            getValue: () => ModEntry.Config.ExpandToolEffectKeybind,
            setValue: (v) => ModEntry.Config.ExpandToolEffectKeybind = v);

        configMenu.AddKeybindList(
            mod: modManifest,
            name: () => L("Contract Tractor Effect"),
            tooltip: () => L("Press this key to reduce the action radius of tools when riding on the tractor.  Capped at -1 tile from the normal size configured in TractorMod."),
            getValue: () => ModEntry.Config.ContractToolEffectKeybind,
            setValue: (v) => ModEntry.Config.ContractToolEffectKeybind = v);
    }
}
