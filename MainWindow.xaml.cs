using Microsoft.WindowsAPICodePack.Dialogs;
using MusicSorter.Classes;
using MusicSorter.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private readonly Settings _settings;
        private Structure _structure;
        private States _state;
        private bool _dropEnabled = true;

        public MainWindow()
        {
            InitializeComponent();

            LabelStatus.Content = null;

            _settings = new Settings();
            _settings.Load();
            UpdateGUI(States.Init);
        }


        private void SetSortOrder(SortingOrder order, bool save = false)
        {
            switch (order)
            {
                case Classes.SortingOrder.Ascending:
                    ButtonAscending.Background = Brushes.SkyBlue;
                    ButtonDescending.Background = Brushes.Transparent;
                    ButtonRandom.Background = Brushes.Transparent;
                    break;
                case Classes.SortingOrder.Descending:
                    ButtonAscending.Background = Brushes.Transparent;
                    ButtonDescending.Background = Brushes.SkyBlue;
                    ButtonRandom.Background = Brushes.Transparent;
                    break;
                case Classes.SortingOrder.Random:
                    ButtonAscending.Background = Brushes.Transparent;
                    ButtonDescending.Background = Brushes.Transparent;
                    ButtonRandom.Background = Brushes.SkyBlue;
                    break;
            }
            if (save)
            {
                _settings.SortingOrder = order;
                _settings.Save();
            }
        }
        private void UpdateGUI(States state)
        {
            Console.WriteLine($"UpdateGUI({state})");

            switch (_state = state)
            {
                case States.Init:
                    SetSortOrder(_settings.SortingOrder);

                    GridInformation.Visibility = Visibility.Collapsed;
                    BorderDropZone.Opacity = 1;
                    ButtonSelect.IsEnabled = true;
                    ButtonStart.IsEnabled = false;
                    break;
                case States.Select:
                    BorderDropZone.Opacity = 0;
                    GridInformation.Visibility = Visibility.Visible;
                    ButtonSelect.IsEnabled = false;
                    ReadFolder();
                    break;
                case States.Read:
                    ButtonStart.IsEnabled = true;
                    ButtonSelect.IsEnabled = true;
                    break;
                case States.Process:
                    break;
                case States.Idle:
                    break;
                case States.Reset:
                    break;
                default:
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
                UpdateGUI(States.Select);
            }
        }
        private async void ReadFolder()
        {
            Console.WriteLine("Read folder...");
            SetStatus("Reading folder...", true);

            labelPath.Content = "...";
            LabelDetailsFilesCount.Content = "...";
            LabelDetailsFilesHiddenCount.Content = "...";
            LabelDetailsFolderCount.Content = "...";

            try
            {
                if (!Directory.Exists(_settings.Path))
                {
                    throw new DirectoryNotFoundException(_settings.Path);
                }

                _structure = new Structure(_settings);
                await _structure.Load();

                //var drive = FileSystemHelper.GetDrive(_structure.GetBasePath());
                labelPath.Content = _settings.Path;

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

                UpdateGUI(States.Read);
                if (_structure == null || _structure.Files.Count > 0)
                {
                    SetStatus();
                }
                else
                {
                    SetStatus("No files. Check Extensions!");
                }
                SetFocus();
            }
        }
        private void StartProcessing()
        {
            Console.WriteLine("Start processing...");
            UpdateGUI(States.Process);
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

                LabelStatus.Content = null;
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
            UpdateGUI(States.Init);
        }

        #region EVENTS
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
                BorderDropZone.BorderBrush = Brushes.AliceBlue;

                UpdateGUI(States.Select);
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
                    BorderDropZone.BorderBrush = Brushes.AliceBlue;
                }
                else
                {
                    if (BorderDropZone.Opacity == 0)
                    {
                        BorderDropZone.Opacity = 0.999;
                    }
                    BorderDropZone.BorderThickness = new Thickness(4);
                    BorderDropZone.BorderBrush = Brushes.SkyBlue;
                }
            }
            else
            {
                _dropEnabled = false;
                BorderDropZone.BorderThickness = new Thickness(0);
                BorderDropZone.BorderBrush = Brushes.AliceBlue;
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
            BorderDropZone.BorderBrush = Brushes.AliceBlue;

            if (BorderDropZone.Opacity == 0.999)
            {
                BorderDropZone.Opacity = 0;
            }
        }
        #endregion
    }
}
