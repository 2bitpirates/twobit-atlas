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
	[TypeConverter(typeof(Vec2fConverter))]
	public struct Vec2f : IEquatable<Vec2f>, IIndexValue<float>
	{
		public float X;
		public float Y;

		public Vec2f(float x, float y)
		{
			X = x; Y = y;
		}

		public override string ToString()
		{
			return String.Format("{0}, {1}", X, Y);
		}

		public override bool Equals(Object obj)
		{
			if (obj is Vec2f)
			{
				return this.Equals((Vec2f)obj);
			}

			return false;
		}

		public bool Equals(Vec2f other)
		{
			return X == other.X && Y == other.Y;
		}

		public override int GetHashCode()
		{
			return (X.GetHashCode() + Y.GetHashCode());
		}

		public static bool operator !=(Vec2f value1, Vec2f value2)
		{
			return value1.X != value2.X || value1.Y != value2.Y;
		}

		public static bool operator ==(Vec2f value1, Vec2f value2)
		{
			return value1.X == value2.X && value1.Y == value2.Y;
		}

		public static Vec2f operator -(Vec2f value1, Vec2f value2)
		{
			return new Vec2f(value1.X - value2.X, value1.Y - value2.Y);
		}

		public static Vec2f operator +(Vec2f value1, Vec2f value2)
		{
			return new Vec2f(value1.X + value2.X, value1.Y + value2.Y);
		}

		public static implicit operator Vec2i(Vec2f value)
		{
			Vec2i vec;
			vec.X = (int)value.X;
			vec.Y = (int)value.Y;
			return vec;
		}

		public static readonly Vec2f Zero = new Vec2f(0, 0);

		public float this[int index]
		{
			get
			{
				if (index == 0) return X;
				if (index == 1) return Y;
				return 0.0f;
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
