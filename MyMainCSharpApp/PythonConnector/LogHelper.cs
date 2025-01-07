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

namespace Muscle
{
    public class LogHelper
    {
        public static void Setup(string level)
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date{ABSOLUTE} [%logger] -%thread-  %level - %message%newline%exception";
            patternLayout.ActivateOptions();

            RollingFileAppender roller = new RollingFileAppender();
            roller.AppendToFile = true;
            roller.File = Path.Combine(AccessToAll.Main_Folder, @"Logs\LogFile.txt");
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
                case "Debug":
                    lvl = Level.Debug;
                    break;
                case "Info":
                    lvl = Level.Info;
                    break;
                case "Warn":
                    lvl = Level.Warn;
                    break;
                case "Error":
                    lvl = Level.Error;
                    break;
                case "Fatal":
                    lvl = Level.Fatal;
                    break;
                case "Off":
                    lvl = Level.Off;
                    break;
            }

            hierarchy.Root.Level = lvl;
            hierarchy.Configured = true;
        }
        public static log4net.ILog GetLogger(System.Type fileName)
        {
            var log = log4net.LogManager.GetLogger(fileName);
            //log4net.Config.XmlConfigurator.Configure();
            return log;
        }
    }

}
