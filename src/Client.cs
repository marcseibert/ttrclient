using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TTR{
	public enum ClientMode { Log, Live }
	public abstract class Client {

		public ClientMode Mode { protected set; get; }
		public bool IsConnected { protected set; get; }

		protected Thread clientThread;

		protected StreamWriter outStream;
		protected StreamReader inStream;

		public event EventHandler<string> OnMessageReceived;

		public Client() {

			this.IsConnected = false;
		}

		public void StartMessageReceiver() {
				this.Log("Starting Message Receiver..");

				this.clientThread = new Thread(new ThreadStart(MessageReceiver));
				this.clientThread.IsBackground = true;
				this.clientThread.Start();

				IsConnected = true;
		}

		public abstract void MessageReceiver();
		public abstract bool Connect();
		public abstract bool Close();

		public virtual void OnNewMessageReceived(string message) {
			EventHandler<string> handler = OnMessageReceived;
			if (handler != null) {
				handler(this, message);
			}
		}

		protected void Log(string message) {
			Debug.Log("[GameServer] " + message);
		}

		protected void Error(string message) {
			Debug.LogError("[GameServer] " + message);
		}

		public virtual void Send(string message) {
			this.Error("Sending is not implemented.");
		}

	}

}