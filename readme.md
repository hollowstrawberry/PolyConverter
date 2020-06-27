# PolyConverter

This small program will convert Poly Bridge 2 level files (from the Sandbox or from regular bridges)
into an editable text format.

It will be helpful if you need to merge layouts together, or edit parts of your level
that can't normally be edited, such as object depth (Z coordinate).


### How to use

Place `PolyConverter.exe` inside any folder that has .layout or .slot files in it
(the game stores its files in `Documents\Dry Cactus\Poly Bridge 2\`).
When you run it, the .layout and .slot files will be converted into .json files.  
By pressing Enter to run the program again, all the changes you made in the .json files
will be applied to the .layout and .slot files (or vice-versa).
A backup of the original file (before any changes) will be created if one doesn't exist.  
You can keep applying new changes as many times as you want without having to close the window.
    
