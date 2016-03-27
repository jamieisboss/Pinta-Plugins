#!/bin/sh

echo creating .mpack file
rm AlphaAdjustmentEffectsAddin.mpack -f
rm addin.info -f
rm AlphaAdjustmentEffectsAddin.dll -f 
cp ../.addin.xml addin.info
cp Release/AlphaAdjustmentEffectsAddin.dll AlphaAdjustmentEffectsAddin.dll
zip -r0 AlphaAdjustmentEffectsAddin.mpack addin.info
zip -r0 AlphaAdjustmentEffectsAddin.mpack AlphaAdjustmentEffectsAddin.dll
rm addin.info
rm AlphaAdjustmentEffectsAddin.dll