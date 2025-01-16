
using Foster.Framework;

namespace TinyLink;

public class Controls(Input input)
{
	public readonly VirtualStick Move = new(input, 
		new StickBinding(0.2f, AxisBinding.Overlaps.TakeNewer)
			.AddArrowKeys()
			.AddDPad()
			.AddLeftJoystick()
	);

	public readonly VirtualAction Jump = new(input,
		new ActionBinding()
			.Add(Keys.X)
			.Add(Buttons.South)
	);

	public readonly VirtualAction Attack = new(input,
		new ActionBinding()
			.Add(Keys.C)
			.Add(Buttons.West)
	);
}