----- PolyConverter -----

This small program will convert Poly Bridge 2 layout files (from the sansbox/workshop)
into an easily-editable text format.

It should be helpful when you need to merge layouts together, or edit parts of your level
that can't normally be edited, such as object depth (Z coordinates).



[ INSTALLATION INSTRUCTIONS ]

1. Place the contents of the "Poly Bridge 2_Data" folder inside your Poly Bridge 2 install location.
   This will merge folders together but it won't replace any existing game files.
   By default, the game is installed in "C:\Program Files (x86)\Steam\steamapps\common\Poly Bridge 2".

2. Place PolyConverter.bat in the same folder as the layout files you want to convert.
   The game stores its layout files in "Documents\Dry Cactus\Poly Bridge 2\Sandbox",
   so I recommend putting it there.

3. Right click PolyConverter.bat and select Edit to open it in notepad. You will see a line with the
   default Poly Bridge 2 install location. If your game is not installed in that folder, change it and save.

4. You can now double-click PolyConverter.bat to run the program.



[ HOW TO USE ]

When you run PolyConverter.bat, all .layout files in the current folder will be converted into .json files.
Then you can edit them as much as you like.

When you run the program again, all the changes you made in the .json files will be applied to the .layout files.
A backup of the original .layout file (before any changes) will be created the first time.
You can run the program again every time you want to apply any new changes.
    
