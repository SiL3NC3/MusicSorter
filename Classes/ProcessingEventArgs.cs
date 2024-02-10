using System.IO;

namespace MusicSorter.Classes
{
    public class ProcessingEventArgs
    {
        internal string Message;
        internal int Progress;
        internal readonly FileInfo File;
        internal readonly DirectoryInfo Directory;

        public ProcessingEventArgs(string msg, int progress = -1, FileInfo file = null, DirectoryInfo directory = null)
        {
            Message = msg;
            Progress = progress;
            File = file;
            Directory = directory;
        }
    }
}
