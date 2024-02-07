using MS.WindowsAPICodePack.Internal;
using MusicSorter.Classes;
using MusicSorter.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicSorter
{
    internal class ProcessingOLD
    {
        //private async void ProcessFiles()
        //{
        //    GetSettings();

        //    result = new ProcessingResult();

        //    if (!CheckBoxSimulate.Checked)
        //    {
        //        var drive = FileSystemHelper.GetDrive(_structure.GetBasePath());

        //        if (MessageBoxEx.Show(this,
        //            "WARNING!" + nl + nl +
        //            "Proceeding will make unrecoverable changes to the filesystem in the selected folder." + nl + nl +
        //            "Target drive " + $"{drive.Name} ({drive.VolumeLabel})" + nl + nl +
        //            "Do you want to continue?",
        //            "Please confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
        //        {
        //            return;
        //        }
        //    }

        //    this.Cursor = Cursors.WaitCursor;
        //    SetGUI(Actions.Processing, false);

        //    try
        //    {
        //        ProgressBarStatus.Value = 0;
        //        ProgressBarStatus.Maximum = _structure.Files.Count;

        //        switch (_settings.StampingOption)
        //        {
        //            case StampingOptions.FilesEachFolder:
        //                processingTask = new Task<ProcessingResult>(StampFiles_EachFolder);
        //                processingTask.Start();

        //                LabelStatus.Text = "Processing...";
        //                result = await processingTask;
        //                LabelStatus.Text = "Finished.";

        //                break;
        //            case StampingOptions.OverallFiles:
        //                processingTask = new Task<ProcessingResult>(StampFiles_OverallFiles);
        //                processingTask.Start();

        //                LabelStatus.Text = "Processing...";
        //                result = await processingTask;
        //                LabelStatus.Text = "Finished.";

        //                break;
        //        }
        //        //resultTask = new Task(ProcessResult);
        //        //resultTask.Start();
        //        //await resultTask;

        //        ProcessResult();
        //    }
        //    catch (Exception ex)
        //    {
        //        ProcessError(ex);
        //    }
        //    finally
        //    {
        //        SaveSettings();
        //        Reset();
        //        SetGUI(Actions.Processing, true);
        //    }
        //}
        //private ProcessingResult StampFiles_EachFolder()
        //{
        //    ProcessingResult result = new ProcessingResult();
        //    DateTime time = DateTime.Now; // DateTime.Now.AddMinutes(-_structure.Files.Count());
        //    List<FileInfo> files = null;
        //    int foldersCount = 0;
        //    string folder = null;

        //    foreach (var dir in _structure.Folders)
        //    {
        //        try
        //        {
        //            if (stopProcessing)
        //            {
        //                result.SetCanceled();
        //                break;
        //            }

        //            foldersCount++;
        //            folder = dir.FullName.EndsWith("\\") ? dir.FullName : dir.FullName + "\\";

        //            switch (_settings.SortOrder)
        //            {
        //                case SortOrder.Ascending:
        //                    files = _structure.Files.Where(f => f.FullName == (folder + f.Name)).OrderBy(a => a.Name).ToList();
        //                    break;
        //                case SortOrder.Descending:
        //                    files = _structure.Files.Where(f => f.FullName == (folder + f.Name)).OrderByDescending(a => a.Name).ToList();
        //                    break;
        //                case SortOrder.Random:
        //                    files = _structure.Files.Where(f => f.FullName == (folder + f.Name)).OrderBy(a => _random.Next()).ToList();
        //                    break;
        //            }
        //            Console.WriteLine($"DIR: {dir} ({files.Count})");

        //            if (files.Count() > 0)
        //                result = ProcessFiles(StampingOptions.FilesEachFolder, files, foldersCount, folder);

        //            //foreach (var file in files)
        //            //{
        //            //    try
        //            //    {
        //            //        if (stopProcessing)
        //            //        {
        //            //            result.SetCanceled();
        //            //            break;
        //            //        }

        //            //        fileCount++;
        //            //        LabelStatus.InvokeEx(l => l.Text = $"Processing Folder {foldersCount} => File {fileCount}...");
        //            //        Console.WriteLine($"Processing Folder {foldersCount} => File {fileCount}...");
        //            //        Console.WriteLine($" - File: {file}, Time: {time}");

        //            //        time = time.AddMinutes(1);

        //            //        ProgressBarStatus.InvokeEx(f => f.SetProgressNoAnimation(f.Value += 1));

        //            //        ProcessFiles(file, time);
        //            //    }
        //            //    catch (Exception ex)
        //            //    {
        //            //        SimpleLogger.Instance.Error(ex);
        //            //        result.Errors.Add(new Error(ex));
        //            //    }

        //            //}
        //        }
        //        catch (Exception ex)
        //        {
        //            SimpleLogger.Instance.Error(ex);
        //        }
        //        finally { }
        //    }
        //    return result;
        //}
        //private ProcessingResult StampFiles_OverallFiles()
        //{
        //    ProcessingResult result = null;
        //    DateTime time = DateTime.Now; // DateTime.Now.AddMinutes(-_structure.Files.Count());
        //    List<FileInfo> files = null;

        //    switch (_settings.SortOrder)
        //    {
        //        case SortOrder.Ascending:
        //            files = _structure.Files.OrderBy(a => a.Name).ToList();
        //            break;
        //        case SortOrder.Descending:
        //            files = _structure.Files.OrderByDescending(a => a.Name).ToList();
        //            break;
        //        case SortOrder.Random:
        //            files = _structure.Files.OrderBy(a => _random.Next()).ToList();
        //            break;
        //    }
        //    DateTime.Now.AddMinutes(-files.Count());

        //    try
        //    {
        //        result = ProcessFiles(StampingOptions.OverallFiles, files);
        //        //foreach (var file in files)
        //        //{
        //        //    if (stopProcessing)
        //        //    {
        //        //        result.SetCanceled();
        //        //        break;
        //        //    }

        //        //    fileCount++;
        //        //    LabelStatus.InvokeEx(l => l.Text = $"Processing File {fileCount}...");
        //        //    Console.WriteLine($"Processing File {fileCount}...");
        //        //    Console.WriteLine($" - File: {file}, Time: {time}");

        //        //    time = time.AddMinutes(1);

        //        //    ProgressBarStatus.InvokeEx(f => f.SetProgressNoAnimation(f.Value += 1));

        //        //    ProcessFiles(file, time);
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        SimpleLogger.Instance.Error(ex);
        //        //error_list.Add(ex.ToString());
        //        //errors = true;
        //    }
        //    finally { }

        //    return result;
        //}
        //private ProcessingResult ProcessFiles(StampingOptions option, List<FileInfo> files, int foldersCount = 0, string folder = null)
        //{
        //    int fileCount = 0;
        //    DriveInfo driveInfo = null;
        //    string tmpFolder = null;
        //    ProcessingResult result = new ProcessingResult();
        //    string tempFile = null;
        //    bool dirNotRecreated = true;



        //    // Move to ~tmp
        //    foreach (var file in files)
        //    {
        //        if (stopProcessing)
        //        {
        //            result.SetCanceled();
        //            break;
        //        }

        //        if (driveInfo == null)
        //        {
        //            driveInfo = FileSystemHelper.GetDrive(file.FullName);
        //            tmpFolder = driveInfo.Name + "~tmp\\";
        //        }

        //        if (!CheckBoxSimulate.Checked)
        //        {
        //            if (!Directory.Exists(tmpFolder))
        //                Directory.CreateDirectory(tmpFolder);

        //            if (file.IsReadOnly)
        //            {
        //                try { File.SetAttributes(file.FullName, FileAttributes.Normal); }
        //                catch (Exception) { }
        //            }

        //            tempFile = tmpFolder + file.Name;
        //            File.Move(file.FullName, tempFile);
        //        }
        //    }
        //    // Recreate folder
        //    switch (option)
        //    {
        //        case StampingOptions.FilesEachFolder:
        //            break;
        //        case StampingOptions.OverallFiles:
        //            break;
        //    }

        //    // Move back to origin
        //    foreach (var file in files)
        //    {
        //        if (stopProcessing)
        //        {
        //            result.SetCanceled();
        //            break;
        //        }

        //        fileCount++;

        //        switch (option)
        //        {
        //            case StampingOptions.FilesEachFolder:
        //                LabelStatus.InvokeEx(l => l.Text = $"Processing Folder {foldersCount} => File {fileCount}...");
        //                //Console.WriteLine($"Processing Folder {foldersCount} => File {fileCount}...");
        //                break;
        //            case StampingOptions.OverallFiles:
        //                LabelStatus.InvokeEx(l => l.Text = $"Processing File {fileCount}...");
        //                //Console.WriteLine($"Processing File {fileCount}...");
        //                break;
        //        }
        //        ProgressBarStatus.InvokeEx(f => f.SetProgressNoAnimation(f.Value += 1));

        //        if (!CheckBoxSimulate.Checked)
        //        {
        //            if (CheckBoxSortFolders.Checked && dirNotRecreated)
        //            {
        //                var dir = file.DirectoryName;
        //                if (FileSystemHelper.IsDirectoryEmpty(dir))
        //                {
        //                    Directory.Delete(dir);
        //                    Directory.CreateDirectory(dir);
        //                    dirNotRecreated = false;
        //                    Console.WriteLine($"Folder '{dir}' recreated (Sorted)");
        //                }
        //                else
        //                {
        //                    Console.WriteLine($"WARNING! Folder '{dir}' cannot be recreated (Sorted), folder not empty!");
        //                }
        //            }

        //            tempFile = tmpFolder + file.Name;
        //            File.Move(tempFile, file.FullName);
        //            Console.WriteLine($" - File: {file} moved.");
        //        }
        //        else
        //            Thread.Sleep(100);
        //    }
        //    if (!CheckBoxSimulate.Checked)
        //    {
        //        Directory.Delete(tmpFolder);
        //    }
        //    return result;
        //}

    }
}
