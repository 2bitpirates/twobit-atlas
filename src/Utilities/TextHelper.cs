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
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TwoBit.Utilities
{
	enum GlyphOutline : uint
	{
		GGO_BEZIER = 3,
		GGO_BITMAP = 1,
		GGO_GLYPH_INDEX = 0x80,
		GGO_GRAY2_BITMAP = 4,
		GGO_GRAY4_BITMAP = 5,
		GGO_GRAY8_BITMAP = 6,
		GGO_METRICS = 0,
		GGO_NATIVE = 2,
		GGO_UNHINTED = 0x100
	}

	[Flags]
	enum DrawTextFormats : int
	{
		DT_BOTTOM = 8,
		DT_CALCRECT = 0x400,
		DT_CENTER = 1,
		DT_EDITCONTROL = 0x2000,
		DT_END_ELLIPSIS = 0x8000,
		DT_EXPANDTABS = 0x40,
		DT_EXTERNALLEADIN = 0x200,
		DT_HIDEPREFIX = 0x100000,
		DT_INTERNAL = 0x1000,
		DT_LEFT = 0,
		DT_MODIFYSTRING = 0x10000,
		DT_NOCLIP = 0x100,
		DT_NOFULLWIDTHCHARBREAK = 0x80000,
		DT_NOPREFIX = 0x800,
		DT_PATH_ELLIPSIS = 0x4000,
		DT_PREFIXONLY = 0x200000,
		DT_RIGHT = 2,
		DT_RTLLEADIN = 0x20000,
		DT_SINGLELINE = 0x20,
		DT_TABSTOPS = 0x80,
		DT_TOP = 0,
		DT_VCENTER = 4,
		DT_WORD_ELLIPSIS = 0x40000,
		DT_WORDBREAK = 0x10
	}

	[StructLayout(LayoutKind.Sequential)]
	struct GLYPHMETRICS
	{
		public uint gmBlackBoxX;
		public uint gmBlackBoxY; 
		[MarshalAs(UnmanagedType.Struct)]
		public Point gmptGlyphOrigin;
		public short gmCellIncX;
		public short gmCellIncY; 
	}

	[StructLayout(LayoutKind.Sequential)]
	struct FIXED
	{ 
		public ushort fract; 
		public short value;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct MAT2
	{
		[MarshalAs(UnmanagedType.Struct)]
		public FIXED eM11;
		[MarshalAs(UnmanagedType.Struct)]
		public FIXED eM12;
		[MarshalAs(UnmanagedType.Struct)]
		public FIXED eM21;
		[MarshalAs(UnmanagedType.Struct)]
		public FIXED eM22; 
	}

	[StructLayout(LayoutKind.Sequential)]
	struct LOGFONT
	{
		public int lfHeight;
		public int lfWidth;
		public int lfEscapement;
		public int lfOrientation;
		public int lfWeight;
		public byte lfItalic;
		public byte lfUnderline;
		public byte lfStrikeOut;
		public byte lfCharSet;
		public byte lfOutPrecision;
		public byte lfClipPrecision;
		public byte lfQuality;
		public byte lfPitchAndFamily;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
		public byte[] lfFaceName;
	}

	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	struct TEXTMETRIC
	{
		public int tmHeight;
		public int tmAscent;
		public int tmDescent;
		public int tmInternalLeading;
		public int tmExternalLeading;
		public int tmAveCharWidth;
		public int tmMaxCharWidth;
		public int tmWeight;
		public int tmOverhang;
		public int tmDigitizedAspectX;
		public int tmDigitizedAspectY;
		public char tmFirstChar;
		public char tmLastChar;
		public char tmDefaultChar;
		public char tmBreakChar;
		public byte tmItalic;
		public byte tmUnderlined;
		public byte tmStruckOut;
		public byte tmPitchAndFamily;
		public byte tmCharSet;

		private static TEXTMETRIC _default = new TEXTMETRIC();

		public static TEXTMETRIC Default
		{
			get { return _default; }
		}
	}

	static class TextHelper
	{
		private class FontDC : IDisposable
		{
			private IntPtr hdc;
			private IntPtr hfont;
			private bool disposed;

			public FontDC(Font font)
			{
				hdc = GetDC(IntPtr.Zero);
				hfont = SelectObject(hdc, font.ToHfont());
				disposed = false;
			}

			public IntPtr handle
			{
				get { return hdc; }
			}

			public void Dispose()
			{
				if (!disposed)
				{
					disposed = true;
					SelectObject(hdc, hfont);
					ReleaseDC(IntPtr.Zero, hdc);
				}
			}

			[DllImport("gdi32.dll")]
			private extern static IntPtr SelectObject(IntPtr dc, IntPtr obj);

			[DllImport("user32.dll")]
			private extern static IntPtr GetDC(IntPtr handle);

			[DllImport("user32.dll", ExactSpelling = true)]
			public extern static int ReleaseDC(IntPtr handle, IntPtr dc);
		}

		public static void GetTextMetrics(Font font, out TEXTMETRIC tm)
		{
			using (FontDC dc = new FontDC(font))
			{
				GetTextMetrics(dc.handle, out tm);
			}
		}

		public static void GetCharABCWidths(Font font, char ch, out ABC abc)
		{
			char[] chars = new char[1];
			chars[0] = ch;
			ABC[] _abc = GetCharABCWidths(font, chars);
			abc = _abc[0];
		}

		public static ABC[] GetCharABCWidths(Font font, char[] chars)
		{
			int count = chars.GetLength(0);
			if (count <= 0)
			{
				throw new ArgumentException("chars count invalid");
			}

			ABC[] abc = new ABC[count];

			using (FontDC dc = new FontDC(font))
			{
				int idx = 0;
				foreach (char ch in chars)
				{
					ABC _abc;
					if (!GetCharABCWidths(dc.handle, ch, ch, out _abc))
					{
						throw new Exception("GetCharABCWidths failed");
					}
					abc[idx++] = _abc;
				}
			}
			return abc;
		}

		public static ABC[] GetCharABCWidths(Font font, char firstChar, char lastChar)
		{
			int count = (int)lastChar - (int)firstChar + 1;
			if (count <= 0)
			{
				throw new ArgumentException("invalid character range");
			}

			char[] chars = new char[count];
			int idx = (int)firstChar;
			for (int i=0; i<count; i++)
			{
				chars[i] = (char)(firstChar + i);
			}

			return GetCharABCWidths(font, chars);
		}

		public static Image GetGlyphOutlineImage(Font font, char ch, out GLYPHMETRICS gm)
		{
			byte[] alpha;
			int size;

			using (FontDC dc = new FontDC(font))
			{
				MAT2 mat = new MAT2();
				mat.eM11.value = 1;
				mat.eM22.value = 1;

				size = (int)GetGlyphOutline(dc.handle, ch, (uint)GlyphOutline.GGO_GRAY8_BITMAP, out gm, 0, IntPtr.Zero, ref mat);
				if (size <= 0)
				{
					return null;
				}

				IntPtr bufptr = Marshal.AllocHGlobal(size);

				try
				{
					alpha = new byte[size];
					GetGlyphOutline(dc.handle, ch, (uint)GlyphOutline.GGO_GRAY8_BITMAP, out gm, (uint)size, bufptr, ref mat);
					Marshal.Copy(bufptr, alpha, 0, size);
				}
				finally
				{
					Marshal.FreeHGlobal(bufptr);
				}
			}

			int stride = (int)(gm.gmBlackBoxX + 3) & ~3; // dword alinged
			Bitmap bmp = new Bitmap(stride, (int)gm.gmBlackBoxY, PixelFormat.Format32bppArgb);
			BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			int bmpSize = bd.Stride * bd.Height;
			byte[] bmpValues = new byte[bmpSize];
			Marshal.Copy(bd.Scan0, bmpValues, 0, bmpSize);

			const int bias = 0x40;
			for (int i = 0; i < size; i++)
			{
				byte v = (byte)((int)alpha[i] * 255 / bias);
				bmpValues[i * 4 + 0] = 0xff;
				bmpValues[i * 4 + 1] = 0xff;
				bmpValues[i * 4 + 2] = 0xff;
				bmpValues[i * 4 + 3] = v;
			}

			Marshal.Copy(bmpValues, 0, bd.Scan0, bmpSize);
			bmp.UnlockBits(bd);

			return bmp;
		}

		[DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
		private static extern uint GetGlyphOutline(IntPtr hdc, uint ch, uint format, out GLYPHMETRICS gm, uint cb, IntPtr buffer, ref MAT2 mat2);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto)]
		private static extern bool GetTextMetrics(IntPtr hdc, out TEXTMETRIC lptm);

		[DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
		private static extern bool GetCharABCWidths(IntPtr hdc, uint firstChar, uint lastChar, out ABC abc);
	}
}
