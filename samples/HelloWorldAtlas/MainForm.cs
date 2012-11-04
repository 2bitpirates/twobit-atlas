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
using System.Windows.Forms;

namespace HelloWorldAtlas
{
	partial class MainForm : Form
	{
		private Atlas currentAtlas = null;

		// button-font.atlas from the samples has a button image at character 256
		private readonly string defaultText = String.Format("Hello World! Press {0} to do nothing. This is from the atlas font.", (char)256);

		public MainForm()
		{
			InitializeComponent();

			// turn on double buffering for flicker free drawing
			DoubleBuffered = true;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// clean up the atlas resources
				if (currentAtlas != null)
				{
					currentAtlas.Dispose();
				}

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnDragOver(DragEventArgs e)
		{
			// allow explorer file drag-n-drop
			if (!e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = DragDropEffects.None;
				return;
			}

			e.Effect = DragDropEffects.Link;
		}

		protected override void OnDragDrop(DragEventArgs e)
		{
			// get the array of files dropped
			var files = e.Data.GetData(DataFormats.FileDrop) as string[];

			// attempt load an atlas file
			LoadAtlas(files);
		}

		private void LoadAtlas(string[] files)
		{
			// no files
			if (files == null || files.Length == 0)
			{
				return;
			}

			// only try to load the first file dropped
			var file = files[0];

			// create a temp atlas while loading
			var tempAtlas = new Atlas();

			try
			{
				// load it
				tempAtlas.Load(file);

				// dispose the current atlas
				if (currentAtlas != null)
				{
					currentAtlas.Dispose();
				}

				// set the temp atlas as the main atlas
				currentAtlas = tempAtlas;

				// redraw
				Invalidate();
			}
			catch (Exception ex)
			{
				// cleanup the temp atlas
				if (tempAtlas != null)
				{
					tempAtlas.Dispose();
				}

				// show error
				MessageBox.Show(this, "Unable to load atlas. Check file: " + ex.Message, "Warning", MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}
		}

		protected virtual void DrawAtlas(Graphics g, string text, int x, int y)
		{
			Point pt = Point.Empty;
			RectangleF r = RectangleF.Empty;

			foreach (var ch in text)
			{
				Glyph glyph;

				// check if the character is in the atlas
				if (currentAtlas.Glyphs.TryGetValue(ch, out glyph))
				{
					// apply the glyphs origin
					pt.X = x + glyph.Offset.X;
					pt.Y = y + glyph.Offset.Y;

					// get the correct image. usually an atlas only has one
					var img = currentAtlas.Images[glyph.Page];

					// get the glyph rectangle region on the image
					r.X = glyph.UV.X;
					r.Y = glyph.UV.Y;
					r.Width = glyph.Size.Width;
					r.Height = glyph.Size.Height;

					// draw it
					g.DrawImage(img, pt.X, pt.Y, r, GraphicsUnit.Pixel);

					// use ABC kerning to position next character
					x += glyph.A + glyph.B + glyph.C;
				}
			}
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			// redraw on window sizing
			Invalidate();
		}

		private string DisplayText
		{
			get
			{
				// use the default text if the text box value is empty
				return !String.IsNullOrWhiteSpace(textBox1.Text) ? textBox1.Text : defaultText;
			}

			set { textBox1.Text = value; }
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			var g = e.Graphics;
			var r = ClientRectangle;

			// check if we have an atlas
			if (currentAtlas != null)
			{
				// draw string
				DrawAtlas(g, DisplayText, 20, r.Height / 2);
			}
			else
			{
				// no atlas display a friendly how-to
				g.DrawString("Drag-n-Drop an atlas file (that has a font) to view.", Font, Brushes.White, r);
			}
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			// redraw the screen win the textbox value has changed
			Invalidate();
		}
	}
}
