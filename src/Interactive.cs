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
using System.ComponentModel;
using System.Linq;
using System.Drawing;
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

		private class Generated : IDisposable
		{
			public List<Image> Images;
			public GlyphDescriptorCollection Glyphs;

			public Generated()
			{
				Images = new List<Image>();
				Glyphs = new GlyphDescriptorCollection();
			}

			public void Dispose()
			{
				foreach (var image in Images)
				{
					image.Dispose();
				}
				Images.Clear();

				foreach (var glyph in Glyphs)
				{
					glyph.Dispose();
				}
				Glyphs.Clear();
			}
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

		private class Property
		{
			public PropertyInfo Pi;
			public object Target;
		}

		private IServiceProvider service;
		private CommandSet cmds;
		private const int OptionWidth = 15;
		private IAtlasBuilder builder;
		private List<Property> properties;
		private Generated generated;
		private bool modified;
		private bool needsBuild;
		private string documentName;
		private IAtlasDescriptor atlasDescriptor;
		private SpriteDescriptor spriteDescriptor;
		private ImageDescriptor imageDescriptor;

		public Interactive(IServiceProvider service)
		{
			this.service = service;
			builder = service.GetService<IAtlasBuilder>();
			atlasDescriptor = builder.CreateAtlasDescriptor();
			spriteDescriptor = new SpriteDescriptor();
			imageDescriptor = new ImageDescriptor();
			properties = new List<Property>();

			CollectProperties(atlasDescriptor);
			CollectProperties(spriteDescriptor);
			CollectProperties(imageDescriptor);
		}

		private void CollectProperties(object target)
		{
			foreach (var pi in Reflect.GetReadWriteProperties(target))
			{
				properties.Add(new Property() { Pi = pi, Target = target });
			}
		}

		private void DisplayPrompt()
		{
			if (modified || needsBuild)
			{
				ConsoleEx.Write("\n[Red]X[Yellow]>[default]");
			}
			else
			{
				ConsoleEx.Write("\nO[Yellow]>[default]");
			}
		}

		private string DocumentName
		{
			get { return String.IsNullOrWhiteSpace(documentName) ? "{untitled}" : Path.GetFileName(documentName); }
			set { documentName = value; }
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
				{"get", "get [Yellow]property[default] - show the property value", v => WriteProperty(v) },
				{"set", "set [Yellow]property[default] [Yellow]value[default] - change the property value", v => ApplyProperty(v) },
				{"build", "generate an atlas with the current settings", v => Build() },
				{"save", "save [Yellow]file[default] - Save to disk the last built atlas", v => Save(v) },
				{"status", "show atlas current status", v => WriteStatus() },
				{"reset", "set all setting to its default state", v => ResetSettings() }
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
					ConsoleEx.WriteLine(String.Format("\n[Red]{0}[default]", e.Message));
				}
			}
		}

		private void ResetSettings()
		{
			atlasDescriptor.Reset();
			spriteDescriptor.Reset();
			imageDescriptor.Reset();
			modified = false;
			needsBuild = false;
			DocumentName = null;

			if (generated != null)
			{
				generated.Dispose();
				generated = null;
			}
		}

		private void ApplyProperty(IEnumerable<string> args)
		{
			var param = args.ToArray();
			if (param.Length != 3)
			{
				throw new ArgumentException("Invalid arguments");
			}

			var p = FindProperty(param[1]);
			if (p == null)
			{
				throw new ArgumentException("Invalid property name");
			}

			var value = param[2].RemoveAll('\"');
			var pi = p.Pi;
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

			pi.SetValue(p.Target, newValue);
			ConsoleEx.WriteLine(String.Format("{0} = [Yellow]{1}[default]", pi.Name, pi.GetValue(p.Target).ToString()));

			// mark that we made changes
			modified = true;

			// check if property change would require a rebuild
			var attrib = Reflect.GetPropertyAttribute<AtlasMutableAttribute>(pi, true);
			if (attrib != null &&  ((atlasDescriptor.UseFonts && (attrib.AtlasMutable & AtlasMutable.Font) != 0) || (atlasDescriptor.UseImages && (attrib.AtlasMutable & AtlasMutable.Image) != 0)))
			{
				needsBuild = true;
			}
		}

		private void WriteProperty(IEnumerable<string> args)
		{
			var param = args.ToArray();
			if (param.Length != 2)
			{
				throw new ArgumentException("Invalid arguments");
			}

			var p = FindProperty(param[1]);
			if (p == null)
			{
				throw new ArgumentException("Invalid property name");
			}

			ConsoleEx.WriteLine(String.Format("{0} = [Yellow]{1}[default]", p.Pi.Name, p.Pi.GetValue(p.Target).ToString()));
		}

		private Property FindProperty(string name)
		{
			return properties.Find(pi => String.Compare(name, pi.Pi.Name, true) == 0);
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

			ConsoleEx.WriteLine("[Yellow]Atlas[default]");
			ShowProperty(atlasDescriptor, showValues);

			ConsoleEx.WriteLine("\n[Yellow]Sprite[default]");
			ShowProperty(spriteDescriptor, showValues);

			ConsoleEx.WriteLine("\n[Yellow]Image[default]");
			ShowProperty(imageDescriptor, showValues);
		}

		private void ShowProperty(object target, bool showValues)
		{
			foreach (var p in properties)
			{
				if (p.Target == target)
				{
					var b = Reflect.GetPropertyAttribute<BrowsableAttribute>(p.Pi, true);
					if (b == null || b.Browsable)
					{
						int written = ConsoleEx.Write(p.Pi.Name);
						ConsoleEx.WritePad(written, OptionWidth);

						if (showValues)
						{
							ConsoleEx.WriteLine(String.Format("[Yellow]{0}[default]", p.Pi.GetValue(p.Target).ToString()));
						}
						else
						{
							ConsoleEx.WriteLine(Reflect.Description(p.Pi));
						}
					}
				}
			}
		}

		private void Save(IEnumerable<string> args)
		{
			var param = args.ToArray();
			if (param.Length == 1 && String.IsNullOrWhiteSpace(documentName))
			{
				throw new ArgumentException("Can not auto save an untitled document");
			}

			if (param.Length != 1 && param.Length != 2)
			{
				throw new ArgumentException("Invalid arguments");
			}

			if (generated == null || needsBuild)
			{
				Build();
			}

			string file = param.Length == 1 ? documentName : param[1];
			if (!Path.HasExtension(file))
			{
				file = Path.ChangeExtension(file, "atlas");
			}

			builder.Save(file, generated.Images, generated.Glyphs, atlasDescriptor, spriteDescriptor);
			DocumentName = file;
			modified = false;
			needsBuild = false;
			ConsoleEx.WriteLine(String.Format("Saved [Yellow]{0}[default]", DocumentName));
		}

		private void SearchForImages()
		{
			atlasDescriptor.ClearImages();

			if (atlasDescriptor.UseImages)
			{
				if (!Directory.Exists(imageDescriptor.ImagePath))
				{
					throw new IOException("Image search path does not exist");
				}

				var pngList = Directory.GetFiles(imageDescriptor.ImagePath, "*.png", imageDescriptor.RecurseDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
				if (pngList == null || pngList.Length == 0)
				{
					throw new IOException("Image path does not contain any png images");
				}

				foreach (var png in pngList)
				{
					try
					{
						// create IImageInfo
						var ii = builder.ImageInfoFromFile(png);

						// set image offsets
						if (imageDescriptor.CenterImage)
						{
							ii.Offset = new Vec2f(ii.Image.Width * 0.5f, ii.Image.Height * 0.5f);
						}
						else
						{
							ii.Offset = ii.Offset + (Vec2f)imageDescriptor.ImageOffset;
						}

						atlasDescriptor.Images.Add(ii);
					}
					catch (Exception ex)
					{
						throw new Exception("Error reading image: " + Path.GetFileName(png), ex);
					}
				}
			}
		}

		private void Build()
		{
			var temp = new Generated();

			try
			{
				SearchForImages();

				// collect
				temp.Glyphs = builder.CollectGlyphs(atlasDescriptor, VerboseCollector, null);

				VerboseCollector(1.0f);
				Console.WriteLine();

				if (temp.Glyphs.Count == 0)
				{
					throw new ArgumentException("No glyphs specified. Nothing to do");
				}
				
				ConsoleEx.WriteLine(String.Format("Glyphs: [Yellow]{0}[default]", temp.Glyphs.Count));

				// place
				var sizes = builder.PlaceGlyphs(temp.Glyphs, atlasDescriptor, VerbosePlacement);
				VerbosePlacement(1.0f);
				Console.WriteLine();

				ConsoleEx.WriteLine("Complete");
				foreach (var sz in sizes)
				{
					ConsoleEx.WriteLine(String.Format(" Image: [Yellow]{0}x{1}[default]px", sz.X, sz.Y));
				}
				temp.Images.AddRange(builder.BuildImage(sizes, temp.Glyphs, atlasDescriptor));

				if (generated != null)
				{
					generated.Dispose();
				}

				generated = temp;
				temp = null;
				needsBuild = false;
			}
			finally
			{
				if (temp != null)
				{
					temp.Dispose();
				}
			}
		}

		private void VerboseCollector(float complete)
		{
			ConsoleEx.Write(String.Format("Collect: [Yellow]{0:000}%[default]    \r", (int)(complete * 100)));
		}

		private void VerbosePlacement(float complete)
		{
			ConsoleEx.Write(String.Format("Placement: [Yellow]{0:000}%[default]    \r", (int)(complete * 100)));
		}

		private void WriteStatus()
		{
			int written;

			written = ConsoleEx.Write("Document");
			ConsoleEx.WritePad(written, OptionWidth);
			ConsoleEx.WriteLine(String.Format("[Yellow]{0}[default]", DocumentName));

			written = ConsoleEx.Write("Has Atlas");
			ConsoleEx.WritePad(written, OptionWidth);
			ConsoleEx.WriteLine(String.Format("[Yellow]{0}[default]", generated != null));

			written = ConsoleEx.Write("Modified");
			ConsoleEx.WritePad(written, OptionWidth);
			ConsoleEx.WriteLine(String.Format("[Yellow]{0}[default]", modified));

			written = ConsoleEx.Write("Needs Build");
			ConsoleEx.WritePad(written, OptionWidth);
			ConsoleEx.WriteLine(String.Format("[Yellow]{0}[default]", needsBuild));
		}

		private void WriteHelp()
		{
			ConsoleEx.WriteLine("[Yellow]Commands[default]");
			foreach (var kvp in cmds)
			{
				int written = ConsoleEx.Write(kvp.Key);
				ConsoleEx.WritePad(written, OptionWidth);
				ConsoleEx.WriteLine(kvp.Value.Description);
			}

			ConsoleEx.WriteLine("\n[Yellow]Prompt[default]");
			ConsoleEx.WritePad(0, OptionWidth);
			ConsoleEx.WriteLine("O[Yellow]>[default] - default");
			ConsoleEx.WritePad(0, OptionWidth);
			ConsoleEx.WriteLine("[Red]X[Yellow]>[default] - changes made");
		}
	}
}
