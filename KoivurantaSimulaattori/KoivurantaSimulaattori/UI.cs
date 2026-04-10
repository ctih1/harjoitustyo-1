using System;
using System.Collections.Generic;
using Jypeli;

namespace KoivurantaSimulaattori;

/// @author gr313123
/// @version 20.3.2025
/// <summary>
/// Sis‰lt‰‰ Pelin UI elementit
/// </summary>
public class UI
{
    private readonly PhysicsGame game;
    private readonly Label speedometer;
    public readonly Label passengerCount;
    private readonly Label stopMark;
    private readonly Label distanceToStop;
    private readonly Label backdoorStatus;
    private readonly Label stopTime;
    private readonly Label anger;
    private readonly Label temperature;

    private readonly Label generalHateBar;
    private readonly Label heatHateBar;
    private readonly Label waitHateBar;
    private readonly Label speedHateBar;
    private readonly Label scoreText;
    private readonly Label scoreMultiplierText;

    private readonly List<Label> requirements = new();
    private readonly ScreenView screen;

    private static UI instance;

    
    /// <summary>
    /// Luo labelin arvoilla
    /// </summary>
    private Label CreateLabel(double x, double y, string text = "Unset")
    {
        Label baseLabel = new Label();

        baseLabel.Font = Font.DefaultBold;
        baseLabel.TextColor = Color.White;
        baseLabel.Text = text;

        baseLabel.X = x;
        baseLabel.Y = y;

        return baseLabel;
    }

    /// <summary>
    /// Luo linjan arvoilla
    /// </summary>
    private Label CreateLine(double y, double width, Color color, string label)
    {
        Label line = new Label
        {
            Y = y,
            SizeMode = TextSizeMode.None,
            Color = color,
            Width = 0
        };

        Label background = new Label
        {
            Y = y,
            SizeMode = TextSizeMode.None,
            Color = Color.White,
            Width = width
        };

        Label hint = new Label
        {
            Y = y + 25,
            Text = label,
            TextColor = Color.Black
        };

        PositionLine(line);
        PositionLine(background);
        PositionLine(hint, 4);

        game.Add(background);
        game.Add(line);
        game.Add(hint);

        return line;
    }
    
    /// <summary>
    /// Asettaa linjan asennon koon muutoksen j‰lkeem
    /// <param name="line">Linja jota pit‰‰ muuttaa</param>
    /// <param name="xOffset">Mahdollinen lis‰et‰isyys sivusta</param>
    /// </summary>

    public void PositionLine(Label line, double xOffset = 0.0)
    {
        line.Position = new Vector(screen.Left + line.Width / 2.0 + xOffset, line.Position.Y);
    }

    
    /// <summary>
    /// Muuttaa linjan pituuden ja vaihtaa paikkaa
    /// <param name="primaryLine">Linja jota muutetaan</param>
    /// <param name="width">Maksimi pituus</param>
    /// <param name="relativeValue">Arvo (0-1 v‰lill‰)</param>
    /// </summary>
    private void UpdateLine(Label primaryLine, double width, double relativeValue)
    {
        primaryLine.Width = width * Math.Max(0, relativeValue);
        PositionLine(primaryLine);
    }

    /// <summary>
    /// Luo uuden UI instancen.
    /// </summary>
    /// <param name="game">Pelin instance</param>
    /// <param name="busStop">Kuva bussipys‰kista</param>
    public UI(PhysicsGame game, Image busStop)
    {
        this.game = game;
        screen = Game.Screen;

        speedometer = CreateLabel(screen.RightSafe - 70, screen.BottomSafe + 60);
        speedometer.Font = new Font(50);

        stopTime = CreateLine(screen.Top - 250, 300, Color.Blue, "Aikaa pys‰kill‰:");

        passengerCount = CreateLabel(screen.LeftSafe + 50, screen.TopSafe - 20);
        backdoorStatus = CreateLabel(screen.LeftSafe + 50, screen.TopSafe - 40);
        distanceToStop = CreateLabel(screen.LeftSafe + 50, screen.Top - 280);
        temperature = CreateLabel(screen.LeftSafe + 50, screen.TopSafe - 500);
        anger = CreateLine(screen.BottomSafe + 20, screen.Width, Color.Red, "Asiakkaiden vihaisuus:");


        generalHateBar = CreateLine(screen.BottomSafe + 70, 100, Color.Orange, "Ajotyyli ‰rsytt‰‰");
        heatHateBar = CreateLine(screen.BottomSafe + 120, 100, Color.Orange, "L‰mpˆtila ‰rsytt‰‰");
        waitHateBar = CreateLine(screen.BottomSafe + 170, 100, Color.Orange, "Odotus ‰rsytt‰‰");
        speedHateBar = CreateLine(screen.BottomSafe + 220, 100, Color.Orange, "Nopeus ‰rsytt‰‰");

        scoreText = new Label
        {
            Font = new Font(38),
            X = screen.RightSafe - 50,
            Y = screen.TopSafe - 35
        };

        scoreMultiplierText = new Label
        {
            Font = new Font(20),
            X = screen.RightSafe - 5,
            Y = screen.TopSafe - 55
        };

        stopMark = new Label();
        stopMark.Image = busStop;
        stopMark.Image.Scaling = ImageScaling.Nearest;
        stopMark.Size = new Vector(200, 62);
        stopMark.X = 0;
        stopMark.Y = screen.TopSafe - 62;

        Label guidelineTitle = CreateLabel(screen.Right - 200, screen.TopSafe - 120, "Koivuranta Ohjeet");
        guidelineTitle.TextColor = Color.Black;


        Label guidelineDescription = CreateLabel(screen.Right - 200, screen.TopSafe - 165,
            "Yleisi‰ ohjeita liittyen\nasiakkaiden hyvinvointiin");
        guidelineDescription.TextColor = Color.Black;
        guidelineDescription.Font = new Font(20);

        string[] reqs =
        [
            "Patteri on mahdollisimman kovalla", "Ajoneuvon nopeus on alle 30 kmh", "Takaovi pidetty kiinni pys‰kill‰",
            "Takaovi auki ajon aikana"
        ];

        for (int i = 0; i < reqs.Length; i++)
        {
            string text = reqs[i];
            Label reqLabel = CreateLabel(screen.Right - 200, screen.TopSafe - 200 - (25 * (i + 1)));
            reqLabel.TextColor = Color.Black;
            reqLabel.Text = text;
            reqLabel.HorizontalAlignment = HorizontalAlignment.Right;
            requirements.Add(reqLabel);
            game.Add(reqLabel);
        }


        HideStop();

        UpdatePassangerCount(0);
        UpdateTemperature(20);
        UpdateBackdoorStatus(false);

        game.Add(speedometer);
        game.Add(passengerCount);
        game.Add(stopMark);
        game.Add(distanceToStop);
        game.Add(stopTime);
        game.Add(backdoorStatus);
        game.Add(anger);
        game.Add(temperature);
        game.Add(guidelineTitle);
        game.Add(guidelineDescription);
        game.Add(scoreText);
        game.Add(scoreMultiplierText);

        instance = this;
    }

    
    /// <summary>
    /// Hankkii valmiin UI:n instancen
    /// </summary>
    public static UI GetInstance()
    {
        return instance;
    }

    /// <summary>
    /// P‰ivitt‰‰ nopeusmittarin nopeutta
    /// </summary>
    /// <param name="speed">Nopeus (kmh)</param>
    public void UpdateSpeedo(double speed)
    {
        speedometer.Text = speed + "km/h";
    }


    /// <summary>
    /// P‰ivitt‰‰ et‰isyytt‰ seuraavalle pys‰kille
    /// </summary>
    /// <param name="distance">Matka l‰himm‰lle pys‰kille</param>
    public void UpdateDistance(double distance)
    {
        distanceToStop.Text = Math.Round(distance) / 100 + " metri‰ l‰himm‰lle pys‰kille";
        PositionLine(distanceToStop, 4);
    }

    /// <summary>
    /// N‰ytt‰‰ bussin STOP-merkin
    /// </summary>
    public void ShowStop()
    {
        stopMark.X = 0;
    }

    /// <summary>
    /// Piilottaa bussin STOP-merkin
    /// </summary>
    public void HideStop()
    {
        stopMark.X = -500000;
    }

    /// <summary>
    /// P‰ivitt‰‰ piste-teksti‰
    /// </summary>
    /// <param name="score">Pisteet</param>
    public void UpdateScoreText(double score)
    {
        scoreText.Text = $"{score:000000}";
    }

    /// <summary>
    /// P‰ivitt‰‰ pisteen kerrointa
    /// </summary>
    /// <param name="multiplier">Pisteen kerroin</param>
    public void UpdateScoreMultiplier(double multiplier)
    {
        scoreMultiplierText.Text = (Math.Round(multiplier * 100) / 100) + "x";
    }


    /// <summary>
    /// P‰ivitt‰‰ pys‰kin aika-palkkia
    /// </summary>
    /// <param name="countdown">Aika pys‰kill‰</param>
    public void UpdateCountdown(double countdown)
    {
        UpdateLine(stopTime, 300, Math.Min(3, countdown) / 3.0);
    }

    /// <summary>
    /// P‰ivitt‰‰ matkustajien m‰‰r‰‰
    /// </summary>
    /// <param name="count">Matkustajien m‰‰r‰</param>
    public void UpdatePassangerCount(int count)
    {
        passengerCount.Text = "Matkustajia: " + count;
        PositionLine(passengerCount, 4);
    }

    /// <summary>
    /// P‰ivitt‰‰ taka-oven asentoa
    /// </summary>
    /// <param name="status">Onko taka-ovi auki</param>
    public void UpdateBackdoorStatus(bool status)
    {
        backdoorStatus.Text = "Takaovi: " + (status ? "auki" : "kiinni");
        PositionLine(backdoorStatus, 4);
    }

    /// <summary>
    /// P‰ivitt‰‰ taka-oven asentoa
    /// </summary>
    /// <param name="amount">Relatiivinen arvo (0-1), jossa 1 on huonoin</param>
    /// <returns>V‰rin joka vastaa arvoa</returns>
    private Color GetRangeColor(double amount)
    {
        if (amount < 0.3)
        {
            return Color.Green;
        }
        if (amount < 0.7)
        {
            return Color.Yellow;
        }
        if (amount < 0.8)
        {
            return Color.Orange;
        }
        if (amount < 0.9)
        {
            return Color.Red;
        }

        return Color.Purple;
    }

    /// <summary>
    /// P‰ivitt‰‰ yhteist‰ raivoa
    /// </summary>
    /// <param name="amount">Yhteisen vihan m‰‰r‰ (0-1)</param>
    public void UpdateAnger(double amount)
    {
        anger.Text = "Viha: " + Math.Round(amount * 100) + "%";
        UpdateLine(anger, screen.Width, amount);
        anger.Color = GetRangeColor(amount);


        anger.TextColor = Color.White;
    }

    /// <summary>
    /// P‰ivitt‰‰ sis‰ist‰ l‰mpˆtilaa
    /// </summary>
    /// <param name="newTemperature">L‰mpˆ bussin sis‰ll‰</param>
    public void UpdateTemperature(int newTemperature)
    {
        if (newTemperature > 0)
        {
            temperature.Text = "Patterin l‰mpˆtila: " + newTemperature + " *C";
        }
        else
        {
            temperature.Text = "Patteri pois p‰‰lt‰";
        }

        PositionLine(temperature, 4);
    }

    /// <summary>
    /// Muuttaa tekstin v‰rin joko vihre‰ksi tai punaiseksi
    /// </summary>
    /// <param name="label">Label jonka v‰ri‰ muutetaan</param>
    /// <param name="condition">Toteutuuko, eli true = vihre‰, false = punainen</param>
    private void UpdateBoolColor(Label label, bool condition)
    {
        label.TextColor = condition ? Color.Green : Color.Red;
    }


    /// <summary>
    /// P‰ivitt‰‰ takaoven ehtoa pys‰kill‰
    /// </summary>
    /// <param name="status">Toteutuuko ehto</param>
    public void UpdateHoldingBackRequirement(bool status)
    {
        UpdateBoolColor(requirements[2], status);
    }


    /// <summary>
    /// P‰ivitt‰‰ nopeuden ehtoa
    /// </summary>
    /// <param name="status">Toteutuuko ehto</param>
    public void UpdateSpeedRequirement(bool status)
    {
        UpdateBoolColor(requirements[1], status);
    }

    /// <summary>
    /// P‰ivitt‰‰ l‰mpˆtilan ehtoa
    /// </summary>
    /// <param name="status">Toteutuuko ehto</param>
    public void UpdateHeatRequirement(bool status)
    {
        UpdateBoolColor(requirements[0], status);
    }

    /// <summary>
    /// P‰ivitt‰‰ taka-oven ehtoa matkalla
    /// </summary>
    /// <param name="status">Toteutuuko ehto</param>
    public void UpdateBackDoorRequirement(bool status)
    {
        UpdateBoolColor(requirements[3], status);
    }

    /// <summary>
    /// P‰ivitt‰‰ kaikkien vihojen omia mittareita
    /// </summary>
    /// <param name="heat">L‰mpˆtilasta tullut viha</param>
    /// <param name="wait">Odotuksesta tullut viha</param>
    /// <param name="normal">Yleinen viha</param>
    /// <param name="speed">Nopeudesta tullut viha</param>
    public void UpdateHates(double heat, double wait, double normal, double speed)
    {
        heat /= 0.9;
        normal /= 0.7;

        UpdateLine(waitHateBar, 100, wait);
        waitHateBar.Color = GetRangeColor(wait);

        UpdateLine(heatHateBar, 100, Math.Min(1, heat));
        heatHateBar.Color = GetRangeColor(heat);

        UpdateLine(generalHateBar, 100, normal);
        generalHateBar.Color = GetRangeColor(normal);

        UpdateLine(speedHateBar, 100, speed);
        speedHateBar.Color = GetRangeColor(speed);
    }
}