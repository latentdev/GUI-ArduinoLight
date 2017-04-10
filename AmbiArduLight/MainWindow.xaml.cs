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
            if (!screen.getState())
            {
                screen.start();
            }
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
            screen.stop();
        }
    }
}
