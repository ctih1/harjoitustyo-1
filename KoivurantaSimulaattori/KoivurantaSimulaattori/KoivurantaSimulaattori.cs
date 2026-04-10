using System;
using System.Collections.Generic;
using System.Diagnostics;
using Jypeli;
using Jypeli.Widgets;

namespace KoivurantaSimulaattori;

/// @author gr313123
/// @version 12.11.2025
/// <summary>
/// Peliluokka ja yleinen haltija muuttujille
/// </summary>
public class KoivurantaSimulaattori : PhysicsGame
{
    private Road road;
    private Bus bus;
    private Logger logger;
    private Label label;
    private UI gameUi;
    private ScoreList scoreList;
    private bool highScoreOpen;

    private Image roadTexture;
    private Image leftSign;
    private Image rightSign;
    private Image busSign;
    private Image busZone;
    private Image busStop;
    private Image person;

    /// <summary>
    /// Asettaa pelin arvot ja lataa resurssit käynnistyessä
    /// </summary>
    public override void Begin()
    {
        logger = new Logger("main");
        roadTexture = VerboseImageLoad("road");
        leftSign = VerboseImageLoad("left");
        rightSign = VerboseImageLoad("right");
        busSign = VerboseImageLoad("stopsign");
        busZone = VerboseImageLoad("busstop");
        busStop = VerboseImageLoad("stop");
        person = VerboseImageLoad("person");
        Level.BackgroundColor = Color.SkyBlue;

        gameUi = new UI(this, busStop);

        label = new Label("fps");
        label.Position = new Vector(Screen.LeftSafe + 20, Screen.TopSafe - 100);
        Add(label);

        road = new Road();
        bus = new Bus();
        PhysicsObject busObject = bus.GetObject();
        Add(busObject);
        int stopIndex = 0;

        road.LoadBus(bus);
        Keyboard.Listen(Key.W, ButtonState.Down, bus.Move, "Drives forward", new Vector(0, 1));
        Keyboard.Listen(Key.A, ButtonState.Down, bus.Move, "Steer left", new Vector(-1, 0));
        Keyboard.Listen(Key.S, ButtonState.Down, bus.Brake, "Brake");
        Keyboard.Listen(Key.D, ButtonState.Down, bus.Move, "Steer right", new Vector(1, 0));
        Keyboard.Listen(Key.D, ButtonState.Released, bus.SteeringRelease, "Steer right");
        Keyboard.Listen(Key.A, ButtonState.Released, bus.SteeringRelease, "Steer right");
        Keyboard.Listen(Key.F, ButtonState.Released, bus.ToggleBackdoor, "Lets people leave the bus");
        Keyboard.Listen(Key.Space, ButtonState.Down, bus.Handbrake, "Hand brake");
        Keyboard.Listen(Key.Space, ButtonState.Released, bus.HandbrakeRelease, "Hand brake");
        Keyboard.Listen(Key.R, ButtonState.Released, ResetGame, "Resets Game's state");
        Keyboard.Listen(Key.Enter, ButtonState.Pressed,
            () =>
            {
                bus.GetObject().Position = road.stops[stopIndex].Position;
                stopIndex++;
            }, "");
        Keyboard.Listen(Key.Down, ButtonState.Released, bus.DecreaseTemperature, "Decrease bus temp");
        Keyboard.Listen(Key.Up, ButtonState.Released, bus.IncreaseTemperature, "Increase bus temp");

        ControllerOne.ListenAnalog(AnalogControl.LeftStick, 1.0 / 1000000000000000, bus.StickMove, "Steer");
        ControllerOne.ListenAnalog(AnalogControl.RightTrigger, 0.00001, bus.TriggerAccel, "Gas!");
        ControllerOne.Listen(Button.RightShoulder, ButtonState.Down, bus.Handbrake, "Hand brake");
        ControllerOne.Listen(Button.RightShoulder, ButtonState.Up, bus.HandbrakeRelease, "Release");
        Camera.ZoomFactor = 0.3;
        Camera.FollowedObject = busObject;
        road.GenerateAll(this, roadTexture, leftSign, rightSign, busZone, busSign, person);

        CreatehighsSoreList();
    }

    private void DestroyList(List<GameObject> objs)
    {
        foreach (GameObject obj in objs)
        {
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
        DestroyList(road.peopleObjects);

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

    private void CreatehighsSoreList()
    {
        scoreList = DataStorage.TryLoad(scoreList, "scoreList.xml");
        scoreList = scoreList ?? new ScoreList(10, true, 0);
    }

    /// <summary>
    /// Näyttää highscore listan, kysyy nimeä, ja tallentaa pisteet
    /// </summary>
    public void ShowHighscoreList()
    {
        if (highScoreOpen) return;
        int score = (int)bus.score;
        HighScoreWindow window = new HighScoreWindow(
            "High scores", $"You achieved a score of {score}",
            scoreList, score
        );

        window.Closed += _ => { SaveScores(); };
        Add(window);
        highScoreOpen = true;
    }

    
    private void SaveScores()
    {
        DataStorage.Save(scoreList, "scoreList.xml");
    }

    
    /// <summary>
    /// Päivittää pelin sekä tien ja bussin fysiikat
    /// </summary>
    /// <param name="time">Tämän hetkinen aika</param>
    protected override void Update(Time time)
    {
        Stopwatch sw = Stopwatch.StartNew();
        road.PhysicsUpdate();
        base.Update(Time);
        bus.GameLoop();
        sw.Stop();
        double fps = Math.Round(1 / (sw.Elapsed.TotalMilliseconds / 1000));
        label.Text = fps + "fps";
        gameUi.PositionLine(label, 4);
    }
}