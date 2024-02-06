using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicSorter.Classes
{
    public static class ISynchronizeInvokeExtensions
    {
        public static void InvokeEx2<T>(this T @this, Action<T> action) where T : ISynchronizeInvoke
        {
            // USAGE: this.InvokeEx2(f => f.listView1.Items.Clear());

            if (@this.InvokeRequired)
            {
                @this.Invoke(action, new object[] { @this });
            }
            else
            {
                action(@this);
            }
        }
    }
    public static class ControlExtensions
    {
        public static TResult InvokeEx<TControl, TResult>(this TControl control,
                                                   Func<TControl, TResult> func)
          where TControl : Control
        {
            return control.InvokeRequired
                    ? (TResult)control.Invoke(func, control)
                    : func(control);
        }

        public static void InvokeEx<TControl>(this TControl control,
                                              Action<TControl> func)
          where TControl : Control
        {
            control.InvokeEx(c => { func(c); return c; });
        }

        public static void InvokeEx<TControl>(this TControl control, Action action)
          where TControl : Control
        {
            control.InvokeEx(c => action());
        }
        /// <summary>
        /// Sets the progress bar value, without using 'Windows Aero' animation.
        /// This is to work around a known WinForms issue where the progress bar 
        /// is slow to update. 
        /// </summary>
        public static void SetProgressNoAnimation(this ProgressBar pb, int value)
        {
            // To get around the progressive animation, we need to move the 
            // progress bar backwards.
            if (value == pb.Maximum)
            {
                // Special case as value can't be set greater than Maximum.
                pb.Maximum = value + 1;     // Temporarily Increase Maximum
                pb.Value = value + 1;       // Move past
                pb.Maximum = value;         // Reset maximum
            }
            else
            {
                pb.Value = value + 1;       // Move past
            }
            pb.Value = value;               // Move to correct value
        }
        public static IEnumerable<string> FilterFiles(string path, params string[] exts)
        {
            return
                exts.Select(x => "*." + x) // turn into globs
                .SelectMany(x =>
                    Directory.EnumerateFiles(path, x)
                    );
        }
    }
}
