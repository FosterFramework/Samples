using System.Numerics;
using Foster.Framework;

App.Register<Game>();
App.Run("Image Scaling", 960, 540);

class Game : Module
{
	readonly Texture Logo = new(new Image(System.IO.Path.Join("Assets", "FosterLogo.png")));
	readonly Batcher Batcher = new();

	static RectInt Viewport => new(0, 0, App.WidthInPixels, App.HeightInPixels);
	static int Scale => Calc.Min(App.WidthInPixels / App.Width, App.HeightInPixels / App.Height);

	public override void Render()
	{
		Graphics.Clear(0xdeb813);

		Batcher.Clear();
		Batcher.Image(Logo, Viewport.Center, Logo.Size / 2, Vector2.One * Scale, 0, Color.White);
		Batcher.Render();
	}
}
