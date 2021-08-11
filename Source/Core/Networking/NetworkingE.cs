using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace RavEngine {
	public class NetworkingE : EngineCore {
		private UdpClient server;
		private IPEndPoint serverGlobalEndPoint;
		private List<IPEndPoint> serverEndPoints;
		private event PacketData ServerDataEvent;

		private UdpClient client;
		private IPEndPoint clientEndpoint;
		private event PacketData ClientDataEvent;

		public delegate void PacketData(byte[] bytes);

		public NetworkingE() {
			this.client = new UdpClient();
			this.server = new UdpClient();
			this.serverEndPoints = new List<IPEndPoint>();
		}

		internal override void Start() { }

		internal override void Stop() { }

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal override void Update() {
			if (this.client.Available > 0) {
				byte[] serverData = this.client.Receive(ref this.clientEndpoint);
				this.ClientDataEvent?.Invoke(serverData);
			}

			if (this.server.Available > 0) {
				byte[] clientData = this.server.Receive(ref this.serverGlobalEndPoint);
				this.ServerDataEvent?.Invoke(clientData);
			}
		}

		internal override void Reset() {
			this.client = new UdpClient();
			this.server = new UdpClient();
			this.serverEndPoints.Clear();
			this.ClientDataEvent = null;
			this.ServerDataEvent = null;
		}

		public void CreateServer(int port) {
			this.server = new UdpClient(port);
			this.serverGlobalEndPoint = new IPEndPoint(IPAddress.Any, port);
			this.server.Connect(this.serverGlobalEndPoint);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ServerConnectToClient(string ip, int port) {
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
			this.serverEndPoints.Add(endPoint);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ServerDisconnectFromClient(string ip, int port) {
			for (int i = 0; i < this.serverEndPoints.Count; i++) {
				if (this.serverEndPoints[i].Address.ToString() == ip) {
					this.serverEndPoints.RemoveAt(i);
					break;
				}
			}
		}

		public void ServerDisconnectFromAllClients() { this.serverEndPoints.Clear(); }

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ServerSendDataToClient(string ip, int port, byte[] byteData) {
			for (int i = 0; i < this.serverEndPoints.Count; i++) {
				if (this.serverEndPoints[i].Address.ToString() == ip) {
					this.server.Send(byteData, byteData.Length, this.serverEndPoints[i]);
					break;
				}
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ServerSendDataToAllClients(byte[] byteData) {
			for (int i = 0; i < this.serverEndPoints.Count; i++) {
				this.server.Send(byteData, byteData.Length, this.serverEndPoints[i]);
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ServerAddListener(PacketData packetData) { this.ServerDataEvent += packetData; }

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ServerRemoveListener(PacketData packetData) { this.ServerDataEvent -= packetData; }

		[ConsoleCommand("IP_CONNECT")] [MethodImpl(MethodImplOptions.Synchronized)]
		public void ClientConnectToServer(string ip, int port) {
			this.clientEndpoint = new IPEndPoint(IPAddress.Parse(ip), port);
			this.client.Connect(this.clientEndpoint);
		}

		[ConsoleCommand("IP_DISCONNECT")] [MethodImpl(MethodImplOptions.Synchronized)]
		public void ClientDisconnectFromServer() { this.client.Close(); }

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ClientSendData(byte[] byteData) { this.client.Send(byteData, byteData.Length); }

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ClientAddListener(PacketData packetData) { this.ClientDataEvent += packetData; }

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ClientRemoveListener(PacketData packetData) { this.ClientDataEvent -= packetData; }
	}
}