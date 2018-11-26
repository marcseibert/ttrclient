using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace TTR {
	public class OnlineClient : Client {

		public string Host {private set; get;}
		public int Port {private set; get;}
		private TcpClient client;

		public OnlineClient (string host, int port) {
			this.Host = host;
			this.Port = port;

			this.Mode = ClientMode.Log;
		}

		public override bool Connect() {

			if(IsConnected) {
				this.Error("Client is already connected.");
				return false;
			}

			this.Log("Opening connection..");
			try{
				this.client = new TcpClient(Host, Port);

				this.inStream = new StreamReader(this.client.GetStream());
				this.outStream = new StreamWriter(this.client.GetStream());

				this.StartMessageReceiver();
				this.IsConnected = true;

				this.Log("Ready.");
				return true;
			}
			catch(SocketException e){
				this.Error(e.Message);
			}

			return false;
		}
		public override bool Close() {
			if(IsConnected) {
				this.inStream.Close();

				this.clientThread.Join();
				this.clientThread = null;

				this.IsConnected = false;

				this.Log("Connection successfully closed.");
				return true;
			} else {
				return false;
			}
		}

		public override void MessageReceiver() {
			string result;
			try{
				while(!this.inStream.EndOfStream && IsConnected) {
					result = this.inStream.ReadLine();

					base.OnNewMessageReceived(result);
				}
			}
			catch(ObjectDisposedException e) {
				this.Log("Stream was closed.");
			}
		}

		public override void Send(String message)
		{
			if(this.outStream != null) {
				this.outStream.WriteLine(message);
				this.outStream.Flush();
			}
			else {
				this.Error("Not connected!");
			}
		}
	}
}