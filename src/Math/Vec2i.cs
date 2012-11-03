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

namespace TwoBit
{
	[TypeConverter(typeof(Vec2iConverter))]
	public struct Vec2i : IEquatable<Vec2i>, IIndexValue<int>
	{
		public int X;
		public int Y;

		public Vec2i(int x, int y)
		{
			X = x; Y = y;
		}

		public override string ToString()
		{
			return String.Format("{0}, {1}", X, Y);
		}

		public override bool Equals(Object obj)
		{
			if (obj is Vec2i)
			{
				return this.Equals((Vec2i)obj);
			}

			return false;
		}

		public bool Equals(Vec2i other)
		{
			return X == other.X && Y == other.Y;
		}

		public override int GetHashCode()
		{
			return (X.GetHashCode() + Y.GetHashCode());
		}

		public static bool operator !=(Vec2i value1, Vec2i value2)
		{
			return value1.X != value2.X || value1.Y != value2.Y;
		}

		public static bool operator ==(Vec2i value1, Vec2i value2)
		{
			return value1.X == value2.X && value1.Y == value2.Y;
		}

		public static Vec2i operator -(Vec2i value1, Vec2i value2)
		{
			return new Vec2i(value1.X - value2.X, value1.Y - value2.Y);
		}

		public static Vec2i operator +(Vec2i value1, Vec2i value2)
		{
			return new Vec2i(value1.X + value2.X, value1.Y + value2.Y);
		}

		public static readonly Vec2i Zero = new Vec2i(0, 0);

		public int this[int index]
		{
			get
			{
				if (index == 0) return X;
				if (index == 1) return Y;
				return 0;
			}

			set
			{
				if (index == 0) X = value;
				else
				if (index == 1) Y = value;
			}
		}
	}
}
