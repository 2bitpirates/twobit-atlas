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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Linq;
using System.Drawing.Imaging;

using TwoBit.Extensions;
using TwoBit.Utilities;

namespace TwoBit.Atlas
{
	public class AtlasBuilder : IAtlasBuilder
	{
		private IServiceProvider service;

		public AtlasBuilder(IServiceProvider service)
		{
			this.service = service;
		}

		public IAtlasDescriptor CreateAtlasDescriptor()
		{
			return new AtlasDescriptor(service);
		}

		public IImageInfo ImageInfoFromFile(string file)
		{
			return new ImageInfo(file);
		}

		public void ValidateImageInfo(IImageInfo ii, IAtlasDescriptor descriptor)
		{
			if (ii.Image != null && (ii.Image.Width > descriptor.MaxSize || ii.Image.Height > descriptor.MaxSize))
			{
				throw new Exception("Image too large for sheet");
			}
		}

		public GlyphDescriptorCollection CollectGlyphs(IAtlasDescriptor descriptor, Action<float> progress, CancellationTokenSource cts)
		{
			ValidateSettings(descriptor);
			GlyphDescriptorCollection glyphs = new GlyphDescriptorCollection();

			HashSet<char> chars = null;

			if (descriptor.UseFonts)
			{
				chars = new HashSet<char>();

				// build set of characters
				foreach (ICharSet charSet in descriptor.CharSets)
				{
					foreach (char ch in charSet.Characters)
					{
						chars.Add(ch);
					}
				}
			}

			int count = 0;
			count += (descriptor.UseImages) ? descriptor.Images.Count : 0;
			count += (chars != null) ? chars.Count : 0;
			int current = 0;

			if (descriptor.UseImages)
			{
				int ch = descriptor.StartCode;

				foreach (ImageInfo ii in descriptor.Images)
				{
					int width = ii.Image.Width;
					int height = ii.Image.Height;

					Vec2i offset;
					Image image = CropImage(ii.Image, out offset);

					ABC abc = new ABC();
					abc.abcA = -(int)ii.Offset.X;
					abc.abcB = (uint)(width + ii.Offset.X);
					abc.abcC = 0;

					ii.SortCode = ii.HasCustomCode ? ii.Code : UniqueCode(ref ch, descriptor.Images);
					GlyphDescriptor desc = new GlyphDescriptor((char)ii.SortCode, abc, image, descriptor.Spacing * 2);
					desc.Offset = offset - (Vec2i)ii.Offset;
					glyphs.Add(desc);

					if (progress != null)
					{
						progress((float)(++current) / count);
					}

					if (cts != null)
					{
						cts.Token.ThrowIfCancellationRequested();
					}
				}
			}

			if (descriptor.UseFonts)
			{
				// build image for each glyph
				GLYPHMETRICS gm = new GLYPHMETRICS();
				foreach (char ch in chars)
				{
					Image image = null;

					try
					{
						if (ch == 159)
						{
						}

						image = TextHelper.GetGlyphOutlineImage(descriptor.Font, ch, out gm);
						if (image == null)
						{
							continue;
						}
					}
					catch
					{
						if (image != null)
						{
							image.Dispose();
						}

						// skip glyph
						continue;
					}

					// flip Y offset
					ABC abc = new ABC();
					TextHelper.GetCharABCWidths(descriptor.Font, ch, out abc);

					GlyphDescriptor glyph = new GlyphDescriptor(ch, abc, image, descriptor.Spacing * 2);
					glyph.Offset = new Vec2i(gm.gmptGlyphOrigin.X, -gm.gmptGlyphOrigin.Y);
					glyphs.Add(glyph);

					if (progress != null)
					{
						progress((float)(++current) / count);
					}

					if (cts != null)
					{
						cts.Token.ThrowIfCancellationRequested();
					}
				}

				if (descriptor.ForceSpace && glyphs.Find(32) == null)
				{
					ABC abc = new ABC();
					TextHelper.GetCharABCWidths(descriptor.Font, (char)32, out abc);
					abc.abcB = (uint)(abc.abcA + abc.abcB + abc.abcC);
					abc.abcC = 0;
					glyphs.Add(new GlyphDescriptor((char)32, abc));
				}
			}

			return glyphs;
		}

		public IEnumerable<Vec2i> PlaceGlyphs(GlyphDescriptorCollection glyphs, IAtlasDescriptor descriptor, Action<float> progress, CancellationTokenSource cts)
		{
			List<Vec2i> sizes = new List<Vec2i>();

			if (descriptor.Alignment == GlyphAlignment.Grid)
			{
				// sort by code
				glyphs.Sort((a, b) => (int)a.CH - (int)b.CH);
				Vec2i sz;
				GridGlyphPosition(glyphs, descriptor, out sz, progress, cts);
				sizes.Add(sz);
			}
			else
			{
				// sort by size
				glyphs.Sort((a, b) => b.Size - a.Size);
				SlotGlyphPosition(glyphs, descriptor, sizes, progress, cts);
			}

			return sizes;
		}

		public IEnumerable<Image> BuildImage(IEnumerable<Vec2i> sizes, GlyphDescriptorCollection glyphs, IAtlasDescriptor descriptor)
		{
			List<Image> images = new List<Image>();

			foreach (var sz in sizes)
			{
				images.Add(new Bitmap(sz.X, sz.Y, System.Drawing.Imaging.PixelFormat.Format32bppArgb));
			}

			// place glyphs on texture
			foreach (GlyphDescriptor gi in glyphs)
			{
				var image = images[gi.Page];
				using (Graphics g = Graphics.FromImage(image))
				{
					if (gi.Image != null)
					{
						g.DrawImage(gi.Image, gi.X + descriptor.Spacing, gi.Y + descriptor.Spacing, gi.Image.Width, gi.Image.Height);
					}
				}
			}

			return images;
		}

		public void Save(string filename, IEnumerable<Image> images, GlyphDescriptorCollection glyphs, IAtlasDescriptor descriptor, SpriteDescriptor sprite)
		{
			// convert to fully qualified path
			var dir = Path.GetDirectoryName(filename);
			string path;

			if (String.IsNullOrWhiteSpace(dir))
			{
				path = Directory.GetCurrentDirectory();
			}
			else
			{
				DirectoryInfo di = new DirectoryInfo(dir);
				// ensure directory exists
				di.Create();
				path = di.FullName;
			}

			string ext = Path.GetExtension(filename);
			string file = Path.GetFileNameWithoutExtension(filename);
			string atlasBase = file + (String.IsNullOrEmpty(ext) ? ".atlas" : ext);
			string dataFile = Path.Combine(path, atlasBase);
			string spriteFile = Path.Combine(path, file) + ".sprite";

			// sort by code
			glyphs.Sort((a, b) => (int)a.CH - (int)b.CH);

			// save xml
			var docCreator = service.GetService<IXmlDocCreator>();
			if (docCreator == null)
			{
				throw new InvalidOperationException("Xml Document Creator not defeind");
			}

			var doc = docCreator.CreateDoc();

			object root = doc.AddRoot("atlas");
			doc.SetAttrib(root, "glyphs", glyphs.Count);

			if (descriptor.UseFonts)
			{
				doc.SetAttrib(root, "fontheight", descriptor.FontHeight);
			}

			// save images
			int count = 0;
			object xi = doc.AddNode(root, "images");
			foreach (var image in images)
			{
				var ifile = file + (count++).ToString() + ".png";
				object node = doc.AddNode(xi, "image");
				doc.SetAttrib(node, "src", ifile);
				doc.SetAttrib(node, "width", image.Width);
				doc.SetAttrib(node, "height", image.Height);
				image.Save(Path.Combine(path, ifile), ImageFormat.Png);
			}

			if (descriptor.Alignment == GlyphAlignment.Grid)
			{
				doc.SetAttrib(root, "cellwidth", descriptor.GridSize.X);
				doc.SetAttrib(root, "cellheight", descriptor.GridSize.Y);
				doc.SetAttrib(root, "gridcols", descriptor.GridCells.X);
				doc.SetAttrib(root, "gridrows", descriptor.GridCells.Y);
			}

			#region save glyphs
			object glyph = doc.AddNode(root, "glyphs");
			foreach (GlyphDescriptor gi in glyphs)
			{
				object node = doc.AddNode(glyph, "glyph");
				doc.SetAttrib(node, "ch", (int)gi.CH);
				doc.SetAttrib(node, "p", gi.Page);
				doc.SetAttrib(node, "x", gi.X + descriptor.Spacing);
				doc.SetAttrib(node, "y", gi.Y + descriptor.Spacing);
				doc.SetAttrib(node, "w", gi.Width - descriptor.Spacing * 2);
				doc.SetAttrib(node, "h", gi.Height - descriptor.Spacing * 2);
				doc.SetAttrib(node, "a", gi.ABC.abcA);
				doc.SetAttrib(node, "b", gi.ABC.abcB);
				doc.SetAttrib(node, "c", gi.ABC.abcC);
				doc.SetAttrib(node, "ox", gi.Offset.X);
				doc.SetAttrib(node, "oy", gi.Offset.Y);

				if (gi.ImageInfo != null)
				{
					doc.SetAttrib(node, "name", Path.GetFileName(gi.ImageInfo.FileInfo.Name));
				}
			}
			#endregion save glyphs

			#region save editor info
			object editor = doc.AddNode(root, "info");

			// save atlas info
			object info = doc.AddNode(editor, "common");
			doc.SetAttrib(info, "usefonts", descriptor.UseFonts);
			doc.SetAttrib(info, "useimages", descriptor.UseImages);
			doc.SetAttrib(info, "alignment", descriptor.Alignment);
			doc.SetAttrib(info, "spacing", descriptor.Spacing);
			doc.SetAttrib(info, "powertwo", descriptor.PowerTwo);
			doc.SetAttrib(info, "maxsize", descriptor.MaxSize);
			doc.SetAttrib(info, "makesprite", descriptor.MakeSprite);

			// save font info
			object font = doc.AddNode(editor, "font");
			doc.SetAttrib(font, "name", descriptor.FontName);
			doc.SetAttrib(font, "size", descriptor.FontSize);
			doc.SetAttrib(font, "bold", descriptor.FontBold);
			doc.SetAttrib(font, "italic", descriptor.FontItalic);

			object charset = doc.AddNode(font, "charsets");
			foreach (var set in descriptor.CharSets)
			{
				var attrib = Reflect.GetAttribute<DataNameAttribute>(set);
				object node = doc.AddNode(charset, "charset");
				doc.SetAttrib(node, "name", attrib != null ? attrib.Name : set.GetType().Name);
			}

			// save sprite info
			if (sprite != null && descriptor.MakeSprite)
			{
				object spr = doc.AddNode(editor, "sprite");
				doc.SetAttrib(spr, "rate", sprite.Rate);
				doc.SetAttrib(spr, "overflow", sprite.Overflow);
				doc.SetAttrib(spr, "origin", sprite.Origin);
			}

			// save image info
			object img = doc.AddNode(editor, "image");
			doc.SetAttrib(img, "startcode", descriptor.StartCode);

			object imgNode = doc.AddNode(img, "images");
			foreach (var ii in descriptor.Images)
			{
				object node = doc.AddNode(imgNode, "image");
				string src = ii.FileInfo.GetRelativePath(path);
				doc.SetAttrib(node, "src", src);
				doc.SetAttrib(node, "ox", ii.Offset.X);
				doc.SetAttrib(node, "oy", ii.Offset.Y);
				doc.SetAttrib(node, "code", ii.Code);
				doc.SetAttrib(node, "usecode", ii.HasCustomCode);
				doc.SetAttrib(node, "loc", ii.Location);
				doc.SetAttrib(node, "angle", ii.Angle);
			}
			#endregion save editor info

			// save it
			doc.Save(dataFile);

			if (sprite != null && descriptor.MakeSprite)
			{
				var sdoc = docCreator.CreateDoc();
				object sr = sdoc.AddRoot("sprite");
				sdoc.SetAttrib(sr, "rate", sprite.Rate);
				sdoc.SetAttrib(sr, "overflow", sprite.Overflow);
				sdoc.SetAttrib(sr, "origin", sprite.Origin);

				// string GetRelativePath
				object x0 = sdoc.AddNode(sr, "elements");
				foreach (var gi in glyphs)
				{
					object x1 = sdoc.AddNode(x0, "element");
					sdoc.SetAttrib(x1, "src", atlasBase);
					sdoc.SetAttrib(x1, "index", (int)gi.CH);
				}
				sdoc.Save(spriteFile);
			}
		}

		protected virtual void ValidateSettings(IAtlasDescriptor descriptor)
		{
			if (descriptor.UseFonts && (descriptor.Font == null || String.IsNullOrEmpty(descriptor.FontName)))
			{
				throw new ArgumentException("Error: Invalid font name provided.");
			}

			if (!descriptor.UseFonts && !descriptor.UseImages)
			{
				throw new ArgumentException("Error: Both fonts and images are disabled.  Nothing to do.");
			}

			if (descriptor.MultiTexture && descriptor.Alignment == GlyphAlignment.Grid)
			{
				throw new ArgumentException("Error: Grid alignment does not support multi texturing.");
			}
		}

		public Image CropImage(Image image, out Vec2i offset)
		{
			// find crop area
			Vec2i min = new Vec2i(int.MaxValue, int.MaxValue);
			Vec2i max = new Vec2i(int.MinValue, int.MinValue);
			Vec2i maxVal = min;

			var img = (Bitmap)image.Clone();
			try
			{
				var bmp = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

				unsafe
				{
					byte* scan0 = (byte*)bmp.Scan0.ToPointer();
					int incx, incy = 0;

					for (int y = 0; y < img.Height; ++y)
					{
						incx = incy;
						for (int x = 0; x < img.Width; ++x, incx += 4)
						{
							if (*(scan0 + incx + 3) != 0)
							{
								if (x < min.X) min.X = x;
								if (y < min.Y) min.Y = y;

								if (x > max.X) max.X = x;
								if (y > max.Y) max.Y = y;
							}
						}

						incy += bmp.Stride;
					}
				}

				img.UnlockBits(bmp);

				// crop it
				if (min == maxVal)
				{
					offset = Vec2i.Zero;
					return (Image)img.Clone();
				}
				else
				{
					offset = min;
					Rectangle cropArea = new Rectangle(min.X, min.Y, max.X - min.X + 1, max.Y - min.Y + 1);
					return img.Clone(cropArea, img.PixelFormat);
				}
			}
			finally
			{
				img.Dispose();
			}
		}

		public int UniqueCode(ref int ch, IEnumerable<IImageInfo> images)
		{
			int code = ch;
			while (images.FirstOrDefault((a) => a.HasCustomCode && a.Code == code) != null)
			{
				code++;
			}

			ch = code + 1;
			return code;
		}

		private void SlotGlyphPosition(GlyphDescriptorCollection glyphs, IAtlasDescriptor descriptor, List<Vec2i> sizes, Action<float> progress, CancellationTokenSource cts)
		{
			long big = (long)descriptor.MaxSize;
			byte[] scan = new byte[big * big];
			Vec2i minsize = Vec2i.Zero;

			int count = glyphs.Count;
			int current = 0;
			int page = 0;

			foreach (var gi in glyphs)
			{
				if (!FindSlot(descriptor, scan, gi, ref minsize))
				{
					if (descriptor.MultiTexture)
					{
						sizes.Add(minsize);

						// inc to next texture
						page++;

						// clear
						Array.Clear(scan, 0, scan.Length);
						minsize = Vec2i.Zero;

						// find again and place on new texture
						FindSlot(descriptor, scan, gi, ref minsize);
					}
					else
					{
						throw new Exception("Unable to fit glyphs on a single texture");
					}
				}

				gi.Page = page;

				// mark glyph area used
				long incr0 = (gi.Y) * big;
				for (long y = 0; y < gi.Height; y++)
				{
					long incrx1 = incr0 + gi.X;
					for (long x = 0; x < gi.Width; x++)
					{
						scan[incrx1 + x] = 0xff;
					}
					incr0 += big;
				}

				if (progress != null)
				{
					progress((float)(++current) / count);
				}

				if (cts != null)
				{
					cts.Token.ThrowIfCancellationRequested();
				}
			}

			// add the last page (or first page if single texture)
			if (sizes.Count <= page)
			{
				sizes.Add(minsize);
			}

			if (descriptor.PowerTwo)
			{
				for (int i = 0; i < sizes.Count; i++)
				{
					int mm = MathEx.HighPow2(Math.Max(sizes[i].X, sizes[i].Y));
					sizes[i] = new Vec2i(mm, mm);
				}
			}
			else
			{
				// calc tightest box
				foreach (var gi in glyphs)
				{
					Vec2i sz = sizes[gi.Page];
					sz.X = Math.Max(sz.X, gi.X + gi.Width);
					sz.Y = Math.Max(sz.Y, gi.Y + gi.Height);
					sizes[gi.Page] = sz;
				}
			}
		}

		private bool FindSlot(IAtlasDescriptor descriptor, byte[] scan, GlyphDescriptor gi, ref Vec2i minsize)
		{
			long reqWidth = gi.Width;
			long reqHeight = gi.Height;
			long big = (long)descriptor.MaxSize;

			if (reqHeight == 0 || reqWidth == 0)
			{
				gi.X = (int)0;
				gi.Y = (int)0;
				return true;
			}

			// try to find an empty space within the current used box
			for (long y = 0; y < (minsize.Y - reqHeight); y++)
			{
				for (long x = 0; x < (minsize.X - reqWidth); x++)
				{
					long cx, cy;
					long incr = y * big;
					for (cy = y; cy < y + reqHeight; cy++)
					{
						for (cx = x; cx < x + reqWidth; cx++)
						{
							if (scan[incr + cx] != 0)
							{
								cy = 9999;
								break;
							}
						}
						incr += big;
					}

					// put it here
					if (cy < 9999)
					{
						gi.X = (int)x;
						gi.Y = (int)y;
						return true;
					}
				}
			}

			// if no space was found, expand the smallest edge of the used box, 
			// which will keep the texture close to square
			if (minsize.X < minsize.Y)
			{
				if (minsize.X == descriptor.MaxSize - 1)
				{
					return false;
				}

				minsize.X += (int)reqWidth;
				if (minsize.X >= descriptor.MaxSize)
				{
					minsize.X = descriptor.MaxSize - 1;
				}
			}
			else
			{
				if (minsize.Y == descriptor.MaxSize - 1)
				{
					return false;
				}

				minsize.Y += (int)reqHeight;
				if (minsize.Y >= descriptor.MaxSize)
				{
					minsize.Y = descriptor.MaxSize - 1;
				}
			}

			return FindSlot(descriptor, scan, gi, ref minsize);
		}

		private void GridGlyphPosition(GlyphDescriptorCollection glyphs, IAtlasDescriptor descriptor, out Vec2i texSize, Action<float> progress, CancellationTokenSource cts)
		{
			texSize = Vec2i.Zero;

			int mx = 0;
			int my = 0;

			// calc max sizes
			foreach (GlyphDescriptor gi in glyphs)
			{
				mx = Math.Max(mx, gi.Width);
				my = Math.Max(my, gi.Height);
			}

			// calc smallest size block
			int count = glyphs.Count;
			int sz = Math.Max(mx, my);
			int dim = sz;
			int cx = 0;

			while (dim <= descriptor.MaxSize)
			{
				cx = dim / sz;
				int cnt = cx * cx;
				if (count <= cnt)
				{
					break;
				}

				// increase
				dim += sz;
			}

			if (dim > descriptor.MaxSize)
			{
				throw new Exception("Unable to fit glyphs on a single image");
			}

			dim = descriptor.PowerTwo ? MathEx.HighPow2(dim + 1) : dim;

			// position
			int current = 0;
			foreach (GlyphDescriptor gi in glyphs)
			{
				gi.X = (current % cx) * sz;
				gi.Y = (current / cx) * sz;
				++current;

				if (progress != null)
				{
					progress((float)(current) / count);
				}

				if (cts != null)
				{
					cts.Token.ThrowIfCancellationRequested();
				}
			}

			Vec2i size = Vec2i.Zero;
			size.X = cx;
			size.Y = ((count - 1) / cx) + 1;
			descriptor.GridCells = size;

			size.X = sz;
			size.Y = sz;
			descriptor.GridSize = size;

			if (descriptor.PowerTwo)
			{
				texSize = new Vec2i(dim, dim);
			}
			else
			{
				// calc tightest box
				foreach (GlyphDescriptor gi in glyphs)
				{
					texSize.X = Math.Max(texSize.X, gi.X + gi.Width);
					texSize.Y = Math.Max(texSize.Y, gi.Y + gi.Height);
				}
			}
		}
	}
}
