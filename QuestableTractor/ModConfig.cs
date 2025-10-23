using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace NermNermNerm.Stardew.QuestableTractor;

public class ModConfig
{
    public KeybindList ExpandToolEffectKeybind { get; set; } = KeybindList.ForSingle(SButton.Add); // On the numeric keypad
    public KeybindList ContractToolEffectKeybind { get; set; } = KeybindList.ForSingle(SButton.Subtract); // On the numeric keypad
}
