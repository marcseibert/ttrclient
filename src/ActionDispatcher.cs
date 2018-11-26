using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace TTR {
    public class ActionDispatcher : MonoBehaviour {

        public Client client {private set; get;}
        public bool RequestsAvailable { private set; get; }

        public bool IsHalted { private set; get; }
        private float haltDuration = 0;
        private Queue<Protocol.Message> pendingMessages;

        // EVENTS
        public event EventHandler<Protocol.TurnReq> OnReceivedTurnRequest; 
        public event EventHandler<Protocol.TurnResp> OnReceivedResponse;
        
        private Action<Protocol.TurnResp>[] OnReceivedActionResponse;
        private Action OnClientReady;
        
        public TTR.ClientMode Mode;

        private bool isInitialized = false;

        void Awake() {
            // Init(ClientMode.Log, "/Users/marcseibert/Desktop/log.txt");
        }

        public void Init(ClientMode mode, string address, int port=8080) {
            if(this.isInitialized) {
                this.Error("ActionDispatcher was initialized already.");
                return;
            }

            this.Mode = mode;
            pendingMessages = new Queue<Protocol.Message>();
            OnReceivedActionResponse = new Action<Protocol.TurnResp>[Enum.GetNames(typeof(Protocol.TurnType)).Length];

            // SETUP CLIENT
            if(Mode == ClientMode.Log) {
                client = new OfflineClient(address);
            }
            else {
                client = new OnlineClient(address, port);
            }
            client.OnMessageReceived += new EventHandler<string>(this.ProcessServerMessage);

            if(this.client.Connect()){

                if(this.OnClientReady != null) {
                    OnClientReady();
                    OnClientReady = null;
                    this.Log("Connected to the server.");
                }
            } else {
                this.Error("Could not connect to the server.");
            }

            IsHalted = false;
        }

        void Update() {
            if(!IsHalted && this.pendingMessages != null) {
                lock(this.pendingMessages){
                    if(this.pendingMessages.Count > 0) {
                        var message = this.pendingMessages.Peek();

                        if(DispatchMessage(message)) {
                            this.pendingMessages.Dequeue();
                        } else {
                            this.Log("Halting Action Dispatcher because there is no event handler.");
                            Pause();
                        }
                    }
                }
            } else {
                if(haltDuration > 0 && haltDuration <= Time.time) {
                    Resume();
                } 
            }
        }
        
        public bool IsMessageAvailable() {
            return this.pendingMessages != null && this.pendingMessages.Count > 0;
        }
        
        /*
            Returns a single message from the pending messages queue.
         */
        public Protocol.Message GetNextMessage() {
            if(this.pendingMessages == null || this.pendingMessages.Count == 0) {
                return null;
            }

            return this.pendingMessages.Dequeue();
        }

        /*
            Can be used to ensure, that the the client is already up and running before calling delayedAcition.  
        */
        public void AwaitConnection(Action delayedAction) {
            if(this.client != null && this.client.IsConnected) {
                delayedAction();
            } else {
                OnClientReady += delayedAction;
            }
        }

        public bool JoinGame(string name, Protocol.ClientType clientType, Action<Protocol.TurnResp> callback=null) { 
            if(IsActionBlocked(Protocol.TurnType.Join, true)){
                return false;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("Join ")
                    .Append(name)
                    .Append(" ")
                    .Append(clientType.ToString());

            this.OnReceivedActionResponse[(int) Protocol.TurnType.Join] += callback;
            this.client.Send(builder.ToString());

            return true;
        }

        public bool DrawHiddenPassengerCar(Action<Protocol.TurnResp> callback=null) { 
            if(IsActionBlocked(Protocol.TurnType.DrawPassengerCars, true)){
                return false;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("DrawPassengerCars true");

            this.OnReceivedActionResponse[(int) Protocol.TurnType.DrawPassengerCars] += callback;
            this.client.Send(builder.ToString());

            return true;
        }
        
        public bool DrawOpenPassengerCar(Protocol.PassengerCarColor color, Action<Protocol.TurnResp> callback=null) {
            if(IsActionBlocked(Protocol.TurnType.DrawPassengerCars, true)){
                return false;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("DrawPassengerCars false ")
                    .Append(color.ToString());

            this.OnReceivedActionResponse[(int) Protocol.TurnType.DrawPassengerCars] += callback;
            this.client.Send(builder.ToString());

            return true;
        }

        public bool DrawDestinationTickets(Action<Protocol.TurnResp> callback=null) { 
            if(IsActionBlocked(Protocol.TurnType.DrawDestinationTickets, true)){
                return false;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("DrawDestinationTickets");

            this.OnReceivedActionResponse[(int) Protocol.TurnType.DrawDestinationTickets] += callback;
            this.client.Send(builder.ToString());

            return true;
        }


        public bool ClaimDestinationTickets(bool cardA, bool cardB, bool cardC, Action<Protocol.TurnResp> callback=null) { 
            if(IsActionBlocked(Protocol.TurnType.ClaimDestinationTickets, true)){
                return false;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("ClaimDestinationTickets ")
                    .Append(cardA.ToString())
                    .Append(" ")
                    .Append(cardB.ToString())
                    .Append(" ")
                    .Append(cardC.ToString());

            this.OnReceivedActionResponse[(int) Protocol.TurnType.ClaimDestinationTickets] += callback;

            this.client.Send(builder.ToString());

            return true;
        }

        public bool ClaimRoute(Protocol.Destination destA, Protocol.Destination destB, Protocol.PassengerCarColor routeColor, Protocol.PassengerCarColor passengerCarColor, Action<Protocol.TurnResp> callback=null) {
            if(IsActionBlocked(Protocol.TurnType.ClaimRoute, true)){
                return false;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("ClaimRoute ")
                    .Append(destA.ToString())
                    .Append(" ")
                    .Append(destB.ToString())
                    .Append(" ")
                    .Append(routeColor)
                    .Append(" ")
                    .Append(passengerCarColor);

            this.OnReceivedActionResponse[(int) Protocol.TurnType.ClaimRoute] += callback;

            this.client.Send(builder.ToString());

            return true;
         }

        public bool GetBoardState(Action<Protocol.TurnResp> callback=null) { 
            if(IsActionBlocked(Protocol.TurnType.BoardState, true)){
                return false;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("BoardState");

            this.OnReceivedActionResponse[(int) Protocol.TurnType.BoardState] += callback;

            this.client.Send(builder.ToString());
            
            return true;
        }

        public bool GetAllRoutes(Action<Protocol.TurnResp> callback=null) { 
            if(IsActionBlocked(Protocol.TurnType.ListAllRoutes, true)){
                return false;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("ListAllRoutes");

            this.OnReceivedActionResponse[(int) Protocol.TurnType.ListAllRoutes] += callback;
            this.client.Send(builder.ToString());

            return true;
        }

        /*
            There can always be just one active request-response cycle at a time.
         */
        public bool IsActionBlocked(Protocol.TurnType turnType, bool log=false) {
            if(this.OnReceivedActionResponse[(int) turnType] != null) {

                if(log) {
                    this.Error("This Action (" + turnType +") is already awaiting a response.");
                }

                return true;
            }
            return false;
        }

        /* 
            Stops the ActionDispatcher from automatically dispatching new event.
         */
        public void Pause() {
            this.IsHalted = true;
            this.haltDuration = 0f;
        }

        public void Pause(float duration) {
            this.IsHalted = true;
            
            if(duration > 0) {
                this.haltDuration = duration + Time.time;
            } else {
                this.Error("Pause duration has to be positive.");
            }
        }

        /*
            Resumes the ActionDispatcher.
         */
        public void Resume() {
            this.IsHalted = false;
            this.haltDuration = 0;
        }

        /*
            Handles the OnMessageReceived Event from the client.
         */
        private void ProcessServerMessage(object sender, string message) {
            Debug.Log("Received Server Message: " +message);
            var messageObject = JSONParser.Parse(message);
            Debug.Log("parsed object " + (pendingMessages == null));

            lock(this.pendingMessages) {
                this.pendingMessages.Enqueue(messageObject);
                Debug.Log("enqueued this request!");
            }
        }

        /*
            Decides upon the message type which event to trigger.
         */
        private bool DispatchMessage(Protocol.Message message) {
            if(message.Type == Protocol.MessageType.Info) {
                Debug.Log("i: " + message);
                Protocol.TurnResp response = (Protocol.TurnResp) message;
                if(OnReceivedActionResponse[(int) response.turnType] != null) {
                    OnReceivedActionResponse[(int) response.turnType](response);
                    OnReceivedActionResponse[(int) response.turnType] = null; // COULD BE AN ISSUE IF AWAITING MULTIPLE RESPONSES OF SAME TYPE
                } else if(OnReceivedResponse != null) {
                    EventHandler<Protocol.TurnResp> handler = OnReceivedResponse;
                    handler(this, response);
                } else {
                    return false;
                }
                
            } else if(message.Type == Protocol.MessageType.Request) {
                Debug.Log("r: " + message);
                Protocol.TurnReq request = (Protocol.TurnReq) message;

                if(OnReceivedTurnRequest != null) {
                    // TURN REQUEST
                    EventHandler<Protocol.TurnReq> handler = OnReceivedTurnRequest;
                    handler(this, request);
                } else {
                    return false;
                }
            }
            else if(message.Type == Protocol.MessageType.TextMessage) {
                Debug.Log("[Server Message] " + ((Protocol.TextResp)message).text);
            }

            return true;
        }

        void OnApplicationQuit() {
            if(this.client != null) this.client.Close();
        }
        protected void Log(string message) {
			Debug.Log("[ActionDispatcher] " + message);
		}

		protected void Error(string message) {
			Debug.LogError("[ActionDispatcher] " + message);
		}
    }
}