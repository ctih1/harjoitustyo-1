using System;
using System.Collections.Generic;
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

    public void GenerateRoad(PhysicsGame gameInstance, Image roadSegmentTexture, Image leftTurn, Image rightTurn)
    {
        int currentRotation = 0;
        int currentX = 0;
        int currentY = -SIZE ;
        int stepsFromLastTurn = 0;
        int nextRotation = 0;
        int stepsUntilTurn = -1;
        int minSteps = 15;


        for (int i=0; i<TERRAIN_PIECES; i++)
        {
            stepsFromLastTurn++;
            if (stepsFromLastTurn > minSteps && stepsUntilTurn != 0 && RandomNumberGenerator.GetInt32(0, 10) == 1)
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

                if (nextRotation == 180)
                {
                    minSteps = 12;
                }

                if (nextRotation < 0)
                {
                    nextRotation = 360 - Math.Abs(nextRotation);
                }

                stepsFromLastTurn = 0;
                stepsUntilTurn = 15;

                gameInstance.Add(sign);
            }

            stepsUntilTurn--;
            if (stepsUntilTurn == 0)
            {
                currentRotation = nextRotation;
            }

            GameObject roadSegment = new GameObject(SIZE, SIZE);
            roadSegment.Image = roadSegmentTexture;

            switch (currentRotation)
            {
                case 90:
                    currentX += SIZE;
                    break;
                case 180:
                    currentY -= SIZE;
                    break;
                case 270:
                    currentX -= SIZE;
                    break;
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