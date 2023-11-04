using System.Diagnostics;
using System.Numerics;
using Foster.Framework;
using ImGuiNET;

namespace FosterImGui;

public static class Renderer
{
	private static readonly VertexFormat VertexFormat;
	private static IntPtr context;
	private static Mesh? mesh = null;
	private static Material? material = null;
	private static Texture? fontTexture = null;
	private static readonly List<Texture> boundTextures = new();
	private static readonly List<Batcher> userBatches = new();
	private static readonly List<(ImGuiKey, Keys)> keys = new()
	{
		(ImGuiKey.Tab, Keys.Tab),
		(ImGuiKey.LeftArrow, Keys.Left),
		(ImGuiKey.RightArrow, Keys.Right),
		(ImGuiKey.UpArrow, Keys.Up),
		(ImGuiKey.DownArrow, Keys.Down),
		(ImGuiKey.PageUp, Keys.PageUp),
		(ImGuiKey.PageDown, Keys.PageDown),
		(ImGuiKey.Home, Keys.Home),
		(ImGuiKey.End, Keys.End),
		(ImGuiKey.Insert, Keys.Insert),
		(ImGuiKey.Delete, Keys.Delete),
		(ImGuiKey.Backspace, Keys.Backspace),
		(ImGuiKey.Space, Keys.Space),
		(ImGuiKey.Enter, Keys.Enter),
		(ImGuiKey.Escape, Keys.Escape),
		(ImGuiKey.LeftCtrl, Keys.LeftControl),
		(ImGuiKey.LeftShift, Keys.LeftShift),
		(ImGuiKey.LeftAlt, Keys.LeftAlt),
		(ImGuiKey.LeftSuper, Keys.LeftOS),
		(ImGuiKey.RightCtrl, Keys.RightControl),
		(ImGuiKey.RightShift, Keys.RightShift),
		(ImGuiKey.RightAlt, Keys.RightAlt),
		(ImGuiKey.RightSuper, Keys.RightOS),
		(ImGuiKey.Menu, Keys.Menu),
		(ImGuiKey._0, Keys.D0),
		(ImGuiKey._1, Keys.D1),
		(ImGuiKey._2, Keys.D2),
		(ImGuiKey._3, Keys.D3),
		(ImGuiKey._4, Keys.D4),
		(ImGuiKey._5, Keys.D5),
		(ImGuiKey._6, Keys.D6),
		(ImGuiKey._7, Keys.D7),
		(ImGuiKey._8, Keys.D8),
		(ImGuiKey._9, Keys.D9),
		(ImGuiKey.A, Keys.A),
		(ImGuiKey.B, Keys.B),
		(ImGuiKey.C, Keys.C),
		(ImGuiKey.D, Keys.D),
		(ImGuiKey.E, Keys.E),
		(ImGuiKey.F, Keys.F),
		(ImGuiKey.G, Keys.G),
		(ImGuiKey.H, Keys.H),
		(ImGuiKey.I, Keys.I),
		(ImGuiKey.J, Keys.J),
		(ImGuiKey.K, Keys.K),
		(ImGuiKey.L, Keys.L),
		(ImGuiKey.M, Keys.M),
		(ImGuiKey.N, Keys.N),
		(ImGuiKey.O, Keys.O),
		(ImGuiKey.P, Keys.P),
		(ImGuiKey.Q, Keys.Q),
		(ImGuiKey.R, Keys.R),
		(ImGuiKey.S, Keys.S),
		(ImGuiKey.T, Keys.T),
		(ImGuiKey.U, Keys.U),
		(ImGuiKey.V, Keys.V),
		(ImGuiKey.W, Keys.W),
		(ImGuiKey.X, Keys.X),
		(ImGuiKey.Y, Keys.Y),
		(ImGuiKey.Z, Keys.Z),
		(ImGuiKey.F1, Keys.F1),
		(ImGuiKey.F2, Keys.F2),
		(ImGuiKey.F3, Keys.F3),
		(ImGuiKey.F4, Keys.F4),
		(ImGuiKey.F5, Keys.F5),
		(ImGuiKey.F6, Keys.F6),
		(ImGuiKey.F7, Keys.F7),
		(ImGuiKey.F8, Keys.F8),
		(ImGuiKey.F9, Keys.F9),
		(ImGuiKey.F10, Keys.F10),
		(ImGuiKey.F11, Keys.F11),
		(ImGuiKey.F12, Keys.F12),
		(ImGuiKey.Apostrophe, Keys.Apostrophe),
		(ImGuiKey.Comma, Keys.Comma),
		(ImGuiKey.Minus, Keys.Minus),
		(ImGuiKey.Period, Keys.Period),
		(ImGuiKey.Slash, Keys.Slash),
		(ImGuiKey.Semicolon, Keys.Semicolon),
		(ImGuiKey.Equal, Keys.Equals),
		(ImGuiKey.LeftBracket, Keys.LeftBracket),
		(ImGuiKey.Backslash, Keys.Backslash),
		(ImGuiKey.RightBracket, Keys.RightBracket),
		(ImGuiKey.GraveAccent, Keys.Tilde),
		(ImGuiKey.CapsLock, Keys.Capslock),
		(ImGuiKey.ScrollLock, Keys.ScrollLock),
		(ImGuiKey.NumLock, Keys.Numlock),
		(ImGuiKey.PrintScreen, Keys.PrintScreen),
		(ImGuiKey.Pause, Keys.Pause),
		(ImGuiKey.Keypad0, Keys.Keypad0),
		(ImGuiKey.Keypad1, Keys.Keypad1),
		(ImGuiKey.Keypad2, Keys.Keypad2),
		(ImGuiKey.Keypad3, Keys.Keypad3),
		(ImGuiKey.Keypad4, Keys.Keypad4),
		(ImGuiKey.Keypad5, Keys.Keypad5),
		(ImGuiKey.Keypad6, Keys.Keypad6),
		(ImGuiKey.Keypad7, Keys.Keypad7),
		(ImGuiKey.Keypad8, Keys.Keypad8),
		(ImGuiKey.Keypad9, Keys.Keypad9),
		(ImGuiKey.KeypadDecimal, Keys.KeypadPeroid),
		(ImGuiKey.KeypadDivide, Keys.KeypadDivide),
		(ImGuiKey.KeypadMultiply, Keys.KeypadMultiply),
		(ImGuiKey.KeypadSubtract, Keys.KeypadMinus),
		(ImGuiKey.KeypadAdd, Keys.KeypadPlus),
		(ImGuiKey.KeypadEnter, Keys.KeypadEnter),
		(ImGuiKey.KeypadEqual, Keys.KeypadEquals),
	};

	/// <summary>
	/// UI Scaling
	/// </summary>
	public static float Scale = 2.0f;

	/// <summary>
	/// Mouse Position relative to ImGui elements
	/// </summary>
	public static Vector2 MousePosition => Input.Mouse.Position / Scale;

	static Renderer()
	{
		int stride;
		unsafe { stride = sizeof(ImDrawVert); }

		VertexFormat = new(stride,
			new VertexFormat.Element(0, VertexType.Float2, false),
			new VertexFormat.Element(1, VertexType.Float2, false),
			new VertexFormat.Element(2, VertexType.UByte4, true)
		);
	}

	public static unsafe void Startup(string? customFontPath = null)
	{
		Debug.Assert(context == IntPtr.Zero);

		// create imgui context
		context = ImGui.CreateContext(null);
		ImGui.SetCurrentContext(context);

		var io = ImGui.GetIO();
		io.BackendFlags = ImGuiBackendFlags.None;
		io.ConfigFlags = ImGuiConfigFlags.DockingEnable;

		// load ImGui Font
		{
			if (customFontPath != null && File.Exists(customFontPath))
			{
				io.Fonts.AddFontFromFileTTF(customFontPath, 64);
				io.FontGlobalScale = 16.0f / 64.0f;
			}
			else
			{
				io.Fonts.AddFontDefault();
			}
		}

		// create font texture
		{
			io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);
			fontTexture = new Texture(width, height, new ReadOnlySpan<byte>(pixelData, width * height * 4));
		}

		// create drawing resources
		mesh = new Mesh();
		material = new(new Shader(ShaderInfo[Graphics.Renderer]));
	}

	/// <summary>
	/// Begins a new ImGui Frame.
	/// Call this at the start of your Update method.
	/// </summary>
	public static void BeginLayout()
	{
		// clear textures for the next frame
		boundTextures.Clear();

		// clear batches
		userBatches.ForEach(Pool<Batcher>.Return);
		userBatches.Clear();

		// assign font texture again
		var io = ImGui.GetIO();
		io.Fonts.SetTexID(GetTextureID(fontTexture));

		// setup io
		io.DeltaTime = Time.Delta;
		io.DisplaySize = new Vector2(App.WidthInPixels / Scale, App.HeightInPixels / Scale);
		io.DisplayFramebufferScale = Vector2.One * Scale;

		io.AddMousePosEvent(MousePosition.X, MousePosition.Y);
		io.AddMouseButtonEvent(0, Input.Mouse.LeftDown || Input.Mouse.LeftPressed);
		io.AddMouseButtonEvent(1, Input.Mouse.RightDown || Input.Mouse.RightPressed);
		io.AddMouseButtonEvent(2, Input.Mouse.MiddleDown || Input.Mouse.MiddlePressed);
		io.AddMouseWheelEvent(Input.Mouse.Wheel.X, Input.Mouse.Wheel.Y);

		foreach (var k in keys)
		{
			if (Input.Keyboard.Pressed(k.Item2))
				io.AddKeyEvent(k.Item1, true);
			if (Input.Keyboard.Released(k.Item2))
				io.AddKeyEvent(k.Item1, false);
		}

		io.AddKeyEvent(ImGuiKey.ModShift, Input.Keyboard.Shift);
		io.AddKeyEvent(ImGuiKey.ModAlt, Input.Keyboard.Alt);
		io.AddKeyEvent(ImGuiKey.ModCtrl, Input.Keyboard.Ctrl);
		io.AddKeyEvent(ImGuiKey.ModSuper, Input.Keyboard.Down(Keys.LeftOS) || Input.Keyboard.Down(Keys.RightOS));

		if (Input.Keyboard.Text.Length > 0)
		{
			for (int i = 0; i < Input.Keyboard.Text.Length; i++)
				io.AddInputCharacter(Input.Keyboard.Text[i]);
		}

		ImGui.NewFrame();
	}

	/// <summary>
	/// Ends an ImGui Frame. 
	/// Call this at the end of your Update method.
	/// </summary>
	public static void EndLayout()
	{
		ImGui.Render();
	}

	/// <summary>
	/// Begin a new Batch in an ImGui Window
	/// </summary>
	public static void BeginBatch(out Batcher batch, out Rect bounds)
	{
		var min = ImGui.GetCursorScreenPos();
		var max = min + ImGui.GetContentRegionAvail();
		var screenspace = new Rect(min, max);

		// get recycled batcher, add to list
		batch = Pool<Batcher>.Get();
		batch.Clear();
		userBatches.Add(batch);

		// notify imgui
		ImGui.GetWindowDrawList().AddCallback(new IntPtr(userBatches.Count), new IntPtr(0));

		// push relative coords
		batch.PushScissor(screenspace.Scale(Scale).Int());
		batch.PushMatrix(Matrix3x2.CreateScale(Scale));
		batch.PushMatrix(screenspace.TopLeft);

		bounds = new Rect(0, 0, screenspace.Width, screenspace.Height);
	}

	/// <summary>
	/// End a Batch in an ImGui Window
	/// </summary>
	public static void EndBatch()
	{
		var batch = userBatches[^1];
		batch.PopMatrix();
		batch.PopMatrix();
		batch.PopScissor();
	}

	/// <summary>
	/// Renders the ImGui buffers. Call this in your Render method.
	/// </summary>
	public static unsafe void Render()
	{
		if (mesh == null || material == null || material.Shader == null)
			return;

		var data = ImGui.GetDrawData();
		if (data.NativePtr == null || data.TotalVtxCount <= 0)
			return;

		var size = new Point2(App.WidthInPixels, App.HeightInPixels);

		// create pass
		var pass = new DrawCommand(null, mesh, material);
		pass.BlendMode = new BlendMode(BlendOp.Add, BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha);

		// setup ortho matrix
		Matrix4x4 mat =
			Matrix4x4.CreateScale(data.FramebufferScale.X, data.FramebufferScale.Y, 1.0f) *
			Matrix4x4.CreateOrthographicOffCenter(0, size.X, size.Y, 0, 0.1f, 1000.0f);
		material.Set("u_matrix", mat);

		// draw imgui buffers to the screen
		for (int i = 0; i < data.CmdListsCount; i++)
		{
			var list = data.CmdLists[i];

			// update vertices
			mesh.SetVertices(list.VtxBuffer.Data, list.VtxBuffer.Size, VertexFormat);
			mesh.SetIndices(list.IdxBuffer.Data, list.IdxBuffer.Size, IndexFormat.Sixteen);

			// draw each command
			var commands = (ImDrawCmd*)list.CmdBuffer.Data;
			for (ImDrawCmd* cmd = commands; cmd < commands + list.CmdBuffer.Size; cmd++)
			{
				if (cmd->UserCallback != IntPtr.Zero)
				{
					var batchIndex = cmd->UserCallback.ToInt32() - 1;
					if (batchIndex >= 0 && batchIndex < userBatches.Count)
					{
						userBatches[batchIndex].Render();
					}
				}
				else
				{
					// set texture
					var textureIndex = cmd->TextureId.ToInt32();
					if (textureIndex < boundTextures.Count)
						material.Set("u_texture", boundTextures[textureIndex]);

					pass.MeshIndexStart = (int)cmd->IdxOffset;
					pass.MeshIndexCount = (int)cmd->ElemCount;
					pass.Scissor = (RectInt)new Rect(
						cmd->ClipRect.X,
						cmd->ClipRect.Y,
						(cmd->ClipRect.Z - cmd->ClipRect.X),
						(cmd->ClipRect.W - cmd->ClipRect.Y)).Scale(data.FramebufferScale);
					pass.Submit();
				}
			}
		}
	}

	/// <summary>
	/// Shuts down ImGui
	/// </summary>
	public static void Shutdown()
	{
		ImGui.DestroyContext(context);
		context = IntPtr.Zero;
	}

	/// <summary>
	/// Gets a Texture ID to draw in ImGui
	/// </summary>
	public static IntPtr GetTextureID(Texture? texture)
	{
		var id = new IntPtr(boundTextures.Count);
		if (texture != null)
			boundTextures.Add(texture);
		return id;
	}

	private static Dictionary<Renderers, ShaderCreateInfo> ShaderInfo = new()
	{
		[Renderers.OpenGL] = new()
		{
			VertexShader =
				"#version 330\n" +
				"uniform mat4 u_matrix;" +
				"layout(location=0) in vec2 a_position;\n" +
				"layout(location=1) in vec2 a_tex;\n" +
				"layout(location=2) in vec4 a_color;\n" +
				"out vec2 v_tex;\n" +
				"out vec4 v_color;\n" +
				"void main() {\n" +
				"	gl_Position = u_matrix * vec4(a_position.xy, 0, 1);\n" +
				"	v_tex = a_tex;" +
				"	v_color = a_color;\n" +
				"}\n",
			FragmentShader =
				"#version 330\n" +
				"uniform sampler2D u_texture;\n" +
				"in vec2 v_tex;\n" +
				"in vec4 v_color;\n" +
				"out vec4 frag_color;\n" +
				"void main() {\n" +
				"	frag_color = texture(u_texture, v_tex.st) * v_color;\n" +
				"}\n"
		},
		[Renderers.D3D11] = new()
		{
			VertexShader =
				"cbuffer constants : register(b0)\n" +
				"{\n" +
				"	row_major float4x4 u_matrix;\n" +
				"}\n" +
				"struct vs_in\n" +
				"{\n" +
				"	float2 position : POS;\n" +
				"	float2 texcoord : TEX;\n" +
				"	float4 color    : COL;\n" +
				"};\n" +
				"struct vs_out\n" +
				"{\n" +
				"	float4 position : SV_POSITION;\n" +
				"	float2 texcoord : TEX;\n" +
				"	float4 color    : COL;\n" +
				"};\n" +
				"Texture2D    u_texture : register(t0);\n" +
				"SamplerState u_texture_sampler : register(s0);\n" +
				"vs_out vs_main(vs_in input)\n" +
				"{\n" +
				"	vs_out output;\n" +
				"	output.position = mul(float4(input.position, 0.0f, 1.0f), u_matrix);\n" +
				"	output.texcoord = input.texcoord;\n" +
				"	output.color = input.color;\n" +
				"	return output;\n" +
				"}\n" +
				"float4 ps_main(vs_out input) : SV_TARGET\n" +
				"{\n" +
				"	return u_texture.Sample(u_texture_sampler, input.texcoord) * input.color;\n" +
				"}\n",
			Attributes = new ShaderCreateInfo.Attribute[] {
				new("POS", 0),
				new("TEX", 0),
				new("COL", 0),
			}
		},
	};
}
