using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace NermNermNerm.Stardew.QuestableTractor;

public class ModConfig
{
    // Although we're really trying to push folks to use TractorMod's setting, TractorMod has no default.
    // If somebody resets their settings and we don't set a default, then the feature would just
    // appear to quit working altogether.

    public KeybindList ExpandToolEffectKeybind { get; set; } = KeybindList.ForSingle(SButton.Add); // On the numeric keypad
    public KeybindList ContractToolEffectKeybind { get; set; } = KeybindList.ForSingle(SButton.Subtract); // On the numeric keypad

    public int QualityRequiredForNormalRadius { get; set; } = 0;
    public int FarmingLevelForNormalRadius { get; set; } = 0;
    public bool JankyIfAtMinimumForNormalRadius { get; set; } = true;

    public int QualityRequiredForLargeRadius { get; set; } = 2; // Iron
    public int FarmingSkillRequiredForLargeRadius { get; set; } = 10;
    public bool JankyIfAtMinimumForLargeRadius { get; set; } = true;
}
