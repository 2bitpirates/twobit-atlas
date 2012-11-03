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
using System.Collections.Generic;
using System.Reflection;

namespace TwoBit.Utilities
{
	public class Factory<T> : List<Maker>
	{
		public Factory()
		{
		}

		public Predicate<Type> AssemblyTypePredicate { get; set; }

		public T Make<U>(params object[] args)
		{
			return Make(typeof(U), args);
		}

		public T Make(Type type, params object[] args)
		{
			var maker = this.Find((a) => a.Type == type || type.IsInstanceOfType(a.Type));
			if (maker != null)
			{
				return (T)maker.Make(args);
			}

			throw new Exception("Type does not exist");
		}

		public IEnumerable<T> MakeAll(params object[] args)
		{
			foreach (var m in this)
			{
				yield return (T)m.Make(args);
			}
		}

		protected virtual bool NameComparision(string name, Maker m)
		{
			var attrib = Reflect.GetAttribute<DataNameAttribute>(m.Type);
			if (attrib != null)
			{
				return String.Compare(name, attrib.Name) == 0;
			}

			return String.Compare(name, m.Type.Name) == 0;
		}

		public Maker FindMaker(string name)
		{
			return Find(m => NameComparision(name, m));
		}

		public T Make(string name, params object[] args)
		{
			var maker = this.Find(m => NameComparision(name, m));

			if (maker != null)
			{
				return (T)maker.Make(args);
			}

            return default(T);
		}

		public void Add<U>()
		{
			Add(typeof(U));
		}

		public void Add(Type type)
		{
			Add(type, args => Activator.CreateInstance(type, args));
		}

		public void Add<U>(MakeFunc func)
		{
			Add(typeof(U), func);
		}

		public void Add(Type type, MakeFunc func)
		{
			Add(new Maker(type, func));
		}

		public void ScanAssembly(string file)
		{
			ScanAssembly(file, null);
		}

		public void ScanAssembly(string file, Predicate<Type> allowPredicate)
		{
			Assembly assembly = Assembly.LoadFile(file);
			ScanAssembly(assembly, allowPredicate);
		}

		public void ScanAssembly(Assembly assembly)
		{
			ScanAssembly(assembly, null);
		}

		public void ScanAssembly(Assembly assembly, Predicate<Type> allowPredicate)
		{
			var type = typeof(T);

			try
			{
				foreach (Type t in assembly.GetTypes())
				{
					if (t.IsAbstract || !t.IsClass)
					{
						continue;
					}

					if (AssemblyTypePredicate != null && !AssemblyTypePredicate(t))
					{
						continue;
					}

					if (type.IsAssignableFrom(t) && (allowPredicate == null || allowPredicate(t)))
					{
						Add(t);
					}
				}
			}
			catch
			{
			}
		}
	}
}
