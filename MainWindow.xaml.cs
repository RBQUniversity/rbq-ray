using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Text.Json;
using System.IO;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Security.Policy;
using System.Data;

namespace rbq_ray
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Server> servers = new ObservableCollection<Server>();

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists("servers.json"))
            {
                servers = JsonSerializer.Deserialize<ObservableCollection<Server>>(File.ReadAllText("servers.json"));
            }

            serverDataGrid.DataContext = servers;
        }

        private void serverAddHttpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ServerWindow serverWindow = new ServerWindow(ServerWindow.HTTP);
            serverWindow.ShowDialog();
            saveServers();
        }

        private void serverAddSocksMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ServerWindow serverWindow = new ServerWindow(ServerWindow.SOCKS);
            serverWindow.ShowDialog();
            saveServers();
        }

        private void serverAddSsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ServerWindow serverWindow = new ServerWindow(ServerWindow.SS);
            serverWindow.ShowDialog();
            saveServers();
        }

        private void serverAddSs2022MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ServerWindow serverWindow = new ServerWindow(ServerWindow.SS2022);
            serverWindow.ShowDialog();
            saveServers();
        }

        private void serverAddVmessMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ServerWindow serverWindow = new ServerWindow(ServerWindow.VMESS);
            serverWindow.ShowDialog();
            saveServers();
        }

        private void serverAddVlessMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ServerWindow serverWindow = new ServerWindow(ServerWindow.VLESS);
            serverWindow.ShowDialog();
            saveServers();
        }

        private void serverAddTrojanMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ServerWindow serverWindow = new ServerWindow(ServerWindow.TROJAN);
            serverWindow.ShowDialog();
            saveServers();
        }

        protected void editServer()
        {
            if(serverDataGrid.SelectedItem == null) {
                return;
            }
            ServerWindow serverWindow = new ServerWindow((Server)serverDataGrid.SelectedItem, serverDataGrid.SelectedIndex);
            serverWindow.ShowDialog();
            saveServers();
        }

        private void serverEditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            editServer();
        }

        private void serverEditContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            editServer();
        }

        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("An RBQ University Production.\nVersion: 0.0.1", "关于", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected void saveServers()
        {
            JsonSerializerOptions options = new JsonSerializerOptions() { 
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            File.WriteAllText("servers.json", JsonSerializer.Serialize(servers, options));
        }

        protected void copyServers()
        {
            String urls = "";
            foreach(Server server in serverDataGrid.SelectedItems)
            {
                urls = server.url.OriginalString + "\n";
            }
            Clipboard.SetText(urls);
            Debug.WriteLine(urls);
        }

        private void serverCopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            copyServers();
        }

        private void serverCopyContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            copyServers();
        }

        protected void deleteServers()
        {
            ObservableCollection<Server> serversPending = new ObservableCollection<Server>();
            foreach (Server server in serverDataGrid.SelectedItems)
            {
                serversPending.Add(server);
            }
            foreach(Server server in serversPending)
            {
                servers.Remove(server);
            }
            saveServers();
        }

        private void deleteServerContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            deleteServers();
        }

        private void deleteServerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            deleteServers();
        }
    }

    public class Server
    {
        public bool enabled { get; set; }
        public string protocol { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public ushort port { get; set; }
        public string transport { get; set; }
        public string security { get; set; }
        public string subscribe { get; set; }
        public short delay { get; set; }
        public Uri url { get; set; }
    }
}
