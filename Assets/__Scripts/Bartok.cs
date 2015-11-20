using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Bartok : MonoBehaviour {
	static public Bartok S;

	public TextAsset deckXML;
	public TextAsset layoutXML;
	public Vector3 layoutCenter = Vector3.zero;

	//the number of degrees to fan each card in a hand
	public float handFanDegrees = 10f;
	public int numStartingCards = 7;
	public float drawTimeStagger = 0.1f;

	public bool ________________;

	public Deck deck;
	public List<CardBartok> drawPile;
	public List<CardBartok> discardPile;

	public BartokLayout layout;
	public Transform layoutAnchor;

	public List<Player> players;
	public CardBartok targetCard;

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
		LayoutGame ();
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

	//position all the cards in the drawpile properly
	public void ArrangeDrawPile() {
		CardBartok tCB;

		for (int i=0; i<drawPile.Count; i++) {
			tCB = drawPile [i];
			tCB.transform.parent = layoutAnchor;
			tCB.transform.localPosition = layout.drawPile.pos;
			//rotation should start at 0
			tCB.faceUp = false;
			tCB.SetSortingLayerName (layout.drawPile.layerName);
			tCB.SetSortOrder (-i * 4); //order them front to back
			tCB.state = CBState.drawpile;
		}
	}

	//perform the initial game layout
	void LayoutGame() {
		//create an empty GameObjectto serve as an anchor for the tableau
		if(layoutAnchor == null) {
			GameObject tGO = new GameObject("_LayoutAnchor");
			//create an empty GameObject named _LayoutAnchor in the Hierarchy
			layoutAnchor = tGO.transform; //grab its Transform
			layoutAnchor.transform.position = layoutCenter;
		}

		//position the drawpile cards
		ArrangeDrawPile ();

		//set up the players
		Player pl;
		players = new List<Player> ();
		foreach(SlotDef tSD in layout.slotDefs) {
			pl = new Player();
			pl.handSlotDef = tSD;
			players.Add (pl);
			pl.playerNum = players.Count;
		}
		players [0].type = PlayerType.human; // make the Oth player human

		CardBartok tCB;
		//deal 7 cards to each player
		for (int i=0; i<numStartingCards; i++) {
			for (int j=0; j<4; j++) { //there are always 4 players
				tCB = Draw (); //draw a card
				//stagger the draw time a bit. Remember order of operations
				tCB.timeStart = Time.time + drawTimeStagger * (i * 4 + j);
				//by setting the timeStart before calling AddCard, we
				//override the automatic setting of TimeStart in 
				//CardBartok.MoveTo().
				//add the card to the player's hand. the modulus (%4)
				//results in a number from 0 to 3
				players [(j + 1) % 4].AddCard (tCB);
			}
		}

		//call Bartok.DrawFirstTarget() when the hand cards have been drawn.
		Invoke ("DrawFirstTarget", drawTimeStagger * (numStartingCards * 4+4) );
	}

	public void DrawFirstTarget() {
		//flip up the first target card from the drawPile
		CardBartok tCB = MoveToTarget (Draw());
	}

	//this makes a new card the target
	public CardBartok MoveToTarget(CardBartok tCB) {
		tCB.timeStart = 0;
		tCB.MoveTo (layout.discardPile.pos + Vector3.back);
		tCB.state = CBState.toTarget;
		tCB.faceUp = true;

		targetCard = tCB;
		return(tCB);
	}

	//the draw function will pull a single card from the drawPile and return it 
	public CardBartok Draw() {
		CardBartok cd = drawPile [0];
		drawPile.RemoveAt (0);
		return(cd);
	}

	//This Update method is used to test adding cards to players' hands 
	void Update() {
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			players [0].AddCard (Draw ());
		}
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			players [1].AddCard (Draw ());
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			players [2].AddCard (Draw ());
		}
		if (Input.GetKeyDown (KeyCode.Alpha4)) {
			players [3].AddCard (Draw ());
		}
	}
}
