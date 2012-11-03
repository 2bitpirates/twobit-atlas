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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

using TwoBit.Utilities;
using TwoBit.Extensions;

namespace TwoBit.Atlas
{
	public class AtlasDescriptor : IAtlasDescriptor
	{
		private string fontName;
		private float fontSize;
		private int fontSpacing;
		private int startCode;
		private FontStyle fontStyle;
		private TEXTMETRIC tm;
		private List<IImageInfo> images;
		private List<ICharSet> charSets;
		private Font font;
		private IServiceProvider service;

		public event EventHandler FontChanged;

		public AtlasDescriptor(IServiceProvider service)
		{
			this.service = service;

			GridSize = Vec2i.Zero;
			GridCells = Vec2i.Zero;
			images = new List<IImageInfo>();
			charSets = new List<ICharSet>();

			Reset();
		}

		public void Reset()
		{
			UseFonts = false;
			UseImages = false;
			MakeSprite = false;
			Alignment = GlyphAlignment.BestFit;
			PowerTwo = false;
			fontSpacing = 1;
			fontName = "Arial";
			fontSize = 24;
			fontStyle = FontStyle.Regular;
			charSets.Clear();

			// attempt to add an Ascii character set as default
			var charSetProvider = service.GetService<ICharSetProvider>();
			if (charSetProvider != null)
			{
				var charSet = charSetProvider.Factory.Make("ascii");
				if (charSet != null)
				{
					charSets.Add(charSet);
				}
			}

			RebuildFont();

			startCode = 0;
			MaxSize = 4096;
			ForceSpace = true;

			DisposeImages();
		}

		private void DisposeImages()
		{
			foreach (IDisposable image in images)
			{
				image.Dispose();
			}
			images.Clear();
		}

		protected virtual void OnFontChanged(EventArgs e)
		{
			EventHandler eh = FontChanged;
			if (eh != null)
			{
				eh(this, e);
			}
		}

		protected virtual void RebuildFont()
		{
			if (font != null)
			{
				font.Dispose();
			}

			font = new Font(FontName, FontSize, fontStyle, GraphicsUnit.Pixel);
			if (font != null)
			{
				TextHelper.GetTextMetrics(font, out tm);
			}
			else
			{
				tm = TEXTMETRIC.Default;
			}

			OnFontChanged(EventArgs.Empty);
		}

		#region editor properties
		[Category("Appearance")]
		[Description("Set to True to enable fonts on the atlas")]
		[DefaultValue(false)]
		[AtlasMutable]
		public bool UseFonts { get; set; }

		[Category("Appearance")]
		[Description("Set to True to enable custom images on the atlas")]
		[DefaultValue(false)]
		[AtlasMutable]
		public bool UseImages { get; set; }

		[Category("Layout")]
		[Description("The method glyphs are positioned on the atlas")]
		[DefaultValue(GlyphAlignment.BestFit)]
		[AtlasMutable]
		public GlyphAlignment Alignment { get; set; }

		[Category("Layout")]
		[Description("Surrounding spacing between glyphs on the atlas (in pixels)")]
		[DefaultValue(1)]
		[AtlasMutable]
		public int Spacing
		{
			get { return fontSpacing; }
			set { fontSpacing = Math.Max(value, 0); }
		}

		[Category("Layout")]
		[Description("Set to True to enforce the atlas dimensions to be a power of 2")]
		[DefaultValue(false)]
		[AtlasMutable]
		public bool PowerTwo { get; set; }

		[Category("Layout")]
		[Description("Set to True to enabled support for multi textures")]
		[DefaultValue(false)]
		[AtlasMutable]
		public bool MultiTexture { get; set; }

		[Category("Layout")]
		[Description("Set to True to generate an associated sprite")]
		[DefaultValue(false)]
		public bool MakeSprite { get; set; }

		[Category("Fonts")]
		[DefaultValue(true)]
		[Description("When using fonts, set to True to force adding a Space glyph")]
		public bool ForceSpace { get; set; }

		[DisplayName("Font")]
		[Category("Fonts")]
		[TypeConverter(typeof(FontConverter.FontNameConverter))]
		[Editor("System.Drawing.Design.FontNameEditor, System.Drawing.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
		[DefaultValue("Arial")]
		[Description("Name of the system font")]
		[AtlasMutable(AtlasMutable.Font)]
		public string FontName
		{
			get { return fontName; }
			set
			{
				fontName = value;
				RebuildFont();
			}
		}

		[DisplayName("Size")]
		[Category("Fonts")]
		[Description("Size of the font in (pixels)")]
		[DefaultValue(24.0f)]
		[AtlasMutable(AtlasMutable.Font)]
		public float FontSize
		{
			get { return fontSize; }
			set
			{
				fontSize = Math.Max(4.0f, value);
				RebuildFont();
			}
		}

		[DisplayName("Bold")]
		[Category("Fonts")]
		[Description("Set to True for bold style")]
		[DefaultValue(false)]
		[AtlasMutable(AtlasMutable.Font)]
		public bool FontBold
		{
			get { return (fontStyle & FontStyle.Bold) == FontStyle.Bold; }
			set
			{
				if (value)
				{
					fontStyle |= FontStyle.Bold;
				}
				else
				{
					fontStyle &= ~FontStyle.Bold;
				}

				RebuildFont();
			}
		}

		[DisplayName("Italic")]
		[Category("Fonts")]
		[Description("Set to True for italic style")]
		[DefaultValue(false)]
		[AtlasMutable(AtlasMutable.Font)]
		public bool FontItalic
		{
			get { return (fontStyle & FontStyle.Italic) == FontStyle.Italic; }
			set
			{
				if (value)
				{
					fontStyle |= FontStyle.Italic;
				}
				else
				{
					fontStyle &= ~FontStyle.Italic;
				}

				RebuildFont();
			}
		}

		[Category("Images")]
		[Description("Inital glyph code for images")]
		[DefaultValue(0)]
		public int StartCode { get { return startCode; } set { startCode = Math.Max(0, value); } }
		#endregion

		[Browsable(false)]
		public IList<IImageInfo> Images { get { return images; } }

		[Browsable(false)]
		public IList<ICharSet> CharSets { get { return charSets; } }

		[Browsable(false)]
		public Vec2i GridSize { get; set; }

		[Browsable(false)]
		public Vec2i GridCells { get; set; }

		[Browsable(false)]
		public int MaxSize { get; set; }

		[Browsable(false)]
		public Font Font
		{
			get { return font; }
			set
			{
				if (font != null)
				{
					font.Dispose();
				}

				font = value;
				if (font != null)
				{
					fontSize = font.Size;
					fontName = font.Name;
					fontStyle = font.Style;
					TextHelper.GetTextMetrics(font, out tm);
				}
				else
				{
					tm = TEXTMETRIC.Default;
				}
			}
		}

		[Browsable(false)]
		public int FontHeight { get { return tm.tmHeight; } }

		[Browsable(false)]
		internal TEXTMETRIC TextMetric { get { return tm; } }

		public void SortImages()
		{
			images.Sort((a, b) => String.Compare(a.FileInfo.Name, b.FileInfo.Name));
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (font != null)
				{
					font.Dispose();
					font = null;
				}

				DisposeImages();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
