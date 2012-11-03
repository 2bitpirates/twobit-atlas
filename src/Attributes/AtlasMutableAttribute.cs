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

namespace TwoBit.Atlas
{
	[Flags]
	public enum AtlasMutable
	{
		Font = 0x01,
		Image = 0x2,
		All = Font | Image
	}

	public class AtlasMutableAttribute : Attribute
	{
		public AtlasMutableAttribute(AtlasMutable mutable = AtlasMutable.All)
		{
			AtlasMutable = mutable;
		}

		public AtlasMutable AtlasMutable { get; private set; }
	}
}
