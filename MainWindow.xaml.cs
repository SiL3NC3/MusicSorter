using Microsoft.WindowsAPICodePack.Dialogs;
using MS.WindowsAPICodePack.Internal;
using MusicSorter.Classes;
using MusicSorter.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MusicSorter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, System.Windows.Forms.IWin32Window
    {
        public IntPtr Handle
        {
            get { return new WindowInteropHelper(this).Handle; }
        }

        public bool Test { get; set; }

        private readonly string nl = Environment.NewLine;
        private static readonly Random _random = new Random();
        private Settings _settings;
        private Structure _structure;
        private bool _dropEnabled = true;
        private bool _processing = false;

        public MainWindow()
        {
            InitializeComponent();

            LabelStatus.Content = null;

            _settings = new Settings();
            _settings.Load();

            // Register ProgressChanged Event from Structure class
            _structure = new Structure();
            _structure.ProgressChanged += new EventHandler<ProcessingEventArgs>(Structure_ProgressChanged);

            SetState(States.Init);
        }

        private void SetSortOrder(SortingOrder order, bool save = false)
        {
            switch (order)
            {
                case Classes.SortingOrder.Ascending:
                    ButtonAscending.Background = System.Windows.Media.Brushes.SkyBlue;
                    ButtonDescending.Background = System.Windows.Media.Brushes.Transparent;
                    ButtonRandom.Background = System.Windows.Media.Brushes.Transparent;
                    break;
                case Classes.SortingOrder.Descending:
                    ButtonAscending.Background = System.Windows.Media.Brushes.Transparent;
                    ButtonDescending.Background = System.Windows.Media.Brushes.SkyBlue;
                    ButtonRandom.Background = System.Windows.Media.Brushes.Transparent;
                    break;
                case Classes.SortingOrder.Random:
                    ButtonAscending.Background = System.Windows.Media.Brushes.Transparent;
                    ButtonDescending.Background = System.Windows.Media.Brushes.Transparent;
                    ButtonRandom.Background = System.Windows.Media.Brushes.SkyBlue;
                    break;
            }
            if (save)
            {
                _settings.SortingOrder = order;
                _settings.Save();
            }
        }
        private void SetOptions(bool readFolder)
        {
            CheckBoxOptionSubfolders.IsChecked = _settings.Subfolders;
            CheckBoxOptionSortFolders.IsChecked = _settings.SortFolders;
            CheckBoxOptionSimulate.IsChecked = _settings.Simulate;

            if (readFolder && _settings.Path != null)
            {
                ReadFolder();
            }
        }
        // SetState
        private void SetState(States state)
        {
            Console.WriteLine($"SetState({state})");

            switch (state)
            {
                case States.Init:
                    _settings.ResetPath();
                    _structure.Clear();

                    SetStatus();
                    SetSortOrder(_settings.SortingOrder);
                    SetOptions(false);

                    ProgressBarStatus.Value = 0;
                    ProgressBarStatus.Maximum = 100;

                    GridInformation.Visibility = Visibility.Collapsed;
                    BorderDropZone.Visibility = Visibility.Visible;
                    BorderDropZone.Opacity = 1;

                    ButtonSelect.IsEnabled = true;
                    ButtonStart.IsEnabled = false;
#if (!DEBUG)
                    ButtonReset.IsEnabled = true;
#endif
                    GroupBoxSorting.IsEnabled = true;
                    GroupBoxInformation.IsEnabled = true;
                    GroupBoxOptions.IsEnabled = true;

                    CheckBoxOptionSubfolders.IsEnabled = true;
                    CheckBoxOptionSortFolders.IsEnabled = true;
                    CheckBoxOptionSimulate.IsEnabled = true;
                    break;
                case States.Select:
                    BorderDropZone.Opacity = 0;
                    GridInformation.Visibility = Visibility.Visible;
                    ButtonSelect.IsEnabled = false;
                    if (_settings.Path != null)
                        ReadFolder();
                    break;
                case States.Reading:
                    ButtonStart.IsEnabled = false;
                    ButtonSelect.IsEnabled = false;
                    CheckBoxOptionSubfolders.IsEnabled = false;
                    CheckBoxOptionSortFolders.IsEnabled = false;
                    CheckBoxOptionSimulate.IsEnabled = false;
                    break;
                case States.Idle:
                    if (_structure != null && _structure.Files.Count == 0)
                    {
                        SetStatus("No files found!");
                    }
                    else
                    {
                        SetStatus();
                    }
                    ButtonSelect.IsEnabled = true;
                    CheckBoxOptionSubfolders.IsEnabled = true;
                    CheckBoxOptionSortFolders.IsEnabled = true;
                    CheckBoxOptionSimulate.IsEnabled = true;
                    GroupBoxOptions.IsEnabled = true;
                    break;
                case States.Processing:
                    ButtonStart.IsEnabled = false;
#if (!DEBUG)
                    ButtonReset.IsEnabled = false;
#endif
                    GroupBoxSorting.IsEnabled = false;
                    GroupBoxOptions.IsEnabled = false;
                    GroupBoxInformation.IsEnabled = false;
                    break;
            }
        }

        private void SelectFolder()
        {
            Console.WriteLine("Select folder...");
            SetFocus();
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            //dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

            dialog.IsFolderPicker = true;
            dialog.RestoreDirectory = true;
            dialog.Multiselect = false;

            if (dialog.InitialDirectory == null)
                dialog.InitialDirectory = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _settings.Path = dialog.FileName;
                _settings.Save();
                SetState(States.Select);
            }
        }
        private async void ReadFolder()
        {
            try
            {
                if (!Directory.Exists(_settings.Path))
                {
                    throw new DirectoryNotFoundException(_settings.Path);
                }

                Console.WriteLine("Read folder...");
                SetStatus("Reading folder... Please wait.", true);

                _processing = true;

                SetState(States.Reading);

                LabelDetailsFilesCount.Content = ".";
                LabelDetailsFolderCount.Content = ".";

                await _structure.Load(_settings);

                //var drive = FileSystemHelper.GetDrive(_structure.GetBasePath());
                LabelPath.Text = _settings.Path;

                if (_structure.Folders == null)
                {
                    LabelDetailsFolderCount.Content = "0";
                }
                else
                {
                    LabelDetailsFolderCount.Content = _structure.Folders.Count.ToString();

                }
                LabelDetailsFilesCount.Content = _structure.Files.Count.ToString();
                ProgressBarStatus.Maximum = _structure.Folders.Count;

                if (_structure.Files.Count > 0)
                {
                    ButtonStart.IsEnabled = true;
                }

            }
            catch (DirectoryNotFoundException ex)
            {
                var msg = $"Directory:{nl} '{ex.Message}'{nl}not found!{nl}{nl}Please select an existing folder!";
                ShowMessage(msg, MessageBoxIcon.Error, "An Error occurred :(");
            }
            catch (Exception ex)
            {
                ProcessError(ex);
            }
            finally
            {
                _processing = false;
                SetState(States.Idle);
                SetFocus();
            }
        }
        private async void StartProcessing()
        {
            _processing = true;
            Console.WriteLine("Start processing...");
            SetState(States.Processing);
            SetStatus("Processing...", true);

            ProgressBarStatus.Maximum = _structure.Files.Count();

            var result = await _structure.Process();

            this.UpdateLayout();

            SetStatus("Processing finished.", true);

            if (result.HasErrors)
            {

            }
            else
            {
                ShowMessage($"Successfully finished.{nl}Njoy your proper sorted music. ;)", MessageBoxIcon.Information, "Done!");
            }
            SetState(States.Init);
            _processing = false;
        }


        // HELPER
        private void TestDebug()
        {
            Console.WriteLine("Test DEBUG...");
        }
        private void ProcessError(Exception ex)
        {
            //var msg = ex.Message + Environment.NewLine + ex.StackTrace;
            var msg = ex.Message;
            SimpleLogger.Instance.Error(msg);
            //this.InvokeEx2(f => f.ShowMessage(msg));

        }
        internal void ShowMessage(string message, MessageBoxIcon icon = System.Windows.Forms.MessageBoxIcon.Error, string title = "An Error occurred :(")
        {
            MessageBoxEx.Show(this, message,
                       title,
                      MessageBoxButtons.OK,
                       icon);
        }
        private void SetStatus(string msg = null, bool busy = false)
        {
            if (msg != null)
            {
                LabelStatus.Content = msg;
            }
            else
            {
                LabelStatus.Content = " ";
            }

            if (busy)
            {
                this.Cursor = System.Windows.Input.Cursors.Wait;
            }
            else
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }

        }
        private void SetFocus()
        {
            LabelFocus.Focus();
        }
        private void ShowInfo()
        {
            this.Topmost = false;
            var aboutDlg = new AboutWindow();
            aboutDlg.Owner = App.Current.MainWindow;
            aboutDlg.ShowDialog();
            this.Topmost = true;
            SetFocus();
        }
        private void Reset()
        {
            // ShowMessage("RESET");
            SetState(States.Init);
        }

        #region EVENTS
        // SORT-ORDER
        private void ButtonAscending_Click(object sender, RoutedEventArgs e)
        {
            SetSortOrder(SortingOrder.Ascending, true);
        }
        private void ButtonDescending_Click(object sender, RoutedEventArgs e)
        {
            SetSortOrder(SortingOrder.Descending, true);
        }
        private void ButtonRandom_Click(object sender, RoutedEventArgs e)
        {
            SetSortOrder(SortingOrder.Random, true);
        }
        // BUTTONS
        private void ButtonSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectFolder();
        }
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            StartProcessing();
        }
        private void ButtonInfo_Click(object sender, RoutedEventArgs e)
        {
            TestDebug();
            ShowInfo();
        }
        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        // OPTIONS
        private void CheckBoxOptionSubfolders_Click(object sender, RoutedEventArgs e)
        {
            var option = CheckBoxOptionSubfolders.IsChecked.HasValue ? CheckBoxOptionSubfolders.IsChecked : false;
            _settings.Subfolders = option.Value;
            _settings.Save();
            SetOptions(true);
        }
        private void CheckBoxOptionSortFolders_Click(object sender, RoutedEventArgs e)
        {
            var option = CheckBoxOptionSortFolders.IsChecked.HasValue ? CheckBoxOptionSortFolders.IsChecked : false;
            _settings.SortFolders = option.Value;
            _settings.Save();
            SetOptions(false);
        }
        private void CheckBoxOptionSimulate_Click(object sender, RoutedEventArgs e)
        {
            var option = CheckBoxOptionSimulate.IsChecked.HasValue ? CheckBoxOptionSimulate.IsChecked : false;
            _settings.Simulate = option.Value;
            _settings.Save();
            SetOptions(false);
        }
        #endregion

        #region Drag Drop Events
        private void BorderDropZone_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (_dropEnabled)
            {
                string[] droppedFilenames = e.Data.GetData(System.Windows.DataFormats.FileDrop, true) as string[];
                Console.WriteLine("FileDrop => " + droppedFilenames[0]);
                _settings.Path = droppedFilenames[0];

                BorderDropZone.BorderThickness = new Thickness(0);
                BorderDropZone.BorderBrush = System.Windows.Media.Brushes.AliceBlue;

                SetState(States.Select);
            }
            else { _dropEnabled = true; }
        }

        private void BorderDropZone_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            _dropEnabled = true;
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {


                var path = ((string[])e.Data.GetData(System.Windows.DataFormats.FileDrop))[0];
                if (!Directory.Exists(path))
                {

                    _dropEnabled = false;
                    BorderDropZone.BorderThickness = new Thickness(0);
                    BorderDropZone.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                }
                else
                {
                    if (BorderDropZone.Opacity == 0)
                    {
                        BorderDropZone.Opacity = 0.999;
                    }
                    BorderDropZone.BorderThickness = new Thickness(4);
                    BorderDropZone.BorderBrush = System.Windows.Media.Brushes.SkyBlue;
                }
            }
            else
            {
                _dropEnabled = false;
                BorderDropZone.BorderThickness = new Thickness(0);
                BorderDropZone.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
            }

            if (!_dropEnabled)
            {
                e.Effects = System.Windows.DragDropEffects.None;
                e.Handled = false;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.Move;

            }
        }

        private void BorderDropZone_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            BorderDropZone.BorderThickness = new Thickness(0);
            BorderDropZone.BorderBrush = System.Windows.Media.Brushes.AliceBlue;

            if (BorderDropZone.Opacity == 0.999)
            {
                BorderDropZone.Opacity = 0;
            }
        }

        #endregion

        private void Structure_ProgressChanged(object sender, ProcessingEventArgs e)
        {
            this.Dispatcher.Invoke(() => { ProgressBarStatus.Value = e.Progress; });
            this.Dispatcher.Invoke(() => { SetStatus(e.Message, true); });
        }

        ~MainWindow()
        {
            _structure.ProgressChanged -= Structure_ProgressChanged;
            _structure = null;
            _settings = null;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_processing)
            {
                if (MessageBoxEx.Show(this,
                         $"Application is still processing!{nl}{nl}" +
                         $"Please wait for the application to finish the work, otherwise the folder structure can be damaged!{nl}{nl}" +
                         $"Do you really want to quit?",
                         "WARNING!",
                         MessageBoxButtons.YesNo, MessageBoxIcon.Warning
                     ) == System.Windows.Forms.DialogResult.Yes)
                {
                    e.Cancel = false;
                    return;
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }
        }
    }
}
