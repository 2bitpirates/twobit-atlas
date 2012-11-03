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
using System.Reflection;
using System.Diagnostics;

namespace TwoBit.Extensions
{
	public static class AssemblyExtensions
	{
		public static FileVersionInfo GetVersionInfo(this Assembly a)
		{
			return FileVersionInfo.GetVersionInfo(a.Location);
		}

		public static string ProductVersion(this Assembly a)
		{
			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(a.Location);
			return fvi.FileVersion.Trim();
		}

		public static string ProductName(this Assembly a)
		{
			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(a.Location);
			return fvi.ProductName.Trim();
		}

		public static string ProductDescription(this Assembly a)
		{
			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(a.Location);
			return fvi.Comments.Trim();
		}
	}
}
