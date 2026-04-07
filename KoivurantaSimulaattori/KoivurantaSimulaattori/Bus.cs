using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FarseerPhysics.Dynamics.Contacts;
using Jypeli;

namespace KoivurantaSimulaattori;

public class Bus
{
    private PhysicsObject bus;
    private static Bus instance;
    private static readonly int SPEED = 100;
    private static readonly int TURNING_SPEED = 5;
    private static readonly double SLOWDOWN_SPEED = 0.02;
    private static readonly double TURN_SLOWDOWN_SPEED = 0.2;
    private static readonly double MAX_TURN_VELOCITY = 2.5;
    private static readonly int MAX_VELOCITY = 800;
    private static Logger logger = new Logger("bus.cs");
    private UI gameUi;

    private double TurningVelocity = 0;
    private double Velocity = 0;
    private double ControllerTriggerGas = 0;
    private Vector ControllerStickRight = new Vector();
    public double SlowdownMultiplier = 1;
    private bool handbrakePressed = false;

    public int passangerCount = 0;
    
    public Bus(ScreenView screen)
    {
        logger.Info("Creating bus");
        bus = new PhysicsObject(50, 175);
        bus.Shape = Shape.Rectangle;
        bus.Color = Color.Yellow;
        instance = this;
        
        gameUi = UI.GetInstance();
    }

    public PhysicsObject GetObject()
    {
        logger.Info("Returning bus");
        return this.bus;
    }

    public void Forward()
    {
        Velocity += 0.07;
    }

    public void Brake()
    {
        logger.Debug("Breaking");
        if (Velocity > 0)
        {
            Velocity -= 0.2;
        }
        else
        {
            Velocity += 0.2;
        }
        
    }


    public void Handbrake()
    {
        logger.Debug("Handbrake actiavted");
        handbrakePressed = true;
        if (Velocity > 0)
        {
            Velocity -= 0.4;
        }
        else
        {
            Velocity += 0.4;
        }
    }
       
    public void HandbrakeRelease()
    {
        logger.Debug("Handbrake released");
        handbrakePressed = false;
    }

    public void GameLoop()
    {
        Velocity += (ControllerTriggerGas*0.07);
        // y=(x-0.1)^(((1)/(3)))
        TurningVelocity += (-ControllerStickRight.X)*0.28;
        if (TurningVelocity < 0.15 && TurningVelocity > -0.15) {
            TurningVelocity = 0;
        } else if (TurningVelocity > 0) {
            TurningVelocity -= TURN_SLOWDOWN_SPEED;
        } else if (TurningVelocity < 0) {
            TurningVelocity += TURN_SLOWDOWN_SPEED;
        }

        if (TurningVelocity > MAX_TURN_VELOCITY)
        {
            TurningVelocity = MAX_TURN_VELOCITY;
        }

        if (TurningVelocity < -MAX_TURN_VELOCITY)
        {
            TurningVelocity = -MAX_TURN_VELOCITY;
        }

        if (Velocity < 0.05 && Velocity > -0.05) {
            Velocity = 0;
        } else if (Velocity > 0) {
            Velocity -= SLOWDOWN_SPEED*SlowdownMultiplier;
        } else if (Velocity < 0)
        {
            Velocity += SLOWDOWN_SPEED*SlowdownMultiplier;
        }
        
        Velocity = Math.Min(Velocity, MAX_VELOCITY);
        TurningVelocity = TurningVelocity * Math.Min(1, Math.Abs(Velocity*200)) * (!handbrakePressed).GetHashCode();
        bus.Velocity = new Vector(Math.Sin(bus.Angle.Radians), -Math.Cos(bus.Angle.Radians));
        bus.Velocity = bus.Velocity * SPEED * -Velocity;

        gameUi.UpdateSpeedo(Math.Max(0, Math.Round(Velocity*1.4)));
        gameUi.UpdateDebugInfo(string.Format("x,y: {0},{1}", Math.Round(bus.Position.X), Math.Round(bus.Position.Y)));
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

    public void StickMove(AnalogState state)
    {
        ControllerStickRight = state.StateVector;
    }

    public void TriggerAccel(AnalogState state)
    {
        ControllerTriggerGas = (state.State + 1) / 2.0;
    }
}