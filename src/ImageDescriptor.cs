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
using System.ComponentModel;

namespace TwoBit.Atlas
{
	public class ImageDescriptor
	{
		public ImageDescriptor()
		{
			Reset();
		}

		public void Reset()
		{
			CenterImage = false;
			ImageOffset = Vec2i.Zero;
			RecurseDir = true;
			ImagePath = string.Empty;
		}

		[DefaultValue(false)]
		[Description("Set to True to set all images origin at the center")]
		[AtlasMutable(AtlasMutable.Image)]
		public bool CenterImage { get; set; }

		[Description("(x, y) pixel offset to set all image's origin")]
		[AtlasMutable(AtlasMutable.Image)]
		public Vec2i ImageOffset { get; set; }

		[Description("Directory to search for png images")]
		[AtlasMutable(AtlasMutable.Image)]
		public string ImagePath { get; set; }

		[Description("Set to True to recurse directories for png images")]
		[DefaultValue(true)]
		public bool RecurseDir { get; set; }
	}
}
