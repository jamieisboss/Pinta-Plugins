// 
// BlurBrush.cs
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
	public class BlurBrush : BasePaintBrush
	{

		public override string Name {
			get { return AddinManager.CurrentLocalizer.GetString ("Blur"); }
		}

		public override double StrokeAlphaMultiplier {
			get { return 1.0; }
		}

		protected override unsafe Gdk.Rectangle OnMouseMove (Context g, Color strokeColor, ImageSurface surface,
		                                              int x, int y, int lastX, int lastY)
		{
			int rad = (int)(g.LineWidth / 2.0 + 0.5);
			if (rad < 2)
				rad = 2;

			Gdk.Rectangle surface_rect = new Gdk.Rectangle (0, 0, surface.Width, surface.Height);
			Gdk.Rectangle brush_rect = new Gdk.Rectangle (x - rad, y - rad, 2 * rad, 2 * rad);
			Gdk.Rectangle dest_rect = Gdk.Rectangle.Intersect (surface_rect, brush_rect);

			if ((dest_rect.Width > 1) && (dest_rect.Height > 1)) {

				//Allow Clipping through a temporary surface
				ImageSurface tmp_surface = new ImageSurface (Format.Argb32, dest_rect.Width, dest_rect.Height);

				using (Context g2 = new Context (tmp_surface)) {
					g2.Operator = Operator.Source;
					g2.SetSourceSurface (surface, -dest_rect.Left, -dest_rect.Top);
					g2.Rectangle (new Rectangle (0, 0, dest_rect.Width, dest_rect.Height));
					g2.Fill ();
				}

				//Flush to make sure all drawing operations are finished
				tmp_surface.Flush ();
				ColorBgra[,] tmp = new ColorBgra[dest_rect.Width, dest_rect.Height];

				for (int iy = 0; iy < dest_rect.Height; iy++) {
					ColorBgra* srcRowPtr = tmp_surface.GetPointAddressUnchecked (0, iy);
					for (int ix = 0; ix < dest_rect.Width; ix++) {
						tmp[ix,iy] = (*srcRowPtr).ToStraightAlpha();
						srcRowPtr++;
					}
				}

				for (int iy = 1; iy < dest_rect.Height-1; iy++) {
					ColorBgra* dstRowPtr = tmp_surface.GetPointAddressUnchecked (1, iy);
					int dy = dest_rect.Top + iy - y;
					for (int ix = 1; ix < dest_rect.Width-1; ix++) {

						int dx = dest_rect.Left + ix - x;

						if ( (dx*dx + dy*dy) < rad*rad)
						{
                            ColorBgra col = ColorBgra.Black;
							col.R = (byte)( (2 * tmp[ix,iy].R + tmp[ix-1,iy].R + tmp[ix+1,iy].R + tmp[ix,iy-1].R + tmp[ix,iy+1].R + 3)/6);
							col.G = (byte)( (2 * tmp[ix,iy].G + tmp[ix-1,iy].G + tmp[ix+1,iy].G + tmp[ix,iy-1].G + tmp[ix,iy+1].G + 3)/6);
							col.B = (byte)( (2 * tmp[ix,iy].B + tmp[ix-1,iy].B + tmp[ix+1,iy].B + tmp[ix,iy-1].B + tmp[ix,iy+1].B + 3)/6);
							col.A = (byte)( (2 * tmp[ix,iy].A + tmp[ix-1,iy].A + tmp[ix+1,iy].A + tmp[ix,iy-1].A + tmp[ix,iy+1].A + 3)/6);
							*dstRowPtr = col.ToPremultipliedAlpha();
						}
						dstRowPtr++;
					}
				}

				//Draw the final result on the surface
				g.Operator = Operator.Source;
				g.SetSourceSurface (tmp_surface, dest_rect.Left, dest_rect.Top);
				g.Rectangle (new Rectangle (dest_rect.Left, dest_rect.Top, dest_rect.Width, dest_rect.Height));
				g.Fill ();
			}
			return Gdk.Rectangle.Zero;
		}
	}
}


