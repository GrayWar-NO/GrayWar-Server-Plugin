# Graywar's server plugin

This plugin is a dedicated server management solution for Nuclear Option. It is built on top of the [BepInEx](https://docs.bepinex.dev/index.html) modding
framework, and hence requires a BepInEx installation to run.

## Features

- Native support for chat commands with permission levels, with a number of default commands such as `ban`, `kick`,
  `tell`, `nextmission`, and `setpermissionlevel`.
- Those commands can also be ran from a TCP connection via our [IPC](#IPC)

## Installation

1. [Install BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html)
2. Download the latest release from the [releases page](https://github.com/GrayWar-NO/GrayWar-Server-Plugin/releases/)
3. Extract the contents of the archive into the `BepInEx/plugins` folder in your Nuclear Option installation directory
4. Launch the server with BepInEx (using the run_bepinex.sh script that it generates)
5. Go to BepInEx/config and edit the GrayWar.ServerPlugin.cfg file to your liking.
6. After configuring the server, run the script whenever you want to start the server

## Support

To get support on how to use this, you can come to the [GrayWar Discord server](https://discord.gg/R7kv2j6qnr)

## Credits

- [Nuclear Option](https://store.steampowered.com/app/2168680/Nuclear_Option/), since without it, we couldn't make mods
  for it.
- [MaxWasUnavailable](https://github.com/MaxWasUnavailable/Nuclei/commits?author=MaxWasUnavailable), creator of [Nuclei](https://github.com/MaxWasUnavailable/Nuclei), which we have used as a concept base to create what is concretely a full rewrite.

## Notes
This plugin is made to be used with the rest of the GrayWar stack, which enable connecting multiple servers together, to enable logging of everything we could think we'd need and remote management from Discord, as well as an API that is still WIP as I'm writing these words.
These parts are however not publicly available as of right now. They may be publicised in the future but for now we're focussing on making it robust and reliable.
