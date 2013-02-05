using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class God : MissionBaseClass { //TODO rename this GodOfGameplay

	public static God me;

	internal static bool gameStarted = false;

	public MissionInitProps initProps;

	public BookOfEverything theBook;
	public SmartCamera theSmartCamera;
	public FreeCameraController theFreeCameraHolder;
	public Grid theGrid;
	public MissionGUI theGUI;

	void Awake() {
		me = this;
		god = this;
		gui = theGUI;
		smartCamera = theSmartCamera;
		freeCameraHolder = theFreeCameraHolder;
		grid = theGrid;
		allTeams = new List<Team>();
		allUnits = new List<Unit>();
	}

	void Update() {

		if( gameStarted ) {

			//if( GameMode.Is( GameModes.PickUnit ) && 
			//	( targetedUnit == null || !CanTarget( targetedUnit ) ) ) { //TODO TESTER FIX - remove this and find the cause of the NULL attackee bug
			//	GameMode.Reset();
			//}

			//if( selectedUnit && GameMode.interactive ) {

			//	if( selectedUnit.canAct ) {

			//		if( selectedAction == null && TurnManager.isUserTurn ) {

			//			Debug.Log( "God: NULL action handling" );
			//			if( selectedUnit.actions.shouldSelectPreviousAction ) {
			//				selectedUnit.actions.SelectPrevious();
			//			} else {
			//				selectedUnit.actions.SelectDefault();
			//			}

			//		}

			//	} else {

			//		God.OnSelectedUnitCantAct();

			//	}

			//}

		}

	}

	internal static void OnReady_Grid() {
		stage.BuildWorld(); 
	}

	private static int unitsReady=0;
	internal static void OnReady_Unit( Unit unit ) {
		unitsReady++;
		unit.eventActionFinished += OnUnitActionFinished;
		unit.eventActionStarted += OnUnitActionStarted;
		unit.eventDeath += OnUnitDeath;
		if( unitsReady == allUnits.FindAll( u => u.activated ).Count ) {
			OnReady_AllUnits();
		}
	}

	private static void OnReady_AllUnits() {

		if( !gameStarted ) {
			StartGame();
		}

	}
	
	private static void StartGame() {

		Debug.Log( "Game is starting." );

		GameMode.eventChanged += OnModeChange;
		GodOfInteraction.eventTilePicked += OnTilePicked;
		GodOfInteraction.eventUnitPicked += OnUnitPicked;
		SelectionManager.UnitSelectedEvent += OnUnitSelected;
		SelectionManager.UnitTargetedEvent += OnUnitTargeted;

		GameMode.SetDefault( GameModes.Normal );
		GameMode.Reset();
		gameStarted = true;
		TurnManager.Init();

	}

	// LOGIC

	internal static bool CanTarget( Unit unit ) {

		if( selectedUnit == null )
			return false;
		else
			return selectedUnit.CanTarget( unit );

	}

	// GAMEPLAY
			
	internal static void UpdateUnits() {

		Debug.Log( "Updating all units." );

		foreach( Unit unit in allUnits ) {
			if( unit.alive ) {
				unit.collider.enabled = CanTarget( unit );
				unit.UpdateEverything();
			}
		}

		SelectionManager.RefreshList();

		Debug.Log( "Enemies in " + selectedUnit + "'s effectRange: " + selectedUnit.objectsInRange.enemies.Count );

	}

	internal static void PassEquipment( Unit passer , Unit passee , Equippable item ) {

		List<Equippable> eList = new List<Equippable>( passer.equipment.misc );

		if( !eList.Contains( item ) ) {
			throw new UnityException(
				passer + " tried to pass item " + item + " to " + passee + " but it was not found in his equipment." );
		}

		eList.Remove( item );
		passer.equipment.misc = eList.ToArray( );
		passer.actions.RemoveAll( item.actions );

		eList = new List<Equippable>( passee.equipment.misc ) {item};
		passee.equipment.misc = eList.ToArray();
		passee.actions.AddRange( item.actions );

		foreach( Action a in item.actions ) {
			a.SetOwner( passee );
		}

		passee.model.Reload();

	}

	//EVENT HANDLERS

	internal static void OnUnitSelected( Unit unit ) {

	}

	internal static void OnUnitTargeted( Unit unit = null ) {

		UpdateUnits();

	}

	internal static void OnModeChange( GameModes newMode, GameModes oldMode ) {

		Debug.Log( "GameMode changed from " + oldMode + " to " + newMode );

		//switch( oldMode ) {
		//    case GameMode.ATTACK:
		//        targetedUnitN = 0;
		//        break;
		//    default:
		//        break;
		//}

		switch( newMode ) {
			case GameModes.PickUnit:
				CameraMode.Set( CameraMode.TARGET );
				break;
			case GameModes.PickTile:
			default:
				targetedUnit = null;
				allUnits.ForEach( u => u.collider.enabled = true );
				CameraMode.Set( CameraMode.FREE );
				break;
		}

	}

	internal static void OnTilePicked( GridTile tile ) {

		//else {
		//    if( GameMode.Is( GameModes.PickTile ) && selectedUnit.canMove && tile.selectable ) { //TODO move shit to CanDoAction
		//        return;
		//    }
		//    if( GameMode.Is( GameModes.PickUnit ) ) {
		//        GameMode.Reset();
		//    }
		//}

	}

	internal static void OnUnitPicked( Unit unit ) {

		if( GameMode.targeting ) {
			if( unit.targeted ) {
				GodOfInteraction.OnInput_Confirm(); //TODO (delegates?)
			} else if( CanTarget( unit ) ) {
				SelectionManager.TargetUnit( unit );
			}
			return;
		}
		if( GameMode.selecting ) {
			if( unit.selectable ) {
				SelectionManager.SelectUnit( unit, false );
			}
			return;
		}

	}

	internal static void OnTargetedUnitDeath() {

		Debug.Log( "Targeted unit " + targetedUnit + " is dead." );

	}

	internal static void OnUnitActionStarted( Action action ) {

	}

	internal static void OnUnitActionFinished( Action action ) {

		Debug.Log( "Updateing action finished. (" + action + ")" );

		UpdateUnits();

	}

/*
	internal static void OnSelectedUnitCantAct() {

		Debug.Log( "Selected unit can't act anymore" );

		GameMode.Reset();

		if( SelectionManager.selectableUnits.Count == 0 ) {

			Debug.Log( "No selectable units left" );
			TurnManager.EndTurn();

		} else {

			SelectionManager.SelectAnotherUnit();

		}

	}
*/

	internal static void OnUnitDeath( Unit unit, Unit killer ) {

	//	Debug.Log( unit + " has passed away." );
		Logger.Respond( unit + " was killed by " + killer );

		if( unit.team.units.FindAll( u => u.alive ).Count <= 0 ) {
			unit.team.OnWipedOut();
		}
		if( unit.targeted ) {
			OnTargetedUnitDeath();
		}

	}

	internal static void OnTeamWipedOut( Team team ) {

		Logger.Respond( team + " wiped out!" );

		if( allTeams.FindAll( t => t.alive ).Count <= 1 ) {
			GameMode.Set( GameModes.GameOver );
			Logger.Respond( " YOU WON!" );
		} else
			if( allTeams.FindAll( t => t.alive && t.isUserControlled ).Count < 1 ) {
				GameMode.Set( GameModes.GameOver );
				Logger.Respond( " YOU LOST!" );
			}

	}

	internal static void OnSquadActivated( Team team, int squad ) {
		if( gameStarted ) {
			UpdateUnits();
		}
	}

	internal static void OnTurnStart() {

		Debug.Log( TurnManager.currentTeam + "'s turn has started" );

	//	SelectionManager.SelectUnit( null, true );
		SelectionManager.SelectUnit( null, false );

	}

	internal static void OnTurnEnd() {

		Debug.Log( TurnManager.currentTeam + "'s turn has ended" );

		me.SendMessage( "OnTurnEnd" );

	}


}