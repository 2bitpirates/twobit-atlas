# Samples #

### Command line batch files ###
All sample batch files generate the resutls to the ``.\output`` folder.

**simple-font1.bat** creates a Tahoma 12pt atlas font with only numbers and A..Z characters

**simple-font2.bat** creates an Arial 18pt atlas font with a bold style

**explode-sprite.bat** creates an atlas from a folder of images.  It also generates a sprite file.

**button-font.bat** creates a font atlas that also includes button images.

![button font](https://github.com/foobit/twobit-atlas/raw/master/doc/button-font.png)

**button-crop.bat** creates an atlas that has four button images. One of the images ``button03.png`` has unused edge pixel data. It will automatically be cropped in the final atlas image.

![image crop](https://github.com/foobit/twobit-atlas/raw/master/doc/image-crop.png)

### Sample Applications ###

**HelloWorldAtlas** C# WinForms app that shows how to load and render from an atlas xml file.  Just drag-n-drop an atlas file to view.  Type in the textbox to change the default text. Clear the textbox to revert to the default display text.

**ExplodeSpriteAtlas** C# app that shows how to load and render an atlas using OpenGL.  The application renders each glyph in sequenece to show how to do animated sprites.
