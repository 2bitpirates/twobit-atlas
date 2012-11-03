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
using System.Linq;
using System.Reflection;

using TwoBit.Utilities;
using TwoBit.Extensions;

namespace TwoBit.Atlas
{
	class Interactive
	{
		private class CommandOpt
		{
			public string[] Keys;
			public string Description;
			public Action<string[]> Action;
		}

		private class CommandSet : Dictionary<string, CommandOpt>
		{
			public void Add(string key, string description, Action<string[]> action)
			{
				Add(key, new CommandOpt() { Keys = key.Split(new char[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries), Description = description, Action = action });
			}

			public bool Parse(IEnumerable<string> args)
			{
				var cmd = args.FirstOrDefault();
				if (!String.IsNullOrWhiteSpace(cmd))
				{
					foreach (var kvp in this)
					{
						foreach (var key in kvp.Value.Keys)
						{
							if (String.Compare(key, cmd, true) == 0)
							{
								kvp.Value.Action(args.ToArray());
								return true;
							}
						}
					}
				}

				return false;
			}
		}

		private IServiceProvider service;
		private CommandSet cmds;
		private const int OptionWidth = 15;
		private IAtlasDescriptor descriptor;
		private IAtlasBuilder builder;
		private List<PropertyInfo> properties;

		public Interactive(IServiceProvider service)
		{
			this.service = service;
			builder = service.GetService<IAtlasBuilder>();
			descriptor = builder.CreateAtlasDescriptor();
			properties = new List<PropertyInfo>(Reflect.GetReadWriteProperties(descriptor));
		}

		private void DisplayPrompt()
		{
			ConsoleEx.Write("\n[Yellow]>[default]");
		}

		public void Run()
		{
			ConsoleEx.WriteLine("Enter [Yellow]?[default] or [Yellow]help[default] for commands. Enter [Yellow]quit[default] to exit");
			bool done = false;

			cmds = new CommandSet()
			{
				{"?, help", "shows this command list", v => WriteHelp() },
				{"quit, exit", "exits the program", v => done = true },
				{"clear, cls", "clears the screen", v => Console.Clear() },
				{"prop", "show all properties ([Yellow]value[default]|[Yellow]help[default])", v => WriteProperties(v) },
				{"get", "get [Yellow]property[default]", v => WriteProperty(v) },
				{"set", "set [Yellow]property[default] [Yellow]value[default]", v => ApplyProperty(v) },
			};

			while (!done)
			{
				DisplayPrompt();
				var v = Console.ReadLine();

				try
				{
					var args = v.SplitQuotes();
					if (!cmds.Parse(args) && args.Any())
					{
						Console.WriteLine("Invalid command");
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}
		}

		private void ApplyProperty(IEnumerable<string> args)
		{
			var param = args.ToArray();
			if (param.Length != 3)
			{
				throw new ArgumentException("Invalid arguments");
			}

			var pi = FindProperty(param[1]);
			if (pi == null)
			{
				throw new ArgumentException("Invalid property name");
			}

			var value = param[2];
			object newValue;

			if (value.GetType() != pi.PropertyType)
			{
				// attempt to convert
				try
				{
					newValue = TypeDescriptor.GetConverter(pi.PropertyType).ConvertFromString(value);
				}
				catch
				{
					throw new ArgumentException("Unable to convert value to property");
				}
			}
			else
			{
				newValue = value;
			}

			pi.SetValue(descriptor, newValue);
			ConsoleEx.WriteLine(String.Format("{0} = [Yellow]{1}[default]", pi.Name, pi.GetValue(descriptor).ToString()));
		}

		private void WriteProperty(IEnumerable<string> args)
		{
			var param = args.ToArray();
			if (param.Length != 2)
			{
				throw new ArgumentException("Invalid arguments");
			}

			var pi = FindProperty(param[1]);
			if (pi == null)
			{
				throw new ArgumentException("Invalid property name");
			}

			ConsoleEx.WriteLine(String.Format("{0} = [Yellow]{1}[default]", pi.Name, pi.GetValue(descriptor).ToString()));
		}

		private PropertyInfo FindProperty(string name)
		{
			return properties.Find(pi => String.Compare(name, pi.Name, true) == 0);
		}

		private void WriteProperties(IEnumerable<string> args)
		{
			bool showValues = true;

			var param = args.ToArray();
			if (param.Length > 2)
			{
				throw new ArgumentException("Invalid arguments");
			}

			if (param.Length == 2)
			{
				if (String.Compare(param[1], "help", true) == 0)
				{
					showValues = false;
				}
				else
				if (String.Compare(param[1], "value", true) != 0)
				{
					throw new ArgumentException("Invalid argument");
				}
			}

			foreach (var pi in properties)
			{
				var b = Reflect.GetPropertyAttribute<BrowsableAttribute>(pi, true);
				if (b == null || b.Browsable)
				{
					int written = ConsoleEx.Write(pi.Name);

					if (written < OptionWidth)
						ConsoleEx.Write(new string(' ', OptionWidth - written));
					else
					{
						Console.WriteLine();
						Console.Write(new string(' ', OptionWidth));
					}

					if (showValues)
					{
						ConsoleEx.WriteLine(String.Format("[Yellow]{0}[default]", pi.GetValue(descriptor).ToString()));
					}
					else
					{
						ConsoleEx.WriteLine(Reflect.Description(pi));
					}
				}
			}
		}

		private void WriteHelp()
		{
			foreach (var kvp in cmds)
			{
				int written = ConsoleEx.Write(kvp.Key);
				
				if (written < OptionWidth)
					ConsoleEx.Write(new string(' ', OptionWidth - written));
				else
				{
					Console.WriteLine();
					Console.Write(new string(' ', OptionWidth));
				}

				ConsoleEx.WriteLine(kvp.Value.Description);
			}
		}
	}
}
