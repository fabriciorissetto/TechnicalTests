using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecnicalTests.Agent
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO: pegar esses campos virão da tela
            var candidateName = "Beltrano dos Santos";                        
            var candidateCode = File.ReadAllText(@"C:\Users\fabriciosilva\Desktop\CWIDateTime.cs");

            var candidatesProjectPath = ConfigurationManager.AppSettings["candidatesProjectPath"];
            var unitTestProjectPath = ConfigurationManager.AppSettings["unitTestProjectPath"];
            

            var candidatesProject = new ProjectEditor(candidatesProjectPath);            
            candidatesProject.IncludeFileInProject(candidateName, candidateCode, "CWIDateTime.cs");            

            var builder = new ProjectBuilder();
            var cadidatesProjectBuildResult = builder.Build(candidatesProjectPath);

            if (!cadidatesProjectBuildResult.Passed)
            {
                candidatesProject.RollbackEditions();
                //return cadidatesProjectBuildResult;
            }
            
            var tester = new ProjectTester(unitTestProjectPath);
            var testResult = tester.ValidateTest(TestType.ChangeDate, candidateName);

            return;
        }
    }
}
