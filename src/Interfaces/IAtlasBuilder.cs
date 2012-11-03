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
using System.Threading;

namespace TwoBit.Atlas
{
	/// <summary>
	///		Main pipeline for generating atlas sheets.
	/// </summary>
	public interface IAtlasBuilder
	{
		/// <summary>
		///		Creates an instance of IAtlasDescriptor which provides settings and properties on the generated atlas.
		/// </summary>
		/// <returns>a new IAtlasDescriptor instance</returns>
		IAtlasDescriptor CreateAtlasDescriptor();

		/// <summary>
		///		Creates an instance of IImageInfo from an image.
		/// </summary>
		/// <param name="file">Path to an image</param>
		/// <returns>IImageInfo of the loaded image file</returns>
		IImageInfo ImageInfoFromFile(string file);

		/// <summary>
		///		Creates a collection of glyphs specified by an IAtlasDescriptor's settings.
		/// </summary>
		/// <param name="descriptor">Atlas descriptor that defines the type of glyphs to collect</param>
		/// <param name="progress">Callback delegate to provide completion status (0.0 - 1.0 complete)</param>
		/// <param name="cts">Cancellation token to optionally stop work in progress</param>
		/// <returns>A collection of glyphs</returns>
		GlyphDescriptorCollection CollectGlyphs(IAtlasDescriptor descriptor, Action<float> progress = null, CancellationTokenSource cts = null);

		/// <summary>
		///		Layout glyphs on an atlas sheet.
		/// </summary>
		/// <param name="glyphs">Collection of glyphs to place</param>
		/// <param name="descriptor">Descriptor with placement settings</param>
		/// <param name="progress">Callback delegate to provide completion status (0.0 - 1.0 complete)</param>
		/// <param name="cts">Cancellation token to optionally stop work in progress</param>
		/// <returns>An image dimension for each atlas sheet</returns>
		IEnumerable<Vec2i> PlaceGlyphs(GlyphDescriptorCollection glyphs, IAtlasDescriptor descriptor, Action<float> progress = null, CancellationTokenSource cts = null);

		/// <summary>
		///		Builds one or more atlas sheet images.
		/// </summary>
		/// <param name="sizes">Image dimensions for each of the atlas sheets</param>
		/// <param name="glyphs">Collection of glyphs to place on the image</param>
		/// <param name="descriptor">Atlas descriptor settings</param>
		/// <returns>An image for each atlas sheet generated</returns>
		IEnumerable<Image> BuildImage(IEnumerable<Vec2i> sizes, GlyphDescriptorCollection glyphs, IAtlasDescriptor descriptor);

		/// <summary>
		///		Creates a duplicated cropped image by removing transparent edge pixels.
		/// </summary>
		/// <param name="image">Image to crop</param>
		/// <param name="offset">Result offset of cropped image to align properly with the original image</param>
		/// <returns>New cropped image with the smallest dimensions possible</returns>
		Image CropImage(Image image, out Vec2i offset);

		/// <summary>
		///		Utility function to generate a sequential unique glyph ID.
		/// </summary>
		/// <param name="ch">Last unique ID</param>
		/// <param name="images">Collection of IImageInfo with associated ID</param>
		/// <returns>A unique Id for glphy assignment</returns>
		int UniqueCode(ref int ch, IEnumerable<IImageInfo> images);

		/// <summary>
		///		Validates an IImageInfo for atlas inclusion.
		/// </summary>
		/// <param name="ii">IImageInfo to validate</param>
		/// <param name="descriptor">Atlas descriptor settings </param>
		void ValidateImageInfo(IImageInfo ii, IAtlasDescriptor descriptor);

		/// <summary>
		///		Saves the atlas, images and optional sprite data to disk.
		/// </summary>
		/// <param name="filename">Atlas filename [.atlas]</param>
		/// <param name="images">Collection of generated atlas sheets</param>
		/// <param name="glyphs">Collection of generated glyphs</param>
		/// <param name="descriptor">Atlas descriptor settings</param>
		/// <param name="sprite">Sprite descriptor settings</param>
		void Save(string filename, IEnumerable<Image> images, GlyphDescriptorCollection glyphs, IAtlasDescriptor descriptor, SpriteDescriptor sprite);
	}
}
