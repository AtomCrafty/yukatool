# What is this?
"Yuka System" is a visual novel engine used by a few companies such as HookSoft, PeasSoft or feng.  
Unfortunately little to no information about the engine is publicly available so I took a few weeks to carefully analyze the game archives.  
Why? Because we (Kachin Kachin Translations) are currently working on a fan-tl of the game "Lover Able" by Smee (HookSoft): https://forums.fuwanovel.net/topic/15293-lover-able-tl-trial-12-released/  
I've attached my notes on the file formats and some general information I've gathered in the doc/ folder.

# What does it do?
Yukatool aims to be the all in one solution for everything neccessary to properly work with the engine.  
This includes features to deal with the three main components: archives, graphics and scripts.  
At this point, the following operations (tasks) are supported:
* `unpack` Unpacks a yuka archive (.ykc) to a directory tree
* `pack` Packs a directory tree into a yuka archive (.ykc)
* `split` Splits a binary yuka script (.yks) into it's main sectors (code, index, data) and generates a comprehensible breakdown of the former two.
* `decompile` Decompiles a binary yuka script into a yuka source file (.ykd) and an optional string table (.csv). Works on entire directory trees
* `compile` Compiles a yuka source file (.ykd) and an optional string table (.csv) into a binary yuka script (.yks). Works on entire directory trees

The following tasks are planned for future versions:
* `unwrap` Convert a yuka graphics file (.ykg) to png and optional meta information (.meta)
* `wrap` Convert a png file and optional meta information (.meta) to a yuka graphics file (.ykg)
* `disassemble` Unpack a yuka archive and automatically decompile all scripts / unwrap all graphics
* `assemble` Pack a directory tree into a yuka archive and automatically compile all scripts / wrap all graphics

# How do I use it?
Yukatool is completely command-line operated. The basic syntax is as follows:  
`yuka.exe [flags] <task> <arguments...>`  
Flags do **not** have to be at the beginning and can be sepecified in a long or short form:  
`-rvw` adds the flags `--raw`, `--verbose` and `--wait`  
Which flags are available depends on the task you want to execute.  

If you want to contribute or have any questions regarding the project, feel free to contact me at atomcrafty@frucost.net :)
