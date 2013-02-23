using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MissionBaseClass {

	public static readonly List<Unit> selectableUnits = new List<Unit>();
	public static readonly List<Unit> targetableUnits = new List<Unit>();

	internal static event UnitEventHandler UnitSelectedEvent = delegate { };
	internal static event UnitEventHandler UnitTargetedEvent = delegate { };
	internal static event UnitEventHandler UnitUnselectedEvent = delegate { };
	internal static event UnitEventHandler UnitUntargetedEvent = delegate { };

	internal static void RefreshLists() {
		RefreshSelectablesList();
		RefreshTargetablesList();
	}

	internal static void RefreshSelectablesList() {
		selectableUnits.Clear();
		selectableUnits.AddRange( allUnits.FindAll( u => u.selectable ) );
		ReorderSelectablesList();
	}

	internal static void ReorderSelectablesList() {
		selectableUnits.Sort( ( u1, u2 ) =>
							  u2.transform.position.x.CompareTo( u1.transform.position.x ) );
	}


	internal static void RefreshTargetablesList() {
		targetableUnits.Clear();
		targetableUnits.AddRange( allUnits.FindAll( God.CanTarget ) );
		ReorderTargetablesList();

		foreach( Unit unit in allUnits ) {
			unit.MakeTargetable( God.CanTarget( unit ) );
		}
	}

	internal static void ReorderTargetablesList() {
		targetableUnits.Sort( ( u1, u2 ) =>
							  selectedUnit == null ? u2.transform.position.x.CompareTo( u1.transform.position.x ) :
								  selectedUnit.relations.GetAngle( u2 ).CompareTo( selectedUnit.relations.GetAngle( u1 ) )
			);
	}

	internal static bool SelectAnotherUnit( bool backwards = false ) {

		Debug.Log( "Selecting another unit. (Previous selected unit was " + F.ToStringOrNull( selectedUnit ) + ")" );

		RefreshSelectablesList();

		if( selectableUnits.Count == 0 ) {
			Debug.LogError( "Failed to SelectAnotherUnit. Selectable units list is empty" );
			return false;
		}

		if( selectableUnits.Count == 1 && selectableUnits[0] == selectedUnit ) {
			Debug.Log( "Failed to SelectAnotherUnit. This is the last selectable unit" );
			return false;
		}

		int n = selectableUnits.IndexOf( selectedUnit );
		n += ( backwards ? -1 : 1 );
		n = n.LoopMaxMin( selectableUnits.Count - 1 );

		Debug.Log( "Will select unit " + ( n + 1 ) + " of " + selectableUnits.Count + " (" + selectableUnits[n] + ")" );

		SelectUnit( selectableUnits[n], TurnManager.isUserTurn ); //TODO change this back
		//SelectUnit( selectableUnits[n], true );

		RefreshTargetablesList();

		return true;

	}

	internal static bool TargetAnotherUnit( bool backwards = false ) {

		RefreshTargetablesList();
		if( targetableUnits.Count > 0 ) {
			int n = targetableUnits.IndexOf( targetedUnit );
			Debug.Log( selectedUnit + " targeting another unit. (Previous targeted unit was " + F.ToStringOrNull( targetedUnit ) + ", [" + n + "] in the targetableUnits list" );
			n += ( backwards ? -1 : 1 );
			n = n.LoopMaxMin( targetableUnits.Count - 1 );
			Debug.Log( "Will target unit " + ( n + 1 ) + " of " + targetableUnits.Count + " (" + targetableUnits[n] + ")" );
			TargetUnit( targetableUnits[n] );
			return true;
		}

		Debug.Log( "Failed to TargetAnotherUnit. Targetable units list is empty." );
		return false;

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
			RefreshSelectablesList();
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

		RefreshTargetablesList();

		if( moveCamera ) {
			smartCamera.moveTo( selectedUnit.transform );
		}

	}

	internal static void TargetUnit( Unit unit = null, bool setAttackMode = true ) {

		RefreshTargetablesList();

		if( targetableUnits.Count > 0 ) {

			if( unit == null ) {
				unit = selectedUnit.relations.primaryEnemy;
			}

			if( unit == null ) {
				unit = targetableUnits[0];
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

	// NEW SHIT

	internal static void MakeUnitTargetable( Unit unit, bool targetable = true ) {
		
	}

	internal static void ResetUnits() {
		
	}


	//
	/// <summary>
	/// Relations between targetable subject and marker
	/// </summary>
	private static Dictionary<Transform, Transform> targetables = new Dictionary<Transform , Transform>();

	internal static void MarkTargetable( Transform subject , bool targetable = true ) {
		
		if( targetable ) {
			if( targetables.ContainsKey( subject ) )
				return;
			targetables.Add( subject , ( Transform ) Instantiate( markerTargetable , subject.position , Quaternion.identity ) );
		} else {
			if( !targetables.ContainsKey( subject ) )
				return;
			Destroy( targetables[subject].gameObject );
			targetables.Remove( subject );
		}

	}


}