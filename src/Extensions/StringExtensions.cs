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

namespace TwoBit.Extensions
{
	public static class StringExtensions
	{
		public static char Last(this string value)
		{
			if (!String.IsNullOrEmpty(value))
			{
				return value[value.Length - 1];
			}

			return char.MinValue;
		}

		public static string Repeat(this string value, int count)
		{
			string result = string.Empty;

			for (int i = 0; i < count; i++)
			{
				result += value;
			}

			return result;
		}

		public static string RemoveAll(this string value, char ch)
		{
			var v = value;
			int idx;
			while ((idx = v.IndexOf(ch)) != -1)
			{
				v = v.Remove(idx, 1);
			}
			return v;
		}
	}
}
