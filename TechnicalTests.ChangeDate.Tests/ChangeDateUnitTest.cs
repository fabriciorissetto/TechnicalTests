using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace CWI.TechnicalRecruiting.TechnicalTests
{
    [TestClass]
    public class ChangeDateUnitTest
    {
        dynamic candidadeClass;

        public ChangeDateUnitTest()
        {
            var testAssembly = GetUnitTestAssembly();

            var candidateNamespace = "BeltranoDosSantos";
            candidadeClass = Activator.CreateInstance(testAssembly.GetType($"{candidateNamespace}.CWIDateTime"));
        }        

        public ChangeDateUnitTest(dynamic candidateClass)
        {
            candidadeClass = candidateClass;
        }

        private Assembly GetUnitTestAssembly()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var solutionFolder = "\\TechnicalTests\\"; 
            var directory = currentDirectory.Substring(0, currentDirectory.LastIndexOf(solutionFolder) + solutionFolder.Length);
            var assemblyPath = Path.Combine(directory, "TechnicalTests.ChangeDate.Tests/bin/Debug/TechnicalTests.Candidates.dll");
            var testAssembly = Assembly.LoadFrom(assemblyPath);
            return testAssembly;
        }

        [TestMethod]
        [TestCategory("Sum")]
        [TestCategory("Day")]
        //Esse teste é considerado o mínimo para aceitar o submit da prova do candidato via Web.
        //Pois ele é o exemplo de operação e resultado esperado que é enviado para ele por e-mail.
        public void BasicTest()
        {
            string d = candidadeClass.ChangeDate("01/03/2010 23:00", '+', 4000L);
            Assert.AreEqual("04/03/2010 17:40", d);
        }

        [TestMethod]
        [TestCategory("Nothing")]
        public void TestData2()
        {
            string d = candidadeClass.ChangeDate("31/12/2010 23:00", '+', 0L);
            Assert.AreEqual("31/12/2010 23:00", d);
        }

        [TestMethod]
        [TestCategory("Nothing")]
        [TestCategory("OutputFormat")]
        public void TestData3()
        {
            string d = candidadeClass.ChangeDate("31/03/2010 23:00", '+', 0L);
            Assert.AreEqual("31/03/2010 23:00", d);
        }

        [TestMethod]
        [TestCategory("Sum")]
        [TestCategory("Day")]
        public void TestData4()
        {
            string d = candidadeClass.ChangeDate("01/01/2011 23:00", '+', 4000L);
            Assert.AreEqual("04/01/2011 17:40", d);
        }

        [TestMethod]
        [TestCategory("Sum")]
        [TestCategory("Day")]
        public void TestData5()
        {
            string d = candidadeClass.ChangeDate("01/01/2011 23:00", '+', 4000L);
            Assert.AreEqual("04/01/2011 17:40", d);
        }
        
        [TestMethod]
        [TestCategory("Sum")]
        [TestCategory("Hour")]
        public void TestData6()
        {
            string d = candidadeClass.ChangeDate("01/01/1981 23:00", '+', 4000L);
            Assert.AreEqual("04/01/1981 17:40", d);
        }
    }
}
