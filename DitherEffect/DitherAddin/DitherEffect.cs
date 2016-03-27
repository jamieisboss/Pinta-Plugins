// 
// DitherEffect.cs
//  
// Author:
//       Stefan Moebius
// 
// Copyright (c) 2016 Stefan Moebius
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Cairo;
using Pinta;
using Pinta.Gui.Widgets;
using Pinta.Core;
using Pinta.Effects;
using Mono.Addins;

namespace DitherAddin
{
	public class DitherEffect : BaseEffect
	{	
		public override string Icon {
			get { return "DitherAddin.Resources.Menu.Effects.Photo.DitherEffect.png"; }
		}

		public override string Name {
			get { return AddinManager.CurrentLocalizer.GetString ("Dither Effect"); }
		}

		public override bool IsConfigurable {
			get { return false; }
		}

		public override string EffectMenuCategory {
			get { return AddinManager.CurrentLocalizer.GetString ("Photo"); }
		}

		public DitherEffect ()
		{
		}

		public override bool LaunchConfiguration ()
		{
			return EffectHelper.LaunchSimpleEffectDialog (this, AddinManager.CurrentLocalizer);
		}

		public unsafe override void Render (ImageSurface src, ImageSurface dest, Gdk.Rectangle[] rois)
		{

			foreach (Gdk.Rectangle rect in rois) {
				for (int y = rect.Top; y <= rect.GetBottom (); y++) {
					ColorBgra* srcRowPtr = src.GetPointAddressUnchecked (rect.Left, y);
					ColorBgra* dstRowPtr = dest.GetPointAddressUnchecked (rect.Left, y);
					ColorBgra* dstRowEndPtr = dstRowPtr + rect.Width;

					int dither = y & 1;
					while (dstRowPtr < dstRowEndPtr) {
						ColorBgra col = (*srcRowPtr).ToStraightAlpha();

						dither++;
						int r = col.R, g = col.G, b = col.B;;

						if ((dither & 1) == 0)
						{
							r += 2;
							g += 2;
							b += 2;
							if (r > 255) r = 255;
							if (g > 255) g = 255;
							if (b > 255) b = 255;
						}
						else
						{
							r -= 2;
							g -= 2;
							b -= 2;
							if (r < 0) r = 0;
							if (g < 0) g = 0;
							if (b < 0) b = 0;
						}
						col.R = (byte)r;
						col.G = (byte)g;
						col.B = (byte)b;
						*dstRowPtr = col.ToPremultipliedAlpha();
						++dstRowPtr;
						++srcRowPtr;
					}
				}
			}
		}
	}
}
