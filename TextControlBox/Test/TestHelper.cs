using System.Collections.Generic;
using System.Diagnostics;
using TextControlBoxNS.Core;

namespace TextControlBoxNS.Test
{
    internal class TestHelper
    {
        public CoreTextControlBox coreTextbox;

        List<TestCase> TestCases;

        public TestHelper(CoreTextControlBox coreTextbox)
        {
            this.coreTextbox = coreTextbox;
            TestCases = new List<TestCase>{ new HelperTest(coreTextbox), new TextTests("Text Tests", coreTextbox) };
        }

        public async void Evaluate()
        {
            foreach (var test in TestCases)
            {
                Debug.WriteLine("\n");
                Debug.WriteLine(test.name);
                await test.Evaluate();
            }
        }

    }
}
