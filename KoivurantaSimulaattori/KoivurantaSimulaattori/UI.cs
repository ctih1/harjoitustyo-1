using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using Jypeli;

namespace KoivurantaSimulaattori;

public class UI
{
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
    private List<Label> requirements = new List<Label>();

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

    public UI(PhysicsGame game, Image busStop)
    {
        ScreenView screen = Game.Screen;

            
        speedometer = CreateLabel(screen.LeftSafe + 50, screen.BottomSafe);
        debugLabel = CreateLabel(screen.LeftSafe + 50, screen.TopSafe -  100);
        stopTime = CreateLabel(screen.LeftSafe + 50, screen.TopSafe - 200);
        passangerCount = CreateLabel(screen.LeftSafe + 50, screen.TopSafe - 300);
        distanceToStop = CreateLabel(screen.LeftSafe + 50, screen.TopSafe - 350);
        backdoorStatus = CreateLabel(screen.LeftSafe + 50, screen.TopSafe - 400);
        anger = CreateLabel(screen.LeftSafe + 50, screen.TopSafe - 450);
        temperature = CreateLabel(screen.LeftSafe + 50, screen.TopSafe - 500);


        debugLabel.Width = 500;
            
        stopMark = new Label();
        stopMark.Image = busStop;
        stopMark.Image.Scaling = ImageScaling.Nearest;
        stopMark.Size = new Vector(200, 62);
        stopMark.X = 0;
        stopMark.Y = screen.TopSafe-62;

        guidelineContainer = new Label();
        guidelineContainer.Width = 200;
        guidelineContainer.Height = 200;
        guidelineContainer.Color = Color.LightGray;
        guidelineContainer.X = screen.Right - 100;
        guidelineContainer.Y = screen.TopSafe - 100;

        guidelineTitle = CreateLabel(screen.Right - 100, screen.TopSafe - 25, "Koivuranta Ohjeet");
        guidelineTitle.TextColor = Color.Black;


        guidelineDescription = CreateLabel(screen.Right - 100, screen.TopSafe - 60, "Yleisiä ohjeita liittyen\nasiakkaiden hyvinvointiin");
        guidelineDescription.TextColor = Color.Black;
        guidelineDescription.Font = new Font(16);

        string[] reqs = ["Patteri on mahdollisimman kovalla", "Ajoneuvon nopeus on alle 30 kmh", "Takaovi pidetty kiinni pysäkillä", "Takaovi auki ajon aikana"];

        for(int i=0; i<reqs.Length; i++)
        {
            string text = reqs[i];
            Label reqLabel = CreateLabel(screen.Right - 200, screen.TopSafe - 80 - (25 * (i + 1)));
            reqLabel.TextColor = Color.Black;
            reqLabel.Text = text;
            reqLabel.HorizontalAlignment = HorizontalAlignment.Right;
            requirements.Add(reqLabel);
            game.Add(reqLabel);
        }


        HideStop(); 

        game.Add(speedometer);
        game.Add(debugLabel);
        game.Add(passangerCount);
        game.Add(stopMark);
        game.Add(distanceToStop);
        game.Add(stopTime);
        game.Add(backdoorStatus);
        game.Add(anger);
        game.Add(temperature);
        game.Add(guidelineTitle);
        game.Add(guidelineDescription);
            
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

    public void UpdateDebugInfo(string info)
    {
        debugLabel.Text = info;
    }

    public void UpdateDistance(double distance)
    {
        distanceToStop.Text = Math.Round(distance) / 100 + " meters";
    }

    public void ShowStop()
    {
        stopMark.X = 0;
    }

    public void HideStop()
    {
        stopMark.X = -500000;
    }

    public void UpdateCountdown(double countdown)
    {
        stopTime.Text = "Time on stop: " + countdown.ToString();
    }

    public void UpdatePassangerCount(int count)
    {
        passangerCount.Text = "Passangers: " + count.ToString();
    }

    public void UpdateBackdoorStatus(bool status)
    {
        backdoorStatus.Text = status ? "Backdoor open" : "Backdoor closed";
    }

    public void UpdateAnger(double amount)
    {
        anger.Text = "Anger: " + Math.Round(amount*100).ToString() + "%";
    }

    public void UpdateTemperature(int temperature)
    {
        if(temperature > 0)
        {
            this.temperature.Text = "Heater temp: " + temperature.ToString() + " *C";

        } else
        {
            this.temperature.Text = "Heater disabled";
        }
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

}