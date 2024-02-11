using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;
using MS.WindowsAPICodePack.Internal;
using MusicSorter.Helper;
using SVGImage.SVG;

namespace MusicSorter.Classes
{
    internal class Structure
    {
        private static readonly Random _random = new Random();

        internal Settings Settings { get; private set; }
        internal List<DirectoryInfo> Folders { get; private set; }
        internal List<FileInfo> Files { get; private set; }

        public Structure()
        {
            Folders = new List<DirectoryInfo>();
            Files = new List<FileInfo>();
        }

        internal void Clear()
        {
            Files.Clear();
            Folders.Clear();
        }
        internal string GetBasePath()
        {
            return Settings.Path;
        }
        internal async Task Load(Settings settings)
        {
            Settings = settings;

            if (!Directory.Exists(Settings.Path))
            {
                throw new DirectoryNotFoundException(Settings.Path);
            }

            Clear();

            Task task = new Task(() =>
            {
                var baseDir = new DirectoryInfo(Settings.Path);
                DirectoryInfo[] folders = null;

                Folders.Add(baseDir);
                Files.AddRange(baseDir.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly));

                if (Settings.Subfolders)
                {
                    folders = baseDir.GetDirectories().ToArray();

                    foreach (var dir in folders)
                    {
                        if (!dir.Attributes.HasFlag(FileAttributes.Hidden))
                        {
                            Folders.Add(dir);
                            var files = dir.EnumerateFiles("*.*", SearchOption.AllDirectories);

                            if (files.Count() > 0)
                            {
                                foreach (var file in files)
                                {
                                    if (!Files.Exists(f => f.FullName == file.FullName))
                                    {
                                        Files.Add(file);
                                    }
                                }
                                // Files.AddRange(files);


                                var subfolders = FileSystemHelper.GetDirectories(dir.FullName);

                                foreach (var subfolder in subfolders)
                                {
                                    var subdir = new DirectoryInfo(subfolder);
                                    if (!subdir.Attributes.HasFlag(FileAttributes.Hidden))
                                    {
                                        Folders.Add(subdir);
                                        var subfiles = subdir.EnumerateFiles("*.*", SearchOption.AllDirectories);
                                        foreach (var file in subfiles)
                                        {
                                            if (!Files.Exists(f => f.FullName == file.FullName))
                                            {
                                                Files.Add(file);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
            task.Start();
            await task;
            return;
        }
        internal async Task<ProcessingResult> Process()
        {
#if (DEBUG)
            if (Settings.Simulate)
            {
                return await Simulate();
            }
#endif
            var result = new ProcessingResult();
            int filesCount = 0;
            string folder = null;
            int foldersCount = 0;
            List<FileInfo> files = null;
            List<DirectoryInfo> folders = null;

            DriveInfo driveInfo = null;
            string tmpFolder = null;
            string tempFile = null;
            StringBuilder cmd = new StringBuilder();
            string cmdFile = null;
            string msg = null;

            var task = new Task(() =>
            {
                switch (Settings.SortingOrder)
                {
                    case SortingOrder.Ascending:
                        folders = Folders.OrderBy(a => a.FullName).ToList();
                        break;
                    case SortingOrder.Descending:
                        folders = Folders.OrderByDescending(a => a.FullName).ToList();
                        break;
                    case SortingOrder.Random:
                        folders = Folders.OrderBy(a => _random.Next()).ToList();
                        break;
                }

                foreach (var dir in folders)
                {
                    foldersCount++;
                    folder = dir.FullName.EndsWith("\\") ? dir.FullName : dir.FullName + "\\";

                    switch (Settings.SortingOrder)
                    {
                        case SortingOrder.Ascending:
                            files = Files.Where(f => f.FullName == (folder + f.Name)).OrderBy(a => a.Name).ToList();
                            break;
                        case SortingOrder.Descending:
                            files = Files.Where(f => f.FullName == (folder + f.Name)).OrderByDescending(a => a.Name).ToList();
                            break;
                        case SortingOrder.Random:
                            files = Files.Where(f => f.FullName == (folder + f.Name)).OrderBy(a => _random.Next()).ToList();
                            break;
                    }
                    Console.WriteLine($"DIR: {dir} ({files.Count})");

                    // Move to ~tmp
                    cmd.Clear();

                    foreach (var file in files)
                    {
                        if (driveInfo == null)
                        {
                            driveInfo = FileSystemHelper.GetDrive(file.FullName);
                            tmpFolder = driveInfo.Name + "~MusicSorter~TMP\\";

                            if (!Directory.Exists(tmpFolder))
                                Directory.CreateDirectory(tmpFolder);
                        }

                        if (file.IsReadOnly)
                        {
                            try { File.SetAttributes(file.FullName, FileAttributes.Normal); }
                            catch (Exception) { }
                        }
                        try
                        {
                            tempFile = tmpFolder + file.Name;
                            File.Move(file.FullName, tempFile);

                            // Prepare undo batch file
                            cmd.Append($"mv {file.Name} {file.DirectoryName}{Environment.NewLine}");
                        }

                        catch (Exception ex)
                        {
                            result.Errors.Add(new Error(ex));
                            SimpleLogger.Instance.Error(ex.Message);
                        }
                    }

                    // After copying files to temp,
                    cmdFile = $"{tmpFolder}~undo.bat";

                    File.WriteAllText(cmdFile, cmd.ToString());
                    Console.WriteLine("All folder files moved to tmp-folder.");

                    foreach (var file in files)
                    {
                        try
                        {
                            filesCount++;

                            tempFile = tmpFolder + file.Name;
                            File.Move(tempFile, file.FullName);
                            Console.WriteLine($" - File: {file} moved.");
                            if (File.Exists(cmdFile))
                                File.Delete(cmdFile);
                            Thread.Sleep(8);
                            msg = Properties.Resources.MessageProcessing.Replace("{foldersCount}", foldersCount.ToString()).Replace("{filesCount}", filesCount.ToString());
                            OnProgressChanged(new ProcessingEventArgs(msg, filesCount, file, dir));
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(new Error(ex));
                            SimpleLogger.Instance.Error(ex.Message);
                        }
                    }
                }

                OnProgressChanged(new ProcessingEventArgs(Properties.Resources.MessageSortingFolders));

                Thread.Sleep(10);

                // SORT FOLDERS
                if (Settings.SortFolders)
                {
                    Console.WriteLine("Sorting Folders...");

                    DateTime now = DateTime.Now;
                    int count = folders.Count;

                    foreach (var dir in folders)
                    {
                        var date = now.AddMinutes(-count);
                        // Check for root path avoid recreation
                        if (!dir.FullName.Equals(driveInfo.Name))
                        {
                            try
                            {
                                // Recreate folder when not empty
                                var tempTarget = tmpFolder + dir.Name;
                                Directory.Move(dir.FullName, tempTarget);
                                Directory.Move(tempTarget, dir.FullName);

                                //Directory.CreateDirectory(dir.FullName);

                                //var tmpFiles = dir.EnumerateFiles();
                                //foreach (var file in tmpFiles)
                                //{
                                //    string destFile = Path.Combine(dir.FullName, Path.GetFileName(file.FullName));

                                //    if (!File.Exists(destFile))
                                //        File.Move(file.FullName, destFile);
                                //}

                                Console.WriteLine(dir + " recreated (sorted).");
                            }
                            catch (Exception ex)
                            {
                                result.Errors.Add(new Error(ex));
                                SimpleLogger.Instance.Error(ex.Message);
                            }
                        }
                        count--;
                    }
                }

                // Finally delete tmpfolder
                Directory.Delete(tmpFolder, true);

            });
            task.Start();
            await task;
            Thread.Sleep(10);
            return result;
        }
        private void SortFolders()
        {
        }
        private async Task<ProcessingResult> Simulate()
        {
            var result = new ProcessingResult();
            int filesCount = 0;
            string folder = null;
            int foldersCount = 0;
            List<FileInfo> files = null;
            List<DirectoryInfo> folders = null;
            string msg = null;

            var task = new Task(() =>
            {
                try
                {
                    switch (Settings.SortingOrder)
                    {
                        case SortingOrder.Ascending:
                            folders = Folders.OrderBy(a => a.Name).ToList();
                            break;
                        case SortingOrder.Descending:
                            folders = Folders.OrderByDescending(a => a.Name).ToList();
                            break;
                        case SortingOrder.Random:
                            folders = Folders.OrderBy(a => _random.Next()).ToList();
                            break;
                    }
                    foreach (var dir in folders)
                    {
                        foldersCount++;
                        folder = dir.FullName.EndsWith("\\") ? dir.FullName : dir.FullName + "\\";

                        switch (Settings.SortingOrder)
                        {
                            case SortingOrder.Ascending:
                                files = Files.Where(f => f.FullName == (folder + f.Name)).OrderBy(a => a.Name).ToList();
                                break;
                            case SortingOrder.Descending:
                                files = Files.Where(f => f.FullName == (folder + f.Name)).OrderByDescending(a => a.Name).ToList();
                                break;
                            case SortingOrder.Random:
                                files = Files.Where(f => f.FullName == (folder + f.Name)).OrderBy(a => _random.Next()).ToList();
                                break;
                        }
                        Console.WriteLine($"DIR: {dir} ({files.Count})");

                        foreach (var file in files)
                        {
                            filesCount++;

                            Console.WriteLine($"Simulating file: {file}");
                            Thread.Sleep(8);
                            msg = Properties.Resources.MessageSimulating.Replace("{foldersCount}", foldersCount.ToString()).Replace("{filesCount}", filesCount.ToString());
                            OnProgressChanged(new ProcessingEventArgs(msg, filesCount, file, dir));
                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }


            });
            task.Start();
            await task;
            Thread.Sleep(10);
            return result;
        }

        // EVENTS
        public event EventHandler<ProcessingEventArgs> ProgressChanged;
        private void OnProgressChanged(ProcessingEventArgs progress)
        {
            ProgressChanged?.Invoke(this, progress);
        }
    }
}
