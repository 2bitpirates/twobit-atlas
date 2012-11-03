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
	public class SpriteDescriptor
	{
		public SpriteDescriptor()
		{
			Reset();
		}

		public void Reset()
		{
			Rate = 10.0f;
			Overflow = OverflowAction.Loop;
		}

		[Category("Attributes")]
		[Description("Frames per second for playback speed")]
		[DefaultValue(10.0f)]
		public float Rate { get; set; }

		[Category("Attributes")]
		[Description("Action to perform when the sprite reaches the last frame")]
		[DefaultValue(OverflowAction.Loop)]
		public OverflowAction Overflow { get; set; }

		[Category("Attributes")]
		[Description("Additional offset to set the sprite")]
		public Vec2f Origin { get; set; }
	}
}
