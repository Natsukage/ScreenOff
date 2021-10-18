using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScreenOff
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")] private static extern IntPtr PostMessage(int hWnd, int msg, int wParam, int lParam);

        private HttpServer _httpServer;
        public MainWindow()
        {
            InitializeComponent();
            ServerStart();
        }

        private void ServerStart(object sender = null, EventArgs e = null)
        {
            try
            {
                _httpServer = new HttpServer(30001);
                _httpServer.ReceivedCommandRequest += DoTextCommand;
                _httpServer.OnException += OnException;
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void ServerStop(object sender = null, EventArgs e = null)
        {
            _httpServer.Stop();
            _httpServer.ReceivedCommandRequest -= DoTextCommand;
            _httpServer.OnException -= OnException;
        }
        private void DoTextCommand(string command)
        {
            switch (command.ToLower(CultureInfo.CurrentCulture))
            {
                case "screen=off":
                    _ = PostMessage(-1, 0x0112, 0xF170, 2);
                    return;
                default:
                    return;
            }
        }
        private void OnException(Exception ex)
        {
            string errorMessage = $"无法在{_httpServer.Port}端口启动监听\n{ex.Message}";
            
            MessageBox.Show(errorMessage);
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
