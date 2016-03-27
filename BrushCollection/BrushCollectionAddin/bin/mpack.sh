#!/bin/sh

echo creating .mpack file
rm BrushCollectionAddin.mpack -f
rm addin.info -f
rm BrushCollectionAddin.dll -f 
cp ../.addin.xml addin.info
cp Release/BrushCollectionAddin.dll BrushCollectionAddin.dll
zip -r0 BrushCollectionAddin.mpack addin.info
zip -r0 BrushCollectionAddin.mpack BrushCollectionAddin.dll
rm addin.info
rm BrushCollectionAddin.dll