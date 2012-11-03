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
using System.Globalization;

namespace TwoBit
{
	public class IndexValueConverter<T, U> : TypeConverter where T : struct, IIndexValue<U>
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string)
			{
				return Convert((string)value);
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				return value.ToString();
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
		
		public static T Convert(string value)
		{
			T t = default(T);
			var values = value.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
			int count = values.Length;
			for (int i = 0; i < count; i++)
			{
				var converter = TypeDescriptor.GetConverter(typeof(U));
				t[i] = (U)converter.ConvertFromString(values[i]);
			}

			return t;
		}
	}
}
