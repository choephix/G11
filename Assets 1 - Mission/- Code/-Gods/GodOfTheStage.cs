using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class GodOfTheStage : MissionBaseClass {

	internal static GodOfTheStage me;

	internal static List<WorldObject> objects;

	internal MissionInitProps initProps;

	public WorldContainer worldContainer;

	public Obstruction genericObstruction;
	public Transform obstruction50;
	public Transform obstruction100;
	public Transform obstruction200;

	public Transform genericFloorTile;

	void Awake() {
		me =
		stage = this;
	}

	void Start() {

		objects = new List<WorldObject>();
		grid.Build( StageMap.dimensions ); //TODO add a delegate to grid and attack BuildWorld();

		print( JsonMapper.ToJson( new UnitData() ) );

	}

	internal void BuildWorld() {
		int i = 0;

		InitTeams();

		List<Unit> userUnits = new List<Unit>();
		List<Unit> enemies1 = new List<Unit>();
		List<Unit> enemies2 = new List<Unit>();

		foreach( Team team in allTeams ) {
			if( team.isUserControlled ) {
				userUnits.AddRange( team.units );
			} else 
			if( enemies1.Count == 0 ) {
				enemies1.AddRange( team.units );
			} else {
				enemies2.AddRange( team.units );
			}
		}

		print( "enemies1:" + enemies1.Count + " enemies2:" + enemies2 );

		foreach( GridTile tile in grid.GetAllTiles() ) {
			switch( StageMap.MAP[i] ) {
				case StageMap.O1:
					AddObstruction( genericObstruction, tile, 0.5f, obstruction50 );
					break;
				case StageMap.O2:
					AddObstruction( genericObstruction, tile, 1.0f, obstruction100 );
					break;
				case StageMap.O3:
					AddObstruction( genericObstruction, tile, 1.5f, obstruction200 );
					break;
				case StageMap.U1:
				case StageMap.U2:
				case StageMap.U3:
				case StageMap.U4:
					if( userUnits.Count > 0 ) {
						userUnits[0].currentTile = tile;
						userUnits[0].transform.position = tile.transform.position;
						userUnits.RemoveAt( 0 );
					}
					AddDecor( genericFloorTile, tile );
					break;
				case StageMap.EB:
					if( enemies1.Count > 0 ) {
						enemies1[0].currentTile = tile;
						enemies1[0].transform.position = tile.transform.position;
						enemies1.RemoveAt( 0 );
					}
					AddDecor( genericFloorTile, tile );
					break;
				case StageMap.EA:
					if( enemies2.Count > 0 ) {
						enemies2[0].currentTile = tile;
						enemies2[0].transform.position = tile.transform.position;
						enemies2.RemoveAt( 0 );
					}
					AddDecor( genericFloorTile, tile );
					break;
				default:
					AddDecor( genericFloorTile, tile );
					break;
			}
			i++;
		}

	}

	internal void InitTeams() {
		byte i = 1;
		Team team;
		Unit unit;
		foreach( TeamInitProps teamProps in god.initProps.teams ) {

			team = AddTeam();
			team.SetProps( teamProps.name, teamProps.color, teamProps.isUserControlled );

			foreach( UnitInitProps unitProps in teamProps.units ) {

				unit = SpawnUnit( god.initProps.unitSample, team, grid.GetTile( teamProps.spawnTileCoordinates ) );
				//unit.SetModel( unitProps.model );
				unit.SetModel( god.initProps.defaultUnitModel );
				unit.SetMaterial( unitProps.skin );
				//unit.SetMaterial( god.initProps.defaultUnitMaterial );
				unit.props.unitName = unitProps.name;
				unit.props.skillRanged = unitProps.accuracy;
				unit.props.maxHealth = unitProps.health;
				unit.props.armor = unitProps.armor;
				unit.props.size = unitProps.size;
				unit.transform.localScale = new Vector3( 1, unitProps.size * 2, 1 );
				
				if( unitProps.equipment.weaponPrimary == null )
					unit.equipment = god.initProps.defaultEquipment.instance;
				else
					unit.equipment = unitProps.equipment.instance;

				i++;

			}

			team.eventSquadActivated += God.OnSquadActivated;
			if( team.isUserControlled ) {
				team.ActivateSquad(0);
			}

		}
	}

	/// ADDING REMOVING ENTITIES IN THE GAME

	internal Team AddTeam() {
		Team team = new Team();
		allTeams.Add( team );
		return team;
	}

	internal Obstruction AddObstruction( Obstruction prefab, GridTile tile, float coverValue, Transform model = null ) {
		Obstruction o = Instantiate( prefab, tile.transform.position, prefab.transform.rotation ) as Obstruction;
		o.transform.parent = worldContainer.obstructionsHolder;
		o.coverValue = coverValue;
		tile.setObstruction( o );
		if( model ) {
			AddDecor( model, tile );
		//	objects.Add( model as WorldObject );
		}
		return o;
	}

	internal void AddDecor( Transform model, GridTile tile ) {
		Transform o = Instantiate( model, tile.transform.position, tile.transform.rotation ) as Transform;
		o.parent = worldContainer.decorationHolder;
	}

	internal Unit SpawnUnit( Unit prefab, Team team, GridTile spawnTile, GridTile firstTile = null ) {
		Unit unit = Instantiate( prefab, spawnTile.transform.position, prefab.transform.rotation ) as Unit;
		unit.currentTile = firstTile ?? spawnTile;
		unit.team = team;
		allUnits.Add( unit );
		team.units.Add( unit );
		objects.Add( unit.model );
		unit.transform.parent = worldContainer.unitsHolder;
		return unit;
	}

}

public class Mockups {
	
}