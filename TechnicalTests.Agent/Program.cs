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
            // Esses campos virão da tela
            var candidateName = "Beltrano dos Santos";
            var candidateCode = File.ReadAllText(@"C:\Users\fabriciosilva\Desktop\CWIDateTime3.cs");

            var result = BuildCandidateCode(candidateName, candidateCode);

            Console.ReadKey();
        }

        private static OverallResult BuildCandidateCode(string candidateName, string candidateCode)
        {
            var candidatesProjectPath = ConfigurationManager.AppSettings["candidatesProjectPath"];
            var unitTestProjectPath = ConfigurationManager.AppSettings["unitTestProjectPath"];

            var candidatesProject = new ProjectEditor(candidatesProjectPath);
            candidatesProject.IncludeFileInProject(candidateName, candidateCode, "CWIDateTime.cs");

            var builder = new ProjectBuilder();
            var cadidatesProjectBuildResult = builder.Build(candidatesProjectPath);


            if (!cadidatesProjectBuildResult.Passed)
            {
                candidatesProject.RollbackEditions();
                return new OverallResult() { Error = "Não foi possível compilar sua classe. Por favor, verifique se ela possui erros de sintaxe. \nDetalhes do erro:\n" + cadidatesProjectBuildResult.Log };
            }

            var tester = new ProjectTester(unitTestProjectPath, candidateName);
            var overallTestsResult = tester.ValidateTests(TestType.ChangeDate);

            return overallTestsResult;
        }
    }
}
