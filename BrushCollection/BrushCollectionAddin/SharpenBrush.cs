// 
// SharpenBrush.cs
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
	public class SharpenBrush : BasePaintBrush
	{
		const int LUT_Resolution = 256;
		byte[,] lut_factor = null;


		public override string Name {
			get { return AddinManager.CurrentLocalizer.GetString ("Sharpen"); }
		}

		public override double StrokeAlphaMultiplier {
			get { return 1.0; }
		}

		protected override unsafe Gdk.Rectangle OnMouseMove (Context g, Color strokeColor, ImageSurface surface,
		                                              int x, int y, int lastX, int lastY)
		{
			int rad = (int)(g.LineWidth / 2.0 + 0.5);
			rad /= 2;
			rad *= 2;
			if (rad < 2) rad = 2;


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

				int[,] mean_r = new int[dest_rect.Width/2+1, dest_rect.Height/2+1];
				int[,] mean_g = new int[dest_rect.Width/2+1, dest_rect.Height/2+1];
				int[,] mean_b = new int[dest_rect.Width/2+1, dest_rect.Height/2+1];
				int[,] mean_a = new int[dest_rect.Width/2+1, dest_rect.Height/2+1];
				int[,] mean_c = new int[dest_rect.Width/2+1, dest_rect.Height/2+1];

				for (int iy = 0; iy < dest_rect.Height/2; iy++) {
					for (int ix = 0; ix < dest_rect.Width/2; ix++) {
						mean_a[ix, iy] = 0;
						mean_r[ix, iy] = 0;
						mean_g[ix, iy] = 0;
						mean_b[ix, iy] = 0;
						mean_c[ix, iy] = 0;
					}
				}

				for (int iy = 0; iy < dest_rect.Height; iy++) {
					ColorBgra* srcRowPtr = tmp_surface.GetPointAddressUnchecked (0, iy);
					for (int ix = 0; ix < dest_rect.Width; ix++) {
						ColorBgra col = (*srcRowPtr).ToStraightAlpha();
						int pos_x = ix >> 1, pos_y = iy >> 1;
						mean_r[pos_x, pos_y] += col.R;
						mean_g[pos_x, pos_y] += col.G;
						mean_b[pos_x, pos_y] += col.B;
						mean_a[pos_x, pos_y] += col.A;
						mean_c[pos_x, pos_y]++;
						srcRowPtr++;
					}
				}

				for (int iy = 0; iy < dest_rect.Height; iy++) {
					ColorBgra* dstRowPtr = tmp_surface.GetPointAddressUnchecked (0, iy);
					int dy = ((iy + dest_rect.Top  - y) * LUT_Resolution) / rad;
					if (dy < 0) dy = -dy;

					for (int ix = 0; ix < dest_rect.Width; ix++) {

							ColorBgra col = (*dstRowPtr).ToStraightAlpha();

							int dx = ((ix + dest_rect.Left - x) * LUT_Resolution) / rad;
							if (dx < 0) dx = -dx;

							int factor = lut_factor[dx,dy]; 

							int pos_x = ix >> 1, pos_y = iy >> 1;
							int count = mean_c[pos_x, pos_y];

							int red   =  col.R + (col.R - mean_r[pos_x, pos_y] / count);
							int green =  col.G + (col.G - mean_g[pos_x, pos_y] / count);
							int blue  =  col.B + (col.B - mean_b[pos_x, pos_y] / count);
							int alpha =  col.A + (col.A - mean_a[pos_x, pos_y] / count);

							/*
							int diff_red   = (4*red   - tmp[ix-1,iy].R - tmp[ix,iy-1].R - tmp[ix+1,iy].R - tmp[ix,iy+1].R)/4;
							int diff_green = (4*green - tmp[ix-1,iy].G - tmp[ix,iy-1].G - tmp[ix+1,iy].G - tmp[ix,iy+1].G)/4;
							int diff_blue  = (4*blue  - tmp[ix-1,iy].B - tmp[ix,iy-1].B - tmp[ix+1,iy].B - tmp[ix,iy+1].B)/4;
							int diff_alpha = (4*alpha  - tmp[ix-1,iy].A - tmp[ix,iy-1].A - tmp[ix+1,iy].A - tmp[ix,iy+1].A)/4;
							*/
							//red -= diff_red;
							//if ((red & 255) != 0) { //Negative or grater than 255
								if (red > 255) red = 255;
								if (red < 0) red = 0;
							//}
							//green -= diff_green;
							//if ((green & 255) != 0) { //Negative or grater than 255
								if (green > 255) green = 255;
								if (green < 0) green = 0;
							//}
							//blue -= diff_blue;
							//if ((blue & 255) != 0) { //Negative or grater than 255
								if (blue > 255) blue = 255;
								if (blue < 0) blue = 0;
							//}
							//alpha -= diff_alpha;
							//if ((alpha & 255) != 0) { //Negative or grater than 255
								if (alpha > 255) alpha = 255;
								if (alpha < 0) alpha = 0;
							col.R = (byte)((red   * factor + col.R * (512-factor)) >> 9);
							col.G = (byte)((green * factor + col.G * (512-factor)) >> 9);
							col.B = (byte)((blue  * factor + col.B * (512-factor)) >> 9);
							col.A = (byte)((alpha * factor + col.A * (512-factor)) >> 9);
							*dstRowPtr = col.ToPremultipliedAlpha();
		
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


