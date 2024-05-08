# RBQray

![](img/1.png)

RBQray旨在实现一个基于V2Ray最新稳定内核的Windows平台图形代理客户端，使用tun2socks实现虚拟网卡接管全局流量，支持随机选择和连接观测的负载均衡。

由于精力有限，本项目已暂停开发。已经实现的功能主要有：主窗口、节点设置窗口、添加节点、编辑节点信息、自动保存节点列表、复制单条或多条节点链接、删除单个或多个节点。

经审慎分析，本项目的所有预期功能都具备可行性。理论上，根据本文指引，可以完成本项目的开发。

## 背景

2013年至2016年左右，中国网民“翻墙”访问国际网络主要借助传统VPN协议。当时通过中国国内搜索引擎很容易搜索到各种“VPN”、“加速器”服务，例如安徽龙田信息科技有限公司的“豆荚加速器”产品等，都是使用PPTP、L2TP、OpenVPN等VPN协议实现的。前北京邮电大学校长方滨兴曾接受记者采访，表示自己电脑上就安装了多款VPN产品，包括前述“豆荚加速器”，用于“测试GFW的可靠性”。2016年左右，中国国内合法经营个人VPN产品的企业都被勒令停止相关业务，安徽龙田信息科技有限公司还发布公告给客户退款。

除了收费的VPN产品，还有赛风、国立筑波大学的VPN Gate等免费可用，不过在GFW迎来一次大规模升级后，传统VPN协议在“翻墙”上已经无可奈何了，后来逐渐有了能混淆流量的Shadowsocks、V2Ray之类的代理协议。

VPN和代理服务器的主要差异在于：VPN可以创建虚拟网卡并配置虚拟专用网，使虚拟网络上的主机可以相互访问，本机通过虚拟网络的远程网关访问互联网，从而绕过了本地广域网的审查和阻断，本机的所有软件都通过虚拟网络联网；代理服务器通常只用于网页访问，需要具体软件支持通过代理上网，局限性比较大。Shadowsocks、V2Ray等工具都是基于代理协议开发的，实现的都是代理服务器和代理客户端。VPN和代理服务器在数据传输上的处理方式不同，VPN以加密为目的，“翻墙”代理以混淆为目的，所以VPN的流量特征极为明显，可直接被防火墙识别并阻断，这使得传统VPN协议在跨境场景不再可用。

代理服务器可以解决日常上网的大多数需求，不过有时遇到某些软件不支持设置代理服务器，或者是操作系统的功能（如ICMP），就需要设法使它们的流量被接管到代理软件中，常见的做法有Netfilter、Pcap和虚拟网卡（TUN、TAP）。

对于网络游戏场景，一些商业游戏加速器的做法可能是Netfilter，它可以实现分应用代理。对于需要接管系统全局网络流量的场景，不太适合使用Netfilter，因为Netfilter一般是精确到进程的，维护一个系统的所有应用程序列表可能不现实，所以一般用虚拟网卡或Pcap应对需要接管全局网络流量的场景。

虚拟网卡是VPN实现的一部分，它负责与操作系统协商适配器、为自身分配网络地址并在路由表中占位。引入了虚拟网卡的代理协议与真正VPN的唯一区别只是没有形成虚拟专用网的拓扑结构，代理客户端不能作为一个虚拟网络上的主机被其它主机主动访问。虚拟网卡只是与代理客户端配合，在配置了路由表后，把本机的网络请求发给本地代理客户端，再由代理客户端联系远程代理服务器。

TUN虚拟网卡工作在网络层（第三层），只能分配IP地址作为网络地址；TAP虚拟网卡工作在数据链路层（第二层），具备MAC地址，可以实现网桥。TUN和TAP都可以与代理客户端配合实现真全局代理，从结果看没有差别。

Pcap原本用于网络抓包分析，它可以捕获指定网络接口上通过的数据包。不像虚拟网卡的方式是创建网卡，利用PCAP实现代理就是将发送到指定已有网络接口（网卡）上的源数据包截获并转交代理服务器处理。在Windows操作系统中，实现Pcap需要系统安装有Npcap或WinPcap。

在日常上网的情景下，通常是只用代理服务器，因为可以设置分流规则，比较灵活，而且上网内容主要是浏览网页，主流应用也支持设置代理服务器。只有网络游戏和个别应用需要做额外的操作。

### 考察

近年来，主流的“翻墙”代理工具是V2Ray和基于它开发的其它套件，它们支持多种代理协议和混淆方式。由于它们本身是没有图形界面的控制台程序，在Windows操作系统上的用户要想使用它们，一般会选择一些基于V2Ray等内核开发的图形客户端，最流行的是v2rayN。

由于Clash已经删除代码仓库并永久停止维护，本文不会过多讨论Clash。

#### v2rayN

v2rayN是目前最流行的代理客户端，在5及以前的版本只有代理服务器功能，并且逐渐支持了V2Fly、Xray、SagerNet、sing-box多种内核。从6开始新增了TUN模式功能，这本来是一件可喜的事，可惜v2rayN的TUN模式是对sing-box内核的TUN功能的套壳，并且不能够确保随时在所有计算机上都可用。经常出现的错误是，当启用了v2rayN的TUN模式，应该被创建的虚拟网卡没有被创建，日志输出反复显示一些GEO文件无法联网更新的错误，没有设置代理的软件继续直连上网，设置过代理的软件不能上网。用v2rayN 6.43的TUN配置`tunConfig.json`文件启动自带的sing-box测试发现v2rayN配置的虚拟网卡名称不能被创建为虚拟网卡（原因未知，猜测是偶然残留的TUN驱动引发了冲突），将虚拟网卡名称更改为其它字符串或置空取默认，sing-box即可启动，但又出现连接远程代理服务器时发生无限本地回环的问题，`tunConfig.json`配置的进程排除规则没有生效，依旧不能正常使用。

由于v2rayN的TUN模式的诸多异常导致一众用户无法使用，开发者专门在Github仓库里开了一个Issue“统一回复”TUN模式的问题，坦承“需要大家帮忙”，对虚拟网卡不创建的问题回答得很模糊，提示用户可去sing-box查找反馈。面对放弃套壳sing-box的TUN转而仿照Netch或Clash for Windows独立实现虚拟网卡的提议，开发者说“太复杂，开发者也不会”。

#### NekoRay

在v2rayN的Issue中有网友提议换用NekoRay。这是一个基于Qt的跨平台GUI代理配置管理器，使用了V2Ray和sing-box内核，它的TUN功能也是基于sing-box实现的，尽管没有像v2rayN那样无法创建虚拟网卡，但还是出现了无限本地回环的情形，所以不可用。

#### sing-box

在sing-box的Github仓库中尚未找到相关的TUN功能问题，对照文档也没有发现配置错误。这可能意味着无论是v2rayN还是sing-box本体的TUN实现都处于不成熟的状态，因此它是不宜采用的。

#### Proxifier

Proxifier是一个闭源的商业软件，可以使计算机上不支持代理服务器设置的应用程序的流量被接管到指定的代理服务器，可以设置分流规则，支持分应用接管。缺点是不支持处理UDP流量，不能代理大部分网络游戏（如《原神》）的游戏操作通信。不太清楚Proxifier是如何实现分应用接管的，猜测用到了Netfilter。

#### SSTAP

SSTAP是一个早已停止维护的虚拟网卡代理工具。SSTAP必须通过安装程序安装到系统中才能使用，安装向导会安装一个OpenVPN虚拟网卡，后续会使用这个TAP虚拟网卡实现联网。卸载SSTAP时，向导不会一并卸载其安装的OpenVPN虚拟网卡，需要在设备管理器中手动移除。没有找到SSTAP的源代码，它可能是一个闭源软件。

#### Netch

Netch是已知功能最丰富的代理客户端，支持多种接管系统流量的方式，包括：Netfilter、Pcap、WinTUN和Web代理。在代理协议的实现上，不像其它代理客户端是对代理内核整个套壳、只实现选用的代理内核支持的代理协议，而是对不同代理协议选用开发者认为合适的代理内核或类库实现，例如分别支持了SOCKS、Shadowsocks、ShadowsocksR、Trojan和WireGuard，对VMess和VLESS使用V2Ray内核。分别实现造成了一个问题，即Trojan无法使用V2Ray支持的多种传输协议组合，如WebSocket、mKCP、gRPC、QUIC等，只能用TCP作为Trojan的传输层，造成某些基于V2Ray功能组合配置的节点无法在Netch上使用。此外Netch也是一个停止维护的项目，用户无法获得任何支持。

考虑到Netch接管系统流量的方式包括Pcap和WinTUN，可以发现它们是分别基于pcap2socks和tun2socks实现的。

#### pcap2socks

pcap2socks被设计用于加速游戏主机、掌机的网络连接，使用Rust开发。它需要系统中已安装WinPcap或兼容WinPcap模式的Npcap。不支持IPv6，并且已经很久没有维护了。

#### pcap2socks GUI

pcap2socks GUI是pcap2socks配套的图形客户端，是使用Node.js实现的界面。软件启动后总是卡在首页，无法点击“下一步”按钮。

#### go-pcap2socks

go-pcap2socks是pcap2socks的Go语言实现，不清楚是否解决了pcap2socks尚不支持IPv6的问题，在它的Github仓库里没有找到发布的编译好的可执行文件，或许需要自行编译使用。

#### tun2socks

tun2socks依赖WireGuard的WinTUN创建TUN虚拟网卡，可处理来自本设备的任意网络应用的所有网络流量并通过代理转发。开发者将tun2socks与badvpn-tun2socks、go-tun2socks进行了性能比较，得出了tun2socks性能最佳的结论。Clash for Windows似乎也是用它实现TUN的。

#### Leaf

Leaf自称是一个高效的多功能代理框架，提供了简洁的配置文件格式，支持TUN模式入站，支持负载均衡，使用Rust开发。

#### Mellow

Mellow是一个代理功能上类似于Proxifier的，可以代理所有流量的，可以使用域名、IP、GEOIP、进程等规则分流的，可以同时有多个协议和出口的工具。此项目已不维护，开发者推荐Windows用户尝试使用Maple。

#### Maple

Maple基于Leaf内核，是一个UWP应用。由于没有上架Microsoft Store，安装它需要先将一个自签名证书导入系统根证书目录。它可以在系统中创建一个VPN连接。严格地说，这不是一个完全的图形客户端，更像一个配置编辑器，用户必须手工编写节点配置和路由规则，也不支持订阅功能，非常极客，不适合一般用户。实测TUN模式依旧出现了无限本地回环的问题，Maple似乎没有将远程服务器的地址做排除，查阅Leaf的文档也尚未找到解决办法。

#### TunMax

TunMax是一个使用Python开发的tun2socks图形客户端，配合v2rayN等代理客户端可以实现接管全局流量，不存在无限本地回环的问题，前提是本地代理软件必须工作在全局代理模式，不能设置直连规则，用户还需要把代理软件的远程服务器地址写到TunMax的配置文件中，以便TunMax配置直连路由。TunMax在启动时会调用tun2socks以创建虚拟网卡，TunMax为虚拟网卡设置IP地址和网关地址，同时更改了系统的路由表，将虚拟网卡相关路由规则的Metric设为3使其比物理网卡更加优先（Netch设Metric为1），添加一条0.0.0.0默认路由指向虚拟网卡的网关地址，不删除物理网卡的0.0.0.0默认路由（无必要），并为前述的代理软件的远程服务器地址设置一条指向物理网卡网关的路由规则。当TunMax关闭时，在tun2socks进程结束前，tun2socks会自行拆除虚拟网卡，系统会自动从路由表删除与虚拟网卡关联的路由规则，TunMax只需要删除系统路由表中代理软件的远程服务器地址对应的路由规则和TunMax的非Full模式及可选功能的额外路由规则即可。

TunMax不支持IPv6，因为TunMax只配置了IPv4路由。如果用户的网络环境是IPv4/IPv6双栈的，连接到远程双栈网站或IPv6网站时会使用IPv6路由，这些流量不会被代理，远程网站亦可以看到用户的真实IPv6地址。另外有用户反馈Windows API的GetAddrInfo没有被TUN接管，存在DNS泄露的问题。开发者当时表示Netch已经重启开发，TunMax不会再有太大变化。

## 目标

RBQray想要实现一个功能简单、直观易用的代理客户端软件，它应该具备以下主要特性：

1. 仅支持V2Ray原版内核，不对Xray、SagerNet、sing-box等其它内核提供支持，以降低系统复杂度；
2. 允许用户多选节点，即同时启用多个节点，通过V2Ray内置的负载均衡功能提供聚合，可以支持随机切换和根据连接观测的结果自动选择；
3. 提供类似v2rayN的代理服务器基本分流（路由）规则；
4. 借助tun2socks和操作系统路由表实现基于TUN虚拟网卡的真全局代理，同时使用系统路由表实现一定程度的分流规则，类似Netch的“Bypass LAN and China”预定义规则并支持自定义；
5. 为HTTP、SOCKS、Shadowsocks、VMess、VLESS、Trojan各项代理协议实现标准URI，并且兼容V2RayN的订阅链接导入和更新订阅。

## 选型

### 开发语言：C#

- 选择C#作为开发语言，因为可以配合.NET Framework方便地创建Windows窗口应用程序。

### 开发框架：.NET Framework 4.8

1. 使用.NET Framework创建的窗口程序，即使不做任何美化，默认的风格就比较简洁美观。Java Swing和Java FX的默认蓝白渐变风格奇丑无比。不愿意花费太多精力在界面的美化上。
2. Python没有成熟的界面设计工具，开发图形应用程序十分繁琐。
3. 目标平台是Windows，不需要考虑跨平台兼容性。
4. 4.8是.NET Framework目前的最新版本。

### 界面框架：WPF

1. 对比WinForm，WPF提供的界面组织功能更多，有Grid、StackPanel、DockPanel、WrapPanel、Canvas等格式化容器控件。（Netch是WinForm，v2rayN是WPF。）
2. WPF可以对控件进行数据绑定，可以实现MVVM模型。

### JSON序列化类库：System.Text.Json

用于保存节点列表和配置参数。.NET Framework默认不带，可以通过Nuget包管理器安装。
- 对比Newtonsoft.Json，System.Text.Json默认是强类型，比较安全。

## 设计

### 主窗口 `MainWindow`

![](img/2.png)

主窗口由三个控件组成：菜单栏`Menu`、数据网格`serverDataGrid`、状态栏`StatusBar`。

-   数据网格有字段：启用、协议、名称、地址、端口、传输层、安全层、订阅组、延迟。

    数据网格绑定了一个泛型为`Server`的`ObservableCollection`实例对象`servers`。`ObservableCollection`是一种在自身内容发生改变时可以通知绑定的WPF控件改变显示的集合。本程序没有实现MVVM框架或MVVM模型，因为这样做比较麻烦。`servers`是`MainWindow`的属性。

-   `Server`类的定义如下：
    ```cs
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
    ```
    其中`Uri`是`System.Uri`，本程序就是使用它存储各项代理协议的标准URI的。URI是参照V2Ray v5的文档定义的。整理的代理协议、传输协议、安全协议的各项参数如下三表所示：

    | protocol | address | port | username | password | method | psk |
    | -------- | ------- | ---- | -------- | -------- | ------ | --- |
    | http     | address | port | user     | pass     |        |     |
    | socks    | address | port | user     | pass     |        |     |
    | shadowsocks | address | port |       | password | method |     |
    | shadowsocks2022 | address | port |   | ipsk     | method | psk |
    | vmess    | address | port | uuid     |          |        |     |
    | vless    | address | port | uuid     |          |        |     |
    | trojan   | address | port |          | password |        |     |

    由于VLite是一个只支持UDP的协议，实用性比较低，不适合作为代理客户端的出站协议，因此不会实现它。

    | transport | path | host |
    | --------- | ---- | ---- |
    | tcp       |      |      |
    | ws        | path |      |
    | kcp       | seed |      |
    | grpc      | serviceName | |
    | quic      |      |      |
    | meek      | url  |      |
    | httpupgrade | path | host |

    | security | sni |
    | -------- | --- |
    | (none)   |     |
    | tls      | serverName |

    由于uTLS只支持TCP、WebSocket传输方式，不便于统一实现，并且uTLS是修改版本的TLS实现，TLS的混淆水平通常已经足够，目前认为没有必要在本程序中支持它。

    最终形成的标准URI大概是这样：
    ```
    protocol://username:password@address:port/(host/)path?method=&psk=&transport=&security=&sni=#name
    ```

#### 添加节点

![](img/3.png)

选择`节点/添加`将打开节点设置窗口，并且会根据选择的协议类型将代理协议下拉选择框的选项对应设置。

#### 编辑节点

选择`节点/编辑`，如果已经选中了节点，将会打开节点设置窗口，并设置好对应的选项以便修改。

#### 复制节点

可以多选，选中后会复制节点的`url.OriginalString`，每行一个。这里不是`url.toString()`，因为`Uri.toString()`方法会进行URI字符的反转义。如果没有选中节点，将复制空文本。

#### 粘贴节点

可以粘贴节点，支持本程序定义的标准URI，兼容v2rayN的订阅链接和订阅内容。

#### 移动节点

![](img/4.png)

可以上下移动选中的节点的顺序。

#### 订阅管理

![](img/5.png)

选择`订阅/管理`应该打开一个订阅管理窗口，用户必须配置订阅项的名称和链接，订阅数据会保存在`subscriptions.json`文件中。

#### 更新订阅

选择`订阅/更新`会根据订阅列表中的订阅链接下载订阅数据，可以是本程序定义的标准URI清单，兼容v2rayN的订阅内容。节点的URL被解析后会写入数据网格，“订阅组”字段会显示对应的订阅项的名称。

#### 代理服务器模式

![](img/6.png)

代理服务器模式是默认启用的，不可关闭。在代理服务器模式下，当程序启动时加载节点列表完毕后、每当数据网格的内容（含“启用”字段复选框）更新时，程序会筛选“启用”的节点列表，结合配置的路由（分流），在系统临时目录下生成一个V2Ray的负载均衡配置文件`%TEMP%/rbq-ray/config.json`，以此在子进程中启动V2Ray内核，根据是否显示输出窗口的参数配置决定是否显示V2Ray的输出窗口。

入站的HTTP端口默认为`7500`，Socks端口默认为`7700`。

#### 虚拟网卡模式

当虚拟网卡模式被选中时，当程序启动时加载节点列表完毕后、每当数据网格的内容（含“启用”字段复选框）更新时，程序会筛选“启用”的节点列表，在系统临时目录下生成一个全局代理的V2Ray的负载均衡配置文件`%TEMP%/rbq-ray/config.json`，以此在子进程中启动V2Ray内核，根据是否显示输出窗口的参数配置决定是否显示V2Ray的输出窗口。

同时，调用tun2socks创建一个名为“RBQray”的TUN虚拟网卡，将虚拟网卡的地址设为`169.254.77.9`，子网掩码为`255.255.255.0`，网关地址为`169.254.77.1`<sup>[1]</sup>，确定远程代理服务器的IP地址、解析配置的路由（分流）一并写入系统路由表，指向原先的物理网卡的网关地址，将Metric设为1。设置相应的IPv6地址和路由，以支持IPv6。根据是否显示输出窗口的参数配置决定是否显示tun2socks的输出窗口。

当虚拟网卡模式需要关闭、所有节点被取消启用、程序即将退出时，除了结束tun2socks进程，还要从系统路由表删除由RBQray添加的路由规则。

另外，必须以管理员身份启动程序才能使用虚拟网卡模式，考虑每次启动都申请管理员权限或只有当启动虚拟网卡模式时才申请管理员权限。

#### 参数配置

![](img/7.png)

选择`配置/参数`将打开一个参数配置窗口，可以配置的选项如下：

1. HTTP代理端口：输入框；
2. Socks代理端口：输入框；
3. 显示输出窗口：复选框；
4. 开机自启动：复选框。

参数配置会保存在`settings.json`文件中。

#### 路由配置

路由配置中的“排除局域网”、“排除中国大陆”、“屏蔽广告”、“屏蔽Bittorrent”是预定义选项，“自定义”会打开一个自定义路由配置窗口，用户可以指定需要排除和需要代理的域名或IP地址。

“排除局域网”、“排除中国大陆”、“自定义”路由配置对代理服务器模式和虚拟网卡模式都有效，但是实现的方式不同。在仅开启代理服务器的模式中，分流交给V2Ray内核的路由实现；在虚拟网卡模式下，把相应的分流规则转换成路由表规则写入系统路由表。虽然操作系统路由表的代价比较高，但目前没有更好的办法，Netch的“Bypass LAN and China”预定义规则也是这样实现的。

“屏蔽广告”、“屏蔽Bittorrent”路由配置仅在代理服务器模式有效，在虚拟网卡模式下无法实现。

#### 延迟检测

每当新节点添加、旧节点修改、程序启动时，自动对全部节点地址进行ICMP PING，超时时间为999毫秒，一旦超时显示则`999`，未检测的显示`-1`。应当在新线程中执行检测操作，以免阻塞用户界面。

#### 上下文菜单

![](img/8.png)

上下文菜单是菜单栏的子集，功能一致。

#### 网速显示

通过V2Ray的API应该可以获取到实时网络流量大小。

#### 节点自动保存和读取

每当节点新增、修改、删除时，自动将当前节点列表写入`servers.json`文件。程序启动时从文件中读取。

-   `System.Text.Json`库在默认情况下进行JSON序列化时会进行Unicode字符转义，降低了JSON文本的可读性，某些情形下在其它编程语言平台上会引发解析异常。使用以下设置可以禁用所有字符的Unicode代码转义。
    ```cs
    JsonSerializerOptions options = new JsonSerializerOptions() { 
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    File.WriteAllText("servers.json", JsonSerializer.Serialize(servers, options));
    ```

### 节点设置窗口 `ServerWindow`

![](img/9.png)

节点设置窗口可以根据代理协议、传输协议、安全协议的选项开启或禁用对应的输入框和下拉框，保证用户只能输入对当前选择的协议有效的选项。

-   节点设置窗口类定义了以下常量：
    ```cs
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
    ```
    这些常量的数值是与三个协议下拉框的选项序号对应的，方便对`Server`类的实例`server`的属性进行选择性的操作，包括生成URL（标准URI）。

-   `ServerWindow`类定义了三个构造函数：
    ```cs
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

        //...
    }
    ```
    其中只有后两个构造函数是被用到的，分别被用于添加和编辑节点的场景，传递了需要的参数。

-   URI的字符转义和反转义可以使用以下两个函数，v2rayN也是这样用的：
    ```cs
    public static string EscapeDataString (string stringToEscape);
    public static string UnescapeDataString (string stringToUnescape);
    ```

-   获取主窗口的实例引用可以这样做：
    ```cs
    private MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
    ```
    比网上盛传的写法要简洁得多：
    ```cs
    var _mainWindow = Application.Current.Windows
        .Cast<Window>()
        .FirstOrDefault(window => window is MainWindow) as MainWindow;
    ```

## 结语

如果您对本项目感兴趣，欢迎提出Issue和Pull Request，或通过电子邮件联系`ray@rbq.edu.pl`。希望本文能给您带来一些帮助和启发。

## 参考

1. [什么是外网IP地址、内网IP地址和保留IP地址？](https://zh-hans.ipshu.com/whats_pubip_prvip_revip)

## 日志

### 0.0.1 Alpha 20240508

1. 实现了主窗口；
2. 实现了节点设置窗口；
3. 基本实现了本程序定义的代理协议标准URI，可以进行复制；
4. 实现了节点的添加、编辑、删除功能；
5. 可以将节点列表自动保存到文件，启动时读取。