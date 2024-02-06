using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSorter.Helper
{
    internal static class CultureHelper
    {
        public static void SetMachineCulture()
        {
            // Get culture of OS
            CultureInfo ci = CultureInfo.InstalledUICulture;
            //Culture for any thread
            CultureInfo.DefaultThreadCurrentCulture = ci;
            //Culture for UI in any thread
            CultureInfo.DefaultThreadCurrentUICulture = ci;
        }

        public static void SetCulture(string code)
        {
            // Get culture of OS
            CultureInfo ci = new CultureInfo(code);
            //Culture for any thread
            CultureInfo.DefaultThreadCurrentCulture = ci;
            //Culture for UI in any thread
            CultureInfo.DefaultThreadCurrentUICulture = ci;
        }

        public static CultureInfo GetThreadCurrentCulture() { return CultureInfo.DefaultThreadCurrentCulture; }
        public static CultureInfo GetThreadCurrentUICulture() { return CultureInfo.DefaultThreadCurrentUICulture; }

    }
}
