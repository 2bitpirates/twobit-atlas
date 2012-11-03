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

namespace TwoBit.Utilities
{
	public static class ConsoleEx
	{
		public static void WritePad(int count, int width)
		{
			if (count < width)
				Console.Write(new String(' ', width - count));
			else
			{
				Console.WriteLine();
				Console.Write(new String(' ', width));
			}
		}

		public static void WriteLine(string value)
		{
			Write(value);
			Console.WriteLine();
		}

		public static int Write(string value)
		{
			bool inColor = false;
			string mt = string.Empty;
			var defaultColor = Console.ForegroundColor;
			int count = 0;

			foreach (var ch in value)
			{
				if (!inColor && ch == '[')
				{
					inColor = true;
					mt = string.Empty;
				}
				else
				if (inColor)
				{
					if (ch == ']')
					{
						inColor = false;
						ConsoleColor color;
						if (Enum.TryParse<ConsoleColor>(mt, out color))
						{
							Console.ForegroundColor = color;
						}
						if (String.Compare(mt, "default") == 0)
						{
							Console.ForegroundColor = defaultColor;
						}
					}
					else
					{
						mt += ch;
					}
				}
				else
				{
					Console.Write(ch);
					count++;
				}
			}

			return count;
		}
	}
}
