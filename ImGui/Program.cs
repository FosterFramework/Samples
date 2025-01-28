using Foster.Framework;
using System.Numerics;
using ImGuiNET;

namespace FosterImGui;

class Program
{
	public static void Main()
	{
		using var editor = new Editor();
		editor.Run();
	}
}

class Editor : App
{
	private readonly Texture image;
	private readonly Renderer imRenderer;

	public Editor() : base(new AppConfig()
	{
		ApplicationName = "ImGuiExample",
		WindowTitle = "Dear ImGui x Foster",
		Width = 1280,
		Height = 720
	})
	{
		image = new Texture(GraphicsDevice, new Image("button.png"));
		imRenderer = new(this);
	}

	protected override void Startup()
	{
	}

	protected override void Shutdown()
	{
		imRenderer.Dispose();
	}

	protected override void Update()
	{
		imRenderer.BeginLayout();

		ImGui.SetNextWindowSize(new Vector2(400, 300), ImGuiCond.Appearing);
		if (ImGui.Begin("Hello Foster x Dear ImGui"))
		{
			// show an Image button
			var imageId = imRenderer.GetTextureID(image);
			if (ImGui.ImageButton("Image", imageId, new Vector2(32, 32)))
				ImGui.OpenPopup("Image Button");

			// image buttton popup
			if (ImGui.BeginPopup("Image Button"))
			{
				ImGui.Text("You pressed the Image Button!");
				ImGui.EndPopup();
			}

			// custom sprite batcher inside imgui window
			ImGui.Text("Some Foster Sprite Batching:");
			imRenderer.BeginBatch(out var batch, out var bounds);

			batch.CheckeredPattern(bounds, 16, 16, Color.DarkGray, Color.Gray);
			batch.Circle(bounds.Center, 32, 16, Color.Red);

			imRenderer.EndBatch();			
		}
		ImGui.End();

		ImGui.ShowDemoWindow();

		imRenderer.EndLayout();
	}

	protected override void Render()
	{
		Window.Clear(Color.Black);
		imRenderer.Render();
	}
}