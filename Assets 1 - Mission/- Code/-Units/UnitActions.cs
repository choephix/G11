
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Actions {

	public static Action selectedAction {
		set { God.selectedAction = value; }
		get { return God.selectedAction; }
	}
	public Action previousAction;

	public Action defaultAction;
	public Action defaultAttackAction;
	//public int count { get { return possibleActionsList.Count; } }
	public bool shouldSelectPreviousAction {
		get {
			return previousAction != null
				&& previousAction.canDoAgain;
		}
	}

	private Unit owner;
	private bool active { get { return selectedAction != null && owner.selected; } }

	private readonly List<Action> actionsList = new List<Action>();
	private List<Action> possibleActionsList = new List<Action>();

	public event EventHandler eventListChanged;

	internal void Init( Unit owner ) {
		this.owner = owner;
		eventListChanged += UpdateShit;
		owner.eventSelected += OnOwnerSelected;
		owner.eventDeselected += OnOwnerDeselected;
		owner.eventActionFinished += OnFinished;
		owner.eventWeaponEquipped += OnWeaponEquipped;
		owner.eventWeaponUnequipped += OnWeaponUnequipped;
		GodOfInteraction.eventTilePicked += OnTilePicked;
		GodOfInteraction.eventUnitPicked += OnUnitPicked;
	}

	internal void UpdateShit() {
		possibleActionsList = actionsList.FindAll( a => a.possible );
	}

	internal void OnTurnStart() {
		foreach( Action a in actionsList ) {
			a.OnTurnStart();
		}
		UpdateShit();
	}

	internal void Add( Action action ) {
		actionsList.Add( action );
		eventListChanged.Invoke();
	}
	internal void AddRange( IEnumerable<Action> collection ) {
		actionsList.AddRange( collection );
		eventListChanged.Invoke();
	}
	internal void Remove( Action action ) {
		actionsList.Remove( action );
		eventListChanged.Invoke();
	}
	internal void RemoveAll( List<Action> list ) {
		actionsList.RemoveAll( list.Contains );
		eventListChanged.Invoke();
	}


	internal void Select( int i = 0 ) {

		if( i.IsIntInRange( possibleActionsList.Count - 1 ) ) {
			Select( possibleActionsList[i] );
		} else {
			Debug.Log( "No action at index " + i );
		}

	}
	internal void Select( Action action ) {

		if( action.possible ) {

			if( selectedAction != null ) {
				AbortSelected();
			}

			selectedAction = action;

			action.OnSelected();

			if( action.instant ) {
				ConfirmSelectedAction();
			}

			if( action.subjectType == ActionSubjectType.GridTile ) { //RE-FUCKING-MOVE THIS SHIT AND REWRITE GAMEMODE
				GameMode.Set( GameModes.PickTile );
			} else
				if( action.subjectType == ActionSubjectType.Unit ) {
					GameMode.Set( GameModes.PickUnit );
				}

		} else {

			Debug.Log( owner + " tried to select impossible action " + action.ToLongString() );

		}

	}
	internal void SelectDefault() {
		if( defaultAction != null ) {
			Select( defaultAction );
		}
	}
	internal void SelectPrevious() {

		Debug.Log( "Selecting previous action " + previousAction );

		Select( previousAction );

	}
	internal void SelectAttack() {

		Debug.Log( "Selecting default Attack action " + defaultAttackAction );

		if( defaultAttackAction != null ) {
			Select( defaultAttackAction );
		} else {
			Debug.Log( "... which was NULL" );
		}

	}

	public void ConfirmSelectedActionOn( object subject ) {

		if( selectedAction.IsSubjectViable( subject ) ) {

			Debug.Log( owner + " begins action " + selectedAction + " on subject " + subject );

			selectedAction.Execute( subject );

		} else {

			Debug.LogWarning( owner + " tried action " + selectedAction + " on inviable Subject " + subject );

		}

	}

	public void ConfirmSelectedAction() {

		if( selectedAction.possible ) {

			selectedAction.Execute( null );

		} else {

			Debug.LogWarning( owner + " tried impossible action " + selectedAction );

		}

	}


	public void OnFinished( Action action ) {

		Debug.Log( owner + " finished selected action " + selectedAction );

		if( !action.canDoAgain ) {
			Deselect();
		}

		if( action.oneUse ) {
			Remove( action );
		}

	}
	public void AbortSelected() {

		Debug.Log( owner + " aborted selected action " + selectedAction );

		Deselect();
		previousAction = null;

	}

	private void Deselect() {

		Debug.Log( owner + " deselecting action " + selectedAction );

		GameMode.cinematic = false;
		if( selectedAction != null ) {
			selectedAction.OnDeselected();
		}
		previousAction = selectedAction;
		selectedAction = null;
		UpdateShit();

	}

	internal void OnTilePicked( GridTile tile ) {
		if( !active || selectedAction.subjectType != ActionSubjectType.GridTile ) return;
		if( ( selectedAction ).IsSubjectViable( tile ) ) {
			ConfirmSelectedActionOn( tile );
		}
	}

	internal void OnUnitPicked( Unit unit ) {
		if( !active ) return;
		if( selectedAction.subjectType != ActionSubjectType.Unit ) return;
		if( selectedAction.subjectType != ActionSubjectType.Damageable ) return;
		if( ( selectedAction ).IsSubjectViable( unit ) ) {
			ConfirmSelectedActionOn( unit );
		}
	}

	private void OnOwnerSelected() {
		if( owner.team.isUserControlled ) {
			SelectDefault();
		}
	}
	private void OnOwnerDeselected() {
		Deselect();
	}

	private void OnWeaponEquipped( Unit owner, Weapon weapon ) {
		AddRange( weapon.actions );
		defaultAttackAction = weapon.actions[0];
	}
	private void OnWeaponUnequipped( Unit owner, Weapon weapon ) {
		RemoveAll( weapon.actions );
	}

	public Action FindAction( string name ) {
		return actionsList.FirstOrDefault( a => a.name.Equals( name ) && a.possible );
	}

	//----

	internal string ToStringRibbon() {
		string s="";
		for( int i = 1 ; i <= possibleActionsList.Count ; i++ ) {
			s += '[' + i.ToString() + ']' + ' ' + possibleActionsList[i - 1].name + "   ";
		}
		return s;
	}

	public override string ToString() {
		string s="";
		bool sel;
		foreach( Action a in possibleActionsList ) {
			sel = a.Equals( selectedAction );
			s += ( sel ? '[' : '{' ) + a.name + ( sel ? ']' : '}' ) + " ";
		}
		return s;
	}

	public static Actions operator +( Actions o, Action a ) { o.Add( a ); return o; }

}





