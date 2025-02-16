﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TextControlBoxNS.Test
{
    internal abstract class TestCase
    {
        public abstract Func<bool>[] GetAllTests();

        public int failRate { get; set; }
        public int totalTests { get; set; }
        public abstract string name { get; set; }
        public TestResult Evaluate()
        {
            foreach(var function in GetAllTests())
            {
                bool res = function.Invoke();
                totalTests++;
                if (!res)
                    failRate++;

                Debug.WriteLine(" => " + (res ? "Success" : "Failed"));
                Debug.Assert(res);
            }

            if (failRate == 0)
                return TestResult.Success;
            else
                return TestResult.Failed;
        }
    }

    internal enum TestResult
    {
        Success,
        Failed
    }
}
