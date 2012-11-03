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

using TwoBit.Utilities;

namespace TwoBit.Atlas
{
	public abstract class BaseCharSet : ICharSet
	{
		public override string ToString()
		{
			var attrib = Reflect.GetAttribute<DataNameAttribute>(GetType());
			return attrib != null ? attrib.Name : Reflect.DisplayName(GetType());
		}

		public abstract IEnumerable<char> Characters { get; }
	}

	[DisplayName("Ascii")]
	[Description("Ascii displayable: 32d .. 255d")]
	[DataName("ascii")]
	class CharSetAscii : BaseCharSet
	{
		public override IEnumerable<char> Characters
		{
			get 
			{
				for (char ch = (char)32; ch < (char)256; ch++)
				{
					yield return ch;
				}
			}
		}
	}

	[DisplayName("Numbers")]
	[Description("Numerical characters: '0'..'9'")]
	[DataName("numbers")]
	class CharSetNumbers : BaseCharSet
	{
		public override IEnumerable<char> Characters
		{
			get
			{
				for (char ch = '0'; ch <= '9'; ch++)
				{
					yield return ch;
				}
			}
		}
	}

	[DisplayName("Letters")]
	[Description("Standard Alphabet: 'A'..'Z', 'a'..'z'")]
	[DataName("letters")]
	class CharSetLetters : BaseCharSet
	{
		public override IEnumerable<char> Characters
		{
			get
			{
				for (char ch = 'A'; ch <= 'Z'; ch++)
				{
					yield return ch;
				}

				for (char ch = 'a'; ch <= 'z'; ch++)
				{
					yield return ch;
				}
			}
		}
	}
}
