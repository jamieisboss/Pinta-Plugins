// 
// BrushesCollectionExtension.cs
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
using Pinta.Core;

namespace BrushesCollectionAddin
{
	[Mono.Addins.Extension]
	public class BrushesCollectionExtension : IExtension
	{
		#region IExtension Members
		public void Initialize ()
		{
			PintaCore.PaintBrushes.AddPaintBrush (new LeafBrush ());
			PintaCore.PaintBrushes.AddPaintBrush (new BlurBrush ());
			PintaCore.PaintBrushes.AddPaintBrush (new BlurAltBrush ());
			PintaCore.PaintBrushes.AddPaintBrush (new BlurAltBrush2 ());
			PintaCore.PaintBrushes.AddPaintBrush (new SharpenBrush ());
			PintaCore.PaintBrushes.AddPaintBrush (new SmoothReplaceBrush ());
			PintaCore.PaintBrushes.AddPaintBrush (new IntelligentEraseBrush ());
			PintaCore.PaintBrushes.AddPaintBrush (new UVPaintBrush ());
		}

		public void Uninitialize ()
		{
			PintaCore.PaintBrushes.RemoveInstanceOfPaintBrush (typeof (LeafBrush));
			PintaCore.PaintBrushes.RemoveInstanceOfPaintBrush (typeof (BlurBrush));
			PintaCore.PaintBrushes.RemoveInstanceOfPaintBrush (typeof (BlurAltBrush));
			PintaCore.PaintBrushes.RemoveInstanceOfPaintBrush (typeof (BlurAltBrush2));
			PintaCore.PaintBrushes.RemoveInstanceOfPaintBrush (typeof (SharpenBrush));
			PintaCore.PaintBrushes.RemoveInstanceOfPaintBrush (typeof (SmoothReplaceBrush));
			PintaCore.PaintBrushes.RemoveInstanceOfPaintBrush (typeof (IntelligentEraseBrush));
		}
		#endregion
	}
}

