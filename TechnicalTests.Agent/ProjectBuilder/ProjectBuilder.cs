using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TecnicalTests.Agent
{
    public class ProjectBuilder
    {
        public BuildResult Build(string projectPath)
        {
            var logFile = Path.Combine(Directory.GetCurrentDirectory(), "tempLog.txt");

            var projectCollection = new ProjectCollection();
            var globalProperty = new Dictionary<string, string>();
            globalProperty.Add("Configuration", "Debug");
            globalProperty.Add("Platform", "Any CPU");
            globalProperty.Add("OutputPath", @"bin\Debug\");

            var buildParameters = new BuildParameters(projectCollection);

            var fileLogger = new FileLogger() { Parameters = $"logfile={logFile}" };
            buildParameters.Loggers = new List<Microsoft.Build.Framework.ILogger> { fileLogger }.AsEnumerable();

            var buildRequest = new BuildRequestData(projectPath, globalProperty, "14.0", new string[] { "Build" }, null);

            Microsoft.Build.Execution.BuildResult result = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequest);
            var buildresult = new BuildResult();
            buildresult.Passed = result.OverallResult == BuildResultCode.Success;
            buildresult.Log = File.ReadAllText(logFile);

            return buildresult;
        }
    }
}
