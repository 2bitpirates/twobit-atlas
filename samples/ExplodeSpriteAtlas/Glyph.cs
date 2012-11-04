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
using System.Drawing;
using System.Xml.Linq;

namespace ExplodeSpriteAtlas
{
	public class Glyph
	{
		public Glyph(XElement node)
		{
			Page = Convert.ToInt32(node.Attribute("p").Value);
			UV.X = Convert.ToInt32(node.Attribute("x").Value);
			UV.Y = Convert.ToInt32(node.Attribute("y").Value);
			Size.Width = Convert.ToInt32(node.Attribute("w").Value);
			Size.Height = Convert.ToInt32(node.Attribute("h").Value);

			A = Convert.ToInt32(node.Attribute("a").Value);
			B = Convert.ToInt32(node.Attribute("b").Value);
			C = Convert.ToInt32(node.Attribute("c").Value);

			Offset.X = Convert.ToInt32(node.Attribute("ox").Value);
			Offset.Y = Convert.ToInt32(node.Attribute("oy").Value);
		}

		public int Page;
		public Point UV;
		public Size Size;
		public int A;
		public int B;
		public int C;
		public Point Offset;
	}
}
