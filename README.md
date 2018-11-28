# WICHTIG!! Die .Net Version muss auf 4.0 gestellt werden
# Wähle im Menü aus Edit/ Project Settings/ Player. Dort muss unter Configuration die Scripting Runtime Version auf ".NET 4.x equivalent gestellt" werden
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
