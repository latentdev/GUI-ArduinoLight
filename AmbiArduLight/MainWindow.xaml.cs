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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using Screen_Color;
using System.Drawing;
using System.Threading;

namespace AmbiArduLight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Screen screen = new Screen();
        public MainWindow()
        {
            InitializeComponent();

        }

        void Application_Exit(object sender, ExitEventArgs e, SerialPort port)
        {
            if (port.IsOpen)
                port.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            screen.CoordsReceived += Set_Text;
            screen.ConsoleOutput += Set_Console;
            if (!screen.getState())
            {
                
                screen.start();
                if (screen.getState())
                    text_state.Text = "Running";
            }
        }

        private void Set_Console(object sender, ConsoleEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                text_error.Text = e.console;
            }
);
        }

        private void Set_Text(object sender, StringEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                text_top.Text = e.top;
                text_bottom.Text = e.bottom;
            }
        );
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            text_state.Text = "Stopped";
            screen.stop();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SerialPort port = screen.getPort();
            screen.stop();
            if(port != null && !port.IsOpen)
            {
                port.Close();
            }
            Application.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Title_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.MainWindow.DragMove();
        }

        private void RadioButton_Changed(object sender, RoutedEventArgs e)
        {
            if((bool)radio_button1.IsChecked)
            {
                screen.COM = "COM1";
            }
            if ((bool)radio_button2.IsChecked)
            {
                screen.COM = "COM2";
            }
            if ((bool)radio_button3.IsChecked)
            {
                screen.COM = "COM3";
            }
            if ((bool)radio_button4.IsChecked)
            {
                screen.COM = "COM4";
            }
            if ((bool)radio_button5.IsChecked)
            {
                screen.COM = "COM5";
            }
            if ((bool)radio_button6.IsChecked)
            {
                screen.COM = "COM6";
            }
        }
    }
}
