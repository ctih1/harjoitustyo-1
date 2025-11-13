using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    private Road road;
    private Thread roadThread;
    public override void Begin()
    {
        road = new Road(this, LoadImage("road"), LoadImage("left"), LoadImage("right"));
        Bus bus = new Bus(Screen);
        Add(bus.GetObject());
        Add(bus.GetSpeedo());
        road.LoadBus(bus);
        Keyboard.Listen(Key.W, ButtonState.Down, bus.Forward, "Drives forward");
        Keyboard.Listen(Key.A, ButtonState.Down, bus.Left, "Steer left");
        Keyboard.Listen(Key.S, ButtonState.Down, bus.Brake, "Brake");
        Keyboard.Listen(Key.D, ButtonState.Down, bus.Right, "Steer right");
        Keyboard.Listen(Key.D, ButtonState.Released, bus.SteeringRelease, "Steer right");
        Keyboard.Listen(Key.A, ButtonState.Released, bus.SteeringRelease, "Steer right");
        Camera.ZoomFactor = 0.5;
        Camera.FollowedObject = bus.GetObject();
    }

    protected override void Update(Time time)
    {
        Task.Run(() => road.PhysicsUpdate(Camera, Screen));
        base.Update(Time);
    }
}