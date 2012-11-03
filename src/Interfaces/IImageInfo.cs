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
	public interface IImageInfo : IDisposable
	{
		FileInfo FileInfo { get; }
		Image Image { get; }
		Vec2f Offset { get; set; }
		int Code { get; set; }
		bool HasCustomCode { get; set; }
		int SortCode { get; set; }
		Vec2f Location { get; set; }
		float Angle { get; set; }
	}
}
