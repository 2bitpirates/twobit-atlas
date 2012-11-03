# twobit-atlas #

**twobit-atlas** is a Windows command line tool to generate image atlas sheets.  It can generate atlases from a directory of images, specified true type font, or a combination of both.

## Usage ##
To get the full list of options type the following at the command prompt:
>twobit-atlas.exe --help

## Features ##
_Image Atlas:_ Combine multiple images to a single compact image. Optionally use the ``--make-sprite`` option to create a sprite animation info file assocated with the atlas.

_Glyph Layout:_ layout on the atlas can be placed in a grid or best fit.

>twobit-atlas.exe --font Tahoma --font-size 24 --glyph-align BestFit -o tahoma24-fit.atlas

![Tahoma 24pt best fit](https://github.com/foobit/twobit-atlas/raw/master/doc/tahoma24-fit.png)

>twobit-atlas.exe --font Tahoma --font-size 24 --glyph-align Grid -o tahoma24-fit.atlas

![Tahoma 24pt grid](https://github.com/foobit/twobit-atlas/raw/master/doc/tahoma24-grid.png)

_Interactive Console Mode:_ Generate atlases via a console menu ``twobit-atlas.exe -C``

![Interactive](https://github.com/foobit/twobit-atlas/raw/master/doc/interactive.png)

## Plugin Support ##
The tool can be expanded via C# assembly plugins during startup.

In addition the executable can be used as an assembly in your existing .Net managed projects. Just add a reference to the executable itself.
