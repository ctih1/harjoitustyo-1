using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using Jypeli;
using Jypeli.Widgets;
using SixLabors.ImageSharp.ColorSpaces;

namespace KoivurantaSimulaattori;

public class UI
{
    private PhysicsGame game;
    private Label speedometer;
    public Label passangerCount;
    public Label debugLabel;
    private Label stopMark;
    private Label distanceToStop;
    private Label backdoorStatus;
    private Label stopTime;
    private Label anger;
    private Label temperature;
    private Label guidelineContainer;
    private Label guidelineTitle;
    private Label guidelineDescription;

    private Label generalHateBar;
    private Label heatHateBar;
    private Label waitHateBar;
    private Label speedHateBar;
    private Label scoreText;
    private Label scoreMultiplierText;

    private List<Label> requirements = new List<Label>();
    private ScreenView screen;

    private static UI instance;

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

    private (Label, Label) CreateLine(double y, double width, Color color, string label)
    {
        Label line = new Label {
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

        this.game.Add(background);
        this.game.Add(line);
        this.game.Add(hint);

        return (line, background);
    }

    public void PositionLine(Label line, double xOffset = 0.0)
    {
        line.Position = new Vector(screen.Left + line.Width / 2.0 + xOffset, line.Position.Y);
    }

    private void UpdateLine(Label primaryLine, double width, double relativeValue)
    {
        primaryLine.Width = width * Math.Max(0, relativeValue);
        PositionLine(primaryLine);
    }

    public UI(PhysicsGame game, Image busStop)
    {
        this.game = game;
        screen = Game.Screen;

        speedometer = CreateLabel(screen.RightSafe - 70, screen.BottomSafe+60);
        speedometer.Font = new Font(50);

        (stopTime, _) = CreateLine(screen.Top - 250, 300, Color.Blue, "Aikaa pys‰kill‰:");

        passangerCount = CreateLabel(screen.LeftSafe + 50, screen.TopSafe-20);
        backdoorStatus = CreateLabel(screen.LeftSafe + 50, screen.TopSafe - 40);
        distanceToStop = CreateLabel(screen.LeftSafe + 50, screen.Top - 280);
        temperature = CreateLabel(screen.LeftSafe + 50, screen.TopSafe - 500);
        (anger, _) = CreateLine(screen.BottomSafe + 20, screen.Width, Color.Red, "Asiakkaiden vihaisuus:");


        (generalHateBar, _) = CreateLine(screen.BottomSafe + 70, 100, Color.Orange, "Ajotyyli ‰rsytt‰‰");
        (heatHateBar, _) = CreateLine(screen.BottomSafe + 120, 100, Color.Orange, "L‰mpˆtila ‰rsytt‰‰");
        (waitHateBar, _) = CreateLine(screen.BottomSafe + 170, 100, Color.Orange, "Odotus ‰rsytt‰‰");
        (speedHateBar, _) = CreateLine(screen.BottomSafe + 220, 100, Color.Orange, "Nopeus ‰rsytt‰‰");

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
        stopMark.Y = screen.TopSafe-62;

        guidelineTitle = CreateLabel(screen.Right - 200, screen.TopSafe - 120, "Koivuranta Ohjeet");
        guidelineTitle.TextColor = Color.Black;


        guidelineDescription = CreateLabel(screen.Right - 200, screen.TopSafe - 165, "Yleisi‰ ohjeita liittyen\nasiakkaiden hyvinvointiin");
        guidelineDescription.TextColor = Color.Black;
        guidelineDescription.Font = new Font(20);

        string[] reqs = ["Patteri on mahdollisimman kovalla", "Ajoneuvon nopeus on alle 30 kmh", "Takaovi pidetty kiinni pys‰kill‰", "Takaovi auki ajon aikana"];

        for(int i=0; i<reqs.Length; i++)
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
        game.Add(passangerCount);
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

    public static UI GetInstance()
    {
        return instance;
    }

    public void UpdateSpeedo(double speed)
    {
        speedometer.Text = string.Format("{0} km/h", speed);
    }


    public void UpdateDistance(double distance)
    {
        distanceToStop.Text = Math.Round(distance) / 100 + " metri‰ l‰himm‰lle pys‰kille";
        PositionLine(distanceToStop, 4);
    }

    public void ShowStop()
    {
        stopMark.X = 0;
    }
    
    public void UpdateScoreText(double score)
    {
        scoreText.Text = $"{score:000000}";
    }

    public void UpdateScoreMultiplier(double multiplier)
    {
        scoreMultiplierText.Text = (Math.Round(multiplier * 100) / 100).ToString() + "x";
    }

    public void HideStop()
    {
        stopMark.X = -500000;
    }


    public void UpdateCountdown(double countdown)
    {
        UpdateLine(stopTime, 300, Math.Min(3, countdown) / 3.0);
    }

    public void UpdatePassangerCount(int count)
    {
        passangerCount.Text = "Matkustajia: " + count.ToString();
        PositionLine(passangerCount, 4);
    }

    public void UpdateBackdoorStatus(bool status)
    {
        backdoorStatus.Text = "Takaovi: " + (status ? "auki" : "kiinni");
        PositionLine(backdoorStatus, 4);
    }

    private Color GetRangeColor(double amount)
    {
        if (amount < 0.3)
        {
            return Color.Green;
        }
        else if (amount < 0.5)
        {
            return Color.Yellow;
        }
        else if (amount < 0.7)
        {
            return Color.Orange;
        }
        else if (amount < 0.9)
        {
            return Color.Red;
        }
        else
        {
            return Color.Purple;
        }
    } 

    public void UpdateAnger(double amount)
    {
        anger.Text = "Viha: " + Math.Round(amount*100).ToString() + "%";
        UpdateLine(anger, screen.Width, amount);
        anger.Color = GetRangeColor(amount);
        

        anger.TextColor = Color.White;

    }

    public void UpdateTemperature(int temperature)
    {
        if(temperature > 0)
        {
            this.temperature.Text = "Patterin l‰mpˆtila: " + temperature.ToString() + " *C";

        } else
        {
            this.temperature.Text = "Patteri pois p‰‰lt‰";
        }
        PositionLine(this.temperature, 4);
    }

    public void UpdateBoolColor(Label label, bool condition)
    {
        label.TextColor = condition ? Color.Green : Color.Red;
    }

    public void UpdateHoldingBackRequirement(bool status)
    {
        UpdateBoolColor(requirements[2], status);
    }

    public void UpdateSpeedRequirement(bool status)
    {
        UpdateBoolColor(requirements[1], status);
    }

    public void UpdateHeatRequirement(bool status)
    {
        UpdateBoolColor(requirements[0], status);
    }

    public void UpdateBackDoorRequirement(bool status)
    {
        UpdateBoolColor(requirements[3], status);
    }

    public void UpdateHates(double heat, double wait, double normal, double speed)
    {
        heat /= 0.9;
        normal /= 0.7;

        UpdateLine(waitHateBar, 100, wait);
        waitHateBar.Color = GetRangeColor(wait);

        UpdateLine(heatHateBar, 100, heat);
        heatHateBar.Color = GetRangeColor(heat);

        UpdateLine(generalHateBar, 100, normal);
        generalHateBar.Color = GetRangeColor(normal);

        UpdateLine(speedHateBar, 100, speed);
        speedHateBar.Color = GetRangeColor(speed);
    }

    

}