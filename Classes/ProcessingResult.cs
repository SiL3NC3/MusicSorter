using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSorter.Classes
{
    internal class ProcessingResult
    {
        public List<Error> Errors { get; set; }
        public bool HasErrors { get { return Errors.Count > 0; } }

        public bool Canceled { get; private set; }
        internal void SetCanceled()
        {
            Canceled = true;
        }

        public ProcessingResult()
        {
            Errors = new List<Error>();
        }
    }
}
