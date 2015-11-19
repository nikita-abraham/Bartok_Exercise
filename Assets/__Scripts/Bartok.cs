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

	public BartokLayout layout;
	public Transform layoutAnchor;

	void Awake() {
		S = this;

	}

	void Start () {
		deck = GetComponent<Deck> ();     // Get the Deck
		deck.InitDeck (deckXML.text);     // Pass DeckXML to it
		Deck.Shuffle (ref deck.cards);    // This shuffles the deck
		// The ref keyword passes a reference to deck.cards, which allows
		//   deck.cards to be modified by Deck.Shuffle()

		layout = GetComponent<BartokLayout> (); //get the layout
		layout.ReadLayout (layoutXML.text); //pass layoutXML to it

		drawPile = UpgradeCardsList (deck.cards);
	}

	//UpgradeCardsList casts the Cards in 1CD to be CardBartoks
	//this lets Unity know
	List<CardBartok> UpgradeCardsList(List<Card> lCD) {
		List<CardBartok> lCB = new List<CardBartok> ();
		foreach (Card tCD in lCD) {
			lCB.Add (tCD as CardBartok);
		}
		return(lCB);
	}
}
