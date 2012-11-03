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
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Reflection;

using NDesk.Options;

using TwoBit.Extensions;
using TwoBit.Utilities;

namespace TwoBit.Atlas
{
	class Program : IServiceProvider
	{
		private enum Command
		{
			None,
			Help,
			List,
			Build,
			Plugin,
			Console
		}

		private OptionSet opt;
		private FileVersionInfo fvi;
		private string fontName;
		private string outFile;
		private string imagePath;
		private string pluginPath;
		private string pluginName;
		private List<string> charSets;
		private string[] pngList;
		private float fontSize;
		private bool makeSprite;
		private bool powTwo;
		private bool fontBold;
		private bool fontItalic;
		private bool forceSpace;
		private bool recurseDir;
		private bool centerImages;
		private bool verbose;
		private bool multiTexture;
		private Vec2f imageOrigin;
		private SpriteDescriptor sprite;
		private GlyphAlignment alignment;
		private int startCode;
		private int spacing;
		private int maxSize;
		private object[] services;
		private Command cmd;

		private Program()
		{
			fvi = Assembly.GetExecutingAssembly().GetVersionInfo();
			sprite = new SpriteDescriptor();
			charSets = new List<string>();
			fontSize = 24.0f;
			spacing = 1;
			alignment = GlyphAlignment.BestFit;
			forceSpace = true;
			maxSize = 2048;

			// create services
			services = new object[]
			{
				new XmlDocCreator(),
				new CharSetProvider(),
				new AtlasBuilder(this)
			};
		}

		public object GetService(Type serviceType)
		{
			// search for the service
			foreach (object service in services)
			{
				if (serviceType.IsInstanceOfType(service))
				{
					return service;
				}

				// check for nested service providers
				if (service is IServiceProvider)
				{
					object s = ((IServiceProvider)service).GetService(serviceType);
					if (s != null)
					{
						return s;
					}
				}
			}

			return null;
		}

		private int Run(string[] args)
		{
			cmd = Command.None;

			opt = new OptionSet()
			{
				{"h|?|help", "show this message", v => cmd = Command.Help },
				{"f=|font=", "generate the specifed {font}", v => { fontName = v; cmd = Command.Build;} },
				{"o=|output=", "output atlas {file}[.atlas]", v => outFile = v },
				{"c=|char-set=", "character {set} to use", v => charSets.Add(v) },
				{"i=|image-path=", "directory {path} of png images to add", v => { imagePath = v; cmd = Command.Build; } },
				{"l|list-sets", "list available character sets", v => cmd = Command.List },
				{"v|verbose", "enable maximum verbosity", v => verbose = v != null },
				{"m|make-sprite", "make an associated sprite", v => makeSprite = v != null },
				{"p=|plugin", "execute module plugin in {assembly}", v => { pluginPath = v; cmd = Command.Plugin; } },
				{"C|console-mode", "run in interactive console mode", v => cmd = Command.Console },
				{"plugin-module=", "optional plugin module {name}", v => pluginName = v },
				{"power-two", "force atlas dimensions to be a power of 2", v => powTwo = v != null},
				{"max-size=", "maximum texture {size} in pixels [2048]", v => int.TryParse(v, out maxSize) },
				{"multi-texture", "enable multi-texture support", v => multiTexture = v != null},
				{"font-size=", "font {size}", v => float.TryParse(v, out fontSize) },
				{"font-bold", "use bold font", v => fontBold = v != null},
				{"font-italic", "use italic font", v => fontItalic = v != null},
				{"force-space", "force a space glyph", v => forceSpace = v != null},
				{"sprite-rate=", "sprite play back {speed} in frames per second", v => { float r; if (float.TryParse(v, out r)) sprite.Rate = r; }},
				{"sprite-overflow=", "sprite action after playing last {frame} (Hold|Loop)", v => sprite.Overflow = (OverflowAction)Enum.Parse(typeof(OverflowAction), v) },
				{"glyph-align=", "the {layout} of each glyph (BestFit|Grid)", v => alignment = (GlyphAlignment)Enum.Parse(typeof(GlyphAlignment), v) },
				{"glyph-space=", "the {space} between glyphs in pixels", v => int.TryParse(v, out spacing)},
				{"image-start=", "image starting {index}", v => int.TryParse(v, out startCode)},
				{"image-recurse", "search sub directories when adding images", v => recurseDir = v != null},
				{"image-center", "center all image glyphs", v => centerImages = v != null},
				{"image-origin=", "{\"x, y\"} offset of all image glyphs", v => imageOrigin = Transpose.FromString<Vec2f>(v)},
			};

			try
			{
				var param = opt.Parse(args);

				switch (cmd)
				{
					case Command.List:
						WriteSets();
						return 0;

					case Command.None:
					case Command.Help:
						WriteHelp();
						return 1;

					case Command.Plugin:
						ExecutePlugin(param);
						break;

					case Command.Build:
						Validate();
						Process();
						break;

					case Command.Console:
						new Interactive(this).Run();
						break;
				}
			}
			catch (Exception e)
			{
				WriteError(e);
				return 2;
			}

			return 0;
		}

		private void ExecutePlugin(IEnumerable<string> args)
		{
			if (String.IsNullOrWhiteSpace(pluginPath))
			{
				throw new ArgumentException("Plugin assembly not specified");
			}

			if (!File.Exists(pluginPath))
			{
				throw new IOException("Plugin assembly not found");
			}

			var factory = new Factory<IPlugin>();

			if (verbose)
			{
				Console.WriteLine("scanning plugin path: {0}", pluginPath);
			}

			try
			{
				factory.ScanAssembly(pluginPath);
			}
			catch (Exception e)
			{
				throw new Exception("Error loading plugin", e);
			}

			if (factory.Count == 0)
			{
				throw new Exception("Assembly does not contain any plugins");
			}

			if (verbose)
			{
				Console.WriteLine("found {0} plugin module(s)", factory.Count);
			}

			IPlugin p = null;

			if (!String.IsNullOrWhiteSpace(pluginName))
			{
				try
				{
					p = factory.Make(pluginName);
				}
				catch (Exception e)
				{
					throw new Exception("Error creating plugin: " + pluginName, e);
				}

				if (verbose)
				{
					Console.WriteLine("using plugin module: {0}", pluginName);
				}
			}
			else
			{
				try
				{
					p = (IPlugin)factory[0].Make();
				}
				catch (Exception e)
				{
					throw new Exception("Error creating default plugin: " + factory[0].ToString(), e);
				}

				if (verbose)
				{
					Console.WriteLine("using plugin module: {0}", Reflect.DisplayName(p));
				}
			}

			if (p != null)
			{
				p.Execute(args);
			}
		}

		private void Validate()
		{
			if (String.IsNullOrWhiteSpace(outFile))
			{
				throw new ArgumentException("Output name not specified");
			}

			// check for font generation
			if (!String.IsNullOrWhiteSpace(fontName))
			{
				if (fontSize <= 0)
				{
					throw new ArgumentException("Invalid font size");
				}

				// add ascii as default
				if (charSets.Count == 0)
				{
					charSets.Add("ascii");
				}

				var csp = this.GetService<ICharSetProvider>();
				foreach (var set in charSets)
				{
					if (csp.Factory.FindMaker(set) == null)
					{
						throw new ArgumentException("Invalid character set: " + set);
					}
				}
			}

			// check for image generation
			if (!String.IsNullOrWhiteSpace(imagePath))
			{
				if (!Directory.Exists(imagePath))
				{
					throw new ArgumentException("Image path does not exist");
				}

				pngList = Directory.GetFiles(imagePath, "*.png", recurseDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
				if (pngList == null || pngList.Length == 0)
				{
					throw new IOException("Image path does not contain any png images");
				}
			}
		}

		private void CollectImages(IAtlasDescriptor descriptor)
		{
			foreach (var png in pngList)
			{
				try
				{
					var ii = new ImageInfo(png);
					if (centerImages)
					{
						ii.Offset = new Vec2f(ii.Image.Width * 0.5f, ii.Image.Height * 0.5f);
					}
					else
					{
						ii.Offset = ii.Offset + imageOrigin;
					}

					descriptor.Images.Add(ii);
				}
				catch (Exception ex)
				{
					throw new Exception("Error reading image: " + Path.GetFileName(png), ex);
				}
			}
		}

		private void Process()
		{
			IAtlasBuilder builder = this.GetService<IAtlasBuilder>();
			ICharSetProvider csp = this.GetService<ICharSetProvider>();

			using (var descriptor = builder.CreateAtlasDescriptor())
			{
				descriptor.PowerTwo = powTwo;
				descriptor.Alignment = alignment;
				descriptor.StartCode = startCode;
				descriptor.Spacing = spacing;
				descriptor.MakeSprite = makeSprite;
				descriptor.MaxSize = maxSize;
				descriptor.MultiTexture = multiTexture;

				if (String.IsNullOrWhiteSpace(fontName))
				{
					descriptor.UseFonts = false;
				}
				else
				{
					descriptor.UseFonts = true;
					descriptor.FontName = fontName;
					descriptor.FontSize = fontSize;
					descriptor.FontBold = fontBold;
					descriptor.FontItalic = fontItalic;
					descriptor.ForceSpace = forceSpace;

					charSets.ForEach(set => descriptor.CharSets.Add(csp.Factory.Make(set)));
				}

				if (pngList != null)
				{
					CollectImages(descriptor);
					descriptor.UseImages = true;
				}

				var glyphs = builder.CollectGlyphs(descriptor, VerboseCollector, null);
				if (verbose)
				{
					Console.WriteLine();
				}

				if (glyphs.Count == 0)
				{
					throw new ArgumentException("No glyphs specified. Nothing to do");
				}

				var size = builder.PlaceGlyphs(glyphs, descriptor, VerbosePlacement);
				if (verbose)
				{
					Console.WriteLine();
				}

				if (verbose)
				{
					Console.WriteLine("Total Glyphs: {0}", glyphs.Count);
				}

				IEnumerable<Image> images = null;

				try
				{
					images = builder.BuildImage(size, glyphs, descriptor);
					builder.Save(outFile, images, glyphs, descriptor, sprite);
				}
				finally
				{
					if (images != null)
					{
						foreach (var img in images)
						{
							img.Dispose();
						}
					}
				}
			}
		}

		private void VerboseCollector(float complete)
		{
			if (verbose)
			{
				Console.Write("Collect: {0:000}%    \r", (int)(complete * 100));
			}
		}

		private void VerbosePlacement(float complete)
		{
			if (verbose)
			{
				Console.Write("Placement: {0:000}%    \r", (int)(complete * 100));
			}
		}

		private void WriteSets()
		{
			ICharSetProvider csp = this.GetService<ICharSetProvider>();
			foreach (var m in csp.Factory)
			{
				DataNameAttribute attrib = Reflect.GetAttribute<DataNameAttribute>(m.Type);
				string name = attrib != null ? attrib.Name : m.Type.Name;
				Console.WriteLine("{0}\t\t{1}", name, Reflect.Description(m.Type));
			}
		}

		private void WriteHelpHeader()
		{
			Console.WriteLine("{0} v{1}\n", fvi.ProductName.Trim(), fvi.ProductVersion.Trim());
			Console.WriteLine(fvi.Comments + "\n");
		}

		private void WriteHelp()
		{
			WriteHelpHeader();

			opt.WriteOptionDescriptions(Console.Out);

			Console.WriteLine("\narguments after -- are passed to any specified plugins");
		}

		private void WriteError(Exception e)
		{
			Console.WriteLine(e.Message);
			if (e.InnerException != null)
			{
				Console.WriteLine("\t" + e.InnerException.Message);
			}
			Console.WriteLine("\nTry `{0} --help' for more information.", fvi.ProductName.Trim());
		}

		static int Main(string[] args)
		{
			return new Program().Run(args);
		}
	}
}
