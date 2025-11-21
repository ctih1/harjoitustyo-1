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
    private Bus bus;
    public override void Begin()
    {
        Image roadTexture = LoadImage("road");
        Image leftSign = LoadImage("left");
        Image rightSign = LoadImage("right");
        
        road = new Road();
        bus = new Bus(Screen);
        Add(bus.GetObject());
        Add(bus.GetSpeedo());
        road.LoadBus(bus);
        
        Keyboard.Listen(Key.W, ButtonState.Down, bus.Forward, "Drives forward");
        Keyboard.Listen(Key.A, ButtonState.Down, bus.Left, "Steer left");
        Keyboard.Listen(Key.S, ButtonState.Down, bus.Brake, "Brake");
        Keyboard.Listen(Key.D, ButtonState.Down, bus.Right, "Steer right");
        Keyboard.Listen(Key.D, ButtonState.Released, bus.SteeringRelease, "Steer right");
        Keyboard.Listen(Key.A, ButtonState.Released, bus.SteeringRelease, "Steer right");
        Keyboard.Listen(Key.Space, ButtonState.Down, bus.Handbrake, "Hand brake");
        Keyboard.Listen(Key.Space, ButtonState.Released, bus.HandbrakeRelease, "Hand brake");
        
        Camera.ZoomFactor = 0.3;
        Camera.FollowedObject = bus.GetObject();
        Task.Run(() => road.GenerateRoad(this, roadTexture, leftSign, rightSign));
    }

    protected override void Update(Time time)
    {
        new Task(() => road.PhysicsUpdate(Camera, Screen)).Start();
        base.Update(Time);
        bus.GameLoop();
        
    }
}