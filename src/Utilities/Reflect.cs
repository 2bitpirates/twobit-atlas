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
using System.Reflection;
using System.ComponentModel;

namespace TwoBit.Utilities
{
	public static class Reflect
	{
        public static T GetPropertyAttribute<T>(PropertyInfo pi, bool inherit)
        {
            return (T)GetPropertyAttribute(pi, typeof(T), inherit);
        }

        public static object GetPropertyAttribute(PropertyInfo pi, Type attributeType, bool inherit)
        {
            object[] attrib = pi.GetCustomAttributes(attributeType, inherit);
            if (attrib != null && attrib.Length > 0)
            {
                return attrib[0];
            }
            return null;
        }

		public static T GetAttribute<T>(object obj)
		{
			return GetAttribute<T>(obj, true);
		}

		public static T GetAttribute<T>(object obj, bool inherit)
		{
			if (obj == null)
			{
				return default(T);
			}

			if (obj is Type)
			{
				return (T)GetAttribute((Type)obj, typeof(T), inherit);
			}

			if (obj is PropertyInfo)
			{
				PropertyInfo pi = (PropertyInfo)obj;
				foreach (Attribute a in pi.GetCustomAttributes(true))
				{
					if (a is T)
					{
						return (T)(object)a;
					}
				}
			}

			if (obj is AttributeCollection)
			{
				foreach (Attribute a in (AttributeCollection)obj)
				{
					if (a is T)
					{
						return (T)(object)a;
					}
				}
			}

			var t = obj.GetType();
			if (t.IsEnum)
			{
				FieldInfo fi = t.GetField(obj.ToString());
				object[] attribs = fi.GetCustomAttributes(typeof(T), inherit);
				if (attribs != null && attribs.GetLength(0) > 0)
				{
					return (T)attribs[0];
				}

				return default(T);
			}

			return (T)GetAttribute(t, typeof(T), inherit);
		}

		public static object GetAttribute(Type type, Type attributeType, bool inherit)
		{
			if (type.IsDefined(attributeType, inherit))
			{
				object[] attributes = type.GetCustomAttributes(attributeType, inherit);

				if (attributes != null && attributes.Length != 0)
				{
					return attributes[0];
				}
			}

			return null;
		}

		public static string GetSaveName(object obj)
		{
			var attrib = GetAttribute<DataNameAttribute>(obj);
			return attrib != null ? attrib.Name : obj.GetType().Name;
		}

		public static string DisplayName(object obj)
		{
			return DisplayName(obj, true);
		}

		public static string Description(object obj)
		{
			return Description(obj, true);
		}

        public static object GetEnumValue(Type type, string name)
        {
            return Enum.Parse(type, name, true);
        }

        public static System.Collections.IEnumerable GetEnumValues(Type type)
        {
            if (!type.IsEnum)
            {
                throw new ArgumentException("Type '" + type.Name + "' is not an enum");
            }

            return Enum.GetValues(type);
        }

		public static string DisplayName(object obj, bool inherit)
		{
			var d = GetAttribute<DisplayNameAttribute>(obj, inherit);
			if (d != null)
			{
				return d.DisplayName;
			}

            if (obj is Type)
			{
				return ((Type)obj).Name;
			}

			return obj.GetType().Name;
		}

		public static string Description(object obj, bool inherit)
		{
			if (obj != null)
			{
				var d = GetAttribute<DescriptionAttribute>(obj, inherit);
				if (d != null)
				{
					return d.Description;
				}
			}

			return string.Empty;
		}

		public static IEnumerable<PropertyInfo> GetReadWriteProperties(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException();
			}

			var t = obj.GetType();
			if (!t.IsClass)
			{
				throw new ArgumentException("Object must be a class");
			}

			var members = t.GetMembers();
			foreach (MemberInfo m in members)
			{
				var p = m as PropertyInfo;

				if (p != null)
				{
					// check if this field should explictly not be serialized
					var attrib = p.GetCustomAttributes(typeof(NoSerializeAttribute), true);
					if (attrib != null && attrib.Length > 0)
					{
						continue;
					}

					// only collect public properties with both (get;set)
					var mi = p.GetAccessors(false);
					if (p.CanRead && p.CanWrite && mi != null && mi.Length == 2)
					{
						yield return p;
					}
				}
			}
		}
	}
}
