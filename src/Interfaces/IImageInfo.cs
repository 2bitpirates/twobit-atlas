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
	/// <summary>
	/// Defines an Image glyph
	/// </summary>
	public interface IImageInfo : IDisposable
	{
		/// <summary>
		/// FileInfo for this image
		/// </summary>
		FileInfo FileInfo { get; }

		/// <summary>
		/// Bitmap data of image
		/// </summary>
		Image Image { get; }

		/// <summary>
		/// Image origin location
		/// </summary>
		Vec2f Offset { get; set; }

		/// <summary>
		/// Set to True to use the 
		/// </summary>
		bool HasCustomCode { get; set; }

		/// <summary>
		/// Custom code this Glyph will have on the atlas
		/// </summary>
		int Code { get; set; }

		/// <summary>
		/// Glyph ID value on the atlas
		/// </summary>
		int SortCode { get; set; }

		/// <summary>
		/// GUI Editor workspace locaiton
		/// </summary>
		Vec2f Location { get; set; }

		/// <summary>
		/// GUI Editor image orientation
		/// </summary>
		float Angle { get; set; }
	}
}
