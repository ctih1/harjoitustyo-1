using FarseerPhysics.Collision;
using Jypeli;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text.Json;

namespace KoivurantaSimulaattori;

public class Road
{
    private static bool debug = false;
    private static readonly int SIZE = 400;
    public readonly List<GameObject> segments = new List<GameObject>();
    public readonly List<GameObject> stops = new List<GameObject>();
    public readonly List<GameObject> stopZones = new List<GameObject>();
    public readonly List<GameObject> stopZoneZones = new List<GameObject>();
    public readonly List<GameObject> turnSigns = new List<GameObject>();
    public readonly Dictionary<int, List<GameObject>> people = new Dictionary<int, List<GameObject>>();
    public readonly HashSet<int> visitedStops = new HashSet<int>();

    private KoivurantaSimulaattori gameInstance;

    public Bus bus;
    public PhysicsObject busObject;
    private static readonly int TERRAIN_PIECES = 1000;
    private bool loaded;
    private static readonly int TURN_PIECES = 20;
    private static readonly int BUS_STOPS = 5;
    private static readonly Logger logger = new Logger("Road.cs");
    private UI gameUi = UI.GetInstance();
    private long stopEnterTime = 0;
    public GameObject finalStop;
    public GameObject finalStopZone;

    private int UpdateTick = 0;

    public void GenerateAll(KoivurantaSimulaattori game, Image roadSegmentTexture, Image leftTurn, Image rightTurn, Image stopArea, Image stopSign, Image person)
    {
        this.gameInstance = game;
        logger.Info("Creating everything");
        GenerateRoad(gameInstance, roadSegmentTexture, leftTurn, rightTurn, stopSign, stopArea);
        GenerateStops(gameInstance, stopSign, stopArea, person);
    }

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

    public void GenerateRoad(PhysicsGame gameInstance, Image roadSegmentTexture, Image leftTurn, Image rightTurn, Image stopSign, Image stopZoneImage)
    {
        logger.Info("Creating " + TERRAIN_PIECES + " road pieces");
        int currentRotation = 0;
        int currentX = 0;
        int currentY = -SIZE ;
        int stepsFromLastTurn = 0;
        int stepsUntilTurn = -1;
        int minSteps = -1;
        int turnAngle = 0;

        for (int i=0; i<TERRAIN_PIECES; i++)
        {
            stepsFromLastTurn++;
            if (stepsFromLastTurn > minSteps && stepsUntilTurn != 0 && RandomNumberGenerator.GetInt32(0, 2) == 1)
            {
                GameObject sign = new GameObject(256, 256);
                sign.X = currentX + 256;
                sign.Y = currentY + 256;
                minSteps = RandomNumberGenerator.GetInt32(15, 45);

                if (i % 2 == 0)
                {
                    turnAngle = CapAngle(turnAngle - 90);
                }
                else
                {
                    turnAngle = CapAngle(turnAngle + 90);
                }

                if(turnAngle == 90)
                {
                    sign.Image = rightTurn;
                } else
                {
                    sign.Image = leftTurn; 
                }

                stepsFromLastTurn = 0;
                stepsUntilTurn = 8;

                turnSigns.Add(sign);
                gameInstance.Add(sign);
            }

            stepsUntilTurn--;
            if (stepsUntilTurn == 0)
            {
                logger.Debug("Creating a turn");

                for (int x=0; x<TURN_PIECES; x++)
                {
                    int j = x;

                    if (currentRotation == 0 || currentRotation == 180)
                    {
                        j = TURN_PIECES - x;
                    }

                    double angle = j * (90 / (double)TURN_PIECES);
                    double rad = angle * Math.PI / 180.0;
                    
                    double a = SIZE * Math.Cos(rad);
                    double b = SIZE * Math.Sin(rad);
                    
                    GameObject rotationPiece = new GameObject(SIZE, SIZE);
                    rotationPiece.Angle = Angle.FromDegrees(angle+90);
                    rotationPiece.X =  segments.Last().X + a;
                    rotationPiece.Y = segments.Last().Y + b;
                    if (debug)
                    {
                        rotationPiece.Color = Color.Lighter(Color.Black, (int)(j / (double)TURN_PIECES * 255));
                    }
                    else
                    {
                        rotationPiece.Image = roadSegmentTexture;
                    }
                    
                    segments.Add(rotationPiece);

                    if (debug)
                    {
                        Label dbgLabel = new Label(angle + " #" + j + " / "  + 90);
                        dbgLabel.X = rotationPiece.X;
                        dbgLabel.Y = rotationPiece.Y;
                        dbgLabel.TextColor = Color.Red;
                        dbgLabel.Size = new Vector(200, 200);
                        dbgLabel.TextScale = new Vector(4, 4);
                    
                        gameInstance.Add(dbgLabel, 2);  
                    }

                    gameInstance.Add(rotationPiece, -2);
                    
                    currentY += (int)b;
                    currentX += (int)a;
                }


                currentRotation = turnAngle;
            }

            GameObject roadSegment = new GameObject(SIZE, SIZE);
            roadSegment.Image = roadSegmentTexture;
            
            switch (currentRotation)
            {
                case 90:
                    currentX += SIZE;
                    break;
                               
                case 0:
                    currentY += SIZE;
                    break;

            }

            roadSegment.X = currentX;
            roadSegment.Y = currentY;

            roadSegment.Angle = Angle.FromDegrees(currentRotation);

            if (debug)
            {
                Label roadDbgLabel = new Label(currentRotation + " #" + i);
                roadDbgLabel.X = roadSegment.X;
                roadDbgLabel.Y = roadSegment.Y;
                roadDbgLabel.TextColor = Color.Red;
                roadDbgLabel.Size = new Vector(50, 50);
                roadDbgLabel.TextScale = new Vector(4, 4);
                gameInstance.Add(roadDbgLabel, -2);
            }
            
            roadSegment.Color = Color.Black;
            segments.Add(roadSegment);
            gameInstance.Add(roadSegment, -3);
        }

        Angle finalStopAngle = Angle.FromDegrees(currentRotation+270);
        (finalStop, finalStopZone) = GenerateStop(gameInstance,
            new HashSet<int>(),
            new Vector(currentX + finalStopAngle.Cos * 256, currentY - finalStopAngle.Sin * 256),
            Angle.FromDegrees(currentRotation == 90 ? finalStopAngle.Degrees : finalStopAngle.Degrees - 180),
            -1,
            stopSign,
            stopZoneImage
        );

        loaded = true;
    }

    private (GameObject, GameObject) GenerateStop(
        PhysicsGame instance,
        HashSet<int> usedRoads,
        Vector position,
        Angle angle, 
        int tag,
        Image busStopSignImage,
        Image busStopZoneImage
    ) {
        (int w, int h) = (256, 256);
        GameObject stop = new GameObject(w, h);
        stop.Shape = Shape.Rectangle;
        stop.Image = busStopSignImage;

        GameObject stopZone = new GameObject(w * 2, h * 2);
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

    public void GenerateStops(PhysicsGame instance, Image sign, Image zone, Image personImage)
    {
        logger.Debug("Generating bus stop");
        HashSet<int> usedRoads = new HashSet<int>();

        for (int i = 0; i < BUS_STOPS; i++)
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

            int offset = 256;

            int offsetAngle = 0;

            if (road.Angle.Equals(Angle.FromDegrees(0)))
            {
                position = new Vector(road.Position.X + SIZE / 2.0 * Math.Cos(road.Angle.Radians) + offset,
                    road.Position.Y + SIZE / 2.0 * Math.Sin(road.Angle.Radians));
            }
            else
            {
                offset -= 64;
                position = new Vector(road.Position.X + offset - SIZE / 2.0 * Math.Cos(road.Angle.Radians),
                    road.Position.Y - offset - SIZE / 2.0 * Math.Sin(road.Angle.Radians));
                offsetAngle = 180;
            }
            Angle angle = road.Angle + Angle.FromDegrees(offsetAngle);
            (GameObject stop, GameObject stopZone) = GenerateStop(instance, usedRoads, position, angle, i, sign, zone);
            for (int j = 0; j < RandomGen.NextInt(8) + 1; j++)
            {
                GameObject person = new GameObject(256, 256);
                person.Image = personImage;
                person.Position = new Vector(stop.X - RandomGen.NextInt(100), stop.Y - RandomGen.NextInt(100));
                peopleList.Add(person);

                instance.Add(person);
            }
            people.Add(i, peopleList);
        }
        
    }

    public bool Overlapping(double right, double left, double top, double down, GameObject comparison)
    {
        return right > comparison.Left && left < comparison.Right && top > comparison.Bottom && down < comparison.Top;
    }

    public (GameObject, double) GetNextStopDistance()
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

    public void PhysicsUpdate(Camera Camera, ScreenView Screen)
    {
        if (!loaded || bus == null) return;
        Stopwatch sw = Stopwatch.StartNew();
        bool isOnRoad = false;
        bool onStop = false;
        
        double busLeft = busObject.Left;
        double busRight = busObject.Right;
        double busTop = busObject.Top;
        double busDown = busObject.Bottom;
        
        foreach (GameObject piece in segments)
        {
            if (Overlapping(busRight, busLeft, busTop, busDown, piece))
            {
                isOnRoad = true;
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


        if (UpdateTick % 10 == 0)
        {
            (GameObject nearestStop, double distance) = GetNextStopDistance();
            gameUi.UpdateDistance(distance);

            if (UpdateTick/10 == RandomGen.NextInt(30) && distance > 50 && bus.passengerCount >= 1)
            {
                bus.stopping = true;
                gameUi.ShowStop();
            }
        }


        if(onStop && (int)overlappingStop.Tag == -1)
        {
            gameInstance.ShowHighscoreList();
        }

        if (isOnRoad || onStop)
        {
            bus.SlowdownMultiplier = 1;
            busObject.Color = Color.Yellow;
        } else {
            bus.SlowdownMultiplier = 3;
            busObject.Color = Color.Red;
        }

        if(!isOnRoad)
        {
            bus.IncreaseAnger(0.001);
        } else
        {
            bus.DecreaseAnger(0.0001);
        }
        if (isOnRoad && !onStop)
        {
            gameUi.UpdateBackDoorRequirement(bus.backdoorOpen);
            if (bus.backdoorOpen)
            {
                bus.IncreaseAnger(0.0007);
            }
            else
            {
                bus.DecreaseAnger(0.0014);
            }
        }

        int stopNumber = overlappingStop != null ? (int)overlappingStop.Tag : 0;
        if (onStop)
        {
            if(stopEnterTime == 0)
            {
                stopEnterTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            else
            {
                long timeOnStop = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - stopEnterTime;
                gameUi.UpdateCountdown(timeOnStop);
                 
                if(timeOnStop >= 2 && bus.stopping)
                {
                    if(!bus.backdoorOpen) {
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
                        bus.passengerCount = Math.Max(1, bus.passengerCount-RandomGen.NextInt(3));
                        gameUi.UpdatePassangerCount(bus.passengerCount);
                        bus.waitingAnger = 0;
                        gameUi.UpdateHoldingBackRequirement(false);
                        bus.ChangeScore(1200);

                    }
                }

                if (timeOnStop >= 4 && !visitedStops.Contains(stopNumber) && stopNumber != -1)
                {
                    foreach(GameObject person in people[stopNumber])
                    {
                        person.Destroy();
                    }

                    visitedStops.Add(stopNumber);
                    bus.passengerCount += 3;
                    gameUi.UpdatePassangerCount(bus.passengerCount);
                    bus.ChangeScore(1200 * (2 + RandomGen.NextInt(3)));
                }

            }
        }
        else if(stopEnterTime != 0)
        {
            stopEnterTime = 0;
            gameUi.UpdateCountdown(0);
        }

        sw.Stop();
        
        Console.WriteLine("ELAPSED ROAD " + sw.ElapsedMilliseconds + "ms");
        UpdateTick++;

        if(UpdateTick > 300)
        {
            UpdateTick = 0;
        }
    }

    public void LoadBus(Bus bus)
    {
        logger.Debug("Loading bus");
        this.bus = bus;
        this.busObject = bus.GetObject();
    }
}