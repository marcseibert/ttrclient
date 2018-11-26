using System.Text;

namespace TTR.Protocol {
  public enum MessageType {
    Request, Info, TextMessage
  }

  public enum ClientType {
    Player, Observer
  }
  
  public enum TurnType {
    // client can do these turns
    Join,
    DrawPassengerCars, 
    DrawDestinationTickets, 
    ClaimDestinationTickets, 
    ClaimRoute, 
    BoardState, 
    ListAllRoutes,
    
    // server asks for next turn
    Turn,    
    FinalScore,
    
    Unknown
  }
    
  public enum PlayerState {
    Turn, FirstPassengerCarDraw, ClaimDestinationTicket
  }
 
  public enum ErrorCode {
    //   Some cities are connected by Double-Routes. One player cannot claim both routes to the same cities.
    DoubleRouteClaimed,     
    NoCardLeft,         
    RuleViolation,
    NotPlayersTurn,
    InternalError,
    WrongTurnFormat
  }
  
  public enum PassengerCarColor {
    Purple, White, Blue, Yellow, Orange, Black, Red, Green, Rainbow  
  }

  public enum PlayerColor {
    None, Blue, Red, Green, Yellow, Black  
  }
  
  public enum Destination {
    Chicago, LosAngeles, Montreal, Atlanta, Calgary, Denver, ElPaso, 
    Houston, LasVegas, NewOrleans, OklahomaCity, Phoenix, Portland, 
    SaintLouis, SaultStMarie, Washington, SanFrancisco, Toronto, Boston, 
    Charleston, Dallas, Duluth, Helena, KansasCity, LittleRock, Miami, 
    Nashville, NewYork, Omaha, Pittsburgh, Raleigh, SaltLakeCity, SantaFe, 
    Seattle, Vancouver, Winnipeg,
  }
  
  public class Message {
    public MessageType Type = MessageType.Info;
    public Message(MessageType type) {
      this.Type = type;
    }
    
    public override string ToString() {
      return this.Type.ToString();
    }
  }
  
  public class TextResp : Message {
    public string text;
    public TextResp (string text) 
      : base(MessageType.TextMessage) {
      this.text = text;
    }
    public override string ToString() {
      return this.text;    
    }    
  }
  
  public class TurnResp : Message {
    public TurnType turnType = TurnType.Unknown;
    public bool success;
    public PlayerColor player;    
    public ErrorCode errorCode;
        
    public TurnResp (bool success, PlayerColor currentPlayer, TurnType type) 
      : base(MessageType.Info) {
      this.turnType = type;
      this.success = success;
      this.player = currentPlayer;
    }
    
    public TurnResp (ErrorCode errorCode, PlayerColor currentPlayer, TurnType type)
      : base(MessageType.Info) {
      this.turnType = type;
      this.success = false;
      this.errorCode = errorCode;
      this.player = currentPlayer;
    }
    
    public override string ToString() {
      StringBuilder s = new StringBuilder();
      s.Append(base.ToString());
      s.Append(";turnType:" + this.turnType);
      s.Append(";player:" + player);      
    	s.Append(";success:" + success);
      	
      if (!success) {
      	s.Append(";errorCode:"+errorCode);
      }
      
      return s.ToString();
    }
  }
    
  public class TurnReq : Message {
    public TurnType turnType = TurnType.Unknown;
    public PlayerColor player;
    
    public TurnReq (PlayerColor playerColor, TurnType moveType)
      : base(MessageType.Request) {
      this.turnType = moveType;
      this.player = playerColor;
    }

    public override string ToString() {
      StringBuilder s = new StringBuilder();
      s.Append(base.ToString());
      s.Append(";turnType:" + this.turnType);
      s.Append(";player:" + this.player);

      return s.ToString();
    }    
  }
  /*
  public class BoardState : TurnReq {
    public BoardState(PlayerColor color) 
      :base(color, TurnType.BoardState) { }
  }
  
  public class ListAllRoutes : TurnReq {
    public ListAllRoutes(PlayerColor color)
      : base(color, TurnType.ListAllRoutes) { } 
  }
  
  public class Join : TurnReq {
    public string playerName;
    public ClientType clientType;
    public Join (PlayerColor color, string name, ClientType joinType)
      : base(color, TurnType.Join) {
      this.playerName = name;
      this.clientType = joinType;
    }
    public override string ToString() {
      StringBuilder s = new StringBuilder();
      s.Append(base.ToString());
      if (playerName != null) {
        s.Append(";playerName:" + this.playerName);
      }
      s.Append(";clientType:" + this.clientType);      
      return s.ToString();
    }    
  }
  
  public class DrawPassengerCards : TurnReq {
    public bool hiddenDeck;  // true: draw from deck, false: draw from dace-up cards 
    public PassengerCarColor passengercarColor; // for hidden=true, also choose spotnr from 1 to 5
    public DrawPassengerCards(PlayerColor color, bool hidden, PassengerCarColor passengercarColor)
      : base(color, TurnType.DrawPassengerCars) {
      this.hiddenDeck = hidden;
      this.passengercarColor = passengercarColor;
    }

    public DrawPassengerCards(PlayerColor color, bool hidden)
      : base(color, TurnType.DrawPassengerCars) {
      this.hiddenDeck = hidden;
    }
  }
  
  class DrawDestinationTickets : TurnReq {
    public DrawDestinationTickets(PlayerColor color)
      : base(color, TurnType.DrawDestinationTickets) { }
  }
  
  public class ClaimDestinationTickets : TurnReq {
    public bool[] keep;   // Cards to keep. If nothing is passed, none is kept
    public ClaimDestinationTickets(PlayerColor color, params bool[] keep)
      : base(color, TurnType.ClaimDestinationTickets) {
      this.keep = keep;
    }
  }
  
  public class ClaimRoute : TurnReq {
    public Destination d1;
    public Destination d2;
    public PassengerCarColor color;
    public ClaimRoute(PlayerColor playerColor, Destination d1, Destination d2, PassengerCarColor color)
      : base(playerColor, TurnType.ClaimRoute){
      this.d1 = d1;
      this.d2 = d2;
      this.color = color;
    }
    public override string ToString() {
      StringBuilder s = new StringBuilder();
      s.Append(base.ToString());
      s.Append(";d1:"+d1);
      s.Append(";d2:"+d2);
      s.Append(";color:"+color);
      return s.ToString();
    }
  }*/
  
  public class ListAllRoutesResp : TurnResp {
    public Route[] routes;
    
    public ListAllRoutesResp(PlayerColor nextPlayer, Route[] allRoutes)
      : base(true, nextPlayer, TurnType.ListAllRoutes) {
      this.routes = allRoutes;
    }
    
    public override string ToString() {
      StringBuilder s = new StringBuilder();
      s.Append(base.ToString());
      s.Append(";routes:"+routes.ToString()); // TODO: PROPER FORMATTING
      return s.ToString();
    }
  }
  
  public class JoinResp : TurnResp {
    public string playerName;
    public ClientType clientType;
//    public DestinationTicket[] drawnDestinationTickets;
    public JoinResp(PlayerColor playerColor, string name, ClientType joinType)
      : base(true, playerColor, TurnType.Join) { 
      this.playerName = name;
      this.clientType = joinType;
//      this.drawnDestinationTickets = drawnDestinationTickets;
    }       
  }
  
  public class BoardStateResp : TurnResp {
    
    public PassengerCarColor[] faceUpPassengerCarDeck;
    public int topdownPassengerCarDeckCount;
    public int destinationTicketsCount;
    
    public int[] drawnPassengerCars;
    public int leftPassengerCars;

    public DestinationTicket[] drawnDestinationTickets;
    public DestinationTicket[] toBeClaimedDestinationTickets;
    
    public Route[] ownRoutes;
    
    public bool finalTurn;
    
//    List<PlayerTurn> lastTurnsPlayed; // by default: in a k-people game, the last k-1 turns played
    public BoardStateResp(
        PassengerCarColor[] faceUpPassengerCarDeck,
        int topdownPassengerCarDeckCount,
        int destinationTicketsCount,
        PlayerColor player,
        int[] drawnPassengerCars,
        DestinationTicket[] ownDestinationTickets,
        DestinationTicket[] activeDestinationTickets,
        Route[] ownRoutes,
        int leftPassengerCars,
        bool finalTurn
        )
      : base(true, player, TurnType.BoardState) {
      this.faceUpPassengerCarDeck = faceUpPassengerCarDeck;
      this.topdownPassengerCarDeckCount = topdownPassengerCarDeckCount;
      this.destinationTicketsCount = destinationTicketsCount;
      this.drawnDestinationTickets = ownDestinationTickets;
      this.toBeClaimedDestinationTickets = activeDestinationTickets;
      this.drawnPassengerCars = drawnPassengerCars;
      this.ownRoutes = ownRoutes;
      this.leftPassengerCars = leftPassengerCars;
      this.finalTurn = finalTurn;
    }

    public override string ToString() {
      StringBuilder s = new StringBuilder();
      s.Append(base.ToString());
      s.Append(";faceUpPassengerCarDeck:"+string.Join(",",faceUpPassengerCarDeck));
      
      s.Append(";topdownPassengerCarDeckCount:"+topdownPassengerCarDeckCount);
      s.Append(";destinationTicketsCount:"+destinationTicketsCount);
      
      s.Append(";ownPassengerCards:"+string.Join(",", drawnPassengerCars));
      s.Append(";ownDestinationTickets:"+string.Join(",", (object[]) drawnDestinationTickets));
      s.Append(";activeDestinationTickets:"+string.Join(",", (object[]) toBeClaimedDestinationTickets));
      s.Append(";ownRoutes:"+string.Join(",", (object[]) ownRoutes));
      
      s.Append(";leftPassengerCars:" + leftPassengerCars);
      s.Append(";lastTurn:"+finalTurn);
      return s.ToString();
    }
  }
  
  public class DrawPassengerCarsResp : TurnResp {
    public PassengerCarColor drawnCard;  // the card retrieved
    public PassengerCarColor[] faceUpPassengerCarDeck; // 1 to 5 open cards
    public bool hiddenDeck; // drawn from hidden deck?
    public DrawPassengerCarsResp (PlayerColor currentPlayer, PassengerCarColor drawnCard, bool hidden, PassengerCarColor[] open)
      : base(true, currentPlayer, TurnType.DrawPassengerCars) {
      this.drawnCard = drawnCard;
      this.faceUpPassengerCarDeck = open;
      this.hiddenDeck = hidden;
    }
    
    public override string ToString() {
      StringBuilder s = new StringBuilder();
      s.Append(base.ToString());
      s.Append(";drawnCard:"+drawnCard);
      s.Append(";hiddenDeck:"+hiddenDeck);
      s.Append(";faceUpPassengerCarDeck:"+string.Join(",", faceUpPassengerCarDeck));
      return s.ToString();
    }
  }
  
  public class ClaimRouteResp : TurnResp {
    public Destination d1;
    public Destination d2;
    public PassengerCarColor routeColor;
    public PassengerCarColor[] passengerCarColors;
    public ClaimRouteResp (
        PlayerColor player, Destination d1, Destination d2, PassengerCarColor routeColor,
        PassengerCarColor[] passengerCarColor)
      : base(true, player, TurnType.ClaimRoute) {
      this.d1 = d1;
      this.d2 = d2;
      this.routeColor = routeColor;
      this.passengerCarColors = passengerCarColor;
    }
    
    public override string ToString() {
      StringBuilder s = new StringBuilder();
      s.Append(base.ToString());
      s.Append(";d1:" + d1);
      s.Append(";d2:" + d2);
      s.Append(";routeColor:" + routeColor);
      s.Append(";passengerCarColors:" + string.Join(",", passengerCarColors));

      return s.ToString();
    }
  }
  
  public class DrawDestinationTicketsResp : TurnResp {
    public DestinationTicket[] drawnCards;
    public DrawDestinationTicketsResp (PlayerColor nextPlayer, DestinationTicket[] drawnCards)
      : base(true, nextPlayer, TurnType.DrawDestinationTickets) {
      this.drawnCards = drawnCards;
    } 
    
    public override string ToString() {
      StringBuilder s = new StringBuilder();
      s.Append(base.ToString());
      s.Append(";drawnCards:"+string.Join(",",(object[]) drawnCards));      
      return s.ToString();
    }
  }
  
  public class ClaimDestinationTicketsResp : TurnResp {
    public DestinationTicket[] drawnCards;
    public ClaimDestinationTicketsResp (PlayerColor nextPlayer, DestinationTicket[] drawnCards)
      : base(true, nextPlayer, TurnType.ClaimDestinationTickets){
      this.drawnCards = drawnCards;
    }
    
    public override string ToString() {
      StringBuilder s = new StringBuilder();
      s.Append(base.ToString());      
      s.Append(";drawnCards:"+ drawnCards.ToString()); // TODO: PROPER FORMATTING     
      return s.ToString();
    }    
  }
  
  public class PlayerScore : TurnResp {
    public readonly string name;
    public int totalScore;
    public readonly int scorePassengerCars;
    public int longestRouteLength;
    public bool winner;
    public bool longestRoute;
    public readonly DestinationTicket[] claimedTickets;
    public readonly DestinationTicket[] nonClaimedTickets;
    
    public PlayerScore(
        PlayerColor color,
        string name,
        int totalScore,
        int scorePassengerCars,        
        int longestRouteLength,
        bool longestRoute,
        bool winner,
        DestinationTicket[] claimedTickets,
        DestinationTicket[] nonClaimedTickets)
      : base(true, color, TurnType.FinalScore) {
      this.name = name;
      this.longestRouteLength = longestRouteLength;
      this.longestRoute = longestRoute;
      this.totalScore = totalScore;
      this.scorePassengerCars = scorePassengerCars;
      this.winner = winner;
      this.claimedTickets = claimedTickets;
      this.nonClaimedTickets = nonClaimedTickets;
    }
    
    public override string ToString() {
      StringBuilder buffer = new StringBuilder();
      buffer.Append("---------------------------\n");
      buffer.Append("Final Score");
      buffer.Append("---------------------------\n");      
      buffer.Append("Player:              \t " + player + "(" +  name + ") \n");
      buffer.Append("Total Score:         \t+" + totalScore+"\n");
      buffer.Append("Passenger Cars:      \t+" + scorePassengerCars+"\n");
      buffer.Append("#Tickets:            \t" + (this.claimedTickets.Length + nonClaimedTickets.Length)+"\n");
      buffer.Append("Claimed Tickets:     \t+" + string.Join(",", (object[]) claimedTickets) +"\n");
      buffer.Append("Not Claimed Tickets: \t-" + string.Join(",", (object[]) nonClaimedTickets) +"\n");
      buffer.Append("Longest Route:       \t" + longestRoute +"\n");
      buffer.Append("Longest Route Length:\t" + longestRouteLength +"\n");
      buffer.Append("---------------------------\n");
      buffer.Append("Total:               \t" + (totalScore)+"\n");
      buffer.Append("Winner:              \t" + (winner)+"\n");
      return buffer.ToString();
    }
  }
  

  public class DestinationTicket {
    public Destination city1;
    public Destination city2;
    public int points;
    public DestinationTicket(Destination city1, Destination city2, int points) {
      this.city1 = city1;
      this.city2 = city2;
      this.points = points;
    }
    public override string ToString() {
      StringBuilder s = new StringBuilder();
      s.Append("city1:" + city1);
      s.Append(";city2:" + city2);
      s.Append(";points:"+ points);
      return s.ToString();
    }
  }
  
  public class Route {
    public readonly Destination d1;
    public readonly Destination d2;
    public readonly int cost;
    public readonly PassengerCarColor color;
    
    public PlayerColor claimedBy;
    
    public Route(Destination d1, Destination d2, int cost, PassengerCarColor color) {
      this.d1 = d1;
      this.d2 = d2;
      this.cost = cost;
      this.color = color;
      this.claimedBy = PlayerColor.None;
    }
    
    public void setClaimedBy(PlayerColor claimedBy) {
      this.claimedBy = claimedBy;
    }
    
    
    public int calcScore() {
      if (cost == 1) {
        return 1;
      }
      else if (cost == 2) {
        return 2;
      }
      else if (cost == 3) {
        return 4;
      }
      else if (cost == 4) {
        return 7;
      }
      else if (cost == 5) {
        return 10;
      }
      else if (cost == 6) {
        return 15;
      }
      return 0;
    }
    
    public override string ToString() {
      StringBuilder s = new StringBuilder();
      s.Append("d1:" + this.d1);
      s.Append(";d2:" + this.d2);      
      s.Append(";cost:" + this.cost);        
      s.Append(";color:" + this.color);
      s.Append(";claimedBy:" + this.claimedBy);
      return s.ToString();
    }
  }
}
