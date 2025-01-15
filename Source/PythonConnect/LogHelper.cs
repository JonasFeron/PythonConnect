using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using log4net;
using log4net.Repository.Hierarchy;
using log4net.Core;
using log4net.Appender;
using log4net.Layout;
using System.IO;
using System.Net.NetworkInformation;

namespace PythonConnect
{
    public class LogHelper
    {
        public static void Setup(string level, string solutionDirectory)
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date{ABSOLUTE} [%logger] -%thread-  %level - %message%newline%exception";
            patternLayout.ActivateOptions();

            RollingFileAppender roller = new RollingFileAppender();
            roller.AppendToFile = true;
            roller.File = Path.Combine(solutionDirectory, @".logs\LogFile.txt");
            roller.Layout = patternLayout;
            roller.MaxSizeRollBackups = 3;
            roller.MaximumFileSize = "5MB";
            roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            roller.StaticLogFileName = true;
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            MemoryAppender memory = new MemoryAppender();
            memory.ActivateOptions();
            hierarchy.Root.AddAppender(memory);

            Level lvl = Level.All;
            switch (level)
            {
                case string s when s == Level.Debug.DisplayName:
                    lvl = Level.Debug;
                    break;
                case string s when s == Level.Info.DisplayName:
                    lvl = Level.Info;
                    break;
                case string s when s == Level.Warn.DisplayName:
                    lvl = Level.Warn;
                    break;
                case string s when s == Level.Error.DisplayName:
                    lvl = Level.Error;
                    break;
                case string s when s == Level.Fatal.DisplayName:
                    lvl = Level.Fatal;
                    break;
                case string s when s == Level.Off.DisplayName:
                    lvl = Level.Off;
                    break;
            }

            hierarchy.Root.Level = lvl;
            hierarchy.Configured = true;
        }
        public static log4net.ILog GetLogger(System.Type fileName)
        {
            var log = log4net.LogManager.GetLogger(fileName);
            return log;
        }
    }

}
