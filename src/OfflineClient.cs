using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TTR {
	public class OfflineClient : Client {

        private float readDelay;
		public string LogFile {private set; get; }
		public OfflineClient (string logFile, float readDelay=1f) {
			this.Mode = ClientMode.Log;
			this.LogFile = logFile;
			this.readDelay = readDelay;
		}

		public override bool Connect() {
			this.Mode = ClientMode.Log;

			if(IsConnected) {
				this.Error("Client is already connected.");
			}
			try {

				this.inStream = new StreamReader(File.Open(LogFile, FileMode.Open));
				this.outStream = null;

				this.StartMessageReceiver();

				return true;
			} catch(Exception e) {
				this.Error(e.ToString());
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
					if(Mode == ClientMode.Log) Thread.Sleep((int)(1000* readDelay));
				}
			}
			catch(ObjectDisposedException e) {
				this.Log("Stream was closed.");
			} catch(Exception e){
				this.Error(e.ToString());
			}
		}

	}
}