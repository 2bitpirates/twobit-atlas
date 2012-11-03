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
	public interface IAtlasDescriptor : IDisposable
	{
		bool UseFonts { get; set; }
		bool UseImages { get; set; }

		int Spacing { get; set; }
		bool PowerTwo { get; set; }
		bool ForceSpace { get; set; }
		bool MakeSprite { get; set; }
        bool MultiTexture { get; set; }
		string FontName { get; set; }
		float FontSize { get; set; }
		bool FontBold { get; set; }
		bool FontItalic { get; set; }
		int StartCode { get; set; }

		int FontHeight { get; }

		Vec2i GridSize { get; set; }
		Vec2i GridCells { get; set; }
		int MaxSize { get; set; }
		Font Font { get; set; }

		IList<IImageInfo> Images { get; }
		IList<ICharSet> CharSets { get; }

		GlyphAlignment Alignment { get; set; }

		void Reset();
		void SortImages();
		
		event EventHandler FontChanged;
	}
}
