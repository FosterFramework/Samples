using System.Diagnostics;
using System.Numerics;
using Foster.Framework;
using ImGuiNET;

namespace FosterImGui;

public class Renderer : IDisposable
{
	private readonly App app;
	private readonly IntPtr context;
	private readonly Mesh<PosTexColVertex, ushort> mesh;
	private readonly Material material;
	private readonly Texture fontTexture;
	private readonly List<Texture> boundTextures = [];
	private readonly List<Batcher> batchersUsed = [];
	private readonly Stack<Batcher> batchersStack = [];
	private readonly Stack<Batcher> batcherPool = [];
	private readonly List<(ImGuiKey, Keys)> keys =
	[
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
	];

	private PosTexColVertex[] vertices = [];
	private ushort[] indices = [];

	/// <summary>
	/// UI Scaling
	/// </summary>
	public float Scale = 2.0f;

	/// <summary>
	/// Mouse Position relative to ImGui elements
	/// </summary>
	public Vector2 MousePosition => app?.Input.Mouse.Position / Scale ?? Vector2.Zero;

	/// <summary>
	/// If the ImGui Context wants text input
	/// </summary>
	public bool WantsTextInput { get; private set; }

	public Renderer(App app, string? customFontPath = null)
	{
		this.app = app;

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
		unsafe
		{
			io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);
			fontTexture = new Texture(app.GraphicsDevice, width, height, new ReadOnlySpan<byte>(pixelData, width * height * 4));
		}

		// create drawing resources
		mesh = new Mesh<PosTexColVertex, ushort>(app.GraphicsDevice);
		material = new(new TexturedShader(app.GraphicsDevice));
		ImGui.SetCurrentContext(nint.Zero);
	}

	~Renderer() => Dispose();

	/// <summary>
	/// Begins a new ImGui Frame.
	/// ImGui methods are available between BeginLayout and EndLayout.
	/// </summary>
	public void BeginLayout()
	{
		Debug.Assert(ImGui.GetCurrentContext() == nint.Zero);
		ImGui.SetCurrentContext(context);

		// clear textures for the next frame
		boundTextures.Clear();

		// clear batches
		batchersStack.Clear();
		batchersUsed.ForEach(batcherPool.Push);
		batchersUsed.Clear();

		// assign font texture again
		var io = ImGui.GetIO();
		io.Fonts.SetTexID(GetTextureID(fontTexture));

		// setup io
		io.DeltaTime = app.Time.Delta;
		io.DisplaySize = new Vector2(app.Window.WidthInPixels / Scale, app.Window.HeightInPixels / Scale);
		io.DisplayFramebufferScale = Vector2.One * Scale;

		io.AddMousePosEvent(MousePosition.X, MousePosition.Y);
		io.AddMouseButtonEvent(0, app.Input.Mouse.LeftDown || app.Input.Mouse.LeftPressed);
		io.AddMouseButtonEvent(1, app.Input.Mouse.RightDown || app.Input.Mouse.RightPressed);
		io.AddMouseButtonEvent(2, app.Input.Mouse.MiddleDown || app.Input.Mouse.MiddlePressed);
		io.AddMouseWheelEvent(app.Input.Mouse.Wheel.X, app.Input.Mouse.Wheel.Y);

		foreach (var k in keys)
		{
			if (app.Input.Keyboard.Pressed(k.Item2))
				io.AddKeyEvent(k.Item1, true);
			if (app.Input.Keyboard.Released(k.Item2))
				io.AddKeyEvent(k.Item1, false);
		}

		io.AddKeyEvent(ImGuiKey.ModShift, app.Input.Keyboard.Shift);
		io.AddKeyEvent(ImGuiKey.ModAlt, app.Input.Keyboard.Alt);
		io.AddKeyEvent(ImGuiKey.ModCtrl, app.Input.Keyboard.Ctrl);
		io.AddKeyEvent(ImGuiKey.ModSuper, app.Input.Keyboard.Down(Keys.LeftOS) || app.Input.Keyboard.Down(Keys.RightOS));

		if (app.Input.Keyboard.Text.Length > 0)
		{
			for (int i = 0; i < app.Input.Keyboard.Text.Length; i++)
				io.AddInputCharacter(app.Input.Keyboard.Text[i]);
		}

		WantsTextInput = io.WantTextInput;

		ImGui.NewFrame();
	}

	/// <summary>
	/// Ends an ImGui Frame. 
	/// Call this at the end of your Update method.
	/// </summary>
	public void EndLayout()
	{
		Debug.Assert(ImGui.GetCurrentContext() == context);
		ImGui.Render();
		ImGui.SetCurrentContext(nint.Zero);
	}

	/// <summary>
	/// Begin a new Batch in an ImGui Window. Returns true if any batch contents
	/// will be visible. Call <see cref="EndBatch"/> regardless of return value.
	/// </summary>
	public bool BeginBatch(out Batcher batch, out Rect bounds)
	{
		return BeginBatch(ImGui.GetContentRegionAvail(), out batch, out bounds);
	}

	/// <summary>
	/// Begin a new Batch in an ImGui Window. Returns true if any batch contents
	/// will be visible. Call <see cref="EndBatch"/> regardless of return value.
	/// </summary>
	public bool BeginBatch(Vector2 size, out Batcher batch, out Rect bounds)
	{
		var min = ImGui.GetCursorScreenPos();
		var max = min + size;
		var screenspace = Rect.Between(min, max);
		var clip = Rect.Between(ImGui.GetWindowDrawList().GetClipRectMin(), ImGui.GetWindowDrawList().GetClipRectMax());
		var scissor = screenspace.GetIntersection(clip).Scale(Scale).Int();

		// create a dummy element of the given size
		ImGui.Dummy(size);

		// get recycled batcher, add to list
		batch = batcherPool.Count > 0 ? batcherPool.Pop() : new Batcher(app.GraphicsDevice);
		batch.Clear();
		batchersUsed.Add(batch);
		batchersStack.Push(batch);

		// notify imgui
		ImGui.GetWindowDrawList().AddCallback(new IntPtr(batchersUsed.Count), new IntPtr(0));

		// push relative coords
		batch.PushScissor(scissor);
		batch.PushMatrix(Matrix3x2.CreateScale(Scale));
		batch.PushMatrix(screenspace.TopLeft);

		bounds = new Rect(0, 0, screenspace.Width, screenspace.Height);

		return scissor.Width > 0 && scissor.Height > 0;
	}

	/// <summary>
	/// End a Batch in an ImGui Window
	/// </summary>
	public void EndBatch()
	{
		var batch = batchersStack.Pop();
		batch.PopMatrix();
		batch.PopMatrix();
		batch.PopScissor();
	}

	/// <summary>
	/// Renders the ImGui buffers. Call this in your Render method.
	/// </summary>
	public unsafe void Render()
	{
		Debug.Assert(ImGui.GetCurrentContext() == nint.Zero);
		ImGui.SetCurrentContext(context);

		var data = ImGui.GetDrawData();
		if (data.NativePtr == null || data.TotalVtxCount <= 0)
		{
			ImGui.SetCurrentContext(nint.Zero);
			return;
		}

		// build vertex/index buffer lists
		{
			// calculate total size
			var vertexCount = 0;
			var indexCount = 0;
			for (int i = 0; i < data.CmdListsCount; i ++)
			{
				vertexCount += data.CmdLists[i].VtxBuffer.Size;
				indexCount += data.CmdLists[i].IdxBuffer.Size;
			}

			// make sure we have enough space
			if (vertexCount > vertices.Length)
				Array.Resize(ref vertices, vertexCount);
			if (indexCount > indices.Length)
				Array.Resize(ref indices, indexCount);

			// copy data to arrays
			vertexCount = indexCount = 0;
			for (int i = 0; i < data.CmdListsCount; i ++)
			{
				var list = data.CmdLists[i];
				var vertexSrc = new Span<PosTexColVertex>((void*)list.VtxBuffer.Data, list.VtxBuffer.Size);
				var indexSrc = new Span<ushort>((void*)list.IdxBuffer.Data, list.IdxBuffer.Size);

				vertexSrc.CopyTo(vertices.AsSpan()[vertexCount..]);
				indexSrc.CopyTo(indices.AsSpan()[indexCount..]);

				vertexCount += vertexSrc.Length;
				indexCount += indexSrc.Length;
			}

			// upload buffers to mesh
			mesh.SetVertices(vertices.AsSpan(0, vertexCount));
			mesh.SetIndices(indices.AsSpan(0, indexCount));
		}

		var size = new Point2(app.Window.WidthInPixels, app.Window.HeightInPixels);

		// create pass
		var pass = new DrawCommand(app.Window, mesh, material);
		pass.BlendMode = new BlendMode(BlendOp.Add, BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha);

		// setup ortho matrix
		Matrix4x4 mat =
			Matrix4x4.CreateScale(data.FramebufferScale.X, data.FramebufferScale.Y, 1.0f) *
			Matrix4x4.CreateOrthographicOffCenter(0, size.X, size.Y, 0, 0.1f, 1000.0f);
		material.Vertex.SetUniformBuffer(mat);

		// draw imgui buffers to the screen
		var globalVtxOffset = 0;
		var globalIdxOffset = 0;
		for (int i = 0; i < data.CmdListsCount; i++)
		{
			var imList = data.CmdLists[i];
			var imCommands = (ImDrawCmd*)imList.CmdBuffer.Data;

			// draw each command
			for (ImDrawCmd* cmd = imCommands; cmd < imCommands + imList.CmdBuffer.Size; cmd++)
			{
				var scissor = new Rect(
					cmd->ClipRect.X,
					cmd->ClipRect.Y,
					cmd->ClipRect.Z - cmd->ClipRect.X,
					cmd->ClipRect.W - cmd->ClipRect.Y).Scale(data.FramebufferScale).Int();

				if (scissor.Width <= 0 || scissor.Height <= 0)
					continue;

				if (cmd->UserCallback != IntPtr.Zero)
				{
					var batchIndex = cmd->UserCallback.ToInt32() - 1;
					if (batchIndex >= 0 && batchIndex < batchersUsed.Count)
						batchersUsed[batchIndex].Render(app.Window, viewport: null, scissor: scissor);
				}
				else
				{
					var textureIndex = cmd->TextureId.ToInt32();
					if (textureIndex < boundTextures.Count)
						material.Fragment.Samplers[0] = new(boundTextures[textureIndex], new());

					pass.MeshVertexOffset = (int)(cmd->VtxOffset + globalVtxOffset);
					pass.MeshIndexStart = (int)(cmd->IdxOffset + globalIdxOffset);
					pass.MeshIndexCount = (int)cmd->ElemCount;
					pass.Scissor = scissor;

					app.GraphicsDevice.Draw(pass);
				}
			}

			globalVtxOffset += imList.VtxBuffer.Size;
			globalIdxOffset += imList.IdxBuffer.Size;
		}

		ImGui.SetCurrentContext(nint.Zero);
	}

	/// <summary>
	/// Gets a Texture ID to draw in ImGui
	/// </summary>
	public IntPtr GetTextureID(Texture? texture)
	{
		var id = new IntPtr(boundTextures.Count);
		if (texture != null)
			boundTextures.Add(texture);
		return id;
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		ImGui.DestroyContext(context);
	}
}
