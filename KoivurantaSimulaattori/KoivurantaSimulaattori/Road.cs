using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Jypeli;
using Silk.NET.OpenGL;

namespace KoivurantaSimulaattori;

public class Road
{
    private static readonly int SIZE = 400;
    public List<GameObject> segments = new List<GameObject>();
    public Bus bus = null;
    public PhysicsObject busObject = null;
    private static readonly int TERRAIN_PIECES = 1000;
    private bool loaded = false;
    private static readonly int TURN_PIECES = 15;
    private static readonly int TURN_SAFESPACE = 50;

    public void GenerateRoad(PhysicsGame gameInstance, Image roadSegmentTexture, Image leftTurn, Image rightTurn)
    {
        int currentRotation = 0;
        int currentX = 0;
        int currentY = -SIZE ;
        int stepsFromLastTurn = 0;
        int nextRotation = 0;
        int stepsUntilTurn = -1;
        int minSteps = -1;
        int rotationDirection = 0;


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
                    nextRotation -= 90;
                    sign.Image = leftTurn;
                }
                else
                {
                    nextRotation += 90;
                    sign.Image = rightTurn;
                }

                if (nextRotation >= 360)
                {
                    nextRotation = 0;
                }

                stepsFromLastTurn = 0;
                stepsUntilTurn = 8;

                gameInstance.Add(sign);
            }

            stepsUntilTurn--;
            if (stepsUntilTurn == 0)
            {
                for (int j=TURN_PIECES; j>0; j--)
                {
                    double angle = j * (nextRotation / (double)TURN_PIECES);
                    double rad = angle * Math.PI / 180.0;
                    GameObject rotationPiece = new GameObject(SIZE, SIZE);
                    rotationPiece.Angle = Angle.FromDegrees(angle+90);
                    
                    double a = SIZE * Math.Cos(rad);
                    double b = SIZE * Math.Sin(rad);

                    rotationPiece.X =  segments.Last().X + a;
                    rotationPiece.Y = segments.Last().Y + b;

                    rotationPiece.Color = Color.Lighter(Color.Black, (int)(j / (double)TURN_PIECES * 255));
                    segments.Add(rotationPiece);
                    Label dbgLabel = new Label(angle + " #" + j + " / "  + nextRotation);
                    dbgLabel.X = rotationPiece.X;
                    dbgLabel.Y = rotationPiece.Y;
                    dbgLabel.TextColor = Color.Red;
                    dbgLabel.Size = new Vector(50, 50);
                    dbgLabel.TextScale = new Vector(4, 4);
                    gameInstance.Add(dbgLabel, 2);
                    gameInstance.Add(rotationPiece, -2);
                    currentY += (int)b;
                    currentX += (int)a;
                }
                currentRotation = nextRotation;
            }

            GameObject roadSegment = new GameObject(SIZE, SIZE);
            roadSegment.Image = roadSegmentTexture;

            switch (currentRotation)
            {
                case 90:
                    currentX += SIZE;
                    break;
                case 270:
                    currentX -= SIZE;
                    break;
                case 180:
                case 0:
                    currentY += SIZE;
                    break;
            }

            roadSegment.X = currentX;
            roadSegment.Y = currentY;

            roadSegment.Angle = Angle.FromDegrees(currentRotation);

            roadSegment.Color = Color.Black;
            segments.Add(roadSegment);

            gameInstance.Add(roadSegment, -3);
        }

        loaded = true;
    }

    public async void PhysicsUpdate(Camera Camera, ScreenView Screen)
    {
        bool isOnRoad = false;
        if (!loaded) return;
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
        this.bus = bus;
        this.busObject = bus.GetObject();
    }
    
}