using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate bool QuestionHandler();
public delegate bool QuestionHandler<T>( T param );

public delegate void EventHandler();
public delegate void EventHandler<T>( T param );
public delegate void EventHandler<T1, T2>( T1 param1, T2 param2 );

public delegate void UnitEventHandler( Unit unit );
public delegate void UnitUnitEventHandler( Unit unit1, Unit unit2 );
public delegate void UnitExtraEventHandler<T>( Unit unit, T arg );

public class MissionBaseClass : BaseClass {

	protected static God god;
	public static GodOfTheStage stage;
	public static SmartCamera smartCamera;
	public static MissionGUI gui;

	public static ProcessQueue processQueue;

	protected static List<Team> allTeams;
	protected static List<Unit> allUnits;
	public static Grid grid;

	public static Unit selectedUnit;
	public static Unit targetedUnit;
	public static Action selectedAction;

	public static implicit operator Transform( MissionBaseClass instance ) {
		return instance.transform;
	}
	public static implicit operator GameObject( MissionBaseClass instance ) {
		return instance.gameObject;
	}

}

public class SelectionManager : MissionBaseClass {

	public static readonly List<Unit> selectableUnits = new List<Unit>();
	public static readonly List<Unit> targetableUnits = new List<Unit>();

	internal static event UnitEventHandler UnitSelectedEvent = delegate { };
	internal static event UnitEventHandler UnitTargetedEvent = delegate { };
	internal static event UnitEventHandler UnitUnselectedEvent = delegate { };
	internal static event UnitEventHandler UnitUntargetedEvent = delegate { };

	internal static void RefreshList() {
		selectableUnits.Clear();
		selectableUnits.AddRange( allUnits.FindAll( u => u.selectable ) );
		targetableUnits.Clear();
		targetableUnits.AddRange( allUnits.FindAll( God.CanTarget ) );
		ReorderLists();
	}

	internal static void ReorderLists() {
		selectableUnits.Sort( ( Unit u1, Unit u2 ) =>
			u2.transform.position.x.CompareTo( u1.transform.position.x ) );
		targetableUnits.Sort( ( Unit u1, Unit u2 ) =>
			selectedUnit == null ? u2.transform.position.x.CompareTo( u1.transform.position.x ) :
			selectedUnit.relations.GetAngle( u2 ).CompareTo( selectedUnit.relations.GetAngle( u1 ) )
			);
	}

	internal static bool SelectAnotherUnit( bool backwards = false ) {

		Debug.Log( "Selecting another unit. (Previous selected unit was " + F.ToStringOrNull( selectedUnit ) + ")" );

		RefreshList();

		if( SelectionManager.selectableUnits.Count == 0 ) {
			Debug.LogError( "Failed to SelectAnotherUnit. Selectable units list is empty" );
			return false;
		}

		if( selectableUnits.Count == 1 && selectableUnits[0] == selectedUnit ) {
			Debug.Log( "Failed to SelectAnotherUnit. This is the last selectable unit" );
			return false;
		}

		int n = selectableUnits.IndexOf( selectedUnit );
		n += ( backwards ? -1 : 1 );
		n = M.LoopMaxMin( n, selectableUnits.Count - 1 );

		Debug.Log( "Will select unit " + ( n + 1 ) + " of " + selectableUnits.Count + " (" + selectableUnits[n] + ")" );

		SelectUnit( selectableUnits[n], TurnManager.isUserTurn ); //TODO change this back
		//SelectUnit( selectableUnits[n], true );
		return true;

	}

	internal static bool TargetAnotherUnit( bool backwards = false ) {

		RefreshList();
		if( targetableUnits.Count > 0 ) {
			int n = targetableUnits.IndexOf( targetedUnit );
			Debug.Log( selectedUnit + " targeting another unit. (Previous targeted unit was " + F.ToStringOrNull( targetedUnit ) + ", [" + n + "] in the targetableUnits list" );
			n += ( backwards ? -1 : 1 );
			n = M.LoopMaxMin( n, targetableUnits.Count - 1 );
			Debug.Log( "Will target unit " + ( n + 1 ) + " of " + targetableUnits.Count + " (" + targetableUnits[n] + ")" );
			TargetUnit( targetableUnits[n] );
			return true;
		} else {
			Debug.Log( "Failed to TargetAnotherUnit. Targetable units list is empty." );
			return false;
		}

		//selectedUnit.UpdateEverything();

		//if( SelectionManager.TargetAnotherUnit( backwards ) ) {
		//    return true;
		//} else {
		//    Debug.Log( "No targetable units left" );
		//    GameMode.Reset();
		//    return false;
		//}

	}

	internal static void SelectUnit( Unit unit = null, bool moveCamera = false ) {

		Debug.Log( "Selecting " + unit + ". (Previous selected unit was " + F.ToStringOrNull( selectedUnit ) );

		if( unit == null ) {
			RefreshList();
			unit = selectableUnits[0];
		}

		Unit prevSelectedUnit = selectedUnit;

		if( prevSelectedUnit ) {
			prevSelectedUnit.OnDeselected();
			UnitUnselectedEvent( prevSelectedUnit );
		}

		selectedUnit = unit;
		UnitSelectedEvent( unit );
		selectedUnit.OnSelected();

		if( moveCamera ) {
			smartCamera.moveTo( selectedUnit.transform );
		}

	}

	internal static void TargetUnit( Unit unit = null, bool setAttackMode = true ) {

		RefreshList();

		if( targetableUnits.Count > 0 ) {

			if( unit == null ) {
				unit = selectedUnit.relations.primaryEnemy;
			}
			Debug.Log( "Targeting " + unit + ". (Previous targeted unit was " + F.ToStringOrNull( targetedUnit ) );

			Unit prevTargetedUnit = targetedUnit;

			//TODO nullify targetUnit on exiting ATTACK MODE or something
			if( targetedUnit != unit ) {
				targetedUnit = unit;
				selectedUnit.OnTargetingUnit( targetedUnit );
				targetedUnit.OnTargetedBy( selectedUnit );
			}

			UnitTargetedEvent( unit );

			if( prevTargetedUnit ) {
				prevTargetedUnit.OnUntargetedBy( selectedUnit );
				UnitUntargetedEvent( prevTargetedUnit );
			}

			if( setAttackMode ) {
				GameMode.Set( GameModes.PickUnit );
			}

		} else {

			Logger.Respond( "Nothing to target" );

		}

	}

}

public class TurnManager : MissionBaseClass {

	private static byte _roundN = 0;
	internal static byte roundN { get { return _roundN; } }

	internal static List<Team> activeTeams;
	internal static List<Team> activeTeamsWithTurn;
	internal static Team currentTeam;

	public static bool isUserTurn { get { return currentTeam.isUserControlled; } }
	public static bool isCpuTurn { get { return !isUserTurn; } }

	internal static void Init() {
		NextTurn();
	}

	internal static void EndTurn() {
		currentTeam.hasTurn = false;
		God.OnTurnEnd();
		NextTurn();
	}

	private static void NextTurn() {
		activeTeams = allTeams.FindAll( t => t.inPlay );
		if( activeTeams.Count > 0 ) {
			activeTeamsWithTurn = activeTeams.FindAll( t => t.hasTurn );
			if( activeTeamsWithTurn.Count > 0 ) {
				GiveTurn( activeTeamsWithTurn[0] );
			} else {
				NextRound();
			}
		} else if( activeTeams.Count == 1 ) {
		} else {
			throw new UnityException( "no more teams to play" );
		}
	}

	private static void GiveTurn( Team team ) {
		currentTeam = team;
		allUnits.ForEach( u => { if( u.alive ) u.collider.enabled = true; } );
		currentTeam.units.ForEach( u => u.OnOurTurnStart() );
		God.OnTurnStart();
	}

	internal static void NextRound() {
		_roundN++;
		foreach( Team team in activeTeams ) {
			team.hasTurn = true; //TODO implement cooldown
		}
		NextTurn();
	}

}

public class GameMode {

	internal static EventHandler<GameModes, GameModes> eventChanged;

	internal static bool cinematic = false;
	internal static bool selecting { get { return !targeting; } }
	internal static bool targeting { get { return Is( GameModes.PickUnit ); } }
	internal static bool interactive {
		get {
			return
				( !Is( GameModes.Disabled ) && !Is( GameModes.GameOver ) && !cinematic && GodOfPathfinding.ready && !God.selectedUnit.acting && God.processQueue.empty );
		}
	}

	private static GameModes @default = GameModes.Disabled;
	private static GameModes current = @default;
	private static GameModes last =  GameModes.Normal;

	internal static bool Is( GameModes value ) {
		return current == value;
	}

	internal static GameModes Get() {
		return current;
	}

	internal static bool Set( GameModes value ) {

		if( !Is( GameModes.GameOver ) ) {

			if( value == GameModes.Default ) {
				value = @default;
			}

			if( value == GameModes.Disabled && current != GameModes.Disabled ) {
				last = current;
			}

			if( value == current ) {
				return false;
			} else {
				eventChanged( value, current );
				current = value;
				return true;
			}

		}

		return false;

	}

	internal static bool Toggle( GameModes value ) {
		if( value == current ) {
			Set( GameModes.Default );
			return false;
		} else {
			Set( value );
			return true;
		}
	}

	internal static void Disable() {
		Set( GameModes.Disabled );
	}

	internal static void Reenable() {
		Set( last );
	}

	internal static void SetDefault( GameModes value ) {
		@default = value;
	}

	internal static void Reset() {
		Set( @default );
		if( God.selectedUnit ) {
			God.selectedUnit.OnUntargetingUnit(); //REFACTOR THIS CRAP
		}
		GodOfTime.speed = 1f;
	}

}