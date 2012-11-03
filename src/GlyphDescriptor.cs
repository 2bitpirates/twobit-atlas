/*
Copyright 2012 Scott Ramsay

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Drawing;
using System.Collections.Generic;

using TwoBit.Utilities;

namespace TwoBit.Atlas
{
	public class GlyphDescriptor : IDisposable
	{
		public GlyphDescriptor(char ch, ABC abc)
		{
			CH = ch;
			ABC = abc;
		}

		public GlyphDescriptor(char ch, ABC abc, Image image, int spacing)
		{
			CH = ch;
			Image = image;
			Width = image.Width + spacing;
			Height = image.Height + spacing;
			Size = Width * Height;
			ABC = abc;
		}

		public int X { get; set; }
		public int Y { get; set; }
		public Vec2i Offset { get; set; }
		public char CH { get; private set; }
		public Image Image { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		public int Size { get; private set; }
		public ABC ABC { get; private set; }
		public IImageInfo ImageInfo { get; set; }
		public int Page { get; set; }

		protected virtual void Dispose(bool disposing)
		{
			if (Image != null)
			{
				Image.Dispose();
				Image = null;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}

	public class GlyphDescriptorCollection : List<GlyphDescriptor>
	{
		public GlyphDescriptor Find(int ch)
		{
			return this.Find((a) => (int)a.CH == ch);
		}
	}
}
