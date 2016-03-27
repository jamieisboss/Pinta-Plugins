#!/bin/sh

echo creating .mpack file
rm DitherAddin.mpack -f
rm addin.info -f
rm DitherAddin.dll -f 
cp ../.addin.xml addin.info
cp Release/DitherAddin.dll DitherAddin.dll
zip -r0 DitherAddin.mpack addin.info
zip -r0 DitherAddin.mpack DitherAddin.dll
rm addin.info
rm DitherAddin.dll