using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using Jypeli;

namespace KoivurantaSimulaattori;

public class Road
{
    private static bool debug = false;
    private static readonly int SIZE = 400;
    public readonly List<GameObject> segments = new List<GameObject>();
    public readonly List<GameObject> stops = new List<GameObject>();
    public readonly List<GameObject> stopZones = new List<GameObject>();
    public Bus bus;
    public PhysicsObject busObject;
    private static readonly int TERRAIN_PIECES = 4000;
    private bool loaded;
    private static readonly int TURN_PIECES = 15;
    private static readonly int BUS_STOPS = 20;
    private static readonly Logger logger = new Logger("road.cs");

    public void GenerateAll(PhysicsGame gameInstance, Image roadSegmentTexture, Image leftTurn, Image rightTurn, Image stopArea, Image stopSign)
    {
        logger.Info("Creating everything");
        GenerateRoad(gameInstance, roadSegmentTexture, leftTurn, rightTurn);
        GenerateStops(gameInstance, stopSign, stopArea);
    }
    public void GenerateRoad(PhysicsGame gameInstance, Image roadSegmentTexture, Image leftTurn, Image rightTurn)
    {
        logger.Info("Creating " + TERRAIN_PIECES + " road pieces");
        int currentRotation = 0;
        int currentX = 0;
        int currentY = -SIZE ;
        int stepsFromLastTurn = 0;
        int stepsUntilTurn = -1;
        int minSteps = -1;
        string turningDirection = "";

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
                    turningDirection = "left";
                    sign.Image = leftTurn;
                }
                else
                {
                    turningDirection = "right";
                    sign.Image = rightTurn;
                }
                
                stepsFromLastTurn = 0;
                stepsUntilTurn = 8;

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
                        Label dbgLabel = new Label(angle + " #" + j + " / "  + 90 + turningDirection);
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

                if (turningDirection == "left") {
                    currentRotation -= 90;
                } else {
                    currentRotation += 90;
                }

                if (currentRotation > 360)
                {
                    currentRotation = 360 - currentRotation;
                } else if (currentRotation < 0)
                {
                    currentRotation = 360 + currentRotation;
                }

                if (currentRotation == 180)
                {
                    currentRotation = 0;
                }
                if (currentRotation == 270)
                {
                    currentRotation = 90;
                }

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

        loaded = true;
    }

    public void GenerateStops(PhysicsGame instance, Image sign, Image zone)
    {
        logger.Debug("Generating bus stop");

        HashSet<int> usedRoads = new HashSet<int>();
        for (int i = 0; i < BUS_STOPS; i++)
        {
            (int w, int h) = (256, 256);
            GameObject stop = new GameObject(w,h);
            stop.Shape = Shape.Rectangle;
            stop.Image = sign;

            GameObject stopZone = new GameObject(w * 2, h * 2);
            stopZone.Shape = Shape.Rectangle;
            stopZone.Image = zone;
            stopZone.Tag = i;
            
            int targetRoad = RandomNumberGenerator.GetInt32(0, segments.Count);
            while (usedRoads.Contains(targetRoad))
            {
                targetRoad =  RandomNumberGenerator.GetInt32(0, segments.Count);
            }

            GameObject road = segments[targetRoad];
            Vector position;

            int offset = 256;

            int offsetAngle = 0;

            if (road.Angle.Equals(Angle.FromDegrees(0)))
            {
                position = new Vector(road.Position.X + SIZE / 2.0 * Math.Cos(road.Angle.Radians) + offset ,
                    road.Position.Y + SIZE / 2.0 * Math.Sin(road.Angle.Radians));
            }
            else
            {
                offset -= 64;
                position = new Vector(road.Position.X + offset - SIZE/2.0  * Math.Cos(road.Angle.Radians),
                    road.Position.Y - offset - SIZE/2.0  * Math.Sin(road.Angle.Radians));
                offsetAngle = 180;
            }

            usedRoads.Add(targetRoad);
            
            stop.Position = position;
            
            stopZone.Position = stop.Position;
            stopZone.Angle = road.Angle + Angle.FromDegrees(offsetAngle);
            
            stopZones.Add(stopZone);
            stops.Add(stop);
            instance.Add(stopZone, -1);
            instance.Add(stop, 1);
        }
    }

    public bool Overlapping(double right, double left, double top, double down, GameObject comparison)
    {
        return right > comparison.Left && left < comparison.Right && top > comparison.Bottom && down < comparison.Top;
    }

    public void PhysicsUpdate(Camera Camera, ScreenView Screen)
    {
        Stopwatch sw = Stopwatch.StartNew();
        if (!loaded) return;
        bool isOnRoad = false;
        bool onStop = false;
        
        if(bus == null) return;

        double busLeft = busObject.Left;
        double busRight = busObject.Right;
        double busTop = busObject.Top;
        double busDown = busObject.Bottom;
        
        foreach (GameObject piece in segments)
        {
            if (Overlapping(busRight, busLeft, busTop, busDown, piece))
            {
                isOnRoad = true;
                Console.WriteLine("road ");
                break;
            }
        }

        foreach (GameObject stopZone in stopZones)
        {
            if (Overlapping(busRight, busLeft, busTop, busDown, stopZone))
            {
                onStop = true;
                busObject.Color = Color.Green;
                Console.WriteLine("STOPP!");
            }
        }

        if (isOnRoad)
        {
            bus.SlowdownMultiplier = 1;
            busObject.Color = Color.Yellow;
        } else {
            bus.SlowdownMultiplier = 3;
            busObject.Color = Color.Red;
        }
        
        sw.Stop();
        
        Console.WriteLine("ELAPSED ROAD " + sw.ElapsedMilliseconds + "ms");
    }

    public void LoadBus(Bus bus)
    {
        logger.Debug("Loading bus");
        this.bus = bus;
        this.busObject = bus.GetObject();
    }
}