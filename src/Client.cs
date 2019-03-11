using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using TTR.Protocol;

namespace TTR{
	public enum ClientMode { Log, Live }
	public abstract class Client {
		
		public const LogLevel logLevel = LogLevel.Info;
		public ClientMode Mode { protected set; get; }
		public bool Connected { protected set; get; }

		protected Thread clientThread;

		protected StreamWriter outStream;
		protected StreamReader inStream;

		public event EventHandler<string> OnMessageReceived;

        protected Client() {

			this.Connected = false;
		}

		public void StartMessageReceiver() {
			this.Log("Starting Message Receiver..");

            this.clientThread = new Thread(new ThreadStart(MessageReceiver))
            {
                IsBackground = true
            };

            this.clientThread.Start();

			Connected = true;
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
			if(Client.logLevel > 1) {
           		Console.WriteLine("[GameServer] " + message);
			}
		}

		protected void Error(string message) {
			if(Client.logLevel > 0) {
				Console.WriteLine("[GameServer] " + message);
			}
		}

		public virtual void Send(string message) {
			this.Error("Sending is not implemented.");
		}

	}

}
