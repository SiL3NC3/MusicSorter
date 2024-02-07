using System.IO;

namespace MusicSorter.Classes
{
    public class ProcessingEventArgs
    {
        internal string Message;
        internal int Progress;
        internal readonly FileInfo File;
        internal readonly DirectoryInfo Directory;

        public ProcessingEventArgs(string msg, int progress, FileInfo file, DirectoryInfo directory)
        {
            Message = msg;
            Progress = progress;
            File = file;
            Directory = directory;
        }
    }
}
