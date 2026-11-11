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
    }
}
