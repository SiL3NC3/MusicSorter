using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MusicSorter.Helper;
using static System.Net.WebRequestMethods;

namespace MusicSorter.Classes
{
    internal class Structure
    {
        private static readonly Random _random = new Random();
        private readonly Settings _settings;

        internal List<DirectoryInfo> Folders { get; set; }
        internal List<FileInfo> Files { get; set; }

        public Structure(Settings settings)
        {
            Folders = new List<DirectoryInfo>();
            Files = new List<FileInfo>();
            _settings = settings;
        }

        internal string GetBasePath()
        {
            return _settings.Path;
        }


        internal async Task Load()
        {
            if (!Directory.Exists(_settings.Path))
            {
                throw new DirectoryNotFoundException(_settings.Path);
            }

            Files.Clear();

            Task task = new Task(() =>
            {
                var baseDir = new DirectoryInfo(_settings.Path);
                DirectoryInfo[] folders = null;

                Folders.Add(baseDir);
                Files.AddRange(baseDir.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly));

                if (_settings.Subfolders)
                {
                    //var folders = FileSystemHelper.GetDirectories(_basePath);
                    switch (_settings.SortingOrder)
                    {
                        case SortingOrder.Ascending:
                            folders = baseDir.GetDirectories().OrderBy(a => a.Name).ToArray();
                            break;
                        case SortingOrder.Descending:
                            folders = baseDir.GetDirectories().OrderByDescending(a => a.Name).ToArray();
                            break;
                        case SortingOrder.Random:
                            folders = baseDir.GetDirectories().OrderBy(a => _random.Next()).ToArray();
                            break;
                    }

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

        //private static int GetDirRecurseEnumerationOptions(string path, EnumerationOptions options, ref int counter)
        //{
        //    try
        //    {
        //        foreach (string dir in Directory.EnumerateDirectories(path, @"*", options))
        //        {
        //            // Console.WriteLine(dir);
        //            counter++;
        //            GetDirRecurseEnumerationOptions(dir, options, ref counter);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        // Console.WriteLine(e.Message);
        //    }

        //    return counter;
        //}

        //private static int GetDirRecurseSearchOption(string path, ref int counter)
        //{
        //    try
        //    {
        //        foreach (string dir in Directory.EnumerateDirectories(path, @"*", SearchOption.TopDirectoryOnly))
        //        {
        //            // Console.WriteLine(dir);
        //            counter++;
        //            GetDirRecurseSearchOption(dir, ref counter);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        // Console.WriteLine(e.Message);
        //    }

        //    return counter;
        //}

        internal void Clear()
        {
            Folders = null;
        }

        public bool Loaded
        {
            get { return Files.Count > 0 ? true : false; }
        }
    }
}
