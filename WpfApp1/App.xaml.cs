﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfApp1.Models;
using WpfApp1.ViewModels;
using TDDD49Template.Models;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Main(Object Sender, StartupEventArgs e)
        {
            MainWindow mainWindow = new MainWindow(new MainViewModel(new MessageService(), new ConnectionHandler()));
            mainWindow.Title = "Linus Amazing WPF MVVM C# Peer-2-Peer Chat Application!";
            mainWindow.Show();
        }
    }
}
