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
    private const int Speed = 100;
    private const int TurningSpeed = 5;
    private const double SlowdownSpeed = 0.02;
    private const double TurnSlowdownSpeed = 0.2;
    private const double MaxTurnVelocity = 2.5;
    private const int MaxVelocity = 800;
    private const int OptimalTemperature = 20;
    private const double VelocityDeadZone = 0.05;
    private const double TurningVelocityDeadZone = 0.15;
    private const double SpeedAngerOffsetIncrease = 0.0003;
    private const double SpeedAngerIncreaseLimit = 0.3;
    private const int MinimumHeatForGoal = 35;
    private const int MinimumSpeedForGoal = 40;
    private const double ControllerTurningVelocityMultiplier = 0.28;
    private const double ControllerAccelerationMultiplier = 0.07;
    private const int TurningVelocityMultiplier = 200;
    private const double KmhVelocityMultiplier = 1.4;

    private static readonly Logger logger = new("bus.cs");
    private readonly PhysicsObject bus;
    private readonly UI gameUi;

    private double turningVelocity;
    private double velocity;
    private double controllerTriggerGas;
    private Vector controllerStickRight;
    private bool handbrakePressed;

    /// <summary>
    /// Muuttaa bussin nopeutta, esimerkiksi kun ajaa tien ulkopuolella
    /// </summary>
    public double slowdownMultiplier = 1;
    
    /// <summary>
    /// Kuinka monta matkustajaa on bussissa
    /// </summary>
    public int passengerCount = 0;
    
    /// <summary>
    /// Onko bussissa stop-valo päällä (eli haulaako joku pois)
    /// </summary>
    public bool stopping = false;
    
    /// <summary>
    /// Onko bussin takaovi auki
    /// </summary>
    public bool backdoorOpen;
    
    /// <summary>
    /// Bussin sisäinen lämpötila
    /// </summary>
    public int temperature = 20;
    
    /// <summary>
    /// Yleinen vihaisuus, joka kertyy monesta eri asiasta
    /// </summary>
    private double generalAnger;
    
    /// <summary>
    /// Vihaisuus joka tulee kun et päästä matkustajia pois pysäkillä
    /// </summary>
    public double waitingAnger = 0;
    
    /// <summary>
    /// Vihaisuus joka tulee jos ajat liian hiljaa
    /// </summary>
    public double speedAngerOffset;
    
    /// <summary>
    /// Pistemäärä
    /// </summary>
    public double score;
    
    /// <summary>
    /// Pistemäärän kerroin
    /// </summary>
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
        return bus;
    }

    /// <summary>
    /// Jarruttaa bussia
    /// </summary>
    public void Brake()
    {
        logger.Debug("Breaking");
        if (velocity > 0)
        {
            velocity -= 0.2;
        }
        else
        {
            velocity += 0.2;
        }
    }

    /// <summary>
    /// Laittaa käsijarrun päälle
    /// </summary>
    public void Handbrake()
    {
        logger.Debug("Handbrake activated");
        handbrakePressed = true;
        if (velocity > 0)
        {
            velocity -= 0.4;
        }
        else
        {
            velocity += 0.4;
        }

        if (velocity > 3)
        {
            IncreaseAnger(velocity / 10000);
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
        if (temp >= OptimalTemperature)
        {
            index = Math.Pow(temperature - OptimalTemperature, 2) / 10.0 / 100;
        }
        else
        {
            index = -Math.Pow(temperature - (OptimalTemperature + 5), 3) / 100.0 / 100;
        }

        return Math.Min(1, Math.Max(index, 0));
    }

    /// <summary>
    /// Bussin päivitysfunktio, joka päivittää sen position, kulman, ja UI elementtejä
    /// </summary>
    public void GameLoop()
    {
        velocity += (controllerTriggerGas * ControllerAccelerationMultiplier);
        turningVelocity += (-controllerStickRight.X) * ControllerTurningVelocityMultiplier;

        if (turningVelocity is < TurningVelocityDeadZone and > -TurningVelocityDeadZone)
        {
            turningVelocity = 0;
        }
        else if (turningVelocity > 0)
        {
            turningVelocity -= TurnSlowdownSpeed;
        }
        else if (turningVelocity < 0)
        {
            turningVelocity += TurnSlowdownSpeed;
        }

        if (turningVelocity > MaxTurnVelocity)
        {
            turningVelocity = MaxTurnVelocity;
        }

        if (turningVelocity < -MaxTurnVelocity)
        {
            turningVelocity = -MaxTurnVelocity;
        }

        if (velocity is < VelocityDeadZone and > -VelocityDeadZone)
        {
            velocity = 0;
        }
        else if (velocity > 0)
        {
            velocity -= SlowdownSpeed * slowdownMultiplier;
        }
        else if (velocity < 0)
        {
            velocity += SlowdownSpeed * slowdownMultiplier;
        }

        velocity = Math.Min(velocity, MaxVelocity);
        turningVelocity = turningVelocity * Math.Min(1, Math.Abs(velocity * TurningVelocityMultiplier)) *
                          (!handbrakePressed).GetHashCode();
        bus.Velocity = new Vector(Math.Sin(bus.Angle.Radians), -Math.Cos(bus.Angle.Radians));
        bus.Velocity = bus.Velocity * Speed * -velocity;

        double kmh = Math.Max(0, Math.Round(velocity * KmhVelocityMultiplier));
        gameUi.UpdateSpeedo(kmh);

        if (kmh < MinimumSpeedForGoal)
        {
            speedAngerOffset += SpeedAngerOffsetIncrease;
        }
        else
        {
            speedAngerOffset -= SpeedAngerOffsetIncrease;
        }

        speedAngerOffset = Math.Min(SpeedAngerIncreaseLimit, Math.Max(speedAngerOffset, -SpeedAngerIncreaseLimit));

        gameUi.UpdateHeatRequirement(temperature > MinimumHeatForGoal);
        gameUi.UpdateSpeedRequirement(kmh < MinimumSpeedForGoal);

        double heatHate = CalculateHeatAnger(temperature);
        double totalAnger = Math.Max(0, heatHate + waitingAnger + generalAnger + speedAngerOffset);
        gameUi.UpdateAnger(totalAnger);
        gameUi.UpdateHates(heatHate, waitingAnger, generalAnger, speedAngerOffset);

        if (totalAnger > 0.9)
        {
            scoreMultiplier = 0.4;
            gameUi.UpdateScoreMultiplier(scoreMultiplier);
        }
        else if (totalAnger is > 0.7 and < 0.9)
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

        ChangeScore(kmh / 100);

        Angle angle = Angle.FromDegrees(bus.Angle.Degrees + TurningSpeed * turningVelocity);
        bus.Angle = angle;
    }

    /// <summary>
    /// Asettaa bussin arvot takaisin oletuksiin. Käytetään pelin uudelleenkäynnistymisessä
    /// </summary>
    public void Reset()
    {
        bus.Position = new Vector(0, 0);
        bus.Angle = Angle.Zero;
        velocity = 0;
        turningVelocity = 0;
        score = 0;
        scoreMultiplier = 1.0;
        temperature = OptimalTemperature;
        backdoorOpen = false;
    }

    /// <summary>
    /// Liikuttaa bussia vektorin suuntaan
    /// </summary>
    /// <param name="direction">Liikkumisen suunta</param>
    public void Move(Vector direction)
    {
        turningVelocity += direction.X * -0.21;
        velocity += direction.Y * 0.07;
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
        controllerStickRight = state.StateVector;
    }

    /// <summary>
    /// Kaasuttaminen ohjaimen kanssa
    /// </summary>
    /// <param name="state">AnalogState, joka vastaa ohjaimen triggerin arvoa.</param>
    public void TriggerAccel(AnalogState state)
    {
        controllerTriggerGas = (state.State + 1) / 2.0;
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
        temperature = Math.Max(0, temperature - 1);
        gameUi.UpdateTemperature(temperature);
    }

    /// <summary>
    /// Asettaa yleisen raivoarvon
    /// </summary>
    public void SetAnger(double anger)
    {
        generalAnger = Math.Max(0, Math.Min(anger, 0.7));
    }

    /// <summary>
    /// Nostaa yleistä raivoisuutta
    /// </summary>
    public void IncreaseAnger(double amount)
    {
        SetAnger(generalAnger + amount);
    }

    /// <summary>
    /// Laskee yleistä raivoisuutta
    /// </summary>
    public void DecreaseAnger(double amount)
    {
        SetAnger(generalAnger - amount);
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