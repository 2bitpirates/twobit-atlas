# twobit-atlas #

**twobit-atlas** is a windows command line tool to generate image atlas sheets.  It can generate atlases from a directory of images, specified true type font, or a combination of both.

## Usage ##

**twobit-atlas** supports layout of glyphs in a grid or best fit.

>twobit-atlas.exe --font Tahoma --font-size=24 --glyph-align BestFit -o tahoma24-fit.atlas

![Tahoma 24pt best fit](https://github.com/foobit/twobit-atlas/raw/master/doc/tahoma24-fit.png)

>twobit-atlas.exe --font Tahoma --font-size=24 --glyph-align Grid -o tahoma24-fit.atlas

![Tahoma 24pt grid](https://github.com/foobit/twobit-atlas/raw/master/doc/tahoma24-grid.png)
