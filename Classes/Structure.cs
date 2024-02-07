using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;
using MS.WindowsAPICodePack.Internal;
using MusicSorter.Helper;
using SVGImage.SVG;
using static System.Net.WebRequestMethods;

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
                                Files.AddRange(files);

                                foreach (var d in files)
                                {
                                    Console.WriteLine(d.FullName);
                                }
                                Console.WriteLine(files.Count() + " items");

                                var subfolders = FileSystemHelper.GetDirectories(dir.FullName);

                                foreach (var subfolder in subfolders)
                                {
                                    var subdir = new DirectoryInfo(subfolder);
                                    if (!subdir.Attributes.HasFlag(FileAttributes.Hidden))
                                    {
                                        Folders.Add(subdir);
                                        var subfiles = subdir.EnumerateFiles("*.*", SearchOption.AllDirectories);
                                        Files.AddRange(subfiles);
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
            if (Settings.Simulate)
            {
                return await Simulate();
            }

            var result = new ProcessingResult();
            int filesCount = 0;
            string folder = null;
            int foldersCount = 0;
            List<FileInfo> files = null;
            List<DirectoryInfo> folders = null;

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
                            Thread.Sleep(42);
                            OnProgressChanged(new ProcessingEventArgs($"Simulating Folder {foldersCount} > File {filesCount}...", filesCount, file, dir));
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
            return result;
        }
        private async Task<ProcessingResult> Simulate()
        {
            var result = new ProcessingResult();
            int filesCount = 0;
            string folder = null;
            int foldersCount = 0;
            List<FileInfo> files = null;
            List<DirectoryInfo> folders = null;

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
                            OnProgressChanged(new ProcessingEventArgs($"Simulating Folder {foldersCount} > File {filesCount}...", filesCount, file, dir));
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
            Thread.Sleep(100);
            return result;
        }

        [Obsolete]
        public bool Loaded
        {
            get { return Files.Count > 0 ? true : false; }
        }

        // EVENTS
        public event EventHandler<ProcessingEventArgs> ProgressChanged;
        private void OnProgressChanged(ProcessingEventArgs progress)
        {
            ProgressChanged?.Invoke(this, progress);
        }
    }
}
