# Junimatic

This is the source code for the Stardew Valley mod, Questable Tractor.  It's augments
[Pathoschild's TractorMod](https://github.com/Pathoschild/StardewMods/tree/develop/TractorMod)
by making it unlock gradually through a series of quests, making the tractor integrate
into the flow of the game better.

## Contributing

If you'd like to help with the mod, please file an 'issue' here on github first so that we can share
ideas and ensure that it's something that we can agree fits the mission of the mod.  From there, create
a pull request as normal.
  
## Translating

The mod can be translated like other Stardew Valley mods.  Look in the game's installation folder,
then look for `Mods\Junimatic\i18n\default.json`.  Copy that to a file with your language code
(e.g. `es.json` for Spanish) and replace the English string values with the translated strings.

Please don't translate like a bot - if the content needs to change to make sense in your locale,
go ahead and do what you think is right.

Once you're done with the translation, get the .json file to me any way you can - e.g. you can
put it on a cloud drive or file an Issue on GitHub and put the .json file in as an attachment to
the issue.  You can also follow the instructions in the .json file for creating a pull request,
but there's no need to do that.  I don't mind doing the work.

## Compiling the mods

Installing stable releases from Nexus Mods is recommended for most users. If you really want to
compile the mod yourself, read on.

These mods use the [crossplatform build config](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig)
so they can be built on Linux, Mac, and Windows without changes. See [the build config documentation](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig)
for troubleshooting.

Build the project in [Visual Studio](https://www.visualstudio.com/vs/community/) or [MonoDevelop](https://www.monodevelop.com/) to
build it and deploy it to your 'mod' directory in your Stardew Valley installation.

Launching it under the debugger will start Stardew Valley and your mod will be picked up as in the game.
