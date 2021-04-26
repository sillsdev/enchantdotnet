# .NET bindings for enchant

These are the .NET bindings for [libenchant](https://github.com/AbiWord/enchant/). They were last updated on 1st July 2008, shortly after the release of Enchant 1.4.2, so should presumably work with that version, but may not with Enchant 1.5, released in 2009, or later releases.

Anyone interested in bringing them up to date is encouraged to get in contact with the Enchant maintainers.

## Building

Either load and build `Enchant.Net.sln` in an IDE, or build from the command line:

```bash
msbuild /t:Build build/enchantdotnet.proj
```

To create a nuget package, run:

```bash
msbuild /t:Pack build/enchantdotnet.proj
```

This will create the nuget package in the `output` subdirectory.