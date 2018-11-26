# ttrclient
Einfache Bibliothek, die zur Kommunikation mit dem TTR-Server genutzt werden kann. Client-Server Kommunikation und JSON Parsen 
wird automatisch gehandelt.

# Installation

1. Auf *Assets/Import Packages/Custom* Package klicken.
2. Das File *ttrclient.unitypackage* auswählen.
3. Ein GameObject erstellen und den *ActionDispatcher.cs* als Komponente hinzufügen.

# Module
Der Client kann in zwei Varianten initialisiert werden. Die erste Variante ist der sogenannte *Offline Client*. Der Offline Client muss mit *ClientModus.Log* initialisiert werden. Als zweite Option gibt es den *Online Client*.

Der ActionDispatcher kapselt den Client und vereinfacht das Arbeiten mit ihm.

# Online Examples
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

# Offline Examples
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
