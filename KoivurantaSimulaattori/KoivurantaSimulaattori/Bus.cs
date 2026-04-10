using System;
using Jypeli;

namespace KoivurantaSimulaattori;

/// @author gr313123
/// @version 12.11.2025
/// <summary>
/// Pelin bussi, joka sisältää metodeja bussin ohjausta varten.
/// </summary>
public class Bus
{
    private readonly PhysicsObject bus;
    private static readonly int SPEED = 100;
    private static readonly int TURNING_SPEED = 5;
    private static readonly double SLOWDOWN_SPEED = 0.02;
    private static readonly double TURN_SLOWDOWN_SPEED = 0.2;
    private static readonly double MAX_TURN_VELOCITY = 2.5;
    private static readonly int MAX_VELOCITY = 800;
    private static readonly Logger logger = new Logger("bus.cs");
    private readonly UI gameUi;

    private double TurningVelocity;
    private double Velocity;
    private double ControllerTriggerGas;
    private Vector ControllerStickRight;
    public double SlowdownMultiplier = 1;
    private bool handbrakePressed;

    public int passengerCount = 0;
    public bool stopping = false;
    public bool backdoorOpen;
    public int temperature = 20;
    private double generalAnger;
    public double waitingAnger = 0;
    public double speedAngerOffset;
    public double score;
    public double scoreMultiplier = 1.0;
    
    /// <summary>
    /// Luo bussi-objektin
    /// </summary>
    public Bus()
    {
        logger.Info("Creating bus");
        bus = new PhysicsObject(50, 175);
        bus.Shape = Shape.Rectangle;
        bus.Color = Color.Yellow;
        
        gameUi = UI.GetInstance();
    }

        
    /// <summary>
    /// Palauttaa bussin PhysicsObjektin
    /// </summary>
    public PhysicsObject GetObject()
    {
        logger.Info("Returning bus");
        return this.bus;
    }

    /// <summary>
    /// Jarruttaa bussia
    /// </summary>
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
    
    /// <summary>
    /// Laittaa käsijarrun päälle
    /// </summary>
    public void Handbrake()
    {
        logger.Debug("Handbrake activated");
        handbrakePressed = true;
        if (Velocity > 0)
        {
            Velocity -= 0.4;
        }
        else
        {
            Velocity += 0.4;
        }
        if(Velocity > 3)
        {
            IncreaseAnger(Velocity / 10000);

        }
    }
       
    /// <summary>
    /// Poistaa käsijarrun käytöstä
    /// </summary>
    public void HandbrakeRelease()
    {
        logger.Debug("Handbrake released");
        handbrakePressed = false;
    }

           
    /// <summary>
    /// Poistaa käsijarrun käytöstä
    /// </summary>
    /// <param name="temp">Bussin sisäinen lämpötila</param>
    /// <returns>Palauttaa arvon (0-1) lämpötilasta aiheutuneesta vihaisuudesta</returns>
    private double CalculateHeatAnger(double temp)
    {
        double index;
        if(temp >= 20)
        {
            index = Math.Pow(temperature - 20, 2) / 10.0 / 100;
        }
        else
        {
            index = -Math.Pow(temperature - 25, 3) / 100.0 / 100;
        }

        return Math.Min(1, Math.Max(index, 0));
    }
    
    /// <summary>
    /// Bussin päivitysfunktio, joka päivittää sen position, kulman, ja UI elementtejä
    /// </summary>
    public void GameLoop()
    {
        Velocity += (ControllerTriggerGas*0.07);
        TurningVelocity += (-ControllerStickRight.X)*0.28;
        if (TurningVelocity is < 0.15 and > -0.15) {
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

        if (Velocity is < 0.05 and > -0.05) {
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

        double kmh = Math.Max(0, Math.Round(Velocity * 1.4));
        gameUi.UpdateSpeedo(kmh);

        if(kmh < 40)
        {
            speedAngerOffset += 0.0003;
        } else
        {
            speedAngerOffset -= 0.0003;
        }

        speedAngerOffset = Math.Min(0.3, Math.Max(speedAngerOffset, -0.3));

        gameUi.UpdateHeatRequirement(temperature > 35);
        gameUi.UpdateSpeedRequirement(kmh < 40);

        double heatHate = CalculateHeatAnger(temperature);
        double totalAnger = Math.Max(0, heatHate + waitingAnger + generalAnger + speedAngerOffset);
        gameUi.UpdateAnger(totalAnger);
        gameUi.UpdateHates(heatHate, waitingAnger, generalAnger, speedAngerOffset);
        
        if (totalAnger > 0.9)
        {
            scoreMultiplier = 0.4;
            gameUi.UpdateScoreMultiplier(scoreMultiplier);
        }
        else if(totalAnger is > 0.7 and < 0.9)
        {
            scoreMultiplier = 1.4;
            gameUi.UpdateScoreMultiplier(scoreMultiplier);
        }
        else if (totalAnger is > 0.5 and < 0.7)
        {
            scoreMultiplier = 1.2;
        }
        else
        {
            scoreMultiplier = 1.0;
            gameUi.UpdateScoreMultiplier(scoreMultiplier);
        }

        ChangeScore(kmh/100);

        Angle angle = Angle.FromDegrees(bus.Angle.Degrees + TURNING_SPEED * TurningVelocity);
        bus.Angle = angle;
    }

    /// <summary>
    /// Asettaa bussin arvot takaisin oletuksiin. Käytetään pelin uudelleenkäynnistymisessä
    /// </summary>
    public void Reset()
    {
        bus.Position = new Vector(0, 0);
        bus.Angle = Angle.Zero;
        Velocity = 0;
        TurningVelocity = 0;
        score = 0;
        scoreMultiplier = 1.0;
        temperature = 20;
        backdoorOpen = false;
    }
    
    /// <summary>
    /// Liikuttaa bussia vektorin suuntaan
    /// </summary>
    /// <param name="direction">Liikkumisen suunta</param>
    public void Move(Vector direction)
    {
        TurningVelocity += direction.X * -0.21;
        Velocity += direction.Y * 0.07;
    }

    /// <summary>
    /// Päästää ratista irti
    /// </summary>
    public void SteeringRelease()
    {
        bus.StopAngular();
    }

    /// <summary>
    /// Liikuttaminen ohjaimen kanssa
    /// </summary>
    /// <param name="state">AnalogState, joka vastaa ohjaimen tikkua.</param>
    public void StickMove(AnalogState state)
    {
        ControllerStickRight = state.StateVector;
    }

    /// <summary>
    /// Kaasuttaminen ohjaimen kanssa
    /// </summary>
    /// <param name="state">AnalogState, joka vastaa ohjaimen triggerin arvoa.</param>
    public void TriggerAccel(AnalogState state)
    {
        ControllerTriggerGas = (state.State + 1) / 2.0;
    }

    /// <summary>
    /// Vaihtaa takaoven asentoa
    /// </summary>
    public void ToggleBackdoor()
    {
        backdoorOpen = !backdoorOpen;
        gameUi.UpdateBackdoorStatus(backdoorOpen);
    }

    /// <summary>
    /// Nostaa bussin sisäistä lämpötilaa
    /// </summary>
    public void IncreaseTemperature()
    {
        temperature = Math.Min(50, temperature + 1);
        gameUi.UpdateTemperature(temperature);
    }


    /// <summary>
    /// Laskee bussin sisäistä lämpötilaa
    /// </summary>
    public void DecreaseTemperature()
    {
        temperature = Math.Max(0, temperature -1);
        gameUi.UpdateTemperature(temperature);
    }

    /// <summary>
    /// Asettaa yleisen raivoarvon
    /// </summary>
    public void SetAnger(double anger)
    {
        this.generalAnger = Math.Max(0, Math.Min(anger, 0.7));
    }

    /// <summary>
    /// Nostaa yleistä raivoisuutta
    /// </summary>
    public void IncreaseAnger(double amount)
    {
        SetAnger(this.generalAnger + amount);
    }
    
    /// <summary>
    /// Laskee yleistä raivoisuutta
    /// </summary>
    public void DecreaseAnger(double amount)
    {
        SetAnger(this.generalAnger - amount);
    }

    
    /// <summary>
    /// Vaihtaa pisteitä
    /// </summary>
    /// /// <param name="amount">Muutoksen määrä. Jos miinusta, niin pisteet laskevat</param>
    public void ChangeScore(double amount)
    {
        score += amount * scoreMultiplier;
        gameUi.UpdateScoreText(score);
    }

}