using System;
using System.Text;
using System.Collections.Generic;
using TTR.Protocol;

namespace TTR {

    public enum LogLevel {
        None,
        Error,
        Info,
    }

    public class ActionDispatcher {
        
        public const LogLevel logLevel = LogLevel.INFO;

        public Client client {private set; get;}
        private Queue<Message> pendingMessages;

        // PROPERTIES
        public bool RequestsAvailable { private set; get; }
        public bool Paused { private set; get; }
        private DateTime haltDuration;

        // EVENTS
        public event EventHandler<TurnReq> OnReceivedTurnRequest; 
        public event EventHandler<TurnResp> OnReceivedResponse;
        
        private Action<TurnResp>[] OnReceivedActionResponse;
        private Action OnClientReady;
        

        private bool initialized = false;
        private bool closed = false;

        public ActionDispatcher(ClientMode mode, string address, int port=8080) {
            if(this.initialized) {
                this.Error("ActionDispatcher was initialized already.");
                return;
            }

            pendingMessages = new Queue<Message>();
            OnReceivedActionResponse = new Action<TurnResp>[Enum.GetNames(typeof(TurnType)).Length];

            // SETUP CLIENT
            if(mode == ClientMode.Log) {
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

            this.Paused = false;
        }

        public bool Update() {
            if(!closed && !Paused && this.pendingMessages != null) {
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
                if(haltDuration != null && haltDuration <= DateTime.Now) {
                    Resume();
                } 
            }

            return !closed;
        }
        
        public bool IsMessageAvailable() {
            return this.pendingMessages != null && this.pendingMessages.Count > 0;
        }

        public void Close()
        {
            this.closed = true;
            this.client.Close();
        }

        /*
            Returns a single message from the pending messages queue.
         */
        public Message GetNextMessage() {
            if(this.pendingMessages == null || this.pendingMessages.Count == 0) {
                return null;
            }

            return this.pendingMessages.Dequeue();
        }

        /*
            Can be used to ensure that the client is already up and running before calling delayedAction.  
        */
        public void AwaitConnection(Action delayedAction) {
            if(this.client != null && this.client.IsConnected) {
                delayedAction();
            } else {
                OnClientReady += delayedAction;
            }
        }

        public bool JoinGame(string name, ClientType clientType, Action<TurnResp> callback=null) { 
            return JoinGame(name, clientType, PlayerColor.PlayerNone, callback);
        }

        public bool JoinGame(string name, ClientType clientType, PlayerColor colorWish, Action<TurnResp> callback=null) { 
            if(IsActionBlocked(TurnType.Join)){
                return false;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("Join ")
                    .Append(name)
                    .Append(" ")
                    .Append(clientType.ToString())
                    ;
                    if(colorWish != PlayerColor.PlayerNone) {
                        builder.Append(" ")
                            .Append(colorWish.ToString());
                    }

            this.OnReceivedActionResponse[(int) TurnType.Join] += callback;
            this.client.Send(builder.ToString());

            return true;
        }

        public bool DrawHiddenPassengerCar(Action<TurnResp> callback=null) { 
            return DrawPassengerCar(true, PassengerCarColor.Rainbow, callback);
        }
        
        public bool DrawOpenPassengerCar(PassengerCarColor color, Action<TurnResp> callback=null) {
            return DrawPassengerCar(false, color, callback);
        }

        private bool DrawPassengerCar(bool hiddenDeck, PassengerCarColor color, Action<TurnResp> callback=null) {
            if(IsActionBlocked(TurnType.DrawPassengerCars)){
                return false;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("DrawPassengerCars ")
                    .Append(hiddenDeck);

            if(!hiddenDeck) {
                builder.Append(" ")
                       .Append(color.ToString());
            }

            this.OnReceivedActionResponse[(int) TurnType.DrawPassengerCars] += callback;
            this.client.Send(builder.ToString());
            return true;
        }

        public bool DrawDestinationTickets(Action<TurnResp> callback=null) { 
            if(IsActionBlocked(TurnType.DrawDestinationTickets)){
                return false;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("DrawDestinationTickets");

            this.OnReceivedActionResponse[(int) TurnType.DrawDestinationTickets] += callback;
            this.client.Send(builder.ToString());

            return true;
        }


        public bool ClaimDestinationTickets(bool cardA, bool cardB, bool cardC, Action<TurnResp> callback=null) { 
            if(IsActionBlocked(TurnType.ClaimDestinationTickets)){
                return false;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("ClaimDestinationTickets ")
                    .Append(cardA.ToString())
                    .Append(" ")
                    .Append(cardB.ToString())
                    .Append(" ")
                    .Append(cardC.ToString());

            this.OnReceivedActionResponse[(int) TurnType.ClaimDestinationTickets] += callback;

            this.client.Send(builder.ToString());

            return true;
        }

        public bool ClaimRoute(Destination destA, Destination destB, PassengerCarColor targetRouteColor, Action<TurnResp> callback=null) {
            if(IsActionBlocked(TurnType.ClaimRoute)){
                return false;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("ClaimRoute ")
                    .Append(destA.ToString())
                    .Append(" ")
                    .Append(destB.ToString())
                    .Append(" ")
                    .Append(targetRouteColor);

            this.OnReceivedActionResponse[(int) TurnType.ClaimRoute] += callback;

            this.client.Send(builder.ToString());

            return true;
         }

        public bool GetBoardState(Action<TurnResp> callback=null) { 
            if(IsActionBlocked(TurnType.BoardState)){
                return false;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("BoardState");

            this.OnReceivedActionResponse[(int) TurnType.BoardState] += callback;

            this.client.Send(builder.ToString());
            
            return true;
        }

        public bool GetAllRoutes(Action<TurnResp> callback=null) { 
            if(IsActionBlocked(TurnType.ListAllRoutes)){
                return false;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("ListAllRoutes");

            this.OnReceivedActionResponse[(int) TurnType.ListAllRoutes] += callback;
            this.client.Send(builder.ToString());

            return true;
        }

        /*
            Returns false if an action is already awaiting a response.
         */
        public bool IsActionBlocked(TurnType turnType) {
            if(this.OnReceivedActionResponse[(int) turnType] != null) {
                this.Error("Ignoring Action. This Action (" + turnType +") is already awaiting a response.");

                return true;
            }
            return false;
        }

        /* 
            Stops the ActionDispatcher from automatically dispatching new events.
         */
        public void Pause() {
            this.Paused = true;
        }

        /*
            Pauses the ActionDispatcher for a given duration in seconds.
         */
        public void Pause(float duration) {
            this.Paused = true;
            
            if(duration > 0) {
                this.haltDuration = DateTime.Now.AddSeconds(duration);
            } else {
                this.Error("Pause duration has to be positive.");
            }
        }

        /*
            Resumes the ActionDispatcher.
         */
        public void Resume() {
            this.Paused = false;
        }

        /*
            Handles the OnMessageReceived Event from the client.
         */
        private void ProcessServerMessage(object sender, string message) {
            Message messageObject = Message.Parser.ParseJson(message);

            lock(this.pendingMessages) {
                this.pendingMessages.Enqueue(messageObject);
            }
        }

        /*
            Triggers event based on message type.
            Returns false if there is no suitable event.
         */
        private bool DispatchMessage(Message message) {
            if(message.Type == MessageType.Info) {
                this.Log("i: " + message);

                TurnResp response = message.TurnResp;

                // GETS CALLED IF A CALLBACK METHOD IS AWAITING A RESPONSE.
                if(OnReceivedActionResponse[(int) response.TurnType] != null) {
                    OnReceivedActionResponse[(int) response.TurnType](response);
                    OnReceivedActionResponse[(int) response.TurnType] = null; // WRONG IF EVENT HAS MULTIPLE SUBSCRIBERS!

                // IF NO SPECIFIC CALLBACK WAS DEFINIED CHECK FOR A GLOBAL EVENT SUBSCRIBER.
                } else if(OnReceivedResponse != null) {
                    OnReceivedResponse.Invoke(this, response);
                }
                else {
                    return false;
                }
                
            } else if(message.Type == MessageType.Request) {
                this.Log("r: " + message);
                TurnReq request = message.TurnReq;

                if(OnReceivedTurnRequest != null) {
                    // TURN REQUEST
                    EventHandler<TurnReq> handler = OnReceivedTurnRequest;
                    handler(this, request);
                } else {
                    return false;
                }
            }
            else if(message.Type == MessageType.TextMessage) {
                this.Log("[Server Message] " + message.TextResp.Text);
            }

            return true;
        }

        void OnApplicationQuit() {
            if(this.client != null) this.client.Close();
        }

        protected void Log(string message) {
            if((int) ActionDispatcher.logLevel > 1) {
			    Console.WriteLine("[ActionDispatcher] " + message);
            }
		}

		protected void Error(string message) {
	        if((int) ActionDispatcher.logLevel > 0) {
                Console.WriteLine("[ERROR] [ActionDispatcher] " + message);
            }
		}
    }


}
