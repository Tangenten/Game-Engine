using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

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
		}

		internal override void Start() {
			this.client = new UdpClient();
			this.server = new UdpClient();
			this.serverEndPoints = new List<IPEndPoint>();
		}

		internal override void Stop() {
		}

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
			this.serverEndPoints = new List<IPEndPoint>();
		}

		public void CreateServer(int port) {
			this.server = new UdpClient(port);
			this.serverGlobalEndPoint = new IPEndPoint(IPAddress.Any, port);
			this.server.Connect(this.serverGlobalEndPoint);
		}

		public void ServerConnectToClient(string ip, int port) {
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
			this.serverEndPoints.Add(endPoint);
		}

		public void ServerDisconnectFromClient(string ip, int port) {
			for (int i = 0; i < this.serverEndPoints.Count; i++) {
				if (this.serverEndPoints[i].Address.ToString() == ip) {
					this.serverEndPoints.RemoveAt(i);
					break;
				}
			}
		}

		public void ServerDisconnectFromAllClients() {
			this.serverEndPoints.Clear();
		}

		public void ServerSendDataToClient(string ip, int port, byte[] byteData) {
			for (int i = 0; i < this.serverEndPoints.Count; i++) {
				if (this.serverEndPoints[i].Address.ToString() == ip) {
					this.server.Send(byteData, byteData.Length, this.serverEndPoints[i]);
					break;
				}
			}
		}

		public void ServerSendDataToAllClients(byte[] byteData) {
			for (int i = 0; i < this.serverEndPoints.Count; i++) {
				this.server.Send(byteData, byteData.Length, this.serverEndPoints[i]);
			}
		}

		public void ServerAddListener(PacketData packetData) {
			this.ServerDataEvent += packetData;
		}

		public void ServerRemoveListener(PacketData packetData) {
			this.ServerDataEvent -= packetData;
		}

		public void ClientConnectToServer(string ip, int port) {
			this.clientEndpoint = new IPEndPoint(IPAddress.Parse(ip), port);
			this.client.Connect(this.clientEndpoint);
		}

		public void ClientDisconnectFromServer() {
			this.client.Close();
		}

		public void ClientSendData(byte[] byteData) {
			this.client.Send(byteData, byteData.Length);
		}

		public void ClientAddListener(PacketData packetData) {
			this.ClientDataEvent += packetData;
		}

		public void ClientRemoveListener(PacketData packetData) {
			this.ClientDataEvent -= packetData;
		}
	}
}
