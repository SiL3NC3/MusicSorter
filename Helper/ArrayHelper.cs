using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSorter.Helper
{
    public static class ArrayHelper
    {
        public static string[] RemoveDuplicates(string[] s, bool removeNullOrEmpty = false, bool trimStrings = true)
        {
            if (removeNullOrEmpty && trimStrings)
                return s.Distinct().Where(i => !String.IsNullOrEmpty(i)).Select(i => i.Trim()).ToArray();
            else if (removeNullOrEmpty && !trimStrings)
                return s.Distinct().Where(i => !String.IsNullOrEmpty(i)).ToArray();
            else if (!removeNullOrEmpty && trimStrings)
                return s.Distinct().Select(i => i.Trim()).ToArray();
            else
                return s.Distinct().ToArray();
        }
    }
}
