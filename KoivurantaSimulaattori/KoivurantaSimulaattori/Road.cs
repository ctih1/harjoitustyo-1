using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FarseerPhysics.Collision;
using Jypeli;
using Silk.NET.OpenGL;

namespace KoivurantaSimulaattori;

public class Road
{
    private static bool debug = false;
    private static readonly int SIZE = 400;
    public List<GameObject> segments = new List<GameObject>();
    public List<GameObject> stops = new List<GameObject>();
    public Bus bus = null;
    public PhysicsObject busObject = null;
    private static readonly int TERRAIN_PIECES = 4000;
    private bool loaded = false;
    private static readonly int TURN_PIECES = 15;
    private static readonly int TURN_SAFESPACE = 50;
    private static readonly int BUS_STOPS = 20;
    private static Logger logger = new Logger("road.cs");

    public void GenerateAll(PhysicsGame gameInstance, Image roadSegmentTexture, Image leftTurn, Image rightTurn)
    {
        logger.Info("Creating everything");
        GenerateRoad(gameInstance, roadSegmentTexture, leftTurn, rightTurn);
        GenerateStops(gameInstance);
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
        int rotationDirection = 0;
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
                case 180:
                    currentY += SIZE;
                    break;
                
                case 270:
                    currentX -= SIZE;
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

    public void GenerateStops(PhysicsGame instance)
    {
        logger.Debug("Generating bus stop");

        GameObject test = new GameObject(50, 50);
        test.Shape = Shape.Rectangle;
        test.Color = Color.Green;
        test.Position = new Vector(0, 0);

        instance.Add(test);
        
;        HashSet<int> usedRoads = new HashSet<int>();
        for (int i = 0; i < BUS_STOPS; i++)
        {
            GameObject stop = new GameObject(50, 50);
            stop.Shape = Shape.Rectangle;
            stop.Color = Color.Green;

            int targetRoad = RandomNumberGenerator.GetInt32(0, segments.Count);
            while (usedRoads.Contains(targetRoad))
            {
                targetRoad =  RandomNumberGenerator.GetInt32(0, segments.Count);
            }

            stop.Position = segments[targetRoad].Position.RightNormal;
            usedRoads.Add(targetRoad);
            
            stops.Add(stop);
            instance.Add(stop);
        }
    }

    public void PhysicsUpdate(Camera Camera, ScreenView Screen)
    {
        if (!loaded) return;
        bool isOnRoad = false;
        foreach (GameObject piece in segments)
        {
            if(bus == null) return;
            if (busObject.Right > piece.Left && busObject.Left < piece.Right && busObject.Top > piece.Bottom && busObject.Bottom < piece.Top)
            {
                isOnRoad = true;
            }
        }

        if (isOnRoad)
        {
            bus.SlowdownMultiplier = 1;
            bus.GetObject().Color = Color.Yellow;
        } else {
            bus.SlowdownMultiplier = 3;
            bus.GetObject().Color = Color.Red;
        }
    }

    public void LoadBus(Bus bus)
    {
        logger.Debug("Loading bus");
        this.bus = bus;
        this.busObject = bus.GetObject();
    }
    
}