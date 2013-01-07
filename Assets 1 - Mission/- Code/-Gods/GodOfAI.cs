using UnityEngine;
using System.Collections;

public class GodOfAI : MissionBaseClass {

	private Unit markedUnit;

	private bool readyForNextAction = true;

	void Update() {

		if( readyForNextAction ) {
			if( GameMode.interactive && TurnManager.currentTeam.isCpuControlled ) {
				StartCoroutine( ActCoroutine() );
			}
		}

	}

	internal IEnumerator ActCoroutine() {

		if( God.selectedUnit && TurnManager.isCpuTurn ) {

			readyForNextAction = false;

			yield return new WaitForSeconds( .05f );

			if( God.selectedUnit == markedUnit ) {
				if( TurnManager.isCpuTurn ) {
					TurnManager.EndTurn();
				}
			} else {

				if( God.selectedUnit.canAttack ) {
					yield return new WaitForSeconds( .05f );
					GodOfInteraction.OnInput_Attack();
					yield return new WaitForSeconds( .1f );
					GodOfInteraction.OnInput_Confirm();
				} else {
					//if( IsUnit.OutOfAmmo(selectedUnit) ) {
					//    yield return new WaitForSeconds( .5f );
					//    God.selectedUnit.Reload();
					//    yield return new WaitForSeconds( .5f );
					//} else 
					{
						if( markedUnit == null ) {
							markedUnit = God.selectedUnit;
						}
						if( TurnManager.isCpuTurn ) {
							SelectionManager.SelectAnotherUnit();
						}
					}
				}

			}

			yield return new WaitForSeconds( .05f );
			readyForNextAction = true;

		}

	}

	internal void OnTurnEnd() {
		readyForNextAction = true;
		markedUnit = null;
	}

}
