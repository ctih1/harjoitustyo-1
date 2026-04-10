using System;
using System.IO;

namespace KoivurantaSimulaattori;

public class Logger
{
    private readonly string name;
    private readonly string logPath = Path.GetTempPath() + "jypeli.log";

    public Logger(string name)
    {
        this.name = name;
        File.WriteAllText(logPath, "");
    }

    private void Output(string level, string message)
    {
        string text = "[" + level + ":" + name + "]: " + message + "\n";
        Console.WriteLine(text);
    }

    public void Debug(string message)
    {
        Output("debug", message);
    }

    public void Info(string message)
    {
        Output("info", message);
    }
}