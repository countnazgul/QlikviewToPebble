using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace QMSAPIStarter
{
    class ConsoleOptions
    {
        [Option('s', "server", HelpText = "Qlikview server address. For example: http://localhost:4799/QMS/Service")]
        public string Server { get; set; }

        [Option('p', "params", HelpText = @"Full files path and fields. For example: C:\ProgramData\QlikTech\Documents\PebbleData.qvw,PebbleData;C:\ProgramData\QlikTech\Documents\PebbleData1.qvw,PebbleData1;")]
        public string Params { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("Wearable QV", "0.1"),
                Copyright = new CopyrightInfo("stefan.stoichev@gmail.com", 2015),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };

            help.AddPreOptionsLine(@"Usage: wearable_qv.exe -s http://localhost:4799/QMS/Service -p ""C:\ProgramData\QlikTech\Documents\PebbleData.qvw,PebbleData;"" ");
            help.AddOptions(this);
            
            return help;
        }
    }
}