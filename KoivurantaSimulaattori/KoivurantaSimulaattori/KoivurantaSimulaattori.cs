using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

namespace KoivurantaSimulaattori;

/// @author gr313123
/// @version 12.11.2025
/// <summary>
/// 
/// </summary>
public class KoivurantaSimulaattori : PhysicsGame
{
    public override void Begin()
    {
        Bus bus = new Bus();
        Add(bus.GetObject());

        Keyboard.Listen(Key.W, ButtonState.Down, bus.Forward, "Drives forward");
        Keyboard.Listen(Key.A, ButtonState.Down, bus.Left, "Steer left");
        Keyboard.Listen(Key.S, ButtonState.Down, bus.Brake, "Brake");
        Keyboard.Listen(Key.D, ButtonState.Down, bus.Right, "Steer right");
        Keyboard.Listen(Key.D, ButtonState.Released, bus.SteeringRelease, "Steer right");
        Keyboard.Listen(Key.A, ButtonState.Released, bus.SteeringRelease, "Steer right");
        Camera.ZoomFactor = 0.5;

    }
}