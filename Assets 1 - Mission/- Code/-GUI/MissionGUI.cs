using UnityEngine;
using System.Collections;
using System.Text;

public class MissionGUI : MissionBaseClass {

	public GUISkin skin;

	private string consoleLog = "\nGUI ACTIVE\nSelect a unit and press [Space] to enter/exit ATTACK mode. Change target with [Z]/[X]. [Space] to Attack. Important stats are shown on the TOP LEFT corner of the screen.\n\nNEW!:\n[1] Attack\n[2] Reload\n[0] Skip Turn\n[TAB] Change Weapon";

	private MissionGUIRectangles rect = new MissionGUIRectangles();

	internal void Log( string s ) {
		consoleLog = s + "\n" + consoleLog;
	}

	void Awake() {

	}

	void OnGUI() {

		GUI.skin = skin;

		if( GameMode.Is( GameModes.GameOver ) ) {
			ShowGameOver( allTeams.FindAll( t => t.inPlay && t.isUserControlled ).Count > 0 );
			return;
		}
		
		if( true ) {
		//if( GameMode.interactive ) {

			if( true ) {
			//if( TurnManager.currentTeam.isUserControlled ) {

				StringBuilder sb = new StringBuilder();

				F.NullCheck( processQueue );

				sb.AppendLine( TurnManager.currentTeam.name + "'s turn." );
				
				sb.AppendLine();

				sb.AppendLine( "Selected Unit: " + UnitInfo( selectedUnit ) );

				sb.Append( "Health:" );
				for( byte i=0 ; i < selectedUnit.props.maxHealth ; i++ ) { sb.Append( i < selectedUnit.propHealth ? 'H' : '-' ); }
				for( byte i=0 ; i < selectedUnit.props.armor ; i++ ) { sb.Append( 'A' ); }
				sb.AppendLine();


				if( selectedUnit.currentWeapon.ranged ) {
					sb.Append( "Ammo:" );
					for( byte i=0 ; i < ( selectedUnit.currentWeapon as Firearm ).ammoLeft ; i++ ) { sb.Append( "[]" ); }
					sb.AppendLine();
				}

				sb.Append( "Actions:" );
				for( byte i=0 ; i < selectedUnit.status.actionPoints ; i++ ) { sb.Append( "{O}" ); }
				sb.AppendLine();

				sb.AppendLine( "\nBuffs:" + selectedUnit.buffs );

				if( targetedUnit ) {
					sb.AppendLine( 
						"\n\nTarget Unit:\n" + UnitInfo( targetedUnit ) +
						"\nHit Chance:" + selectedUnit.relations.GetAttackResult( targetedUnit ).hitChance + "%\n" +
						"Maximum Damage:" + selectedUnit.propAttackDamage +
						"\n\n" + selectedUnit.relations.GetAttackResult( targetedUnit ).longDescription
						);
				}

				GUI.Label( rect.selectedUnit, new GUIContent( sb.ToString() ), "UnitCard"+(TurnManager.isUserTurn?"":"Enemy") );

				GUI.Label( rect.console, consoleLog, "Log" + ( TurnManager.isUserTurn ? "" : "Enemy" ) );

				sb = new StringBuilder("Round " + TurnManager.roundN +
					"\n[TeamsWithTurn:" + TurnManager.activeTeamsWithTurn.Count + "/" + TurnManager.activeTeams.Count + "]" +
					"\n[Units:" + SelectionManager.selectableUnits.Count + "/" + TurnManager.currentTeam.units.Count + "]" +
					"\nWorldObjects:" + GodOfTheStage.objects.Count +
					"\nGameMode:" + GameMode.Get() +
					"\nTimeSpeed:" + GodOfTime.speed +
					"\n..");

				GUI.Label( rect.watchBox, sb.ToString(), "Log" + ( TurnManager.isUserTurn ? "" : "Enemy" ) );

				GUI.Label( rect.equipment, selectedUnit.currentWeapon + "  " + selectedAction + "\n" + selectedUnit.actions.ToStringRibbon(), "EquipmentRibbon" );

			} else {

				GUI.Label( rect.selectedUnit, new GUIContent( TurnManager.currentTeam.name + "'s Turn." ), "UnitCard" + ( TurnManager.isUserTurn ? "" : "Enemy" ) );

			}

		} else {

			if( God.gameStarted && TurnManager.currentTeam.isCpuControlled ) {

				GUI.Label( rect.selectedUnit, new GUIContent( TurnManager.currentTeam.name + "'s Turn. >>" ), "UnitCard" + ( TurnManager.isUserTurn ? "" : "Enemy" ) );

			}

		}



		GUI.Label( rect.screen, processQueue.ToGuiString(), "DebugProcessQueue" );

		GUI.Label( rect.screen, processQueue.ToGuiStringBackground(), "DebugProcessQueue2" );
		
	}

	private void ShowGameOver( bool win ) {

		string s = "Game Over.";
		s += win ? "\nYou Win!" : "\nYou Don't Win";

		GUI.Label( rect.screen, s, "GameOver" );

	}

	private string UnitInfo( Unit unit ) { //TODO 1 find out what THE FUCK is wrong with this...
		return ( unit.props.unitName + " (" + unit.status.health + "+" + unit.status.armor + "/" + unit.props.maxHealth + ")" );
	}

}

public class MissionGUIRectangles {

	internal Rect screen = new Rect( 0, 0, Screen.width, Screen.height );
	internal Rect selectedUnit = new Rect( 10, 10, 350, 100 );
	internal Rect targetUnit = new Rect( 260, 10, 350, 100 );
	internal Rect console = new Rect( Screen.width - 300, 0, 300, Screen.height );
	internal Rect watchBox = new Rect( 0, Screen.height / 2, Screen.width, Screen.height / 2 );
	internal Rect equipment = new Rect( 0, Screen.height / 2, Screen.width / 2, Screen.height / 2 );

}
