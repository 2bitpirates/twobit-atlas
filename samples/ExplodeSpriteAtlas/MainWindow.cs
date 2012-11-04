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
using System.Windows.Forms;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ExplodeSpriteAtlas
{
	class MainWindow : GameWindow
	{
		private const float playRate = 8.0f;	// frames per second
		private const float invPlayRate = 1.0f / playRate;

		private AtlasGL atlas;
		private int currentFrame = 0;

		public MainWindow() :
			base(480, 320, OpenTK.Graphics.GraphicsMode.Default, Application.ProductName)
		{
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			MakeCurrent();

			// init default GL setings
			GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
			GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);

			GL.DepthFunc(DepthFunction.Lequal);
			GL.Enable(EnableCap.DepthTest);

			GL.AlphaFunc(AlphaFunction.Greater, 0.01f);
			GL.Enable(EnableCap.AlphaTest);
			GL.Enable(EnableCap.Blend);
			GL.Enable(EnableCap.Texture2D);

			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.Zero);

			GL.Disable(EnableCap.LineSmooth);
			GL.Disable(EnableCap.CullFace);
			GL.Disable(EnableCap.Lighting);

			// load atlas
			atlas = new AtlasGL();
			atlas.Load("Content/explode.atlas");
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			SetOrthoView();
			SetViewport();
			Render();
		}

		private void SetViewport()
		{
			GL.Viewport(0, 0, Width, Height);
		}

		private void SetOrthoView()
		{
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(-Width * 0.5f, Width * 0.5f, Height * 0.5f, -Height * 0.5f, 0.0f, 1.1f);
			GL.MatrixMode(MatrixMode.Modelview);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			float totalSeconds = Environment.TickCount / 1000.0f;
			currentFrame = (int)(totalSeconds / invPlayRate);

			// have the animation loop thru all the glyphs
			currentFrame = currentFrame % atlas.Glyphs.Count;
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			Render();
		}

		private void RenderAtlasFrame(int frame, float x, float y)
		{
			GL.PushMatrix();
			GL.Translate(x, y, 0.0f);

			Glyph glyph;
			// check if the frame index is in the atlas
			if (atlas.Glyphs.TryGetValue(frame, out glyph))
			{
				// set the correct texture. usually an atlas only has one
				var tex = atlas.Textures[glyph.Page];
				GL.BindTexture(TextureTarget.Texture2D, tex.ID);

				// get the glyph rectangle region on the image in UV coords
				float u0 = glyph.UV.X / (float)tex.Width;
				float v0 = glyph.UV.Y / (float)tex.Height;

				float u1 = (glyph.UV.X + glyph.Size.Width) / (float)tex.Width;
				float v1 = (glyph.UV.Y + glyph.Size.Height) / (float)tex.Height;

				GL.PushMatrix();
				// apply the glyphs origin
				GL.Translate(glyph.Offset.X, glyph.Offset.Y, 0.0f);

				// scale the unit quad to match the texture size of the glyph
				GL.Scale(glyph.Size.Width, glyph.Size.Height, 1.0f);
				
				// draw a quad
				GL.Begin(BeginMode.Quads);

				GL.TexCoord2(u0, v0); GL.Vertex2(0.0f, 0.0f);
				GL.TexCoord2(u1, v0); GL.Vertex2(1.0f, 0.0f);
				GL.TexCoord2(u1, v1); GL.Vertex2(1.0f, 1.0f);
				GL.TexCoord2(u0, v1); GL.Vertex2(0.0f, 1.0f);

				GL.End();

				GL.PopMatrix();

				// this translate is here only for reference (used when iterating a string vs a single glyph)
				GL.Translate(glyph.A + glyph.B + glyph.C, 0, 0.0f);
			}

			GL.PopMatrix();
		}

		private void Render()
		{
			MakeCurrent();
			GL.ClearColor(0, 0, 0.14f, 1);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.MatrixMode(MatrixMode.Modelview);

			if (atlas != null)
			{
				RenderAtlasFrame(currentFrame, 0, 0);
			}

			SwapBuffers();
		}
	}
}
