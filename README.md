LaTale Tools
---

Minimum DotNet version: 9.0

This project provides utilities for extracting various file formats for the MMORPG LaTale.

Currently supported file formats:
* SPF: uncompressed archive file
* LDT: list file (items, achievements etc; typed)
* TBL: general descriptor of the game's sprites

The project also provides a web server for quickly previewing the game's content, and can be run by simply:
1. Initiate a dotnet user secret with a single key `LaTaleBinaryPath` (absolute path to the directory holding the game's SPF files)
2. Then, run
```
$ dotnet run --project LaTaleTools.WebApp
```
3. Navigate to http://localhost:5126