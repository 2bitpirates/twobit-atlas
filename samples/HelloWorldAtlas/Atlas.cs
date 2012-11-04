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
using System.Xml.Linq;

namespace HelloWorldAtlas
{
	public class Atlas : IDisposable
	{
		public Atlas()
		{
			Images = new List<Image>();
			Glyphs = new Dictionary<int, Glyph>();
		}

		public List<Image> Images { get; private set; }
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
					Images.Add(Image.FromFile(Path.Combine(basePath, src)));
				}
			}

			if (Images.Count == 0)
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
			foreach (var image in Images)
			{
				image.Dispose();
			}

			Images.Clear();
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
