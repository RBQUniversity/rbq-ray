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
using System.Windows.Shapes;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace rbq_ray
{
    /// <summary>
    /// ServerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ServerWindow : Window
    {
        public const int HTTP = 0;
        public const int SOCKS = 1;
        public const int SS = 2;
        public const int SS2022 = 3;
        public const int VMESS = 4;
        public const int VLESS = 5;
        public const int TROJAN = 6;

        public const int TCP = 0;
        public const int WS = 1;
        public const int KCP = 2;
        public const int GRPC = 3;
        public const int QUIC = 4;
        public const int MEEK = 5;
        public const int HTTPUP = 6;

        public const int NONE = 0;
        public const int TLS = 1;

        private MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

        protected int editIndex = -1;

        public ServerWindow()
        {
            InitializeComponent();
        }
        
        public ServerWindow(short protocol)
        {
            InitializeComponent();

            protocolComboBox.SelectedIndex = protocol;
            onProtocolComboBoxSelectionChanged();
        }

        public ServerWindow(Server server, int index)
        {
            InitializeComponent();

            editIndex = index;

            Dictionary<String, String> parameters = new Dictionary<String, String>();
            foreach(string parameter in server.url.Query.Remove(0, 1).Split('&'))
            {
                parameters.Add(parameter.Split('=')[0], Uri.UnescapeDataString(parameter.Split('=')[1]));
            }

            // 代理协议
            switch (server.url.Scheme)
            {
                case "http":
                    protocolComboBox.SelectedIndex = HTTP;
                    usernameTextBox.Text = Uri.UnescapeDataString(server.url.UserInfo.Split(':')[0]);
                    passwordTextBox.Text = Uri.UnescapeDataString(server.url.UserInfo.Split(':')[1]);
                    break;
                case "socks":
                    protocolComboBox.SelectedIndex = SOCKS;
                    usernameTextBox.Text = Uri.UnescapeDataString(server.url.UserInfo.Split(':')[0]);
                    passwordTextBox.Text = Uri.UnescapeDataString(server.url.UserInfo.Split(':')[1]);
                    break;
                case "shadowsocks":
                    protocolComboBox.SelectedIndex = SS;
                    passwordTextBox.Text = Uri.UnescapeDataString(server.url.UserInfo);
                    methodComboBox.Text = parameters["method"];
                    break;
                case "shadowsocks2022":
                    protocolComboBox.SelectedIndex = SS2022;
                    passwordTextBox.Text = Uri.UnescapeDataString(server.url.UserInfo);
                    methodComboBox.Text = parameters["method"];
                    pskTextBox.Text = parameters["psk"];
                    break;
                case "vmess":
                    protocolComboBox.SelectedIndex = VMESS;
                    usernameTextBox.Text = Uri.UnescapeDataString(server.url.UserInfo);
                    break;
                case "vless":
                    protocolComboBox.SelectedIndex = VLESS;
                    usernameTextBox.Text = Uri.UnescapeDataString(server.url.UserInfo);
                    break;
                case "trojan":
                    protocolComboBox.SelectedIndex = TROJAN;
                    passwordTextBox.Text = Uri.UnescapeDataString(server.url.UserInfo);
                    break;
            }
            addressTextBox.Text = Uri.UnescapeDataString(server.address);
            portTextBox.Text = server.port.ToString();

            // 传输协议
            switch (parameters["transport"])
            {
                case "tcp":
                    transportComboBox.SelectedIndex = TCP;
                    break;
                case "ws":
                    transportComboBox.SelectedIndex = WS;
                    pathTextBox.Text = Uri.UnescapeDataString(server.url.AbsolutePath).Remove(0, 1);
                    break;
                case "kcp":
                    transportComboBox.SelectedIndex = KCP;
                    pathTextBox.Text = Uri.UnescapeDataString(server.url.AbsolutePath).Remove(0, 1);
                    break;
                case "grpc":
                    transportComboBox.SelectedIndex = GRPC;
                    pathTextBox.Text = Uri.UnescapeDataString(server.url.AbsolutePath).Remove(0, 1);
                    break;
                case "quic":
                    transportComboBox.SelectedIndex = QUIC;
                    break;
                case "meek":
                    transportComboBox.SelectedIndex = MEEK;
                    pathTextBox.Text = Uri.UnescapeDataString(server.url.AbsolutePath).Remove(0, 1);
                    break;
                case "httpupgrade":
                    transportComboBox.SelectedIndex = HTTPUP;
                    pathTextBox.Text = Uri.UnescapeDataString(server.url.AbsolutePath).Remove(0, 1).Split('/')[1];
                    pathTextBox.Text = Uri.UnescapeDataString(server.url.AbsolutePath).Remove(0, 1).Split('/')[0];
                    break;
            }

            // 安全协议
            if (parameters.ContainsKey("security"))
            {
                switch (parameters["security"])
                {
                    case "tls":
                        securityComboBox.SelectedIndex = TLS;
                        if (parameters.ContainsKey("sni"))
                        {
                            sniTextBox.Text = parameters["sni"];
                        }
                        break;
                }
            }

            // 名称
            if (server.url.Fragment != "")
            {
                serverNameTextBox.Text = Uri.UnescapeDataString(server.url.Fragment.Remove(0, 1));
            }

            onProtocolComboBoxSelectionChanged();
            onTransportComboBoxSelectionChanged();
            onSecurityComboBoxSelectionChanged();
        }

        protected void onProtocolComboBoxSelectionChanged()
        {
            switch (protocolComboBox.SelectedIndex)
            {
                case HTTP:
                    usernameTextBox.IsEnabled = true;
                    passwordTextBox.IsEnabled = true;
                    methodComboBox.IsEnabled = false;
                    pskTextBox.IsEnabled = false;
                    break;
                case SOCKS:
                    usernameTextBox.IsEnabled = true;
                    passwordTextBox.IsEnabled = true;
                    methodComboBox.IsEnabled = false;
                    pskTextBox.IsEnabled = false;
                    break;
                case SS:
                    usernameTextBox.IsEnabled = false;
                    passwordTextBox.IsEnabled = true;
                    methodComboBox.IsEnabled = true;
                    pskTextBox.IsEnabled = false;
                    aes_256_gcmComboBoxItem.IsEnabled = true;
                    aes_128_gcmComboBoxItem.IsEnabled = true;
                    chacha20_poly1305ComboBoxItem.IsEnabled = true;
                    chacha20_ietf_poly1305ComboBoxItem.IsEnabled = true;
                    methodNoneComboBoxItem.IsEnabled = true;
                    methodPlainComboBoxItem.IsEnabled = true;
                    _2022_blake3_aes_128_gcmComboBoxItem.IsEnabled = false;
                    _2022_blake3_aes_256_gcmComboBoxItem.IsEnabled = false;
                    break;
                case SS2022:
                    usernameTextBox.IsEnabled = false;
                    passwordTextBox.IsEnabled = true;
                    methodComboBox.IsEnabled = true;
                    pskTextBox.IsEnabled = true;
                    aes_256_gcmComboBoxItem.IsEnabled = false;
                    aes_128_gcmComboBoxItem.IsEnabled = false;
                    chacha20_poly1305ComboBoxItem.IsEnabled = false;
                    chacha20_ietf_poly1305ComboBoxItem.IsEnabled = false;
                    methodNoneComboBoxItem.IsEnabled = false;
                    methodPlainComboBoxItem.IsEnabled = false;
                    _2022_blake3_aes_128_gcmComboBoxItem.IsEnabled = true;
                    _2022_blake3_aes_256_gcmComboBoxItem.IsEnabled = true;
                    break;
                case VMESS:
                    usernameTextBox.IsEnabled = true;
                    passwordTextBox.IsEnabled = false;
                    methodComboBox.IsEnabled = false;
                    pskTextBox.IsEnabled = false;
                    break;
                case VLESS:
                    usernameTextBox.IsEnabled = true;
                    passwordTextBox.IsEnabled = false;
                    methodComboBox.IsEnabled = false;
                    pskTextBox.IsEnabled = false;
                    break;
                case TROJAN:
                    usernameTextBox.IsEnabled = false;
                    passwordTextBox.IsEnabled = true;
                    methodComboBox.IsEnabled = false;
                    pskTextBox.IsEnabled = false;
                    break;
            }
        }

        protected void onTransportComboBoxSelectionChanged()
        {
            switch (transportComboBox.SelectedIndex)
            {
                case TCP:
                    pathTextBox.IsEnabled = false;
                    hostTextBox.IsEnabled = false;
                    break;
                case WS:
                    pathTextBox.IsEnabled = true;
                    hostTextBox.IsEnabled = false;
                    break;
                case KCP:
                    pathTextBox.IsEnabled = true;
                    hostTextBox.IsEnabled = false;
                    break;
                case GRPC:
                    pathTextBox.IsEnabled = true;
                    hostTextBox.IsEnabled = false;
                    break;
                case QUIC:
                    pathTextBox.IsEnabled = false;
                    hostTextBox.IsEnabled = false;
                    break;
                case MEEK:
                    pathTextBox.IsEnabled = true;
                    hostTextBox.IsEnabled = false;
                    break;
                case HTTPUP:
                    pathTextBox.IsEnabled = true;
                    hostTextBox.IsEnabled = true;
                    break;
            }
        }

        protected void onSecurityComboBoxSelectionChanged()
        {
            switch (securityComboBox.SelectedIndex)
            {
                case NONE:
                    sniTextBox.IsEnabled = false;
                    break;
                case TLS:
                    sniTextBox.IsEnabled = true;
                    break;
            }
        }

        protected void serverConfirm()
        {
            UriBuilder uriBuilder = new UriBuilder();

            // 代理协议
            String scheme = "";
            switch (protocolComboBox.SelectedIndex)
            {
                case HTTP:
                    scheme = "http";
                    break;
                case SOCKS:
                    scheme = "socks";
                    break;
                case SS:
                    scheme = "shadowsocks";
                    break;
                case SS2022:
                    scheme = "shadowsocks2022";
                    break;
                case VMESS:
                    scheme = "vmess";
                    break;
                case VLESS:
                    scheme = "vless";
                    break;
                case TROJAN:
                    scheme = "trojan";
                    break;
            }
            uriBuilder.Scheme = scheme;

            // 用户名
            List<int> protocolsUsername = new List<int>() { HTTP, SOCKS, VMESS, VLESS };
            if (protocolsUsername.Contains(protocolComboBox.SelectedIndex))
            {
                uriBuilder.UserName = Uri.EscapeDataString(usernameTextBox.Text);
            }

            // 密码
            List<int> protocolsPassword1 = new List<int>() { HTTP, SOCKS };
            if (protocolsPassword1.Contains(protocolComboBox.SelectedIndex))
            {
                uriBuilder.Password = Uri.EscapeDataString(passwordTextBox.Text);
            }
            List<int> protocolsPassword2 = new List<int>() { SS, SS2022, TROJAN };
            if (protocolsPassword2.Contains(protocolComboBox.SelectedIndex))
            {
                uriBuilder.UserName = Uri.EscapeDataString(passwordTextBox.Text);
            }

            if(addressTextBox.Text == "" && portTextBox.Text == "") { 
                MessageBox.Show("用户名和密码不得为空。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // 地址
            uriBuilder.Host = Uri.EscapeDataString(addressTextBox.Text);
            // 端口
            uriBuilder.Port = Convert.ToInt32(portTextBox.Text);

            // 路径
            List<int> protocolsPath = new List<int>() { WS, KCP, GRPC, MEEK, HTTPUP };
            if (protocolsPath.Contains(protocolComboBox.SelectedIndex) && pathTextBox.Text != "")
            {
                uriBuilder.Path = Uri.EscapeDataString(pathTextBox.Text);
            }
            if(protocolComboBox.SelectedIndex == HTTPUP && hostTextBox.Text != "")
            {
                uriBuilder.Path = Uri.EscapeDataString(hostTextBox.Text) + "/" + Uri.EscapeDataString(pathTextBox.Text);
            }

            List<String> parameters = new List<string>();

            // 加密方式
            List<int> protocolsMethod = new List<int>() { SS, SS2022 };
            if (protocolsMethod.Contains(protocolComboBox.SelectedIndex))
            {
                parameters.Add("method=" + methodComboBox.Text);
            }

            // PSK
            if(protocolComboBox.SelectedIndex == SS2022) {
                parameters.Add("psk=" + Uri.EscapeDataString(pskTextBox.Text));
            }

            // 传输协议
            String transport = "";
            switch (transportComboBox.SelectedIndex)
            {
                case TCP:
                    transport = "tcp";
                    break;
                case WS:
                    transport = "ws";
                    break;
                case KCP:
                    transport = "kcp";
                    break;
                case GRPC:
                    transport = "grpc";
                    break;
                case QUIC:
                    transport = "quic";
                    break;
                case MEEK:
                    transport = "meek";
                    break;
                case HTTPUP:
                    transport = "httpupgrade";
                    break;
            }
            parameters.Add("transport=" + transport);

            // 安全协议、SNI
            switch (securityComboBox.SelectedIndex)
            {
                case NONE:
                    break;
                case TLS:
                    parameters.Add("security=tls");
                    if(sniTextBox.Text != "")
                    {
                        parameters.Add("sni=" + Uri.EscapeDataString(sniTextBox.Text));
                    }
                    break;
            }

            uriBuilder.Query = String.Join("&", parameters);

            // 名称
            if(serverNameTextBox.Text != "")
            {
                uriBuilder.Fragment = Uri.EscapeDataString(serverNameTextBox.Text);
            }

            Server server = new Server()
            {
                enabled = false,
                protocol = protocolComboBox.Text,
                name = serverNameTextBox.Text,
                address = addressTextBox.Text,
                port = Convert.ToUInt16(portTextBox.Text),
                transport = transportComboBox.Text,
                security = securityComboBox.Text,
                delay = -1,
                url = uriBuilder.Uri,
            };
            if (editIndex >= 0)
            {
                mainWindow.servers[editIndex] = server;
            }
            else
            {
                mainWindow.servers.Add(server);
            }

            Debug.WriteLine(uriBuilder.ToString());

            this.Close();
        }
        
        private void protocolComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            onProtocolComboBoxSelectionChanged();
        }

        private void transportComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                onTransportComboBoxSelectionChanged();
            }
        }

        private void securityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                onSecurityComboBoxSelectionChanged();
            }
        }

        private void serverConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            serverConfirm();
        }
    }
}
