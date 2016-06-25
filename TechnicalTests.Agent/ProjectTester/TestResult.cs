using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecnicalTests.Agent
{
    public class TestResult
    {
        public string Error { get; set; }
        public Dictionary<string, bool> TestsPassed { get; set; } = new Dictionary<string, bool>();

        public bool Success
        {
            get
            {
                return string.IsNullOrEmpty(Error);
            }
        }        
    }
}
