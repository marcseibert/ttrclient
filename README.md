# Diese Version funktioniert auch ohne Unity. Hier in der README gibt es dazu auch Beispiele :)

# ttrclient
Einfache Bibliothek, die zur Kommunikation mit dem TTR-Server genutzt werden kann. Client-Server Kommunikation und JSON Parsen 
wird automatisch gehandhabt.

# Installation

1. Auf **Assets/Import Packages/Custom** Package klicken.
2. Das File **ttrclient.unitypackage** auswählen.
3. Ein GameObject erstellen und den **ActionDispatcher.cs** als Komponente hinzufügen.
4. Die Methode void Init(TTR.ClientMode mode, string address, int port) muss aufgerufen werden, bevor der ActionDispatcher benutzt wird.

# Module
Der Client kann in zwei Varianten initialisiert werden. Die erste Variante ist der sogenannte **Offline Client**. Der Offline Client muss mit **ClientModus.Log** initialisiert werden. Als zweite Option gibt es den **Online Client**. Beide Clients verhalten sich gleich. Der **Offline Client** simuliert den **Online Cient** indem er nach jeder Zeile den Message Receiver Thread für eine gewisse Zeit blockiert.

# Funktionen
Für jeden Befehl des CLI Clients gibt es eine Methode im **ActionDispatcher.cs**. Neben diesen gibt es noch weitere Hilfsmethoden um das verarbeiten zu erleichtern.

```csharp
public void AwaitConnection(System.Action delayedAction)
```
Sobald der Client mit dem Server verbunden ist, wird die Action **delayedAction** ausgeführt. Wenn die Verbindung bereits steht, dann wird die **delayedAction** direkt ausgelöst.

```csharp
public void Pause(float duration)
```
Erlaubt es den Action Dispatcher für eine **duration** (in Sekunden) zu pausieren. Das heißt, dass alle ankommenden Messages gepuffert werden aber keines der Events (OnReceivedTurnRequest, OnReceivedResponse,OnReceivedActionResponse) ausgelöst wird.
Das Argument **duration** ist optional. Wenn man die Methode ohne Parameter bzw. einer **duration** <= 0 aufruft, dann wird der ActionDispatcher dauerhaft angehlaten. Mithilfe der **Resume()** Methode, kann der ActionDispatcher wieder aktiviert werden.


# Wie benutze ich den Client in einer Command Line Application
```csharp
        public static void Main(string[] args)
        {
            string address = "127.0.0.1";
            int port = 8080;

            if(args.Length > 0)
            {
                address = args[0];
                if(args.Length > 1)
                {
                    int.TryParse(args[1], out port);
                }
            }

            ActionDispatcher dispatcher = new ActionDispatcher(ClientMode.Live, address, port);
            dispatcher.OnReceivedResponse += IgnoreInfos; // Ignore Infos ist eine Methode, die Info Messages verwirft
		
	   // Es bietet sich vielleicht an ein Interface oder Aehnliches fuer euren Agent zu verwenden
            AgentController agent = new AgentController(AgentType.RouteFocusedAgent, dispatcher);

            while (dispatcher.Update()) { } // MAIN LOOP

            Environment.Exit(0); // Beendet eine Konsolenanwendung
        }
```

Hier eine beispielhafte OnTurnRequest Methode
```
public void OnTurnRequest(object sender, TurnReq request)
{
	// Der Server wiederholt nach jeder Request vom Client die initiale Request. Das wird hier abgefangen.
	if (turnActive) { return; }
        turnActive = true;

        if(request.turnType == TurnType.Join)
        {
		// Hier werden Lambda Expressions verwendet
       		dispatcher.JoinGame(name, ClientType.Player, (r) =>
               	{
                    dispatcher.GetAllRoutes((response) =>
                    {

                        Console.WriteLine("Hurra du hast es geschafft! Fuer weitere Informationen sieh https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/");
                        turnActive = false;
                    });
                });
        }
}
```

# Online Example
```csharp
TTR.ActionDispatcher dispatcher;

void Start () {
  // GET INSTANCE OF ACTION DISPATCHER
  dispatcher = Transform.FindObjectOfType<TTR.ActionDispatcher>();	

  // SUBSCRIBE TO EVENT
  dispatcher.OnReceivedTurnRequest += new System.EventHandler<TTR.Protocol.TurnReq>(OnReceivedTurnRequest);

  dispatcher.Init(ClientMode.Live, "127.0.0.1", 8080);
}

private void OnReceivedTurnRequest(object sender, TTR.Protocol.TurnReq request) {
  if(request.turnType == TTR.Protocol.TurnType.Join) {
    dispatcher.JoinGame("Marc", TTR.Protocol.ClientType.Player, (response) => {
      if(response.success) {
        Debug.Log("Successfully joined the game.");
        // ...
        }
     });
   }
}
```

# Offline Example
## Befehle empfange und bearbeiten.
```csharp
TTR.ActionDispatcher dispatcher;
  
void Start() {
  dispatcher = Transform.FindObjectOfType<TTR.ActionDispatcher>();
  dispatcher.Init(TTR.ClientMode.Log, "/Users/marcseibert/Desktop/log.txt");
  dispatcher.Pause();
}

void Update() {
  if(this.dispatcher.IsMessageAvailable()) {
    var message = this.dispatcher.GetNextMessage();

    if(message.Type == TTR.Protocol.MessageType.Info) {
       // DO WHATEVER
    }
  }
}
```

# Demo Handler
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TTR {

	public class OldDemoHandler : MonoBehaviour {

		public StatusText statusText;
		public WagoncardStack stack;
		public GameObject character;

		CurrentState state = CurrentState.Joining;
		TTR.ActionDispatcher dispatcher;

		
		// Use this for initialization
		void Start () {
			// GET INSTANCE OF ACTION DISPATCHER
			dispatcher = Transform.FindObjectOfType<TTR.ActionDispatcher>();	

			// SUBSCRIBE TO EVENT
			dispatcher.OnReceivedTurnRequest += new System.EventHandler<TTR.Protocol.TurnReq>(OnReceivedTurnRequest);
			
			dispatcher.Init(ClientMode.Live, "127.0.0.1", 8080);


			statusText.Move(ScreenPosition.Center);
		}

		private void OnReceivedTurnRequest(object sender, TTR.Protocol.TurnReq request) {
			if(request.turnType == TTR.Protocol.TurnType.Join) {
				dispatcher.JoinGame("Marc", TTR.Protocol.ClientType.Player, (response) => {
					if(response.success) {
						this.state = CurrentState.WaitForPlayers;
						this.statusText.text = "Waiting for other players.";
						dispatcher.Pause(2f);
					}		
				});

			} else if(request.turnType == TTR.Protocol.TurnType.ClaimDestinationTickets) {
				dispatcher.ClaimDestinationTickets(true, true, false, (response) => {
					Debug.Log("Claimed Tickets yeah...");

					character.GetComponent<Animator>().Play("Yeah");
					this.state = CurrentState.GetBoardState;
					this.statusText.text = "Claimed Destination Ticktes";
					this.statusText.Move(ScreenPosition.Top);
					dispatcher.Pause(2f);
				});

			} else if(request.turnType == TTR.Protocol.TurnType.Turn) {
				// CHOOSE BETWEEN THREE ACTIONS (CLAIM DESTINATION TICKETS / DRAW PASSENGER CARS / BUILD ROUTE)

				if(this.state == CurrentState.GetBoardState) {
					dispatcher.GetBoardState((response) => {
						var boardState = (TTR.Protocol.BoardStateResp) response;
						statusText.text = "Received Board State";
						stack.StartCoroutine("InitCardDeck", boardState.faceUpPassengerCarDeck);
					});
					this.state = CurrentState.Waiting;
				}
			}
		}

		private void ExecuteLater(System.Action action) {
			if(action != null) {
				action();
			}
		}

		/*
		void Start() {
			dispatcher = Transform.FindObjectOfType<TTR.ActionDispatcher>();
			dispatcher.Init(TTR.ClientMode.Log, "/Users/marcseibert/Desktop/log.txt");
			dispatcher.Pause();
		}

		void Update() {
			if(this.dispatcher.IsMessageAvailable()) {
				var message = this.dispatcher.GetNextMessage();

				if(message.Type == TTR.Protocol.MessageType.Info) {
					// DO WHATEVER

				}
			}
		}*/


	}
}
```
