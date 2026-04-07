using System;
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
    private Label stopTime;

    private static UI instance;

    private Label CreateLabel(double x, double y)
    {
        Label baseLabel = new Label();
        
        baseLabel.Font = Font.DefaultBold;
        baseLabel.TextColor = Color.White;
        baseLabel.Layer = Layer.CreateStaticLayer();

        baseLabel.X = x;
        baseLabel.Y = y;

        return baseLabel;
    }

    public UI(PhysicsGame game, Image busStop)
    {
        ScreenView screen = Game.Screen;
            
        speedometer = CreateLabel(screen.LeftSafe + 50, screen.BottomSafe);
        passangerCount = CreateLabel(screen.LeftSafe + 50, screen.TopSafe - 300);
        debugLabel = CreateLabel(screen.LeftSafe + 50, screen.TopSafe -  100);
        stopTime = CreateLabel(screen.LeftSafe + 50, screen.TopSafe - 200);

        debugLabel.Width = 500;
            
        stopMark = new Label();
        stopMark.Image = busStop;
        stopMark.Image.Scaling = ImageScaling.Nearest;
        stopMark.Size = new Vector(200, 62);
        stopMark.X = 0;
        stopMark.Y = screen.TopSafe-62;

        distanceToStop = CreateLabel(screen.LeftSafe + 50, screen.TopSafe - 400);
            
        HideStop();
            
        game.Add(speedometer);
        game.Add(debugLabel);
        game.Add(passangerCount);
        game.Add(stopMark);
        game.Add(distanceToStop);
        game.Add(stopTime);
            
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
}