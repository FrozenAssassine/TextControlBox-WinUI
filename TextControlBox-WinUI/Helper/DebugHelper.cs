using Collections.Pooled;
using System.Diagnostics;

namespace TextControlBox.Helper
{
    internal class DebugHelper
    {
        public static void DebugList(PooledList<string> List, string DebugTitle = "--DebugList--")
        {
            Debug.WriteLine(DebugTitle);
            if (List == null)
            {
                Debug.WriteLine("\tCan't debug List because it is null");
                return;
            }
            for (int i = 0; i < List.Count; i++)
            {
                Debug.WriteLine("\t" + List[i]);
            }
        }
    }
}
