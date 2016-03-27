// 
// BlurAltBrush2.cs
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
using Mono.Addins;
using Pinta.Core;

namespace BrushesCollectionAddin
{
	public class BlurAltBrush2 : BasePaintBrush
	{
		public override string Name {
			get { return AddinManager.CurrentLocalizer.GetString ("Blur alternative 2"); }
		}

		public override double StrokeAlphaMultiplier {
			get { return 1.0; }
		}

		protected override unsafe Gdk.Rectangle OnMouseMove (Context g, Color strokeColor, ImageSurface surface,
		                                              int x, int y, int lastX, int lastY)
		{
			int sz = (int)(g.LineWidth/2)+2;
			double rad = sz -1;
			ColorBgra[,] tmp = new ColorBgra[2*sz,2*sz];
			for (int ix = 0; ix < 2*sz; ix++) {
				for (int iy = 0; iy < 2*sz; iy++) {
					int _x = x + ix - sz;
					int _y = y + iy - sz;
					if ((_x >= 0) && (_x < surface.Width) && (_y >= 0) && (_y < surface.Height)) {
						ColorBgra* srcRowPtr = surface.GetPointAddressUnchecked (_x, _y);
						tmp[ix,iy] = (*srcRowPtr).ToStraightAlpha();
					}
				}
			}

			for (int ix = 1; ix < 2*sz-1; ix++) {
				int _x = x + ix - sz;
				for (int iy = 1; iy < 2*sz-1; iy++) {
					int _y = y + iy - sz;

					if (Math.Sqrt( (ix-sz)*(ix-sz) + (iy-sz)*(iy - sz) ) <= rad)
					{
						if ((_x >= 1) && (_x < surface.Width-1) && (_y >= 1) && (_y < surface.Height-1)) {
							ColorBgra* srcRowPtr = surface.GetPointAddressUnchecked (_x, _y);

							byte _r = (byte)( (1*tmp[ix,iy].R + tmp[ix-1,iy].R + tmp[ix+1,iy].R + tmp[ix,iy-1].R + tmp[ix,iy+1].R )/5.0 + 0.5);
							byte _g = (byte)( (1*tmp[ix,iy].G + tmp[ix-1,iy].G + tmp[ix+1,iy].G + tmp[ix,iy-1].G + tmp[ix,iy+1].G )/5.0 + 0.5);
							byte _b = (byte)( (1*tmp[ix,iy].B + tmp[ix-1,iy].B + tmp[ix+1,iy].B + tmp[ix,iy-1].B + tmp[ix,iy+1].B )/5.0 + 0.5);
							byte _a = (byte)( (1*tmp[ix,iy].A + tmp[ix-1,iy].A + tmp[ix+1,iy].A + tmp[ix,iy-1].A + tmp[ix,iy+1].A )/5.0 + 0.5);
							
							 *srcRowPtr = ColorBgra.FromBgra(_b,_g,_r,_a).ToPremultipliedAlpha(); 
						}
					}
				}
			}
			return Gdk.Rectangle.Zero;
		}
	}
}
