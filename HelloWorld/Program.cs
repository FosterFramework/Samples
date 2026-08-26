using Foster.Framework;

App.Register<Game>();
App.Run("Hello World", 640, 360);

class Game : Module
{
	public override void Render() => Graphics.Clear(0x7d9345);
}
