using MusicSorter.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MusicSorter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool Test { get; set; }

        private static readonly Random _random = new Random();
        private readonly Settings _settings;


        public MainWindow()
        {
            InitializeComponent();

            _settings = new Settings();
            _settings.Load();
            //UpdateGUI(States.Init);
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

        private void SelectFolder()
        {
            Console.WriteLine("Select folder...");
        }
        private void ReadFolder()
        {
            Console.WriteLine("Read folder...");
        }
        private void StartProcessing()
        {
            Console.WriteLine("Start processing...");
        }

        private void TestDebug()
        {
            Console.WriteLine("Test DEBUG...");
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
            //ShowInfo();
        }
        #endregion

        private void SetFocus()
        {
            LabelFocus.Focus();
        }

        private void ShowInfo()
        {
            this.Topmost = false;
            var aboutDlg = new AboutWindow();
            aboutDlg.ShowDialog();
            this.Topmost = true;
            SetFocus();
        }
    }
}
