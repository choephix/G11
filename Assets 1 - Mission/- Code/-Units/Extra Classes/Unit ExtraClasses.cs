using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actions {

	public static Action selectedAction {
		set { God.selectedAction = value; }
		get { return God.selectedAction; }
	}
	public Action previousAction;

	//TODO add some deleggates here
	//public event EventHandler<object> ActionConfirmedEvent = delegate { };
	public Action defaultAction;
	public Action defaultAttackAction;
	public int count { get { return possibleActionsList.Count; } }
	public bool shouldSelectPreviousAction { get { return previousAction != null 
		&& previousAction.canDoAgain; } }

	private Unit owner;
	private bool active { get { return selectedAction != null && owner.selected; } }

	private List<Action> actionsList = new List<Action>();
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
		actionsList.RemoveAll( a => list.Contains( a ) );
		eventListChanged.Invoke();
	}
	internal void RemoveAll( Action[] list ) {
		actionsList.RemoveAll( a => new List<Action>( list ).Contains( a ) );
		eventListChanged.Invoke();
	}


	internal void Select( int i = 0 ) {

		if( M.IsIntInRange( i, possibleActionsList.Count - 1 ) ) {
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

	public void ConfirmSelectedActionOn(object subject ) {

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
		if( selectedAction!=null ) {
			selectedAction.OnDeselected();
		}
		previousAction = selectedAction;
		selectedAction = null;
		UpdateShit();

	}

	internal void OnTilePicked( GridTile tile ) {

		if( active && selectedAction.subjectType == ActionSubjectType.GridTile ) {
			if( (selectedAction).IsSubjectViable( tile ) ) {
				ConfirmSelectedActionOn( tile );
			}
		}

	}
	internal void OnUnitPicked( Unit unit ) {

		if( active && selectedAction.subjectType == ActionSubjectType.Unit ) {
			if( (selectedAction).IsSubjectViable( unit ) ) {
				ConfirmSelectedActionOn( unit );
			}
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
		foreach( Action a in actionsList ) {
			if( a.name.Equals( name ) && a.possible ) {
				return a;
			}
		}
		return null;
	}

	//----

	internal string ToStringRibbon() {
		string s="";
		for( int i = 1 ; i <= possibleActionsList.Count ; i ++ ) {
			s += '[' + i.ToString() + ']' + ' ' + possibleActionsList[i-1].name + "   ";
		}
		return s;
	}

	public override string ToString() {
		string s="";
		bool sel;
		foreach( Action a in possibleActionsList ) {
			sel = a.Equals(selectedAction);
			s += ( sel ? '[' : '{' ) + a.name + ( sel ? ']' : '}' ) + " ";
		}
		return s;
	}

	public static Actions operator +( Actions o, Action a ) { o.Add( a ); return o; }

}












public static class UnitMath {
	
}

public static class UnitAnimation {

	//looping
	public const string IDLE	= "idle";
	public const string MOVE	= "run";
	public const string AIM		= "aimed";

	public const string JUMP	= "jumped";

	public const string COVER_DUCKED_L	= "coverDuckedL";
	public const string COVER_DUCKED_R	= "coverDuckedR";
	public const string COVER_WALL_L	= "coverL";
	public const string COVER_WALL_R	= "coverR";

	//onetimers
	public const string ATTACK_RANGED	= "shoot";
	public const string ATTACK_WIELD	= "attackSword";
	public const string ATTACK_FIST		= "attackFist";
	public const string HURT			= "shot";
	public const string DEATH			= "die";
	public const string RELOAD			= "reload";
	public const string RELOAD_DUCKED_L	= "reloadDuckedL";
	public const string RELOAD_DUCKED_R	= "reloadDuckedR";

	public const string JUMP_START	= "jumpStart";
	public const string JUMP_END	= "jumpEnd";

	// DEV

	public const float MELEE_HIT_DELAY = .33f;
	public const float RANGE_HIT_DELAY = .09f;

}

[System.Serializable]
public class UnitTransformSpots {
	public Transform left;
	public Transform right;
	public Transform overHead;
	public Transform wayOverHead;
	public Transform head;
	public Transform torso;
	public Transform feet;
	public Transform back;
	public Transform forth;
	public Transform wayBack;
	public Transform wayForth;

	public Transform upBack;
	public Transform upForth;
	public Transform upLeft;
	public Transform upRight;
}

[System.Serializable]
public class UnitStatus {

	private int _maxHealth;
	private float _health;
	public float health {
		get { return _health; }
		set { _health = value.ClipMaxMin( _maxHealth ); }
	}
	private float _armor;
	public float armor {
		get { return _armor; }
		set { _armor = value > 0 ? value : 0; }
	}

	private int _maxActions;
	public int actionPoints;

	public bool fullHealth { get { return health >= _maxHealth; } }

	internal void Init( UnitProperties props ) {
		_armor = props.armor;
		_health = props.maxHealth;
		_maxHealth = props.maxHealth;
		_maxActions = Config.BASE_UNIT_ACTIONS;
		ResetActions();
	}

	internal void ResetActions() {
		actionPoints = _maxActions;
	}

	internal void ReceiveDamage( float amount, DamageType type ) {
		switch( type ) {
			case DamageType.NORMAL:
				if( armor > 0 ) {
					if( armor >= amount ) {
						armor -= amount;
					} else {
						amount -= armor;
						armor = 0;
						health -= amount;
					}
				} else {
					health -= amount;
				}
				return;
			case DamageType.INTERNAL:
				health -= amount;
				return;
			case DamageType.ANTIARMOR:
				if( armor > 0 ) {
					amount *= 2;
					if( armor >= amount ) {
						armor -= amount;
						return;
					} else {
						amount -= armor;
						armor = 0;
						amount = ( amount - amount % 2 ) / 2;
						health -= amount;
						return;
					}
				} else {
					health -= amount;
				}
				return;
			case DamageType.HEALING:
				health += amount;
				return;
			default:
				return;
		}
	}

}

[System.Serializable]
public class UnitProperties {

	public string unitName = "unnamed";
	public float skillRanged = 100f;
	public float skillMelee	 = 100f;
	public int maxHealth = 8;
	public int armor = 0;
	
	public float size = 1.0f;

	internal Weapon weaponPrimary;
	internal Weapon weaponSecondary;

	public override string ToString() {
		return
			" Name=" + unitName +
			"\n Accuracy=" + skillRanged +
			"\n MaxHealth=" + maxHealth +
			"\n Armor=" + armor +
			"\n- - -";
	}

}

[System.Serializable]
public class UnitEquipment {

	public Weapon weaponPrimary;
	public Weapon weaponSecondary;

	public Biomod biomod;

	public Equippable[] misc = { };

	public UnitEquipment( Weapon weaponPrimary, Weapon weaponSecondary, Equippable[] misc, Biomod biomod = null ) {
		this.weaponPrimary = weaponPrimary;
		this.weaponSecondary = weaponSecondary;
		this.misc = misc;
		this.biomod = biomod;
	}

	public List<Equippable> Everything() {
		List<Equippable> list = new List<Equippable>();
		list.Add( weaponPrimary );
		list.Add( weaponSecondary );
		list.Add( biomod );
		list.AddRange( misc );
		return list;
	}

	internal UnitEquipment instance {
		get {
			List<Equippable> newMisc = new List<Equippable>();
			foreach( Equippable e in misc ) 
				newMisc.Add( GameObject.Instantiate( e ) as Equippable );
			return new UnitEquipment(
				GameObject.Instantiate( weaponPrimary ) as Weapon,
				GameObject.Instantiate( weaponSecondary ) as Weapon,
				newMisc.ToArray() , 
				biomod == null ? null : GameObject.Instantiate( biomod ) as Biomod
			);
		}
	}

}

public static class IsUnit {

	public static bool OutOfAmmo( Unit unit ) {
		return unit.currentWeapon.ranged && !( unit.currentWeapon as Firearm ).haveAmmo;
	}

}