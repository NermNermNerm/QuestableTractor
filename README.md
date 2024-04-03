This repository contains my SMAPI mods for Stardew Valley. See the individual mods for
documentation and release notes.

## Mods

* **QuestableTractor** <small>([Nexus](https://www.nexusmods.com/stardewvalley/mods/todo) | [source](QuestableTractor))</small>  
  Supplements TractorMod by making it so that the tractor is unlocked through quests that are expensive in
  terms of time, rather than expensive in terms of late-game resources.  And, of course, is hopefully
  more fun than just whipping out your checkbook.

## Translating the mods

Until the mod's content fully stabilizes, it's going to be English-only.

## Help Wanted!

This is open source for a reason!  Contributions are welcome!  However, check with the author first either
through a GitHub issue here or on Discord.  Most welcome will be contributions from people who enjoy pixel art.
There are several art-heavy features that have been dreamt of but not implemented due to a lack of talent:

1. I'd like to turn the tractor discovery into an "Event".  That way we could show the broken down tractor in a more compelling
   setting, and generally be less handwavy with how the tractor gets out of the muck and into the garage.
2. It'd be great if we could have the tractor on Ginger Island.  Right now you can kindof cheat it in by using the
   "Backspace" shortcut key from TractorMod.  It'd be nice if we could have some quest line for that.  It also seems
   to me that Ginger Island is not a tractor-friendly kind of place.  Perhaps instead of an actual tractor,
   it'd be more of a ganja-powered super-donkey.
3. The graphics for the harpoon are way busted.  It'd be nice to fix that and maybe have something more creative
   than just your basic fishing pole animation.
4. We may be constrained on point #3 by what the game allows -- but failing that, perhaps we could create some
   sort of event in Willy's Shop when you get the harpoon where he dramatically pulls it out of the back room.
5. This would be more for TractorMod, but it'd be nice if the tractor changed appearance depending on which tool
   was selected.

Some non graphics-heavy projects include:

1. It'd be nice if we could change it so that when Sebastian is working on the tractor, he was shown under the
   tractor working, like he does when working on his bike.  (There's already some art for that in the game assets.)
2. There are several places where an Event would be better than the dialog that's there now.  Anything involving
   the wizard is ideal for Events since he's always at his house.

Please discuss this either through issues or Discord before doing any serious work -- wouldn't want to duplicate
effort or create something that isn't accepted because it strays from the mod's vision.

## Compiling the mods

Installing stable releases from Nexus Mods is recommended for most users. If you really want to
compile the mod yourself, read on.

These mods use the [crossplatform build config](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig)
so they can be built on Linux, Mac, and Windows without changes. See [the build config documentation](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig)
for troubleshooting.

### Compiling a mod for testing

To compile a mod and add it to your game's `Mods` directory:

1. Rebuild the project in [Visual Studio](https://www.visualstudio.com/vs/community/) or [MonoDevelop](https://www.monodevelop.com/).  
   <small>This will compile the code and package it into the mod directory.</small>
2. Launch the project with debugging.  
   <small>This will start the game through SMAPI and attach the Visual Studio debugger.</small>

### Compiling a mod for release

To package a mod for release:

1. Switch to `Release` build configuration.
2. Recompile the mod per the previous section.
3. Upload the generated `bin/Release/<mod name>-<version>.zip` file from the project folder.
