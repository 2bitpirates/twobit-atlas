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
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml.Linq;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ExplodeSpriteAtlas
{
	public class AtlasGL : IDisposable
	{
		public class Texture
		{
			public int ID;
			public int Width;
			public int Height;
		}

		public AtlasGL()
		{
			Textures = new List<Texture>();
			Glyphs = new Dictionary<int, Glyph>();
		}

		public List<Texture> Textures { get; private set; }
		public Dictionary<int, Glyph> Glyphs { get; private set; }

		public void Load(string file)
		{
			// load as Xml
			var xroot = XElement.Load(file);
			if (xroot.Name != "atlas")
			{
				throw new IOException("Not atlas");
			}

			Clear();

			var basePath = Path.GetDirectoryName(file);

			// load images
			var ximages = xroot.Elements("images");
			if (ximages != null)
			{
				foreach (var node in ximages.Elements("image"))
				{
					var src = node.Attribute("src").Value;

					// image files are always relative to the atlas
					using (var bitmap = (Bitmap)Image.FromFile(Path.Combine(basePath, src)))
					{
						// create GL texture
						int id;
						GL.GenTextures(1, out id);
						GL.BindTexture(TextureTarget.Texture2D, id);
						Textures.Add(new Texture() { ID = id, Width = bitmap.Width, Height = bitmap.Height });

						// lock and draw image on texture
						var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
							ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

						GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
							OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

						bitmap.UnlockBits(data);

						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
					}
				}
			}

			if (Textures.Count == 0)
			{
				throw new IOException("Atlas does not contain any images");
			}

			// load glyph info
			var xglyphs = xroot.Elements("glyphs");
			if (xglyphs != null)
			{
				foreach (var node in xglyphs.Elements("glyph"))
				{
					int ch = Convert.ToInt32(node.Attribute("ch").Value);
					Glyphs.Add(ch, new Glyph(node));
				}
			}
		}

		protected virtual void Clear()
		{
			foreach (var texture in Textures)
			{
				int id = texture.ID;
				GL.DeleteTextures(1, ref id);
			}

			Textures.Clear();
			Glyphs.Clear();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Clear();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
