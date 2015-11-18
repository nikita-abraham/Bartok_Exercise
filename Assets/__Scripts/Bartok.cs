using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Bartok : MonoBehaviour {
	static public Bartok S;

	public TextAsset deckXML;
	public TextAsset layoutXML;
	public Vector3 layoutCenter = Vector3.zero;

	public bool ________________;

	public Deck deck;
	public List<CardBartok> drawPile;
	public List<CardBartok> discardPile;

	void Awake() {
		S = this;

	}

	void Start () {
		deck = GetComponent<Deck> ();     // Get the Deck
		deck.InitDeck (deckXML.text);     // Pass DeckXML to it
		Deck.Shuffle (ref deck.cards);    // This shuffles the deck
		// The ref keyword passes a reference to deck.cards, which allows
		//   deck.cards to be modified by Deck.Shuffle()
	}
}
