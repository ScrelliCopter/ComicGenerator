# Sassy Comic Generator #
Really stupid little comic generator used by SassyBot of Discord and GameJolt fame.
Meant to be used in conjunction with something else, but you can use it yourself from the command line.
Generates completely incoherent comic strips that sometimes make sense but mostly don't.

The two included panel packs were drawn by Nik Sudan and JuiceFox, so a big thanks to them.

I don't care what you do with it but this thing comes with no warranty so don't blame me if it brings down your company servers.
THIS SOFTWARE IS PRESENTED AS-IS WITH NO EXPRESS OR IMPLIED WARRANTY OH MY GOD WHY IS THIS IN ALL CAPS.

### How do I use it? ###
```
usage: ComicGenerator.exe <outPath> <packName> <[userId] [text] ...>
ex: ComicGenerator.exe "comic.png" "nik" 0 "Hi Bob." 1 "Hi Alice"

outPath --- Path to the output file, should always end in .png
packName -- Name of the pack to use, can be blank "" for a random pack, or "random" to mix a bunch of packs together.
userId ---- Id for each speaker, the generator will assign each user a random text colour to differentiate. Pair with text.
text ------ Text to display for each textbox, pair with userId.
```

### TODO list ###
* Create a panel editor, punching numbers into JSON is a pain.
* Un-hardcode the default packs.
* Test for Mono compatibility.
