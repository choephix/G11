using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Team {

	public string name;
	public Color color;
	public Color color2;

	public bool alive = true;
	public bool inPlay { get { return alive && hasUnitsInPlay; } }
	public bool hasTurn = false;

	public bool isUserControlled;
	public bool isCpuControlled { get { return !isUserControlled; } }

	internal List<Unit> units = new List<Unit>();
	internal List<Unit> unitsInPlay = new List<Unit>(); //TODO implement team.unitsInPlay
	internal List<Team> allyTeams = new List<Team>();
	
	internal event EventHandler<Team, int> eventSquadActivated;

	internal bool isTheirTurn {
		get { return TurnManager.currentTeam == this; }
	}

	internal bool hasUnitsInPlay {
		get { return units.FindAll( u => u.inPlay ).Count > 0; }
	}

	//-----------------------------------------------------------

	internal bool IsAlly( Unit unit ) {
		return ( unit.team == this ) || IsAlly( unit.team );
	}

	internal bool IsAlly( Team team ) {
		return team != this && ( allyTeams.Count > 0 && allyTeams.Contains( team ) );
	}

	internal bool IsEnemy( Unit unit ) {
		return IsEnemy( unit.team );
	}

	internal bool IsEnemy( Team team ) {
		return team != this && ( allyTeams.Count == 0 || !allyTeams.Contains( team ) );
	}

	//-----------------------------------------------------------
	
	internal void SetProps( string name, Color color, bool isUserControlled ) {
		this.name = name;
		this.color = color;
		this.isUserControlled = isUserControlled;
		this.color2 = Color.Lerp( color, Color.white, .5f );
	}

	internal void ActivateSquad(int squad) {

		units.ForEach( u => {
			if( u.squad == squad )
				u.Activate();
			} );

		hasTurn = true;
		eventSquadActivated.Invoke( this, squad );

		Debug.Log( "Team " + name + " activated." );

	}

	internal void OnWipedOut() {
		alive = false;
		God.OnTeamWipedOut( this );
	}

	//-----------------------------------------------------------

	public override string ToString() {
		return name;
	}

}