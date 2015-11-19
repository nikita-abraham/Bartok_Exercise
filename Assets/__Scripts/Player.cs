using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; //enables LINQ queries

	//the player can either be human or an AI

public enum PlayerType {
	human, 
	ai
}

	//the individual player of the game
	//note: player does NOT extend Monobehavior or any other class

[System.Serializable] //make the player class visible in the Inspector pane
	
public class Player {

	public PlayerType type = PlayerType.ai;
	public int playerNum;

	public List<CardBartok> hand; //the cards in this player's hand

	public SlotDef handSlotDef;

	//add a card to the hand
	public CardBartok AddCard(CardBartok eCB) {
		if (hand == null) hand = new List<CardBartok>();

		// add the card to the hand
		hand.Add (eCB);

		return(eCB);
	}

	//remove a card from the hand
	public CardBartok RemoveCard(CardBartok cb) {
		hand.Remove (cb);

		return(cb);
	}
}
