using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TecnicalTests.Agent
{
    public class ProjectTester
    {
        private string csprojPath;
        private string projectPath;
        private string candidateNamespace;

        public ProjectTester(string csprojPath, string candidate)
        {
            this.csprojPath = csprojPath;
            this.projectPath = Path.GetDirectoryName(csprojPath);
            this.candidateNamespace = GetCandidateNamespace(candidate);
        }

        public OverallResult ValidateTests(TestType testType)
        {
            if (testType == TestType.ChangeDate)
            {
                ChangeUnitTestProjectToPointToCandidadeClass();

                var testResult = ValidateChangeDateSignature();
                if (testResult.Success)
                {
                    return ValidateChangeDateBasicTest();                    
                }
                else
                {
                    return testResult;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void ChangeUnitTestProjectToPointToCandidadeClass()
        {
            //Changes "ChangeDateUnitTest.cs" to point to the candidate class
            var unitTestFilePath = Path.Combine(projectPath, "ChangeDateUnitTest.cs");
            var unitTestFile = File.ReadAllText(unitTestFilePath);            
            var newContentUnitTestFile = Regex.Replace(unitTestFile, "(var candidateNamespace = \")(.*)(\";)", m => String.Format("{0}{1}{2}", m.Groups[1], candidateNamespace, m.Groups[3]));            
            File.WriteAllText(unitTestFilePath, newContentUnitTestFile);
            
            //Rebuild the unit test project 
            var builder = new ProjectBuilder();
            var buildResult = builder.Build(csprojPath);

            //If build not pass rollback the changes (rewrites the old version of ChangeDateUnitTest.cs)
            if (!buildResult.Passed)
            {
                File.WriteAllText(unitTestFilePath, unitTestFile);
                throw new Exception("Ocorreu um erro no build do projeto de teste após mudar o apontamento para a versão do candidato.");
            }
        }

        private OverallResult ValidateChangeDateSignature()
        {
            var testResult = new OverallResult();

            var desiredClassName = "CWIDateTime";            
            var candidatesAssembly = Assembly.LoadFrom("../../../TechnicalTests.ChangeDate.Candidates/bin/Debug/TechnicalTests.Candidates.dll");

            var candidateFile = candidatesAssembly
                                .GetTypes()
                                .FirstOrDefault(t => t.IsClass &&
                                                     t.Name == desiredClassName &&
                                                     t.Namespace == candidateNamespace);

            if (candidateFile == null)
            {
                testResult.Error = $@"Arquivo inválido. O namespace desejado era {candidateNamespace}.{desiredClassName}. Onde '{candidateNamespace}' é nomaspace da classe e '{desiredClassName}' é o nome da classe.";
                return testResult;
            }

            var desiredSignature = candidateFile.GetMethods().FirstOrDefault(x =>
                                            x.Name == "ChangeDate" &&
                                            x.IsPublic);

            if (desiredSignature == null)
            {
                testResult.Error = "Não foi encontrado no arquivo um método público chamado 'ChangeDate'.";
                return testResult;
            }

            var parameters = desiredSignature.GetParameters();

            if (parameters.Count() != 3 &&
                desiredSignature.GetParameters().ElementAt(0).ParameterType.FullName != "System.String" &&
                desiredSignature.GetParameters().ElementAt(1).ParameterType.FullName != "System.Char" &&
                desiredSignature.GetParameters().ElementAt(2).ParameterType.FullName != "System.Int64" &&
                desiredSignature.ReturnType.FullName != "System.String")
            {
                testResult.Error = @"Assinatura inválida. A assinatura esperada é 'public string ChangeDate(string date, char operation, long value)'. Verifique se o retorno do método 'ChangeDate é do tipo System.String, se ele possui exatamente 3 parâmetros e se os tipos dos parâmetros são 'System.String, System.Char e System.Int64' (deve obedecer essa ordem).";
                return testResult;
            }

            return testResult;
        }

        private string GetCandidateNamespace(string candidate)
        {
            //Turns "jhon doe" into "Jhon Doe"
            var titleCase = new CultureInfo("en-US", false).TextInfo.ToTitleCase(candidate);
            var userNamespace = titleCase.Replace(" ", "");

            return userNamespace;
        }

        private OverallResult ValidateChangeDateBasicTest()
        {
            var cmd = new Process();
            cmd.StartInfo.FileName = ConfigurationManager.AppSettings["msTestPath"];
            cmd.StartInfo.Arguments = $@"/testcontainer:{projectPath}\bin\Debug\TechnicalTests.ChangeDate.Test.dll";    
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.Start();

            string output = cmd.StandardOutput.ReadToEnd();          
            cmd.WaitForExit();
            var exitCode = cmd.ExitCode;
            cmd.Close();

            if (exitCode == 0)
                throw new Exception("Ocorreu um erro no momento de executar os testes do candidato. Detalhes do erro: \n" + output);

            var testResult = new OverallResult();

            //Takes just the lines about tests that have passed or failed
            var regexPattern = "(?<status>Failed|Passed) {1,}CWI.TechnicalRecruiting.TechnicalTests.ChangeDateUnitTest.(?<teste>[a-zA-Z0-9]*)";
            var matches = Regex.Matches(output, regexPattern);
            foreach (Match match in matches)
            {                
                var passed = match.Groups["status"].Value == "Passed";
                var testName = match.Groups["teste"].Value;

                testResult.TestsPassed.Add(testName, passed);
            }

            var passedInTheBasicTest = testResult.TestsPassed["BasicTest"];
            if (!passedInTheBasicTest)
                testResult.Error = "O projeto falhou no teste 'ChangeDate(\"01/03/2010 23:00\", '+', 4000)'. O resultado esperado era: \"04/03/2010 17:40\".";

            return testResult;
        }
    }
}
