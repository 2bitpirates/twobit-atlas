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
	public interface IAtlasBuilder
	{
        IAtlasDescriptor CreateAtlasDescriptor();
		IImageInfo ImageInfoFromFile(string file);

        GlyphDescriptorCollection CollectGlyphs(IAtlasDescriptor descriptor, Action<float> progress, CancellationTokenSource cts);
        IEnumerable<Vec2i> PlaceGlyphs(GlyphDescriptorCollection glyphs, IAtlasDescriptor descriptor, Action<float> progress);
        IEnumerable<Vec2i> PlaceGlyphs(GlyphDescriptorCollection glyphs, IAtlasDescriptor descriptor, Action<float> progress, CancellationTokenSource cts);
        IEnumerable<Image> BuildImage(IEnumerable<Vec2i> sizes, GlyphDescriptorCollection glyphs, IAtlasDescriptor descriptor);
        Image CropImage(Image image, out Vec2i offset);
        int UniqueCode(ref int ch, IEnumerable<IImageInfo> images);
        void ValidateImageInfo(IImageInfo ii, IAtlasDescriptor descriptor);

        void Save(string filename, IEnumerable<Image> images, GlyphDescriptorCollection glyphs, IAtlasDescriptor descriptor, SpriteDescriptor sprite);
	}
}
