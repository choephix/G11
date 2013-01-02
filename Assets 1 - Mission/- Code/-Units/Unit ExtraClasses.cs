using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionSubjectType { Self, GridTile, Unit }

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
	public bool shouldSelectPreviousAction { get { return previousAction != null && previousAction.canDoAgain; } }

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

		if( selectedAction != null ) {
			AbortSelected();
		}

		if( action.possible ) {

			Debug.Log( owner + " selecting action " + action.ToLongString() );
			selectedAction = action;


			if( action.subjectType == ActionSubjectType.Self ) {
				action._Execute( null );
			} else {
				action.OnSelected();
			}

			if( action.subjectType == ActionSubjectType.GridTile ) {
				GameMode.Set( GameModes.PickTile );
			}

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
		DoSelected( subject );
	}

	private void DoSelected( object subject ) {
		if( God.selectedAction != null ) {
			Do( God.selectedAction, subject );
		}
	}
	private void Do( Action action, object subject ) {

		if( action.IsSubjectViable( subject ) ) {
			Debug.Log( owner + " begins action " + action + " on subject " + subject );
			owner.OnAction( action );
			action._Execute( subject );
		} else {
			Debug.LogWarning( owner + " tried action " + action + " on inviable subject " + subject );
		}

	}

	public void OnFinished( Action action ) {

		Debug.Log( owner + " finished selected action " + selectedAction );

		Deselect();

	}
	public void AbortSelected() {

		Debug.Log( owner + " aborted selected action " + selectedAction );

		Deselect();

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
			if( selectedAction.IsSubjectViable( tile ) ) {
				ConfirmSelectedActionOn( tile );
			}
		}

	}
	internal void OnUnitPicked( Unit unit ) {

		if( active && selectedAction.subjectType == ActionSubjectType.Unit ) {
			if( selectedAction.IsSubjectViable( unit ) ) {
				ConfirmSelectedActionOn( unit );
			}
		}

	}

	private void OnOwnerSelected() {
		SelectDefault();
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











public class UnitUnitRelations {

	private readonly Dictionary<Unit,UnitUnitRelation> relations;

	public Unit primaryEnemy;

	internal UnitUnitRelations() {
		relations = new Dictionary<Unit, UnitUnitRelation>();
	}

	internal void Update( Unit owner, List<Unit> units ) {
		relations.Clear();
		foreach( Unit unit in units ) {
			relations.Add( unit, new UnitUnitRelation( owner, unit ) );
		}
	}

	internal UnitUnitRelation GetRelation( Unit unit ) {
		return relations[unit];
	}

	internal bool IsVisible( Unit unit ) {
		return relations[unit].visible;
	}

	internal float GetDistance( Unit unit ) {
		return relations[unit].distance;
	}

	internal float GetAngle( Unit unit ) {
		return relations[unit].angle;
	}

	internal float GetHitChance( Unit unit ) {
		return relations[unit].hitChance;
	}

	internal int CompareDistances( Unit u1, Unit u2 ) {
		return GetDistance( u1 ).CompareTo( GetDistance( u2 ) );
	}

	internal int CompareHitChances( Unit u1, Unit u2 ) {
		return GetHitChance( u2 ).CompareTo( GetHitChance( u1 ) );
	}

}

public struct UnitUnitRelation {

	internal readonly bool visible;
	internal readonly float angle;
	internal readonly float distance;
	internal readonly float hitChance;
	internal readonly float damageMax;
	internal readonly float critChance;
	internal readonly float critMultiplier;

	public UnitUnitRelation( Unit owner, Unit unit ) {
		this.visible = owner.CanSee( unit.currentTile );
		this.distance = owner.GetDistance( unit.transform );
		this.hitChance = owner.CalculateHitChance( unit );
		this.damageMax = owner.propAttackDamage;
		this.critChance = 50;
		this.critMultiplier = 2;
		this.angle = M.FixAngleDegSigned( owner.currentTile.relations.GetAngle( unit.currentTile ) + owner.rotationY - 90 );
	}

}

public class UnitObjectsInRange {

	internal List<Unit> units;
	internal List<Unit> allies;
	internal List<Unit> enemies;

	internal void Update( Unit owner, List<Unit> allUnits ) {

		UnitUnitRelations rel = owner.relations;

		units = allUnits.FindAll( ( u ) => ( owner.CanTarget( u ) ) );

		units.Sort( delegate( Unit u1, Unit u2 ) {
			return rel.CompareHitChances( u1, u2 );
		} );

		allies = units.FindAll( ( u ) => !owner.team.IsEnemy( u ) );
		enemies = units.FindAll( ( u ) => owner.team.IsEnemy( u ) );

	}

	internal bool HaveUnits() {
		return units.Count > 0;
	}

	internal bool HaveAllies( bool inclMe = false ) {
		return inclMe || ( allies.Count > 0 );
	}

	internal bool HaveEnemies() {
		return enemies.Count > 0;
	}

	internal void Clear() {
		units = new List<Unit>();
		allies = new List<Unit>();
		enemies = new List<Unit>();
	}

}

public static class UnitMath {

	internal static float GetHitChancePenalty_Cover( GridTile attackerTile, GridTile attackeeTile ) {
		if( Config.OVERRIDE_HIT_CHANCE_COVER ) {
			return 1.0f;
		}
		float r = ( 1.0f - attackeeTile.relations.GetTotalCoverValueAgainst( attackerTile ) );
		return M.ClipMaxMin( r );
	}

	private const float POINT_BLANK_RADIUS = 3f; // 1.5f
	internal static float GetHitChancePenalty_Distance( float distance, float range ) {
		if( Config.OVERRIDE_HIT_CHANCE_DISTANCE ) {
			return 1.0f;
		}
		if( distance > range ) {
			return 0.0f;
		}
		if( distance < POINT_BLANK_RADIUS ) {
			return 1.0f;
		}
		//Decrement with POINT_BLANK_RADIUS so that we calculate from there outwards
		distance -= POINT_BLANK_RADIUS;
		range -= POINT_BLANK_RADIUS;
		float r = ( 2.0f - ( distance / range ) ) / 2.0f;
		return M.ClipMaxMin( r );
	}

	internal static float GetHitChancePenalty_UnitSize( Unit attackee ) {
		return Config.OVERRIDE_HIT_CHANCE_UNIT_SIZE ? 1f : (0.5f+attackee.props.size/2);
	}

	//-----

	//internal static float GetHitChance_Melee( Unit attacker, Unit attackee ) {
	internal static float GetHitChancePenalty_MeleeWeapon( MeleeWeapon weapon, float distance ) {

		if( distance > 1.5f ) {
			return 0f;
		}
		return M.ClipMaxMin( weapon.speed / 100 ); //TODO add here units'sb chance to dodge

	}

}

public enum UnitModelPosture { Normal, CoverWall, CoverDucked }

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
	internal float health {
		get { return _health; }
		set { _health = M.ClipMaxMin( value, _maxHealth ); }
	}
	private float _armor;
	internal float armor {
		get { return _armor; }
		set { _armor = value > 0 ? value : 0; }
	}

	internal byte _maxActions;
	internal byte actionsLeft;

	internal void Init( UnitProperties props ) {
		_armor = props.armor;
		_health = props.maxHealth;
		_maxHealth = props.maxHealth;
		_maxActions = Config.BASE_UNIT_ACTIONS;
		ResetActions();
	}

	internal void ResetActions() {
		actionsLeft = _maxActions;
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

public static class IsUnit {

	public static bool OutOfAmmo( Unit unit ) {
		return unit.currentWeapon.ranged && !( unit.currentWeapon as Firearm ).haveAmmo;
	}

}