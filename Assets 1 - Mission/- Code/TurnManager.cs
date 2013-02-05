using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MissionBaseClass {

	private static byte _roundN;
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