using System.Collections;
using UnityEngine;

public class GodOfInteraction : MissionBaseClass {

	internal static void OnInput_Back() {

		if( GameMode.interactive ) {
			selectedUnit.actions.AbortSelected();
		}

	}
	internal static void OnInput_Next() {

		switch( GameMode.Get() ) {
			case GameModes.Normal:
			case GameModes.PickTile:
				SelectionManager.SelectAnotherUnit();
				return;
			case GameModes.PickUnit:
				SelectionManager.TargetAnotherUnit();
				return;
			default:
				Debug.Log( "Next what?" );
				return;
		}

	}
	internal static void OnInput_Prev() {

		switch( GameMode.Get() ) {
			case GameModes.Normal:
			case GameModes.PickTile:
				SelectionManager.SelectAnotherUnit( true );
				return;
			case GameModes.PickUnit:
				SelectionManager.TargetAnotherUnit( true );
				return;
			default:
				Debug.Log( "Previous what?" );
				return;
		}

	}

	public static bool handleUserInput {
		get {
			return GameMode.interactive && TurnManager.isUserTurn;
		}
	}

	public static bool handleUserInputForCamera {
		get {return true;}
	}

	internal static void OnInput_Action( int i ) {
		if( GameMode.interactive ) {
			if( i == 0 ) {
				OnInput_SkipTurn(); return;
			} else {
				selectedUnit.actions.Select( i - 1 );
			}
		}
	}

	private static Vector3 tempV;

	internal static void OnInput_Directional( Vector2 amount ) {
		freeCameraHolder.InputMove( amount );
	}

	internal static void OnInput_Rotary( float amount ) {
		freeCameraHolder.InputRotate( amount );
	}

	internal static void OnInput_Wheel( float amount ) {
		freeCameraHolder.InputZoom( amount );
	}

	/// <summary>
	/// META HANDLERS
	/// </summary>

	internal static void OnInput_Attack() {

		if( GameMode.interactive ) {

			selectedUnit.UpdateEverything(); //TODO temp fix; find order with these at some point

			if( selectedUnit.canAttack ) {
				if( GameMode.Is( GameModes.PickUnit ) ) {
					OnInput_Confirm();
				} else {
					selectedUnit.actions.SelectAttack();
				}
			} else {
				if( !God.selectedUnit.canAttack ) {
					if( IsUnit.OutOfAmmo( selectedUnit ) ) {
						Logger.Respond( "No more ammo" );
						return;
					}
					if( !selectedUnit.hasActions ) {
						Logger.Respond( "No more actions this turn" );
						return;
					}
					if( !selectedUnit.objectsInRange.HaveEnemies() ) {
						Logger.Respond( "No enemies in effectRange" );
						return;
					}
				}
				if( !GameMode.interactive ) {
					Logger.Respond( "Game interaction is disabled at the moment" );
				}
			}

		}

	}

	internal static void OnInput_SwitchWeapon() {

		if( GameMode.interactive ) {

			selectedUnit.SwitchWeapon();

		}

	}
	
	private static void OnInput_SkipTurn() {
		TurnManager.EndTurn();
	}


	public static event EventHandler<GridTile> eventTilePicked = delegate { };
	public static event EventHandler<Unit> eventUnitPicked = delegate { };

	internal static void OnInput_Confirm() {

		switch( GameMode.Get() ) {
			case GameModes.PickTile:
				OnInput_Attack();
				return;
			case GameModes.PickUnit:
				selectedUnit.actions.ConfirmSelectedActionOn( targetedUnit );
				return;
			case GameModes.Disabled:
				Logger.AlertGameDisabled();
				return;
			default:
				Debug.Log( "Confirm what?" );
				return;
		}

	}

	internal static void OnPick_Tile( GridTile tile ) {

		if( GameMode.interactive ) {
			
			eventTilePicked.Invoke( tile );

			if( tile.occupied ) {
				eventUnitPicked.Invoke( tile.currentUnit );
			} 
			
		}

	}

	internal static void OnPick_Unit( Unit unit ) {

		if( GameMode.interactive ) {

			eventUnitPicked.Invoke( unit );

			eventTilePicked.Invoke( unit.currentTile );

		}

	}

}