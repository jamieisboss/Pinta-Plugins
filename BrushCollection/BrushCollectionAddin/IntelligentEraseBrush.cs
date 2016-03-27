// 
// IntelligentEraseBrush.cs
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
	public class IntelligentEraseBrush : BasePaintBrush
	{
		const int LUT_Resolution = 256;
		byte[,] lut_factor = null;

		public override string Name {
			get { return AddinManager.CurrentLocalizer.GetString ("Inteligent erase"); }
		}

		public override double StrokeAlphaMultiplier {
			get { return 1.0; }
		}

		protected override unsafe Gdk.Rectangle OnMouseMove (Context g, Color strokeColor, ImageSurface surface,
		                                              int x, int y, int lastX, int lastY)
		{
			int rad = (int)(g.LineWidth / 2.0) + 1;
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
							lut_factor [dx, dy] = (byte)(Math.Cos (Math.Sqrt (d) * Math.PI / 2.0) * 255.0);
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


				int mean_r = 0, mean_g = 0, mean_b = 0;
				int max_diff_r = 1,max_diff_g = 1,max_diff_b = 1;
				int sz = 0;

				byte[,] factors = new byte[dest_rect.Right-dest_rect.Left, dest_rect.Bottom-dest_rect.Top];

				for (int iy = dest_rect.Top; iy < dest_rect.Bottom; iy++) {
					ColorBgra* srcRowPtr = tmp_surface.GetPointAddressUnchecked (0, iy - dest_rect.Top);
					for (int ix = dest_rect.Left; ix < dest_rect.Right; ix++) {
						ColorBgra col = (*srcRowPtr).ToStraightAlpha();
						int dx = ((ix - x) * LUT_Resolution)/rad;
						if (dx < 0)
							dx = -dx;
						int dy = ((iy - y) * LUT_Resolution)/rad;
						if (dy < 0)
							dy = -dy;

						int factor = lut_factor[dx,dy]; 
						sz += factor;
						mean_r += col.R * factor;
						mean_g += col.G * factor;
						mean_b += col.B * factor;
						srcRowPtr++;
					}
				}
				if (sz > 0)
				{
					mean_r /= sz;
					mean_g /= sz;
					mean_b /= sz;
				}

				for (int iy = dest_rect.Top; iy < dest_rect.Bottom; iy++) {
					ColorBgra* srcRowPtr = tmp_surface.GetPointAddressUnchecked (0, iy - dest_rect.Top);
					for (int ix = dest_rect.Left; ix < dest_rect.Right; ix++) {
						ColorBgra col = (*srcRowPtr).ToStraightAlpha();
						int dr = col.R - mean_r;
						int dg = col.G - mean_g;
						int db = col.B - mean_b;

						if (dr < 0)
							dr = -dr;
						if (dg < 0)
							dg = -dg;
						if (db < 0)
							db = -db;

						if (dr > max_diff_r)
							max_diff_r = dr;
						if (dg > max_diff_g)
							max_diff_g = dg;
						if (db > max_diff_b)
							max_diff_b = db;
						srcRowPtr++;
					}
				}

				for (int iy = dest_rect.Top; iy < dest_rect.Bottom; iy++) {
					ColorBgra* srcRowPtr = tmp_surface.GetPointAddressUnchecked (0, iy - dest_rect.Top);
					int dy = ((iy - y) * LUT_Resolution)/rad;
					if (dy < 0)
						dy = -dy;

					for (int ix = dest_rect.Left; ix < dest_rect.Right; ix++) {
						ColorBgra col = (*srcRowPtr).ToStraightAlpha();
						int dx = ((ix - x) * LUT_Resolution)/rad;
						if (dx < 0)
							dx = -dx;

						int dr = ((col.R - mean_r)*2*LUT_Resolution)/max_diff_r;
						int dg = ((col.G - mean_g)*2*LUT_Resolution)/max_diff_g;
						int db = ((col.B - mean_b)*2*LUT_Resolution)/max_diff_b;

						if (dr < 0)
							dr = -dr;
						if (dg < 0)
							dg = -dg;
						if (db < 0)
							db = -db;

						if (dr > LUT_Resolution)
							dr = LUT_Resolution;
						if (dg > LUT_Resolution)
							dg = LUT_Resolution;
						if (db > LUT_Resolution)
							db = LUT_Resolution;

						int factor;

						if ((max_diff_r > 32) || (max_diff_g > 32) || (max_diff_b > 32)) {
							factor = lut_factor[dx,dy] * lut_factor[dg,(dr+db)/2];
							col.A = (byte)( (col.A  * (0xFFFF-factor)) / 0xFFFF);	
						}
						else
						{
							factor = lut_factor[dx,dy];
							col.A = (byte)( (col.A  * (0xFF-factor)) / 0xFF);	
						}
								
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