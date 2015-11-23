using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//this enum contains the different phases of a game turn
public enum TurnPhase {
	idle,
	pre,
	waiting, 
	post, 
	gameOver
}


public class Bartok : MonoBehaviour {
	static public Bartok S;
	//this field is static to enforce that there is only 1 current player
	static public Player CURRENT_PLAYER;

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

	public TurnPhase phase = TurnPhase.idle;
	public GameObject turnLight;

	void Awake() {
		S = this;

		//find the turnLight by name
		turnLight = GameObject.Find ("TurnLight");

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
		//set the CardBartok to call CBCallback on this Bartok when it is done
		tCB.reportFinishTo = this.gameObject;
	}

	//this callback is used by the last card to be dealt at the beginning
	//it is only used once per game
	public void CBCallback(CardBartok cb) {
		//you sometimes want to have reporting of method called like this
		Utils.tr (Utils.RoundToPlaces (Time.time), "Bartok.CBCallback()", cb.name);

		StartGame (); //start the Game
	}

	public void StartGame() {
		//pick the player to the left of the human to go first
		//(players[0] is the human)
		PassTurn (1);
	}

	public void PassTurn(int num=-1) {
		//if no number was passed in, pick the next player
		if (num == -1) {
			int ndx = players.IndexOf (CURRENT_PLAYER);
			num = (ndx + 1) % 4;
		}
		int lastPlayerNum = -1;
		if (CURRENT_PLAYER != null) {
			lastPlayerNum = CURRENT_PLAYER.playerNum;
		}
		CURRENT_PLAYER = players [num];
		phase = TurnPhase.pre;

		CURRENT_PLAYER.TakeTurn ();

		//move the TurnLight to shine on the new CURRENT_PLAYER
		Vector3 lPos = CURRENT_PLAYER.handSlotDef.pos + Vector3.back * 5;
		turnLight.transform.position = lPos;

		//report the turn passing
		Utils.tr (Utils.RoundToPlaces (Time.time), "Bartok.PassTurn()", "Old: " + lastPlayerNum, "New: " + CURRENT_PLAYER.playerNum);
	}

	//validplay verifies that the card chosen can be played on the discard pile
	public bool ValidPlay(CardBartok cb) {
		//it's a valid play if the rank is the same
		if (cb.rank == targetCard.rank)
			return(true);

		//it's a valid play if the suit is the same
		if (cb.suit == targetCard.suit) {
			return(true);
		}

		//otherwise, return false
		return(false);
	}

	//this makes a new card the target
	public CardBartok MoveToTarget(CardBartok tCB) {
		tCB.timeStart = 0;
		tCB.MoveTo (layout.discardPile.pos + Vector3.back);
		tCB.state = CBState.toTarget;
		tCB.faceUp = true;
		tCB.SetSortingLayerName ("10"); //layout.target.layerName
		tCB.eventualSortLayer = layout.target.layerName;
		if (targetCard != null) {
			MoveToDiscard (targetCard);
		}

		targetCard = tCB;
		return(tCB);
	}

	public CardBartok MoveToDiscard(CardBartok tCB) {
		tCB.state = CBState.discard;
		discardPile.Add (tCB);
		tCB.SetSortingLayerName (layout.discardPile.layerName);
		tCB.SetSortOrder (discardPile.Count + 4);
		tCB.transform.localPosition = layout.discardPile.pos + Vector3.back / 2;

		return(tCB);
	}

	//the draw function will pull a single card from the drawPile and return it 
	public CardBartok Draw() {
		CardBartok cd = drawPile [0];
		drawPile.RemoveAt (0);
		return(cd);
	}

	/*
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
	*/
}
