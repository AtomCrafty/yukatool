# What is this?
"Yuka System" is a visual novel engine used by a few companies such as HookSoft, Smee, PeasSoft and feng.  
Unfortunately little to no information about the engine is publicly available so I took a few weeks to carefully analyze the game archives.  
Why? Because we (Kachin Kachin Translations) are currently working on a fan-tl of the game "Lover Able" by Smee: https://forums.fuwanovel.net/topic/15293-lover-able-tl-trial-12-released/  
I've attached my notes on the file formats and some general information I've gathered in the doc/ folder.

# What does it do?
Yukatool aims to be the all in one solution for everything neccessary to properly work with the engine.  
This includes features to deal with the three main components: archives, graphics and scripts.  
At this point, the following operations (tasks) are supported:
* `help` Displays a list of available commands
* `unpack` Unpacks a yuka archive (.ykc) to a directory tree
* `pack` Packs a directory tree into a yuka archive (.ykc)
* `patch` Copies all files from one archive to another
* `split` Splits a binary yuka script (.yks) into it's main sectors (code, index, data) and generates a comprehensible breakdown of the former two.
* `decompile` Decompiles a binary yuka script into a yuka source file (.ykd) and an optional string table (.csv). Works on entire directory trees
* `compile` Compiles a yuka source file (.ykd) and an optional string table (.csv) into a binary yuka script (.yks). Works on entire directory trees
* `unwrap` Converts a yuka graphics file (.ykg) to png and optional meta information (.meta)
* `wrap` Converts a png file and optional meta information (.meta) to a yuka graphics file (.ykg)

The following tasks are planned for future versions:
* `disassemble` Unpacks a yuka archive and automatically decompiles all scripts / unwraps all graphics
* `assemble` Packs a directory tree into a yuka archive and automatically compiles all scripts / wraps all graphics

# How do I use it?
Yukatool is completely command-line operated. The basic syntax is as follows:  
`yuka.exe [flags] <task> <arguments...>`  
Flags do **not** have to be at the beginning and can be specified in a long or short form:  
`-rvw` adds the flags `--raw`, `--verbose` and `--wait`  
Which flags are available depends on the task you want to execute.  

If you want to contribute or have any questions regarding the project, feel free to contact me at atomcrafty@frucost.net :)
