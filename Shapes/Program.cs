
using Foster.Framework;
using System.Numerics;

using var game = new Game();
game.Run();

class Game : App
{
	private const float Acceleration = 1200;
	private const float Friction = 500;
	private const float MaxSpeed = 800;

	private readonly Batcher batch;
	private readonly Texture texture;
	private Vector2 pos = new(128, 128);
	private Vector2 speed = new();

	public Game() : base(new AppConfig()
	{
		ApplicationName = "Shapes",
		WindowTitle = "Hello Shapes",
		Width = 1280,
		Height = 720
	})
	{
		batch = new(GraphicsDevice);
		texture = new(GraphicsDevice, new Image(128, 128, Color.Blue));
	}

	protected override void Startup() {}
	protected override void Shutdown() {}

	protected override void Update()
	{
		Window.Title = $"Something Else {Window.Width}x{Window.Height} : {Window.WidthInPixels}x{Window.HeightInPixels}";

		if (Input.Keyboard.Down(Keys.Left))
			speed.X -= Acceleration * Time.Delta;
		if (Input.Keyboard.Down(Keys.Right))
			speed.X += Acceleration * Time.Delta;
		if (Input.Keyboard.Down(Keys.Up))
			speed.Y -= Acceleration * Time.Delta;
		if (Input.Keyboard.Down(Keys.Down))
			speed.Y += Acceleration * Time.Delta;

		if (!Input.Keyboard.Down(Keys.Left, Keys.Right))
			speed.X = Calc.Approach(speed.X, 0, Time.Delta * Friction);
		if (!Input.Keyboard.Down(Keys.Up, Keys.Down))
			speed.Y = Calc.Approach(speed.Y, 0, Time.Delta * Friction);

		if (Input.Keyboard.Pressed(Keys.F4))
			Window.Fullscreen = !Window.Fullscreen;

		if (speed.Length() > MaxSpeed)
			speed = speed.Normalized() * MaxSpeed;

		pos += speed * Time.Delta;
	}

	protected override void Render()
	{
		Window.Clear(new Color(
			Calc.Clamp(Input.Mouse.X / Window.WidthInPixels, 0, 1),
			Calc.Clamp(Input.Mouse.Y / Window.HeightInPixels, 0, 1),
			0.25f,
			1.0f
		));

		batch.PushMatrix(
			new Vector2(Window.WidthInPixels, Window.HeightInPixels) / 2,
			Vector2.One,
			new Vector2(texture.Width, texture.Height) / 2,
			(float)Time.Elapsed.TotalSeconds * 4.0f);

		batch.Image(texture, Vector2.Zero, Color.White);
		batch.PopMatrix();

		batch.Circle(new Circle(pos, 64), 16, Color.Red);
		batch.Circle(new Circle(Input.Mouse.Position, 8), 16, Color.White);

		batch.Render(Window);
		batch.Clear();
	}
}