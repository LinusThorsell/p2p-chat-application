using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using WpfApp1.ViewModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();
            this.DataContext = mainViewModel;

        }

        private void HandleLinkClick(object sender, RequestNavigateEventArgs e)
        {
            var ps = new ProcessStartInfo(e.Uri.ToString())
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }



        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //do my stuff before closing

            base.OnClosing(e);
        }
    }
}
