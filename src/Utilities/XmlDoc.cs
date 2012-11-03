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
using System.Xml;
using System.IO;
using System.ComponentModel;
using System.Text;

namespace TwoBit.Utilities
{
	public class XmlDoc : IXmlDoc
	{
		private XmlDocument doc;

		public XmlDoc()
		{
			doc = new XmlDocument();
			doc.InsertBefore(doc.CreateXmlDeclaration("1.0", "utf-8", null), null);
		}

		public XmlDoc(Stream stream)
		{
			doc = new XmlDocument();
			Load(stream);
		}

		public XmlDoc(string filename)
		{
			doc = new XmlDocument();
			Load(filename);
		}
		
		public void Load(Stream stream)
		{
			doc.Load(stream);
		}

		public void Load(string filename)
		{
			doc.Load(filename);
		}

		public void Save(string filename)
		{
			using (var sw = new StreamWriter(filename, false, Encoding.UTF8))
			{
				doc.Save(sw);
			}
		}

		public void Save(Stream stream)
		{
			using (var sw = new StreamWriter(stream, Encoding.UTF8))
			{
				doc.Save(sw);
			}
		}

		public void Clear()
		{
			doc.RemoveAll();
		}

		public object Root { get { return doc; } }

		public bool IsElement(object node)
		{
			return (node is XmlNode) && ((XmlNode)node).NodeType == XmlNodeType.Element;
		}

		public bool IsElement(object node, string name)
		{
			return IsElement(node) && ((XmlNode)node).Name.CompareTo(name) == 0;
		}

		public bool IsText(object node)
		{
			return (node is XmlNode) && ((XmlNode)node).NodeType == XmlNodeType.Text;
		}

		public object GetChild(object node)
		{
			if (node is XmlNode)
			{
				XmlNode n = (XmlNode)node;
				n = n.FirstChild;

				while (n != null && !IsElement(n))
				{
					n = n.NextSibling;
				}

				return n;
			}

			return null;
		}

		public object GetChild(object node, string name)
		{
			if (node is XmlNode)
			{
				XmlNode n = (XmlNode)node;
				n = n.FirstChild;

				while (n != null && !IsElement(n, name))
				{
					n = n.NextSibling;
				}

				return n;
			}

			return null;
		}

		public object GetSibling(object node)
		{
			if (node is XmlNode)
			{
				XmlNode n = (XmlNode)node;
				n = n.NextSibling;
				
				while (n != null && !IsElement(n))
				{
					n = n.NextSibling;
				}

				return n;
			}

			return null;
		}

		public object GetSibling(object node, string name)
		{
			if (node is XmlNode)
			{
				XmlNode n = (XmlNode)node;
				n = n.NextSibling;
				
				while (n != null && !IsElement(n, name))
				{
					n = n.NextSibling;
				}

				return n;
			}

			return null;
		}

		public object GetParent(object node)
		{
			return node is XmlNode ? ((XmlNode)node).ParentNode : null;
		}

		public object AddRoot(string name)
		{
			if (GetChild(doc) != null)
			{
				throw new Exception("Root already exists");
			}

			return AddNode(doc, name);
		}

		public object AddNode(object parent, string name)
		{
			if (parent is XmlNode)
			{
				return ((XmlNode)parent).AppendChild(doc.CreateElement(name));
			}
			return null;
		}

		public object Find(string name)
		{
			return IterateFind(doc, name);
		}

		public object Find(object node, string name)
		{
			return IterateFind(node, name);
		}

		public string GetText(object node)
		{
			if (node is XmlNode)
			{
				XmlNode n = (XmlNode)node;
				for (n = n.FirstChild; n != null; n = n.NextSibling)
				{
					if (n.NodeType == XmlNodeType.Text)
					{
						return n.Value;
					}
				}
			}

			return string.Empty;
		}

		public string GetText(object node, string name)
		{
			return GetText(GetChild(node, name));
		}

		public void SetText(object node, string text)
		{
			if (node is XmlNode)
			{
				RemoveText(node);
				((XmlNode)node).AppendChild(doc.CreateTextNode(text));
			}
		}

		public void SetText(object node, string name, string text)
		{
			if (node is XmlNode)
			{
				object child = Find(node, name);
				if (child == null)
				{
					child = AddNode(node, name);
				}

				SetText(child, text);
			}
		}

		public void RemoveText(object node)
		{
			if (node is XmlNode)
			{
				XmlNode n = (XmlNode)node;
				for (XmlNode child = n.FirstChild; child != null; child = child.NextSibling)
				{
					if (child.NodeType == XmlNodeType.Text)
					{
						n.RemoveChild(child);
						return;
					}
				}
			}
		}

		public void SetAttrib<T>(object node, string name, T value)
		{
			if (node is XmlNode)
			{
				XmlAttribute attrib = doc.CreateAttribute(name);
				attrib.Value = value.ToString();
				((XmlNode)node).Attributes.SetNamedItem(attrib);
			}
		}

		public string GetAttrib(object node, string name)
		{
			return GetAttrib(node, name, "");
		}

		public T GetAttrib<T>(object node, string name)
		{
			return GetAttrib<T>(node, name, default(T));
		}

		public T GetAttrib<T>(object node, string name, T defaultValue)
		{
			if (node is XmlNode)
			{
				XmlAttribute attrib = FindAttribute(node, name);
				if (attrib != null)
				{
					return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(attrib.Value);
				}
			}

			return defaultValue;
		}

		private XmlAttribute FindAttribute(object node, string name)
		{
			if (node is XmlNode)
			{
				XmlAttributeCollection attribs = ((XmlNode)node).Attributes;
				return (XmlAttribute)attribs.GetNamedItem(name);
			}

			return null;
		}

		public string GetNodeName(object node)
		{
			return node is XmlNode ? ((XmlNode)node).Name : null;
		}

		private object IterateFind(object node, string tag)
		{
			if (IsElement(node, tag))
			{
				return node;
			}

			for (node = GetChild(node, tag); node != null; node = GetSibling(node, tag))
			{
				object n = IterateFind(node, tag);
				if (n != null)
				{
					return n;
				}
			}

			return null;
		}
	}
}
