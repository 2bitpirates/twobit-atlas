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

namespace TwoBit.Extensions
{
	public static class IOExtensions
	{
		public static bool ContainsPath(this DirectoryInfo di, string path)
		{
			return path.StartsWith(di.FullName);
		}

		public static string GetRelativePath(this FileInfo file, string basePath)
		{
			if (!String.IsNullOrEmpty(basePath))
			{
				DirectoryInfo di = new DirectoryInfo(basePath);

				basePath = di.FullName;
				string filePath = file.FullName;

				// check if on the same drive
				if (String.Compare(Path.GetPathRoot(basePath), Path.GetPathRoot(filePath), StringComparison.CurrentCultureIgnoreCase) == 0)
				{
					// ensure basePath has trailing directory separator
					char last = basePath.Last();
					if (last != Path.DirectorySeparatorChar && last != Path.AltDirectorySeparatorChar)
					{
						basePath += Path.DirectorySeparatorChar;
					}

					// check if file is a child directory
					if (filePath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
					{
						return filePath.Substring(basePath.Length);
					}

					int levels = 0;
					DirectoryInfo dil = di;
					while (dil != null && !dil.ContainsPath(filePath))
					{
						levels++;
						dil = dil.Parent;
					}

					string result = @"..\".Repeat(levels);
					if (dil == null || dil.Name == dil.Root.Name)
					{
						result += filePath.Substring(Path.GetPathRoot(filePath).Length);
					}
					else
						if (filePath.Length > dil.FullName.Length)
						{
							result += filePath.Substring(dil.FullName.Length + 1);
						}

					return result;
				}
			}

			return file.FullName;
		}
	}
}
