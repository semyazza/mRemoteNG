using mRemoteNG.App;
using mRemoteNG.Connection.Protocol.Http;
using mRemoteNG.Connection.Protocol.SSH;
using mRemoteNG.Connection.Protocol.VNC;
using mRemoteNG.Connection.Protocol.RDP;
using mRemoteNG.Connection.Protocol.Telnet;
using mRemoteNG.Connection.Protocol.Rlogin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;
using mRemoteNG.My;


namespace mRemoteNG.Tools.PortScan
{
	public class ScanHost
    {
        #region Private Variables
        private static int _SSHPort = (int)ProtocolSSH1.Defaults.Port;
        private static int _TelnetPort = (int)ProtocolTelnet.Defaults.Port;
        private static int _HTTPPort = (int)ProtocolHTTP.Defaults.Port;
        private static int _HTTPSPort = (int)ProtocolHTTPS.Defaults.Port;
        private static int _RloginPort = (int)ProtocolRlogin.Defaults.Port;
        private static int _RDPPort = (int)ProtocolRDP.Defaults.Port;
        private static int _VNCPort = (int)ProtocolVNC.Defaults.Port;
        private string _hostName = "";
        private string _hostIp;
        private ArrayList _openPorts = new ArrayList();
        private ArrayList _closedPorts;
        private bool _RDP;
        private bool _VNC;
        private bool _SSH;
        private bool _Telnet;
        private bool _Rlogin;
        private bool _HTTP;
        private bool _HTTPS;
        #endregion

        #region Properties
        public static int SSHPort
		{
			get { return _SSHPort; }
			set { _SSHPort = value; }
		}

        public static int TelnetPort
		{
			get { return _TelnetPort; }
			set { _TelnetPort = value; }
		}

        public static int HTTPPort
		{
			get { return _HTTPPort; }
			set { _HTTPPort = value; }
		}

        public static int HTTPSPort
		{
			get { return _HTTPSPort; }
			set { _HTTPSPort = value; }
		}

        public static int RloginPort
		{
			get { return _RloginPort; }
			set { _RloginPort = value; }
		}

        public static int RDPPort
		{
			get { return _RDPPort; }
			set { _RDPPort = value; }
		}

        public static int VNCPort
		{
			get { return _VNCPort; }
			set { _VNCPort = value; }
		}

        public string HostName
		{
			get { return _hostName; }
			set { _hostName = value; }
		}
				
        public string HostNameWithoutDomain
		{
			get
			{
				if (string.IsNullOrEmpty(HostName) || HostName == HostIp)
				{
					return HostIp;
				}
				return HostName.Split('.')[0];
			}
		}

        public string HostIp
		{
			get { return _hostIp; }
			set { _hostIp = value; }
		}

        public ArrayList OpenPorts
		{
			get { return _openPorts; }
			set { _openPorts = value; }
		}

        public ArrayList ClosedPorts
		{
			get { return _closedPorts; }
			set { _closedPorts = value; }
		}

        public bool RDP
		{
			get { return _RDP; }
			set { _RDP = value; }
		}

        public bool VNC
		{
			get { return _VNC; }
			set { _VNC = value; }
		}

        public bool SSH
		{
			get { return _SSH; }
			set { _SSH = value; }
		}
		
        public bool Telnet
		{
			get { return _Telnet; }
			set { _Telnet = value; }
		}

        public bool Rlogin
		{
			get { return _Rlogin; }
			set { _Rlogin = value; }
		}

        public bool HTTP
		{
			get { return _HTTP; }
			set { _HTTP = value; }
		}

        public bool HTTPS
		{
			get { return _HTTPS; }
			set { _HTTPS = value; }
		}
        #endregion
				
        #region Methods
		public ScanHost(string host)
		{
			_hostIp = host;
			_openPorts = new ArrayList();
			_closedPorts = new ArrayList();
		}
				
		public override string ToString()
		{
			try
			{
				return "SSH: " + Convert.ToString(_SSH) + " Telnet: " + Convert.ToString(_Telnet) + " HTTP: " + Convert.ToString(_HTTP) + " HTTPS: " + Convert.ToString(_HTTPS) + " Rlogin: " + Convert.ToString(_Rlogin) + " RDP: " + Convert.ToString(_RDP) + " VNC: " + Convert.ToString(_VNC);
			}
			catch (Exception)
			{
				Runtime.MessageCollector.AddMessage(Messages.MessageClass.WarningMsg, "ToString failed (Tools.PortScan)", true);
				return "";
			}
		}
				
		public ListViewItem ToListViewItem(bool import)
		{
			try
			{
				ListViewItem listViewItem = new ListViewItem();
				listViewItem.Tag = this;
				if (!string.IsNullOrEmpty(_hostName))
				{
					listViewItem.Text = _hostName;
				}
				else
				{
					listViewItem.Text = _hostIp;
				}
						
				if (import)
				{
					listViewItem.SubItems.Add(BoolToYesNo(_SSH));
					listViewItem.SubItems.Add(BoolToYesNo(_Telnet));
					listViewItem.SubItems.Add(BoolToYesNo(_HTTP));
					listViewItem.SubItems.Add(BoolToYesNo(_HTTPS));
					listViewItem.SubItems.Add(BoolToYesNo(_Rlogin));
					listViewItem.SubItems.Add(BoolToYesNo(_RDP));
					listViewItem.SubItems.Add(BoolToYesNo(_VNC));
				}
				else
				{
					string strOpen = "";
					string strClosed = "";
							
					foreach (int p in _openPorts)
					{
						strOpen += p + ", ";
					}
							
					foreach (int p in _closedPorts)
					{
						strClosed += p + ", ";
					}
							
					listViewItem.SubItems.Add(strOpen.Substring(0, strOpen.Length > 0 ? strOpen.Length - 2 : strOpen.Length));
					listViewItem.SubItems.Add(strClosed.Substring(0, strClosed.Length > 0 ? strClosed.Length - 2 : strClosed.Length));
				}
						
				return listViewItem;
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddExceptionMessage("Tools.PortScan.ToListViewItem() failed.", ex, Messages.MessageClass.WarningMsg, true);
				return null;
			}
		}
				
		private string BoolToYesNo(bool value)
		{
			if (value)
			{
				return Language.strYes;
			}
			else
			{
				return Language.strNo;
			}
		}
				
		public void SetAllProtocols(bool value)
		{
			_VNC = value;
			_Telnet = value;
			_SSH = value;
			_Rlogin = value;
			_RDP = value;
			_HTTPS = value;
			_HTTP = value;
		}
        #endregion
	}
			
	public class Scanner
	{
        #region Private Members
		private List<IPAddress> _ipAddresses = new List<IPAddress>();
		private List<int> _ports = new List<int>();
		private Thread _scanThread;
		private List<ScanHost> _scannedHosts = new List<ScanHost>();
        #endregion
				
        #region Public Methods
		public Scanner(IPAddress ipAddress1, IPAddress ipAddress2)
		{
			IPAddress ipAddressStart = IpAddressMin(ipAddress1, ipAddress2);
			IPAddress ipAddressEnd = IpAddressMax(ipAddress1, ipAddress2);
					
			_ports.Clear();
            _ports.AddRange(new int[] { ScanHost.SSHPort, ScanHost.TelnetPort, ScanHost.HTTPPort, ScanHost.HTTPSPort, ScanHost.RloginPort, ScanHost.RDPPort, ScanHost.VNCPort });
					
			_ipAddresses.Clear();
			_ipAddresses.AddRange(IpAddressArrayFromRange(ipAddressStart, ipAddressEnd));
					
			_scannedHosts.Clear();
		}
		
		public Scanner(IPAddress ipAddress1, IPAddress ipAddress2, int port1, int port2) : this(ipAddress1, ipAddress2)
		{
					
			int portStart = Math.Min(port1, port2);
			int portEnd = Math.Max(port1, port2);
					
			_ports.Clear();
			for (int port = portStart; port <= portEnd; port++)
			{
				_ports.Add(port);
			}
		}
				
		public void StartScan()
		{
			_scanThread = new Thread(new System.Threading.ThreadStart(ScanAsync));
			_scanThread.SetApartmentState(ApartmentState.STA);
			_scanThread.IsBackground = true;
			_scanThread.Start();
		}
				
		public void StopScan()
		{
			_scanThread.Abort();
		}
				
		public static bool IsPortOpen(string hostname, string port)
		{
			try
			{
				System.Net.Sockets.TcpClient tcpClient = new System.Net.Sockets.TcpClient(hostname, Convert.ToInt32(port));
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
        #endregion
				
        #region Private Methods
		private void ScanAsync()
		{
			try
			{
				int hostCount = 0;
				foreach (IPAddress ipAddress in _ipAddresses)
				{
					if (BeginHostScanEvent != null)
						BeginHostScanEvent(ipAddress.ToString());
							
					ScanHost scanHost = new ScanHost(ipAddress.ToString());
					hostCount++;
							
					if (!IsHostAlive(ipAddress))
					{
						scanHost.ClosedPorts.AddRange(_ports);
						scanHost.SetAllProtocols(false);
					}
					else
					{
						foreach (int port in _ports)
						{
							bool isPortOpen = false;
							try
							{
								System.Net.Sockets.TcpClient tcpClient = new System.Net.Sockets.TcpClient(ipAddress.ToString(), port);
								isPortOpen = true;
								scanHost.OpenPorts.Add(port);
							}
							catch (Exception)
							{
								isPortOpen = false;
								scanHost.ClosedPorts.Add(port);
							}
							
							if (port == ScanHost.SSHPort)
							{
								scanHost.SSH = isPortOpen;
							}
                            else if (port == ScanHost.TelnetPort)
							{
								scanHost.Telnet = isPortOpen;
							}
                            else if (port == ScanHost.HTTPPort)
							{
								scanHost.HTTP = isPortOpen;
							}
                            else if (port == ScanHost.HTTPSPort)
							{
								scanHost.HTTPS = isPortOpen;
							}
                            else if (port == ScanHost.RloginPort)
							{
								scanHost.Rlogin = isPortOpen;
							}
                            else if (port == ScanHost.RDPPort)
							{
								scanHost.RDP = isPortOpen;
							}
                            else if (port == ScanHost.VNCPort)
							{
								scanHost.VNC = isPortOpen;
							}
						}
					}
							
					try
					{
						scanHost.HostName = Dns.GetHostEntry(scanHost.HostIp).HostName;
					}
					catch (Exception)
					{
					}
					if (string.IsNullOrEmpty(scanHost.HostName))
					{
						scanHost.HostName = scanHost.HostIp;
					}
							
					_scannedHosts.Add(scanHost);
					if (HostScannedEvent != null)
						HostScannedEvent(scanHost, hostCount, _ipAddresses.Count);
				}
						
				if (ScanCompleteEvent != null)
					ScanCompleteEvent(_scannedHosts);
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(Messages.MessageClass.WarningMsg, "StartScanBG failed (Tools.PortScan)" + Environment.NewLine + ex.Message, true);
			}
		}
				
		private static bool IsHostAlive(IPAddress ipAddress)
		{
			Ping pingSender = new Ping();
			PingReply pingReply;
					
			try
			{
				pingReply = pingSender.Send(ipAddress);
						
				if (pingReply.Status == IPStatus.Success)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}
				
		private static IPAddress[] IpAddressArrayFromRange(IPAddress ipAddress1, IPAddress ipAddress2)
		{
			IPAddress startIpAddress = IpAddressMin(ipAddress1, ipAddress2);
			IPAddress endIpAddress = IpAddressMax(ipAddress1, ipAddress2);
					
			int startAddress = IpAddressToInt32(startIpAddress);
			int endAddress = IpAddressToInt32(endIpAddress);
			int addressCount = endAddress - startAddress;
					
			IPAddress[] addressArray = new IPAddress[addressCount + 1];
			int index = 0;
			for (int address = startAddress; address <= endAddress; address++)
			{
				addressArray[index] = IpAddressFromInt32(address);
				index++;
			}
					
			return addressArray;
		}
				
		private static IPAddress IpAddressMin(IPAddress ipAddress1, IPAddress ipAddress2)
		{
			if (IpAddressCompare(ipAddress1, ipAddress2) < 0) // ipAddress1 < ipAddress2
			{
				return ipAddress1;
			}
			else
			{
				return ipAddress2;
			}
		}
				
		private static IPAddress IpAddressMax(IPAddress ipAddress1, IPAddress ipAddress2)
		{
			if (IpAddressCompare(ipAddress1, ipAddress2) > 0) // ipAddress1 > ipAddress2
			{
				return ipAddress1;
			}
			else
			{
				return ipAddress2;
			}
		}
				
		private static int IpAddressCompare(IPAddress ipAddress1, IPAddress ipAddress2)
		{
			return IpAddressToInt32(ipAddress1) - IpAddressToInt32(ipAddress2);
		}
				
		private static int IpAddressToInt32(IPAddress ipAddress)
		{
			if (!(ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
			{
				throw (new ArgumentException("ipAddress"));
			}
					
			byte[] addressBytes = ipAddress.GetAddressBytes(); // in network order (big-endian)
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(addressBytes); // to host order (little-endian)
			}
			Debug.Assert(addressBytes.Length == 4);
					
			return BitConverter.ToInt32(addressBytes, 0);
		}
				
		private static IPAddress IpAddressFromInt32(int ipAddress)
		{
			byte[] addressBytes = BitConverter.GetBytes(ipAddress); // in host order
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(addressBytes); // to network order (big-endian)
			}
			Debug.Assert(addressBytes.Length == 4);

            return new IPAddress(addressBytes);
		}
        #endregion
				
        #region Events
		public delegate void BeginHostScanEventHandler(string host);
		private BeginHostScanEventHandler BeginHostScanEvent;
				
		public event BeginHostScanEventHandler BeginHostScan
		{
			add
			{
				BeginHostScanEvent = (BeginHostScanEventHandler) System.Delegate.Combine(BeginHostScanEvent, value);
			}
			remove
			{
				BeginHostScanEvent = (BeginHostScanEventHandler) System.Delegate.Remove(BeginHostScanEvent, value);
			}
		}
				
		public delegate void HostScannedEventHandler(ScanHost scanHost, int scannedHostCount, int totalHostCount);
		private HostScannedEventHandler HostScannedEvent;
				
		public event HostScannedEventHandler HostScanned
		{
			add
			{
				HostScannedEvent = (HostScannedEventHandler) System.Delegate.Combine(HostScannedEvent, value);
			}
			remove
			{
				HostScannedEvent = (HostScannedEventHandler) System.Delegate.Remove(HostScannedEvent, value);
			}
		}
				
		public delegate void ScanCompleteEventHandler(List<ScanHost> hosts);
		private ScanCompleteEventHandler ScanCompleteEvent;
				
		public event ScanCompleteEventHandler ScanComplete
		{
			add
			{
				ScanCompleteEvent = (ScanCompleteEventHandler) System.Delegate.Combine(ScanCompleteEvent, value);
			}
			remove
			{
				ScanCompleteEvent = (ScanCompleteEventHandler) System.Delegate.Remove(ScanCompleteEvent, value);
			}
		}
        #endregion
	}
}