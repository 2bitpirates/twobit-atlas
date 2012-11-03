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
using System.Drawing;

namespace TwoBit.Atlas
{
	class ImageInfo : IImageInfo
	{
		private Vec2f offset;

		public ImageInfo(FileInfo file, Image image, float ox, float oy)
		{
			offset.X = ox;
			offset.Y = oy;
			FileInfo = file;
			CloneImage(image);
		}

		public ImageInfo(string file)
		{
			offset = Vec2f.Zero;
			FileInfo = new FileInfo(file);
			using (var image = Bitmap.FromFile(file))
			{
				CloneImage(image);
			}
		}

		protected virtual void CloneImage(Image image)
		{
			// draw image instead of clone to release file lock on source
			Image = new Bitmap(image.Width, image.Height, image.PixelFormat);
			using (var g = Graphics.FromImage(Image))
			{
				g.DrawImage(image, 0, 0, image.Width, image.Height);
			}
		}

		public FileInfo FileInfo { get; private set; }
		public Image Image { get; private set; }
		public Vec2f Offset { get { return offset; } set { offset = value; } }
		public int Code { get; set; }
		public bool HasCustomCode { get; set; }
		public int SortCode { get; set; }
		public Vec2f Location { get; set; }
		public float Angle { get; set; }
		public object Tag { get; set; }

		public override string ToString()
		{
			return String.Format("{0} ({1}x{2})", Path.GetFileName(FileInfo.Name), Image.Width, Image.Height);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Image != null)
				{
					Image.Dispose();
					Image = null;
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
