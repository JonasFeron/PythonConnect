//Copyright < 2021 - 2025 > < ITAO, Université catholique de Louvain (UCLouvain)>

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

//List of the contributors to the development of PythonConnect: see NOTICE file.
//Description and complete License: see NOTICE file.

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
using System.Threading;

namespace PythonConnect
{
    public class LogHelper
    {
        public static void Setup(string level, string tempDirectory)
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            //patternLayout.ConversionPattern = "%date{ABSOLUTE} [%logger] -%thread-  %level - %message%newline%exception"; //old version
            patternLayout.ConversionPattern = "%12.12date{ABSOLUTE} -%2.2thread -%5.5level- %15.15logger.%20.20M(): %message %newline";

            

            patternLayout.ActivateOptions();

            RollingFileAppender roller = new RollingFileAppender();
            roller.AppendToFile = true;
            roller.File = Path.Combine(tempDirectory, @"LogFile.txt");
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
