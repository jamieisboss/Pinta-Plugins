# BrushCollection

A collection of additional brushes for pinta. It contains brushes to blur, sharpen, inteligently erase and tone the image.
NightVisionEffect (https://github.com/PintaProject/NightVisionEffect) was used as template for this plugin.


##Notes

- It has lib copies of the current Pinta.Core and Pinta.Tools to build against, and they currently need manual updating. (Need to look into automatic updates here.)

- It builds a single dll that needs to be copied to Pinta's bin folder 
(when developing) or Pinta's install folder (installed Pinta). It is setup for automatic building and distribution via Pinta's addin server.

##Translations

Anyone wishing to contribute translations can do so by editing ```BrushCollectionAddin/.addin.xml``` and adding translated strings there, using the "de" translation strings as examples.

##Bugs

All bugs should be reported to the issue tracker on this Github page, not to the main Pinta bug tracker.


## License

This is licensed under the MIT/X11 license.
