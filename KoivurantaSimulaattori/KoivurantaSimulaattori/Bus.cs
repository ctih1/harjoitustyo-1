using System;
using System.Runtime.CompilerServices;
using FarseerPhysics.Dynamics.Contacts;
using Jypeli;

namespace KoivurantaSimulaattori;

public class Bus
{
    private PhysicsObject bus;
    private static Bus instance;
    private static readonly int SPEED = 50;
    private static readonly int TURNING_SPEED = 5;
    private static readonly double SLOWDOWN_SPEED = 0.02;
    private static readonly double TURN_SLOWDOWN_SPEED = 0.2;
    private static readonly int MAX_VELOCITY = 30;

    private double TurningVelocity = 0;
    private double Velocity = 0;
    
    public Bus()
    {
        bus = new PhysicsObject(50, 175aaa);
        bus.Shape = Shape.Rectangle;
        bus.Color = Color.Yellow;
        instance = this;
        Timer.CreateAndStart(1.0/60.0, GameLoop);
    }

    public PhysicsObject GetObject()
    {
        return this.bus;
    }

    public void Forward()
    {
        Velocity += 0.07;
    }

    public void Brake()
    {
        if (Velocity > 0)
        {
            Velocity -= 0.2;
        }
        else
        {
            Velocity += 0.2;
        }
        
    }

    private void GameLoop()
    {
        if (TurningVelocity < 0.15 && TurningVelocity > -0.15) {
            TurningVelocity = 0;
        } else if (TurningVelocity > 0) {
            TurningVelocity -= TURN_SLOWDOWN_SPEED;
        } else if (TurningVelocity < 0) {
            TurningVelocity += TURN_SLOWDOWN_SPEED;
        }

        if (Velocity < 0.05 && Velocity > -0.05) {
            Velocity = 0;
        } else if (Velocity > 0) {
            Velocity -= SLOWDOWN_SPEED;
        } else if (Velocity < 0)
        {
            Velocity += SLOWDOWN_SPEED;
        }
        
        Velocity = Math.Min(Velocity, MAX_VELOCITY);
        TurningVelocity = Math.Min(TurningVelocity, 0.3)*(Math.Min(1, Math.Abs((Velocity*100))));
        bus.Velocity = new Vector(Math.Sin(bus.Angle.Radians), -Math.Cos(bus.Angle.Radians));
        bus.Velocity = bus.Velocity * SPEED * -Velocity;
        
        Angle angle = Angle.FromDegrees(bus.Angle.Degrees + TURNING_SPEED * TurningVelocity);
        bus.Angle = angle;
    }
    public void Left()
    {
        TurningVelocity += 0.21;
    }

    public void Right()
    {
        TurningVelocity -= 0.21;
    }

    public void SteeringRelease()
    {
        bus.StopAngular();
    }
    
    public static Bus GetInstance()
    {
        instance ??= new Bus();
        return instance;
    }
}