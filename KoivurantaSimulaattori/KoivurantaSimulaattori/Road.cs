using Jypeli;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

namespace KoivurantaSimulaattori;

/// @author gr313123
/// @version 12.11.2025
/// <summary>
/// Pelin tie- ja pysäkkigenerointi. Hallitsee myös tiettyjä UI-osia
/// </summary>
public class Road
{
    private const int RoadSize = 400;
    private const int TurnPieces = 20;
    private const int BusStops = 5;
    private const int TerrainPieces = 1000;
    private const int SignSize = 256;
    private const int StopSize = 256;
    private const int GenericScoreChange = 1200;
    private const int UpdateTickInterval = 10;
    private const int TimeToPickUp = 4;
    private const int TimeToDropOff = 2;
    
    private const double BackdoorRageIncrease = 0.0007;

    /// <summary>
    /// Lista tien pätkistä
    /// </summary>
    public readonly List<GameObject> segments = new();
    
    /// <summary>
    /// Lista pysäkkien merkeistä
    /// </summary>
    public readonly List<GameObject> stops = new();
    
        
    /// <summary>
    /// Lista pysäkkien alueista (syvennyksistä tiessä)
    /// </summary>
    public readonly List<GameObject> stopZones = new();
    
    /// <summary>
    /// Lista tien kääntömerkeistä
    /// </summary>
    public readonly List<GameObject> turnSigns = new();
    
    /// <summary>
    /// Dictionary jossa avain on pysäkin tag, ja arvo on lista ihmisiä pysäkillä
    /// </summary>
    public readonly Dictionary<int, List<GameObject>> people = new();
    
    /// <summary>
    /// Lista ihmisisten objekteista (käytetään pelin uudelleenaloittamiseen)
    /// </summary>
    public readonly List<GameObject> peopleObjects = new();
    
    /// <summary>
    /// HashSet pysäkkien tageista, joissa on jo vierailtu
    /// </summary>
    public readonly HashSet<int> visitedStops = new();

    private KoivurantaSimulaattori gameInstance;

    private Bus bus;
    private PhysicsObject busObject;
    private bool loaded;

    private static readonly Logger logger = new Logger("Road.cs");
    private readonly UI gameUi = UI.GetInstance();
    private long stopEnterTime;
    
    /// <summary>
    /// Viimeisen päättöpysäkin merkki
    /// </summary>
    public GameObject finalStop;
    
    /// <summary>
    /// Viimeisen päättöpysäkin syvennys
    /// </summary>
    public GameObject finalStopZone;

    private int UpdateTick;

    /// <summary>
    /// Generoi tien ja pysäkit
    /// </summary>
    public void GenerateAll(KoivurantaSimulaattori game, Image roadSegmentTexture, Image leftTurn, Image rightTurn,
        Image stopArea, Image stopSign, Image person)
    {
        gameInstance = game;
        logger.Info("Creating everything");
        GenerateRoad(roadSegmentTexture, leftTurn, rightTurn, stopSign, stopArea);
        GenerateStops(stopSign, stopArea, person);
    }

    
    /// <summary>
    /// Rajoittaa arvon 360 asteeseen
    /// </summary>
    private int CapAngle(int angle)
    {
        if (angle > 360)
        {
            angle = 360 - angle;
        }
        else if (angle < 0)
        {
            angle = 360 + angle;
        }

        if (angle == 180)
        {
            angle = 0;
        }

        if (angle == 270)
        {
            angle = 90;
        }

        return angle;
    }

    
    /// <summary>
    /// Luo tien ja kyltit
    /// </summary>
    /// <param name="roadSegmentTexture">Tien tekstuuri</param>
    /// <param name="leftTurn">Kyltti joka osoittaa käännöstä vasemmalle</param>
    /// <param name="rightTurn">Kyltti joka osoittaa käännöstä oikealle</param>
    /// <param name="stopSign">Bussin pysäkkimerkki</param>
    /// <param name="stopZoneImage">Tien sisännys</param>
    private void GenerateRoad(Image roadSegmentTexture, Image leftTurn, Image rightTurn, Image stopSign,
        Image stopZoneImage)
    {
        logger.Info("Creating " + TerrainPieces + " road pieces");
        int currentRotation = 0;
        int currentX = 0;
        int currentY = -RoadSize;
        int stepsFromLastTurn = 0;
        int stepsUntilTurn = -1;
        int minSteps = -1;
        int turnAngle = 0;

        for (int i = 0; i < TerrainPieces; i++)
        {
            stepsFromLastTurn++;
            if (stepsFromLastTurn > minSteps && stepsUntilTurn != 0 && RandomNumberGenerator.GetInt32(0, 2) == 1)
            {
                GameObject sign = new GameObject(SignSize, SignSize);
                sign.X = currentX + SignSize;
                sign.Y = currentY + SignSize;
                minSteps = RandomNumberGenerator.GetInt32(15, 45);

                turnAngle = i % 2 == 0 ? CapAngle(turnAngle - 90) : CapAngle(turnAngle + 90);

                sign.Image = turnAngle == 90 ? rightTurn : leftTurn;

                stepsFromLastTurn = 0;
                stepsUntilTurn = 8;

                turnSigns.Add(sign);
                gameInstance.Add(sign);
            }

            stepsUntilTurn--;
            if (stepsUntilTurn == 0)
            {
                logger.Debug("Creating a turn");

                for (int x = 0; x < TurnPieces; x++)
                {
                    int j = x;

                    if (currentRotation == 0 || currentRotation == 180)
                    {
                        j = TurnPieces - x;
                    }

                    double angle = j * (90 / (double)TurnPieces);
                    double rad = angle * Math.PI / 180.0;

                    double a = RoadSize * Math.Cos(rad);
                    double b = RoadSize * Math.Sin(rad);

                    GameObject rotationPiece = new GameObject(RoadSize, RoadSize);
                    rotationPiece.Angle = Angle.FromDegrees(angle + 90);
                    rotationPiece.X = segments.Last().X + a;
                    rotationPiece.Y = segments.Last().Y + b;

                    rotationPiece.Image = roadSegmentTexture;
                    
                    segments.Add(rotationPiece);
                    gameInstance.Add(rotationPiece, -2);

                    currentY += (int)b;
                    currentX += (int)a;
                }
                
                currentRotation = turnAngle;
            }

            GameObject roadSegment = new GameObject(RoadSize, RoadSize);
            roadSegment.Image = roadSegmentTexture;

            switch (currentRotation)
            {
                case 90:
                    currentX += RoadSize;
                    break;

                case 0:
                    currentY += RoadSize;
                    break;
            }

            roadSegment.X = currentX;
            roadSegment.Y = currentY;

            roadSegment.Angle = Angle.FromDegrees(currentRotation);

            roadSegment.Color = Color.Black;
            segments.Add(roadSegment);
            gameInstance.Add(roadSegment, -3);
        }

        Angle finalStopAngle = Angle.FromDegrees(currentRotation + 270);
        (finalStop, finalStopZone) = GenerateStop(gameInstance,
            new Vector(currentX + finalStopAngle.Cos * 256, currentY - finalStopAngle.Sin * 256),
            Angle.FromDegrees(currentRotation == 90 ? finalStopAngle.Degrees : finalStopAngle.Degrees - 180),
            -1,
            stopSign,
            stopZoneImage
        );

        loaded = true;
    }

    
    /// <summary>
    /// Luo pysäkin tietylle kohdalle.
    /// </summary>
    private (GameObject, GameObject) GenerateStop(
        PhysicsGame instance,
        Vector position,
        Angle angle,
        int tag,
        Image busStopSignImage,
        Image busStopZoneImage
    )
    {
        GameObject stop = new GameObject(StopSize, StopSize);
        stop.Shape = Shape.Rectangle;
        stop.Image = busStopSignImage;

        GameObject stopZone = new GameObject(StopSize * 2, StopSize * 2);
        stopZone.Shape = Shape.Rectangle;
        stopZone.Image = busStopZoneImage;
        stopZone.Tag = tag;

        stop.Position = position;

        stopZone.Position = stop.Position;
        stopZone.Angle = angle;
        stopZone.Tag = tag;

        stopZones.Add(stopZone);
        stops.Add(stop);
        instance.Add(stopZone, -1);
        instance.Add(stop, 1);

        return (stop, stopZone);
    }

    
    /// <summary>
    /// Luo pysäkit
    /// </summary>
    private void GenerateStops(Image sign, Image zone, Image personImage)
    {
        logger.Debug("Generating bus stop");
        HashSet<int> usedRoads = new HashSet<int>();

        for (int i = 0; i < BusStops; i++)
        {
            List<GameObject> peopleList = new List<GameObject>();

            int targetRoad = RandomNumberGenerator.GetInt32(0, segments.Count);
            while (usedRoads.Contains(targetRoad))
            {
                targetRoad = RandomNumberGenerator.GetInt32(0, segments.Count);
            }

            usedRoads.Add(targetRoad);

            GameObject road = segments[targetRoad];
            Vector position;

            int offset = StopSize;
            int offsetAngle = 0;

            if (road.Angle.Equals(Angle.FromDegrees(0)))
            {
                position = new Vector(road.Position.X + RoadSize / 2.0 * Math.Cos(road.Angle.Radians) + offset,
                    road.Position.Y + RoadSize / 2.0 * Math.Sin(road.Angle.Radians));
            }
            else
            {
                offset -= StopSize / 4;
                position = new Vector(road.Position.X + offset - RoadSize / 2.0 * Math.Cos(road.Angle.Radians),
                    road.Position.Y - offset - RoadSize / 2.0 * Math.Sin(road.Angle.Radians));
                offsetAngle = 180;
            }

            Angle angle = road.Angle + Angle.FromDegrees(offsetAngle);
            (GameObject stop, GameObject _) = GenerateStop(gameInstance, position, angle, i, sign, zone);
            for (int j = 0; j < RandomGen.NextInt(8) + 1; j++)
            {
                GameObject person = new GameObject(256, 256);
                person.Image = personImage;
                person.Position = new Vector(stop.X - RandomGen.NextInt(100), stop.Y - RandomGen.NextInt(100));
                peopleList.Add(person);
                peopleObjects.Add(person);
                gameInstance.Add(person);
            }

            people.Add(i, peopleList);
        }
    }

    
    /// <summary>
    /// Tarkistaa ovatko kaksi kappaletta päällekkäin
    /// </summary>
    private bool Overlapping(double right, double left, double top, double down, GameObject comparison)
    {
        return right > comparison.Left && left < comparison.Right && top > comparison.Bottom && down < comparison.Top;
    }

    
    /// <summary>
    /// Palauttaa lähimmän pysäkin objektin sekä sen etäisyydenm, jos pysäkillä ei ole käyty
    /// </summary>
    private (GameObject, double) GetNextStopDistance()
    {
        GameObject nearest = stopZones[0];
        double distToNearest = busObject.Position.Distance(nearest.Position);
        
        foreach (GameObject stopZone in stopZones)
        {
            if (visitedStops.Contains((int)stopZone.Tag)) continue;

            double busDist = busObject.Position.Distance(stopZone.Position);
            if (busDist < distToNearest)
            {
                distToNearest = busDist;
                nearest = stopZone;
            }
        }

        return (nearest, distToNearest);
    }

    
    /// <summary>
    /// Päivittää pelin UI:n, sekä tarkistaa kollisioita bussin, pysäkkien ja teiden kanssa.
    /// </summary>
    public void PhysicsUpdate()
    {
        if (!loaded || bus == null) return;
        Stopwatch sw = Stopwatch.StartNew();
        bool onRoad = false;
        bool onStop = false;

        double busLeft = busObject.Left;
        double busRight = busObject.Right;
        double busTop = busObject.Top;
        double busDown = busObject.Bottom;

        foreach (GameObject piece in segments)
        {
            if (Overlapping(busRight, busLeft, busTop, busDown, piece))
            {
                onRoad = true;
                break;
            }
        }

        GameObject overlappingStop = null;
        foreach (GameObject stopZone in stopZones)
        {
            if (Overlapping(busRight, busLeft, busTop, busDown, stopZone))
            {
                onStop = true;
                overlappingStop = stopZone;

                break;
            }
        }


        if (UpdateTick % UpdateTickInterval == 0)
        {
            (GameObject _, double distance) = GetNextStopDistance();
            gameUi.UpdateDistance(distance);

            if (UpdateTick / UpdateTickInterval == RandomGen.NextInt(30) && distance > 50 && bus.passengerCount >= 1)
            {
                bus.stopping = true;
                gameUi.ShowStop();
            }
        }


        if (onStop && (int)overlappingStop.Tag == -1)
        {
            gameInstance.ShowHighscoreList();
        }

        if (onRoad || onStop)
        {
            bus.slowdownMultiplier = 1;
            busObject.Color = Color.Yellow;
        }
        else
        {
            bus.slowdownMultiplier = 3;
            busObject.Color = Color.Red;
        }

        if (!onRoad)
        {
            bus.IncreaseAnger(0.001);
        }
        else
        {
            bus.DecreaseAnger(0.0001);
        }

        if (onRoad && !onStop)
        {
            gameUi.UpdateBackDoorRequirement(bus.backdoorOpen);
            if (bus.backdoorOpen)
            {
                bus.IncreaseAnger(BackdoorRageIncrease);
            }
            else
            {
                bus.DecreaseAnger(BackdoorRageIncrease*2);
            }
        }

        int stopNumber = overlappingStop != null ? (int)overlappingStop.Tag : 0;
        if (onStop)
        {
            if (stopEnterTime == 0)
            {
                stopEnterTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            else
            {
                long timeOnStop = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - stopEnterTime;
                gameUi.UpdateCountdown(timeOnStop);

                if (timeOnStop >= TimeToDropOff && bus.stopping)
                {
                    if (!bus.backdoorOpen)
                    {
                        if (!visitedStops.Contains(stopNumber))
                        {
                            bus.waitingAnger += 0.001 * (bus.passengerCount / 3.0);
                            gameUi.UpdateHoldingBackRequirement(true);
                        }
                    }
                    else
                    {
                        gameUi.HideStop();
                        bus.stopping = false;
                        bus.passengerCount = Math.Max(1, bus.passengerCount - RandomGen.NextInt(3));
                        gameUi.UpdatePassangerCount(bus.passengerCount);
                        bus.waitingAnger = 0;
                        gameUi.UpdateHoldingBackRequirement(false);
                        bus.ChangeScore(GenericScoreChange);
                    }
                }

                if (timeOnStop >= TimeToPickUp && !visitedStops.Contains(stopNumber) && stopNumber != -1)
                {
                    foreach (GameObject person in people[stopNumber])
                    {
                        person.Destroy();
                    }

                    visitedStops.Add(stopNumber);
                    bus.passengerCount += 3;
                    gameUi.UpdatePassangerCount(bus.passengerCount);
                    bus.ChangeScore(GenericScoreChange * (2 + RandomGen.NextInt(3)));
                }
            }
        }
        
        else if (stopEnterTime != 0)
        {
            stopEnterTime = 0;
            gameUi.UpdateCountdown(0);
        }

        sw.Stop();

        Console.WriteLine("ELAPSED ROAD " + sw.ElapsedMilliseconds + "ms");
        UpdateTick++;

        if (UpdateTick > 300)
        {
            UpdateTick = 0;
        }
    }

    
    /// <summary>
    /// Asettaa bussin objektin
    /// </summary>
    public void LoadBus(Bus busInstance)
    {
        logger.Debug("Loading bus");
        bus = busInstance;
        busObject = bus.GetObject();
    }
}