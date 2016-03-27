// 
// AlphaToGray.cs
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
using Pinta.Gui.Widgets;
using Pinta.Core;
using Mono.Addins;

namespace AlphaAdjustmentEffectsAddin
{
	public class AlphaToGray : BaseEffect
	{	
		public override string Icon {
			get { return "AlphaAdjustmentEffectsAddin.Resources.Menu.Adjustments.AlphaToGray.png"; }
		}

		public override string Name {
			get { return AddinManager.CurrentLocalizer.GetString ("Alpha to gray"); }
		}

		public override bool IsConfigurable {
			get { return false; }
		}

		public unsafe override void Render (ImageSurface src, ImageSurface dest, Gdk.Rectangle[] rois)
		{
			dest.Flush ();
			foreach (Gdk.Rectangle rect in rois) {
				for (int y = rect.Top; y <= rect.GetBottom (); y++) {
					ColorBgra* srcRowPtr = src.GetPointAddressUnchecked (rect.Left, y);
					ColorBgra* dstRowPtr = dest.GetPointAddressUnchecked (rect.Left, y);
					ColorBgra* dstRowEndPtr = dstRowPtr + rect.Width;

						while (dstRowPtr < dstRowEndPtr) {
							//ToStraightAlpha() and ToPremultipliedAlpha() are not needed here
							ColorBgra col = (*srcRowPtr);

							col.R = col.A;
							col.G = col.A;
							col.B = col.A;
							col.A = 255;

							*dstRowPtr = col;
							++dstRowPtr;
							++srcRowPtr;
						}
				}
			}
		}
	}
}
