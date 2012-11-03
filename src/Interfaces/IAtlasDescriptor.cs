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

namespace TwoBit.Atlas
{
	/// <summary>
	///		Settings that defined how an atlas should be generated
	/// </summary>
	public interface IAtlasDescriptor : IDisposable
	{
		/// <summary>
		/// Set to True to enable fonts (default False)
		/// </summary>
		bool UseFonts { get; set; }

		/// <summary>
		/// Set to True to enable custom images on the atlas (default False)
		/// </summary>
		bool UseImages { get; set; }

		/// <summary>
		/// Surrounding spacing between glyphs on the atlas in pixels (default 1)
		/// </summary>
		int Spacing { get; set; }

		/// <summary>
		/// Set to True to enforce the atlas dimensions to be a power of 2 (default False)
		/// </summary>
		bool PowerTwo { get; set; }

		/// <summary>
		/// When using fonts, set to True to force adding a Space glyph
		/// </summary>
		bool ForceSpace { get; set; }

		/// <summary>
		/// Set to True to generate an associated sprite (default False)
		/// </summary>
		bool MakeSprite { get; set; }

		/// <summary>
		/// Set to True to support generating more than one image per atlas (default False)
		/// </summary>
		bool MultiTexture { get; set; }

		/// <summary>
		/// Name of font to use on the atlas
		/// </summary>
		string FontName { get; set; }

		/// <summary>
		/// Size of the font in points for 72dpi (default 24.0f)
		/// </summary>
		float FontSize { get; set; }

		/// <summary>
		/// Set to True to use a bold style (default False)
		/// </summary>
		bool FontBold { get; set; }

		/// <summary>
		/// Set to True to use an italic style (default False)
		/// </summary>
		bool FontItalic { get; set; }

		/// <summary>
		/// Inital glyph code for images  (default 0)
		/// </summary>
		int StartCode { get; set; }

		/// <summary>
		/// Returns the pixel height of the font from the base line
		/// </summary>
		int FontHeight { get; }

		/// <summary>
		/// For Grid layout: size of each cell in pixels
		/// </summary>
		Vec2i GridSize { get; set; }

		/// <summary>
		/// For Grid layout: number of row / columns
		/// </summary>
		Vec2i GridCells { get; set; }

		/// <summary>
		/// Maximum allowed dimension of an atlas sheet 
		/// </summary>
		int MaxSize { get; set; }

		/// <summary>
		/// Specify an System.Drawing.Font for atlas placement
		/// </summary>
		Font Font { get; set; }

		/// <summary>
		/// Collection of IImageInfo for atlas placement
		/// </summary>
		IList<IImageInfo> Images { get; }

		/// <summary>
		/// List of character sets (default Ascii 32d-255d)
		/// </summary>
		IList<ICharSet> CharSets { get; }

		/// <summary>
		/// Type of glyph layout (default BestFit)
		/// </summary>
		GlyphAlignment Alignment { get; set; }

		/// <summary>
		/// Reset the IAtlasDescriptor to its intial settings
		/// </summary>
		void Reset();

		/// <summary>
		/// Sort the IImageInfo list by file name
		/// </summary>
		void SortImages();

		/// <summary>
		/// Event is triggered when any setting requires the atlas System.Drawing.Font to be regenerated
		/// </summary>
		event EventHandler FontChanged;
	}
}
