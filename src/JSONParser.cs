using System;
using System.IO;
using System.Collections.Generic;
using SimpleJSON;
using TTR.Protocol;
using UnityEngine;

namespace TTR {
	public static class JSONParser {

		public static Message Parse(string s) {

			JSONNode message = JSON.Parse(s);

			if(message == null || message["type"] == null) {
				return new TextResp(s);
			}
			var messageType = ToEnum<MessageType>(message["type"]);//(MessageType) Enum.Parse(typeof(MessageType), message["type"]);
			if(messageType == MessageType.Info) {
				if(message["turnType"] == null) {
					return new TextResp(message["text"]);
				}


				var turnType = (TurnType) Enum.Parse(typeof(TurnType), message["turnType"]);

				if(!message["success"].AsBool) {
					// RETURN ERROR MESSAGE
					var currentPlayer = ToEnum<PlayerColor>(message["player"]);
					var errorCode = ToEnum<ErrorCode>(message["errorCode"]);

					return new TurnResp(errorCode, currentPlayer, turnType);
				}

				if(turnType == TurnType.BoardState) {
					DestinationTicket[] ownDestinationTickets = NodeToDestinationTickets(message["ownDestinationTickets"]);
					DestinationTicket[] activeDestinationTickets = NodeToDestinationTickets(message["activeDestinationTickets"]);
					Route[] ownRoutes = new Route[message["ownRoutes"].Count];
					PassengerCarColor[] faceUpPassengerCarDeck = new PassengerCarColor[message["faceUpPassengerCarDeck"].AsArray.Count];
					int[] drawnPassengerCars = new int[message["drawnPassengerCars"].AsArray.Count];

					// PARSE COLORS
					int i = 0;
					foreach(var color in message["faceUpPassengerCarDeck"].AsArray) {
						faceUpPassengerCarDeck[i++] = ToEnum<PassengerCarColor>(color.Value);
					}
					i = 0;
					foreach(var amount in message["drawnPassengerCars"].AsArray) {
						drawnPassengerCars[i++] = amount.Value.AsInt;
					}

					int topdownPassengerCarDeckCount = message["topdownPassengerCarDeckCount"].AsInt;
					int destinationTicketCount = message["destinationTicketCount"].AsInt;
					int leftPassengerCars = message["leftPassengerCars"].AsInt;
					bool finalTurn = message["finalTurn"].AsBool;
					PlayerColor nextPlayer = ToEnum<PlayerColor>(message["player"]);

					return new BoardStateResp(faceUpPassengerCarDeck, topdownPassengerCarDeckCount, destinationTicketCount, nextPlayer, drawnPassengerCars, ownDestinationTickets, activeDestinationTickets, ownRoutes, leftPassengerCars, finalTurn);
				} else if(turnType == TurnType.ClaimDestinationTickets) {
					var player = ToEnum<PlayerColor>(message["player"]);
					var success = message["success"].AsBool;
					var destinationTickets = NodeToDestinationTickets("drawnCards");

					return new ClaimDestinationTicketsResp(player, destinationTickets);

				}  else if(turnType == TurnType.ClaimRoute) {

					var player = ToEnum<PlayerColor>(message["player"]);
					var destinationA = ToEnum<Destination>(message["d1"]);
					var destinationB = ToEnum<Destination>(message["d2"]);
					PassengerCarColor routeColor = ToEnum<PassengerCarColor>(message["routeColor"]);
					PassengerCarColor[] colors = new PassengerCarColor[message["passengerCarColors"].Count];
					int i = 0; 
					foreach(var color in message["passengerCarColors"].AsArray) {
						colors[i++] = ToEnum<PassengerCarColor>(color.Value); 
					}

					return new ClaimRouteResp(player, destinationA, destinationB, routeColor, colors);

				} else if(turnType == TurnType.DrawPassengerCars) {
					PassengerCarColor drawnCard = ToEnum<PassengerCarColor>(message["drawnCard"]);
					PlayerColor player = ToEnum<PlayerColor>(message["player"]);

					JSONArray array = message["faceUpPassengerCarDeck"].AsArray;
					bool hiddenDeck = message["hiddenDeck"];
					PassengerCarColor[] faceUpPassengerCarDeck = new PassengerCarColor[array.Count];

					for(int i = 0; i < array.Count; i++) { 
						faceUpPassengerCarDeck[i] = ToEnum<PassengerCarColor>(array[i]);
					}

					return new DrawPassengerCarsResp(player, drawnCard, hiddenDeck, faceUpPassengerCarDeck);

				} else if(turnType == TurnType.FinalScore) {
					var playerColor = ToEnum<PlayerColor>(message["player"]);
					bool winner = message["winner"].AsBool;
					bool longestRoute = message["longestRoute"].AsBool;

					int totalScore = message["totalScore"].AsInt;
					int scorePassengerCars = message["scorePassengerCars"].AsInt;
					int longsetRouteLength = message["longestRouteLength"].AsInt;

					string playerName = message["name"];

					DestinationTicket[] claimedTickets = NodeToDestinationTickets("claimedTickets");
					DestinationTicket[] notClaimedTickets = NodeToDestinationTickets("notClaimedTickets");

					return new PlayerScore(playerColor, playerName, totalScore, scorePassengerCars, longsetRouteLength, longestRoute, winner, claimedTickets, notClaimedTickets);

				} else if(turnType == TurnType.Join) {
					PlayerColor playerColor = ToEnum<PlayerColor>(message["player"]);
					ClientType clientType = ToEnum<ClientType>(message["clientType"]);
					string playerName = message["playerName"];
					
					return new JoinResp(playerColor, playerName, clientType);

				} else if(turnType == TurnType.ListAllRoutes) {
					PlayerColor playerColor = PlayerColor.None;
					if(message["player"] != null) {
						ToEnum<PlayerColor>(message["player"]);
					}


					Route[] routes = new Route[message["routes"].Count];
					int i = 0; 

					foreach(JSONNode node in message["routes"]) {
						var passengerColor = ToEnum<PassengerCarColor>(node["color"]);
						var destinationA = ToEnum<Destination>(node["d1"]);
						var destinationB = ToEnum<Destination>(node["d2"]);

						routes[i++] = new Route(destinationA, destinationB, node["cost"].AsInt, passengerColor);
					}

					return new ListAllRoutesResp(playerColor, routes);
				}

			} else if(messageType == MessageType.Request) {
				PlayerColor playerColor = PlayerColor.None;

				if(message["player"] != null){
					playerColor = ToEnum<PlayerColor>(message["player"]);
				}				

				var turnType = ToEnum<TurnType>(message["turnType"]);

				return new TurnReq(playerColor, turnType);
			}

			Debug.Log("Error couldn't parse request! " + s);
			return null;
		}

		public static string ObjectToString(System.Object o) {
			string json = JsonUtility.ToJson(o);

			return json;
		}
		public static string Serialize(TurnResp action){
			//TODO
			return action.ToString();
		}

		private static DestinationTicket[] NodeToDestinationTickets(JSONNode node) {
			DestinationTicket[] destinationTickets = new DestinationTicket[node.Count];
			int i = 0;
			foreach(JSONNode ticket in node) {
				var destinationA = (Destination) Enum.Parse(typeof(Destination), ticket["city1"]);
				var destinationB = (Destination) Enum.Parse(typeof(Destination), ticket["city2"]);

				destinationTickets[i] = new DestinationTicket(destinationA, destinationB, node["points"].AsInt);
			}

			return destinationTickets;
		}

		private static T ToEnum <T> (string s) {
			if(s == null) {
				Debug.LogError("Can't tunr null to enum!");
				return (T) Enum.Parse(typeof(T), Enum.GetNames(typeof(T))[0]);
			}
			return (T) Enum.Parse(typeof(T), s);
		}
	}

}