using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Authentication.ExtendedProtection;
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
/// </summary>
public class KoivurantaSimulaattori : PhysicsGame
{
    private Road road;
    private Thread roadThread;
    private Bus bus;
    private Logger logger;
    private Label label;
    
    public override void Begin()
    {
        logger = new Logger("main");
        logger.Debug("AUTA MINUA");
        Image roadTexture = LoadImage("road");
        logger.Debug("Loading road");
        Image leftSign = LoadImage("left");
        logger.Debug("Loading left sign");
        
        Image rightSign = LoadImage("right");
        logger.Debug("Loading right");
        
        Image busSign = LoadImage("stopsign");
        logger.Debug("Loading stopsign");
        
        Image busZone = LoadImage("busstop");
        logger.Debug("Loading busstop");
        
        Image busStop = LoadImage("stop");
        logger.Debug("Loading stop");
        
        Image person = LoadImage("person");
        logger.Debug("Loading person");
        
        
        UI gameUi = new UI(this, busStop);
        
        busSign.Scaling = ImageScaling.Nearest;
        busZone.Scaling = ImageScaling.Nearest;
        
        label = new Label("fps");
        label.Position = new Vector(Screen.LeftSafe + 20, Screen.TopSafe - 100);
        Add(label);
        
        road = new Road();
        bus = new Bus(Screen);
        PhysicsObject busObject = bus.GetObject();
        Add(busObject);
        int stopIndex = 0;
        
        road.LoadBus(bus);
        Keyboard.Listen(Key.W, ButtonState.Down, bus.Forward, "Drives forward");
        Keyboard.Listen(Key.A, ButtonState.Down, bus.Left, "Steer left");
        Keyboard.Listen(Key.S, ButtonState.Down, bus.Brake, "Brake");
        Keyboard.Listen(Key.D, ButtonState.Down, bus.Right, "Steer right");
        Keyboard.Listen(Key.D, ButtonState.Released, bus.SteeringRelease, "Steer right");
        Keyboard.Listen(Key.A, ButtonState.Released, bus.SteeringRelease, "Steer right");
        Keyboard.Listen(Key.Space, ButtonState.Down, bus.Handbrake, "Hand brake");
        Keyboard.Listen(Key.Space, ButtonState.Released, bus.HandbrakeRelease, "Hand brake");
        Keyboard.Listen(Key.J, ButtonState.Pressed, gameUi.ShowStop, "ÄH");
        Keyboard.Listen(Key.K, ButtonState.Pressed, gameUi.HideStop, "ÄH");
        Keyboard.Listen(Key.Enter, ButtonState.Pressed,
            () => { bus.GetObject().Position = road.stops[stopIndex].Position;
                stopIndex++;    
            }, "");
        ControllerOne.ListenAnalog(AnalogControl.LeftStick, 1.0/1000000000000000, bus.StickMove, "Steer");
        ControllerOne.ListenAnalog(AnalogControl.RightTrigger, 0.00001, bus.TriggerAccel, "Gas!");
        ControllerOne.Listen(Button.RightShoulder, ButtonState.Down, bus.Handbrake, "Hand brake");
        ControllerOne.Listen(Button.RightShoulder, ButtonState.Up, bus.HandbrakeRelease, "Release");
        Camera.ZoomFactor = 0.3;
        Camera.FollowedObject = busObject;
        road.GenerateAll(this, roadTexture, leftSign, rightSign, busZone, busSign, person);
    }

    protected override void Update(Time time)
    {
        Stopwatch sw = Stopwatch.StartNew();
        road.PhysicsUpdate(Camera, Screen);
        base.Update(Time);
        bus.GameLoop();
        sw.Stop();
        Console.WriteLine(("frametime " + sw.ElapsedMilliseconds));
        double fps = Math.Round(1 / (sw.Elapsed.TotalMilliseconds / 1000));
        label.Text = fps + "fps";
    }
}