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

        public ProjectTester(string csprojPath)
        {
            this.csprojPath = csprojPath;
            this.projectPath = Path.GetDirectoryName(csprojPath);
        }

        public TestResult ValidateTest(TestType testType, string candidate)
        {
            if (testType == TestType.ChangeDate)
            {
                var testResult = ValidateChangeDateSignature(candidate);

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

        private TestResult ValidateChangeDateSignature(string candidate)
        {
            var testResult = new TestResult();

            var desiredClassName = "CWIDateTime";
            var desiredNamespace = new CultureInfo("en-US", false).TextInfo.ToTitleCase(candidate).Replace(" ", "");
            var candidatesAssembly = Assembly.LoadFrom("../../../TechnicalTests.ChangeDate.Candidates/bin/Debug/TechnicalTests.Candidates.dll");

            var candidateFile = candidatesAssembly
                                .GetTypes()
                                .FirstOrDefault(t => t.IsClass &&
                                                     t.Name == desiredClassName &&
                                                     t.Namespace == desiredNamespace);

            if (candidateFile == null)
            {
                testResult.Error = $@"Arquivo inválido. O namespace desejado era {desiredNamespace}.{desiredClassName}. Onde '{desiredNamespace}' é nomaspace da classe e '{desiredClassName}' é o nome da classe.";
                return testResult;
            }

            var assinaturaDesejada = candidateFile.GetMethods().FirstOrDefault(x =>
                                            x.Name == "ChangeDate" &&
                                            x.IsPublic);

            if (assinaturaDesejada == null)
            {
                testResult.Error = "Não foi encontrado no arquivo um método público chamado 'ChangeDate'.";
                return testResult;
            }

            var parametros = assinaturaDesejada.GetParameters();

            if (parametros.Count() != 3 &&
                assinaturaDesejada.GetParameters().ElementAt(0).ParameterType.FullName != "System.String" &&
                assinaturaDesejada.GetParameters().ElementAt(1).ParameterType.FullName != "System.Char" &&
                assinaturaDesejada.GetParameters().ElementAt(2).ParameterType.FullName != "System.Int64" &&
                assinaturaDesejada.ReturnType.FullName != "System.String")
            {
                testResult.Error = @"Assinatura inválida. A assinatura esperada é 'public string ChangeDate(string date, char operation, long value)'. Verifique se o retorno do método 'ChangeDate é do tipo System.String, se ele possui exatamente 3 parâmetros e se os tipos dos parâmetros são 'System.String, System.Char e System.Int64' (deve obedecer essa ordem).";
                return testResult;
            }

            return testResult;
        }

        private TestResult ValidateChangeDateBasicTest()
        {
            var cmd = new Process();
            cmd.StartInfo.FileName = ConfigurationManager.AppSettings["msTestPath"];
            cmd.StartInfo.Arguments = $@"/testcontainer:{projectPath}\bin\Debug\TechnicalTests.ChangeDate.Test.dll";    
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.Start();

            string output = cmd.StandardOutput.ReadToEnd();

            var res = Regex.Matches(output, "((Failed)|(Passed))                CWI.TechnicalRecruiting.TechnicalTests.ChangeDateUnitTest");

            cmd.WaitForExit();
            var exitCode = cmd.ExitCode;
            cmd.Close();

            if (exitCode == 0)
                throw new Exception("Ocorreu um erro no momento de executar os testes do candidato. Detalhes do erro: \n" + output);

            var testResult = new TestResult();

            //Pega apenas as linhas de resultado de teste 
            var regexPattern = "(?<status>(Failed)|(Passed))                CWI.TechnicalRecruiting.TechnicalTests.ChangeDateUnitTest.(?<teste>.*([\r]|[\n]))";
            var matches = Regex.Matches(output, regexPattern);
            foreach (Match match in matches)
            {                
                var passou = match.Groups["status"].Value == "Passed";
                var nomeDoTeste = match.Groups["teste"].Value;

                testResult.TestsPassed.Add(nomeDoTeste, passou);
            }

            var falhouNoTesteBasico = !testResult.TestsPassed["BasicTest"];
            if (falhouNoTesteBasico)
                testResult.Error = "O projeto falhou no teste 'ChangeDate(\"01/03/2010 23:00\", '+', 4000)'. O resultado esperado era: \"04/03/2010 17:40\".";

            return testResult;
        }
    }
}
