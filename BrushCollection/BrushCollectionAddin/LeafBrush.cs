// 
// LeafBrush.cs
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
	public class LeafBrush : BasePaintBrush
	{
		public override string Name {
			get { return AddinManager.CurrentLocalizer.GetString ("Leaf"); }
		}

		public override double StrokeAlphaMultiplier {
			get { return 0.05; }
		}

		protected override Gdk.Rectangle OnMouseMove (Context g, Color strokeColor, ImageSurface surface,
		                                              int x, int y, int lastX, int lastY)
		{
			int dx = x - lastX;
			int dy = y - lastY;
			double d = Math.Sqrt (dx * dx + dy * dy) * 2.0;

			double cx = x;
			double cy = y;

			int steps = Random.Next (1, 10);
			double step_delta = d / steps;

			for (int i = 0; i < steps; i++) {
				g.MoveTo(new PointD(cx,cy));
				g.LineTo(new PointD(cx+dx*i*g.LineWidth/2.5,cy+dy*i*g.LineWidth/2.5));
				g.Stroke ();
			}

			return Gdk.Rectangle.Zero;
		}
	}
}
