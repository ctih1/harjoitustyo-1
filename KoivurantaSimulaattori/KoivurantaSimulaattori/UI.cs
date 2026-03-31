using Jypeli;

namespace KoivurantaSimulaattori;

public class UI
{
    private Label speedometer;
    public Label passangerCount;
    public Label debugLabel;
    private Label stopMark;
    private static UI instance;

    public UI(PhysicsGame game, ScreenView screen, Image busStop)
    {
        speedometer = new Label();
        speedometer.Font = Font.DefaultBold;
        speedometer.TextColor = Color.White;
        speedometer.Y = screen.BottomSafe;
        speedometer.X = screen.LeftSafe + 50;
        speedometer.Layer = Layer.CreateStaticLayer();
        
        passangerCount = new Label();
        passangerCount.Font = Font.DefaultBold;
        passangerCount.TextColor = Color.White;
        passangerCount.Y = screen.BottomSafe;
        passangerCount.X = screen.LeftSafe + 50;
        passangerCount.Layer = Layer.CreateStaticLayer();

        debugLabel = new Label();
        debugLabel.Color = Color.White;
        debugLabel.TextColor = Color.Black;
        debugLabel.X = screen.LeftSafe+50;
        debugLabel.Y = screen.TopSafe-50;
        debugLabel.Width = 400;

        stopMark = new Label();
        stopMark.Image = busStop;
        stopMark.Image.Scaling = ImageScaling.Nearest;
        stopMark.Size = new Vector(200, 62);
        stopMark.X = 0;
        stopMark.Y = screen.TopSafe-62;
        
        game.Add(speedometer);
        game.Add(debugLabel);
        game.Add(passangerCount);
        game.Add(stopMark);

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

    public void ShowStop()
    {
        
    }

    public void HideStop()
    {
        
    }
}