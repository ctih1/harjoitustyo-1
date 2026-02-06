using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace KoivurantaSimulaattori;

public class Logger
{
    private string name;
    private List<string> lines = new List<string>();
    private string logPath = Path.GetTempPath() + "jypeli.log";
    
    public Logger(string name)
    {
        this.name = name;
        File.WriteAllText(logPath, "");
    }

    private void Output(string level, string message)
    {
        string text = String.Format("[{0}:{1}]: {2}\n", level.ToUpper(), name, message);
        lines.Add(text);
        
        File.AppendAllText(logPath, text);
    }

    public void Debug(string message)
    {
        Output("debug", message);
    }

    public void Info(string message)
    {
        Output("info", message);
    }

    public void Warn(string message)
    {
        Output("warn", message);
    }

    public void Error(string message)
    {
        Output("error", message);
    }
}