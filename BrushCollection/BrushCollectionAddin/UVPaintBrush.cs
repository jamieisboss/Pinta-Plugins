// 
// UVPaintBrush.cs
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
	public class UVPaintBrush : BasePaintBrush
	{
		const int LUT_Resolution = 256;
		byte[,] lut_factor = null;

		public override string Name {
			get { return AddinManager.CurrentLocalizer.GetString ("UV Paint"); }
		}

		public override double StrokeAlphaMultiplier {
			get { return 1.0; }
		}

		protected override unsafe Gdk.Rectangle OnMouseMove (Context g, Color strokeColor, ImageSurface surface,
		                                              int x, int y, int lastX, int lastY)
		{
			int rad = (int)(g.LineWidth / 2.0) + 1;

			int stroke_a = (int)(255.0 * strokeColor.A);
			int stroke_r = (int)(255.0 * strokeColor.R);
			int stroke_g = (int)(255.0 * strokeColor.G);
			int stroke_b = (int)(255.0 * strokeColor.B);

			int stroke_cb =  (-173 * stroke_r - 339 * stroke_g  + 512 * stroke_b) >> 9;
			int stroke_cr =  ( 512 * stroke_r - 429 * stroke_g  -  83 * stroke_b) >> 9;

			Gdk.Rectangle surface_rect = new Gdk.Rectangle (0, 0, surface.Width, surface.Height);
			Gdk.Rectangle brush_rect = new Gdk.Rectangle (x - rad, y - rad, 2 * rad, 2 * rad);
			Gdk.Rectangle dest_rect = Gdk.Rectangle.Intersect (surface_rect, brush_rect);


			//Initialize lookup table when first used (to prevent slower startup of the application)
			if (lut_factor == null) {
				lut_factor = new byte[LUT_Resolution + 1, LUT_Resolution + 1];

				for (int dy = 0; dy < LUT_Resolution+1; dy++) {
					for (int dx = 0; dx < LUT_Resolution+1; dx++) {
						double d = Math.Sqrt (dx * dx + dy * dy) / LUT_Resolution;
						if (d > 1.0)
							lut_factor [dx, dy] = 0;
						else
							lut_factor [dx, dy] = (byte)(Math.Cos (Math.Sqrt (d) * Math.PI / 2.0) * 255);
					}
				}
			}

			if ((dest_rect.Width > 0) && (dest_rect.Height > 0)) {

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

				for (int iy = dest_rect.Top; iy < dest_rect.Bottom; iy++) {
					ColorBgra* srcRowPtr = tmp_surface.GetPointAddressUnchecked (0, iy - dest_rect.Top);
					int dy = ((iy - y) * LUT_Resolution) / rad;
					if (dy < 0)
						dy = -dy;
					for (int ix = dest_rect.Left; ix < dest_rect.Right; ix++) {
						ColorBgra col = (*srcRowPtr).ToStraightAlpha();
						int dx = ((ix - x) * LUT_Resolution) / rad;
						if (dx < 0)
							dx = -dx;

						int force = (lut_factor[dx,dy] * stroke_a);	
						int col_y =  (  306 * col.R + 601 * col.G  + 117 * col.B + 256) >> 9; 

						int red   = (256 * col_y                   + 359 * stroke_cr + 256) >> 9;
						int green = (256 * col_y -  88 * stroke_cb - 183 * stroke_cr + 256) >> 9;
						int blue  = (256 * col_y + 454 * stroke_cb                   + 256) >> 9;

						if (red > 255) red = 255;
						if (red < 0) red = 0;
						if (green > 255) green = 255;
						if (green < 0) green = 0;
						if (blue > 255) blue = 255;
						if (blue < 0) blue = 0;

						col.R = (byte)((col.R * (255*255 - force) + red   * force + 32512) / (255*255));
						col.G = (byte)((col.G * (255*255 - force) + green * force + 32512) / (255*255));
						col.B = (byte)((col.B * (255*255 - force) + blue  * force + 32512) / (255*255));

						*srcRowPtr = col.ToPremultipliedAlpha();
						srcRowPtr++;
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