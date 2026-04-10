using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
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
    private UI gameUi;
    private ScoreList scoreList;
    private bool highScoreOpen = false;

    private Image roadTexture;
    private Image leftSign;
    private Image rightSign;
    private Image busSign;
    private Image busZone;
    private Image busStop;
    private Image person;

    public override void Begin()
    {
        logger = new Logger("main");
        logger.Debug("AUTA MINUA");
        roadTexture = VerboseImageLoad("road");
        leftSign = VerboseImageLoad("left");
        rightSign = VerboseImageLoad("right");
        busSign = VerboseImageLoad("stopsign");
        busZone = VerboseImageLoad("busstop");
        busStop = VerboseImageLoad("stop");
        person = VerboseImageLoad("person");
        Level.BackgroundColor = Color.Gray;

        gameUi = new UI(this, busStop);
  
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
        Keyboard.Listen(Key.F, ButtonState.Released, bus.ToggleBackdoor, "Lets people leave the bus");
        Keyboard.Listen(Key.Space, ButtonState.Down, bus.Handbrake, "Hand brake");
        Keyboard.Listen(Key.Space, ButtonState.Released, bus.HandbrakeRelease, "Hand brake");
        Keyboard.Listen(Key.R, ButtonState.Released, ResetGame, "Resets Game's state");
        Keyboard.Listen(Key.Enter, ButtonState.Pressed,
            () => { bus.GetObject().Position = road.stops[stopIndex].Position;
                stopIndex++;    
            }, "");
        Keyboard.Listen(Key.Down, ButtonState.Released, bus.DecreaseTemperature, "Decrease bus temp");
        Keyboard.Listen(Key.Up, ButtonState.Released, bus.IncreaseTemperature, "Increase bus temp");

        ControllerOne.ListenAnalog(AnalogControl.LeftStick, 1.0/1000000000000000, bus.StickMove, "Steer");
        ControllerOne.ListenAnalog(AnalogControl.RightTrigger, 0.00001, bus.TriggerAccel, "Gas!");
        ControllerOne.Listen(Button.RightShoulder, ButtonState.Down, bus.Handbrake, "Hand brake");
        ControllerOne.Listen(Button.RightShoulder, ButtonState.Up, bus.HandbrakeRelease, "Release");
        Camera.ZoomFactor = 0.3;
        Camera.FollowedObject = busObject;
        road.GenerateAll(this, roadTexture, leftSign, rightSign, busZone, busSign, person);

        CreatehighscoreList();
    }

    private void DestroyList(List<GameObject> objs)
    {
        foreach (GameObject obj in objs) { 
            if (!obj.IsDestroyed) obj.Destroy();
        }

        objs.Clear();
    }
        
    private void ResetGame()
    {
        DestroyList(road.segments);
        DestroyList(road.stops);
        DestroyList(road.stopZones);
        DestroyList(road.turnSigns);

        road.finalStop.Destroy();
        road.finalStopZone.Destroy();

        bus.Reset();
        road = new Road();
        road.LoadBus(bus);
        road.GenerateAll(this, roadTexture, leftSign, rightSign, busZone, busSign, person);
    }
        
    private Image VerboseImageLoad(string name)
    {
        logger.Debug("Loading " + name + " image...");
        Image image = LoadImage(name);
        image.Scaling = ImageScaling.Nearest;
        
        return image;
    }

    private void CreatehighscoreList()
    {
        scoreList = DataStorage.TryLoad<ScoreList>(scoreList, "scoreList.xml");
        if(scoreList == null)
        {
            scoreList = new ScoreList(10, true, 0);
        }

    }
    public void ShowHighscoreList()
    {
        if (highScoreOpen) return;
        int score = (int)bus.score;
        HighScoreWindow window = new HighScoreWindow(
            "High scores", $"You achieved a score of {score}",
            scoreList, score
        );
;
        window.Closed += (Window _) =>  { SaveScores(); };
        Add(window);
        highScoreOpen = true;
    }
    private void SaveScores()
    {
        DataStorage.Save<ScoreList>(scoreList, "scoreList.xml");
    }

    protected override void Update(Time time)
    {
        Stopwatch sw = Stopwatch.StartNew();
        road.PhysicsUpdate(Camera, Screen);
        base.Update(Time);
        bus.GameLoop();
        sw.Stop();
        double fps = Math.Round(1 / (sw.Elapsed.TotalMilliseconds / 1000));
        label.Text = fps + "fps";
        gameUi.PositionLine(label, 4);
    }
}