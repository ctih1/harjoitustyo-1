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
    private Image roadTexture;

    public Road(PhysicsGame gameInstance, Image roadSegmentTexture, Image leftTurn, Image rightTurn)
    {
        roadTexture = roadSegmentTexture;
        int currentRotation = 0;
        int currentX = 0;
        int currentY = -SIZE ;
        int stepsFromLastTurn = 0;
        int nextRotation = 0;
        int stepsUntilTurn = -1;
        
        for (int i = 0; i < 300; i++)
        {
            stepsFromLastTurn++;
            if (RandomNumberGenerator.GetInt32(0, 3) == 2 && stepsFromLastTurn > 30 && stepsUntilTurn != 0)
            {
                GameObject sign = new GameObject(128, 128);
                sign.X = currentX;
                sign.Y = currentY;
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

                if (nextRotation < 0)
                {
                    nextRotation = 360 -Math.Abs(nextRotation);
                }
                stepsFromLastTurn = 0;
                stepsUntilTurn = 15;
                gameInstance.Add(sign);
            }

            stepsUntilTurn--;
            if (stepsUntilTurn == 0)
            {
                currentRotation = nextRotation;
                nextRotation = 0;
            }
            GameObject roadSegment = new GameObject(SIZE, SIZE);
            roadSegment.Image = roadSegmentTexture;
            roadSegment.Image.Scaling = ImageScaling.Nearest;

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
    }

    public async void PhysicsUpdate(Camera Camera, ScreenView Screen)
    {
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
        this.bus = bus;
        this.busObject = bus.GetObject();
    }
    
}