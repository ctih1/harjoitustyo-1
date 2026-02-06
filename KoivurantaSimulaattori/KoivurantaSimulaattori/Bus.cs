using System;
using System.Runtime.CompilerServices;
using FarseerPhysics.Dynamics.Contacts;
using Jypeli;

namespace KoivurantaSimulaattori;

public class Bus
{
    private PhysicsObject bus;
    private Label speedometer;
    private static Bus instance;
    private static readonly int SPEED = 50;
    private static readonly int TURNING_SPEED = 5;
    private static readonly double SLOWDOWN_SPEED = 0.02;
    private static readonly double TURN_SLOWDOWN_SPEED = 0.2;
    private static readonly int MAX_VELOCITY = 800;
    private static Logger logger = new Logger("bus.cs");

    private double TurningVelocity = 0;
    private double Velocity = 0;
    public double SlowdownMultiplier = 1;
    private bool handbrakePressed = false;
    
    public Bus(ScreenView screen)
    {
        logger.Info("Creating bus");
        bus = new PhysicsObject(50, 175);
        bus.Shape = Shape.Rectangle;
        bus.Color = Color.Yellow;
        instance = this;

        speedometer = new Label();
        speedometer.Font = Font.DefaultBold;
        speedometer.Color = Color.Black;
        speedometer.TextColor = Color.DarkOrange;
        speedometer.Y = screen.BottomSafe;
        speedometer.X = screen.LeftSafe + 50;
        speedometer.Layer = Layer.CreateStaticLayer();
    }

    public PhysicsObject GetObject()
    {
        logger.Info("Returning bus");
        return this.bus;
    }
    
    public Label GetSpeedo()
    {
        logger.Info("Returning speedo");
        return this.speedometer;
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
            Velocity -= SLOWDOWN_SPEED*SlowdownMultiplier;
        } else if (Velocity < 0)
        {
            Velocity += SLOWDOWN_SPEED*SlowdownMultiplier;
        }
        
        Velocity = Math.Min(Velocity, MAX_VELOCITY);
        TurningVelocity = Math.Min(TurningVelocity, 0.3)*Math.Min(1, Math.Abs(Velocity*200))*(!handbrakePressed).GetHashCode();
        bus.Velocity = new Vector(Math.Sin(bus.Angle.Radians), -Math.Cos(bus.Angle.Radians));
        bus.Velocity = bus.Velocity * SPEED * -Velocity;
        
        Angle angle = Angle.FromDegrees(bus.Angle.Degrees + TURNING_SPEED * TurningVelocity);
        bus.Angle = angle;
        
        speedometer.Text = string.Format("{0} km/h", Math.Round(Velocity*1.4));
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
}