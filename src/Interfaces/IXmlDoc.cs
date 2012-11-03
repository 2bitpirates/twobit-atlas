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

namespace TwoBit
{
	public interface IXmlDoc
	{
		void Load(Stream stream);
		void Load(string file);

		void Save(string file);
		void Save(Stream stream);

		void Clear();

		object Root { get; }

		bool IsElement(object node);
		bool IsElement(object node, string name);

		bool IsText(object node);
	
		object GetChild(object node);
		object GetChild(object node, string name);

		object GetSibling(object node);
		object GetSibling(object node, string name);

		object GetParent(object node);

		object AddRoot(string name);
		object AddNode(object parent, string name);

		object Find(string name);
		object Find(object node, string name);

		string GetText(object node);
		string GetText(object node, string name);

		void SetText(object node, string text);
		void SetText(object node, string name, string text);
	
		void RemoveText(object node);

		void SetAttrib<T>(object node, string name, T value);

		string GetAttrib(object node, string name);
		T GetAttrib<T>(object node, string name);
		T GetAttrib<T>(object node, string name, T defaultValue);

		string GetNodeName(object node);
	}
}
