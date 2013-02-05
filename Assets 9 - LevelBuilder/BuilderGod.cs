using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using LitJson;

public class BuilderGod : MonoBehaviour {
	
	public Int2D mapSize;

	[SerializeField]
	private BuilderGridTile prefabTile;
	[SerializeField]
	private Transform prefabEraser;

	[SerializeField]
	private BuildingObjectsCategory[] prefabs;
	[SerializeField]
	private Transform[] floors;

	private int currentCategory;
	private int currentIndex;
	private int currentFloorIndex;
	private Transform sampleObject;
	private Transform sampleFloor;

	private readonly List<BuildingObjectRecord> objectRecords = 
		new List<BuildingObjectRecord>();

	private readonly List<BuilderGridTile> tiles =
		new List<BuilderGridTile>();

	private readonly Dictionary<BuilderGridTile, int> tileFloors =
		new Dictionary<BuilderGridTile, int>();
	
	private BuilderGuiText gui;

	[SerializeField]
	private Transform sampleHolder;
	
	void Awake() {

		gui = transform.Find( "gui" ).GetComponent<BuilderGuiText>();

	}

	void Start () {

		Load();

		SelectObject( 1, 0 );
		SelectFloor( 0 );

	}

	void Update() {

		if( Input.GetKey( KeyCode.LeftShift ) ) {

			if( Input.GetKeyDown( KeyCode.S ) ) {
				Save();
			}

			if( Input.GetKeyDown( KeyCode.D ) ) {
				Load();
			}

			if( Input.GetKeyDown( KeyCode.C ) ) {
				Clear();
			}

		} else {

			if( Input.GetKeyDown( KeyCode.Alpha0 ) ) {
				SelectObject( 0 , 0 );
			}

			if( Input.GetKeyDown( KeyCode.Alpha1 ) ) {
				SelectObject( 1 , 0 );
			}

			if( Input.GetKeyDown( KeyCode.Alpha2 ) ) {
				SelectObject( 2 , 0 );
			}

			if( Input.GetKeyDown( KeyCode.Alpha3 ) ) {
				SelectObject( 3 , 0 );
			}

			if( Input.GetKeyDown( KeyCode.Tab ) ) {
				if( Input.GetKey( KeyCode.LeftShift ) ) {
					SelectObject( currentCategory , ( currentIndex - 1 ) % GetCategory( currentCategory ).Count );
				} else {
					SelectObject( currentCategory , ( currentIndex + 1 ) % GetCategory( currentCategory ).Count );
				}
			}

			if( Input.GetKeyDown( KeyCode.P ) ) {
				Save();
			}

			if( !Input.GetKeyDown( KeyCode.F ) ) return;

			if( Input.GetKey( KeyCode.LeftShift ) ) {
				SelectFloor( ( currentFloorIndex - 1 ) % floors.Length );
			} else {
				SelectFloor( ( currentFloorIndex + 1 ) % floors.Length );
			}

		}

	}

	void SelectObject( int category, int index ) {

		currentCategory = category;

		if( category > 0 && ( index < 0 || index >= GetCategory( category ).Count ) ) {
			index = 0;
			Debug.LogError( "Prefab index out of bounds (" + index + "/" + GetCategory( category ).Count + ")" );
		}

		currentIndex = index;

		SetSampleObject( category > 0 ? GetPrefab( category, index ) : prefabEraser );

		UpdateGui();

		if( GetPrefab( category, index ).GetComponent<UnitModel>() ) print( "MODEL" );
		if( GetPrefab( category, index ).GetComponent<Obstruction>() ) print( "MODEL" );

	}

	void SelectFloor( int index ) {

		currentFloorIndex = index;

		SetSampleFloor( floors[index] );

		UpdateGui();

	}

	void AddShit( BuilderGridTile tile, int category, int index ) {

		if( category > 0 ) {

			if( tile.currentObject != null ) {
				RemoveShit( tile );
			}

			tile.currentObject = (Transform)Instantiate( GetPrefab( category, index ), tile.transform.position, Quaternion.identity );
			objectRecords.Add( Factory.BuildingObjectRecord( tile, category, index ) );
			
		} else {

			RemoveShit( tile );

		}

	}

	void SetFloor( BuilderGridTile tile, int index ) {
		
		if( tile.currentFloor != null ) {
			Destroy( tile.currentFloor.gameObject );
			tile.currentFloor = null;
		}

		tile.currentFloor = (Transform)Instantiate( floors[index], tile.transform.position, floors[index].rotation );
		tileFloors[ tile ] = index;

	}

	void RemoveShit( BuilderGridTile tile ) {

		objectRecords.RemoveAll( rec => rec.x == tile.x && rec.y == tile.y );

		if( tile.currentObject == null ) return;

		Destroy( tile.currentObject.gameObject );
		tile.currentObject = null;

	}

	void UpdateGui() {

		gui.UpdateText( "CAT: [" + GetCategory( currentCategory ).name + "] OBJ: [" + currentIndex + "] FLR: [" + currentFloorIndex + "]" );
		gui.UpdateBars( currentIndex, GetCategory(currentCategory).Count, currentFloorIndex, floors.Length );

	}

	private BuildingObjectsCategory GetCategory( int category ) {
		return prefabs[ category ];
	}

	Transform GetPrefab( int category, int index ) {
		return category <= 0 ? null : prefabs[ category ][ index ];
	}
	
	BuilderGridTile GetTile( int x , int y ) {
		return tiles.FirstOrDefault(tile => tile.x == x && tile.y == y);
	}

	//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|
	
	//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|

	void DoAction( BuilderGridTile tile, int action ) {

		switch( action ) {
			case 0: AddShit( tile, currentCategory, currentIndex );
				break;
			case 1: RemoveShit( tile );
				break;
			case 2:
				SetFloor( tile, currentFloorIndex );
				break;
			default:
				return;
		}
		
	}

	void OnTileClick( BuilderGridTile tile, int button ) {

		DoAction( tile , button );

	}

	void OnTileMouseOver( BuilderGridTile tile ) {

		if( Input.GetMouseButton( 0 ) ) {
			DoAction( tile, 0 );
		} else
		if( Input.GetMouseButton( 1 ) ) {
			DoAction( tile, 1 );
		} else if( Input.GetMouseButton( 2 ) ) {
			DoAction( tile , 2 );
		} else {

			const float DIST_MAX = 5;
			float dist;
			foreach( BuilderGridTile t in tiles ) {

				dist = Vector3.Distance( t.transform.position , tile.transform.position );

				if( dist >= DIST_MAX ) {
					t.marker.renderer.enabled = false;
					continue;
				}

				if( t == tile ) {
					//t.marker.localScale = Vector3.one * 1.5f;
					t.marker.localScale = Vector3.one;
					continue;
				}

				t.marker.renderer.enabled = true;
				//t.marker.localScale = Vector3.one * ( 1 - ( dist / DIST_MAX ) );
				t.marker.localScale = Vector3.one * ( 1 - ( dist / DIST_MAX ) ) / 2;

			}

		}

	}

	void OnTileMouseOut( BuilderGridTile tile ) {

		foreach( BuilderGridTile t in tiles ) {
		}

	}

	void SetSampleObject( Object prefab ) {

		if( sampleObject != null ) {
			Destroy( sampleObject.gameObject );
		}
		sampleObject = (Transform)Instantiate( prefab );
		sampleObject.parent = sampleHolder;
		sampleObject.localPosition = Vector3.zero;

	}

	void SetSampleFloor( Object prefab ) {

		if( sampleFloor != null ) {
			Destroy( sampleFloor.gameObject );
		}
		sampleFloor = (Transform)Instantiate( prefab );
		sampleFloor.parent = sampleHolder;
		sampleFloor.localPosition = Vector3.zero;
		sampleFloor.localScale = Vector3.one * 2;

	}

	//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|

	void Build( BuildingJsonReadyObject data ) {

		Clear();

		int x;
		int y;
		int i = 0;
		BuilderGridTile t;

		for( y = 0 ; y < mapSize.y ; y++ ) {

			for( x = 0 ; x < mapSize.x ; x++ ) {

				t = (BuilderGridTile)Instantiate( prefabTile );
				t.Init( x, y );
				t.onClick += OnTileClick;
				t.onOver += OnTileMouseOver;
				t.onOut += OnTileMouseOut;

				tiles.Add( t );
				SetFloor( t, data.floors[i] );

				i++;

			}

		}

		foreach( BuildingObjectRecord obj in data.objects ) {
			
			AddShit( GetTile(obj.x, obj.y), obj.c, obj.i );

		}


	}

	void Clear() {

		foreach( BuilderGridTile tile in tiles ) {
			RemoveShit( tile );
			Destroy( tile.currentFloor.gameObject );
			Destroy( tile.gameObject );
		}

		tiles.Clear();
		tileFloors.Clear(); 
		objectRecords.Clear();

	}

	void Save() {

		Debug.Log( "saving map" );

		string json = ComposeJsonString();

		File.WriteAllText( Application.dataPath + "/Data/test.stage", json );
		print( json );

	}

	void Load() {

		Debug.Log( "loading map" );

		string json = File.ReadAllText( Application.dataPath + "/Data/test.stage" );

		BuildingJsonReadyObject o = JsonMapper.ToObject<BuildingJsonReadyObject>( json );
		
		Build( o );

	}

	string ComposeJsonString() {
		
		return JsonMapper.ToJson( Factory.BuildingJsonReadyObject( objectRecords, tileFloors ) );

	}

	//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|//|\\|

	[System.Serializable]
	class BuildingObjectsCategory {

		public string name ;

		[SerializeField]
		private Transform[] prefabs;

		private int count;

		public int Count { 
			get {
				if( count <= 0 ) {
					count = prefabs.Length;
				}
				return count;
			}
		}

		public Transform this[int i] {
			get { return prefabs[i]; }
		}

	}

	private static class Factory {

		public static BuildingObjectRecord BuildingObjectRecord( BuilderGridTile tile, int category, int index ) {
			BuildingObjectRecord o = new BuildingObjectRecord {x = tile.x , y = tile.y , c = category , i = index};
			return o;
		}

		public static BuildingJsonReadyObject BuildingJsonReadyObject( List<BuildingObjectRecord> objects, Dictionary<BuilderGridTile, int> floors ) {
			BuildingJsonReadyObject o = new BuildingJsonReadyObject
				                            {
					                            objects = objects.ToArray() ,
					                            floors = floors.Values.ToArray()
				                            };
			return o;
		}

	}
	
}

[System.Serializable]
internal class BuildingJsonReadyObject {

	public BuildingObjectRecord[] objects;
	public int[] floors;

}

[ System.Serializable ]
internal class BuildingObjectRecord {

	public int x; //TILE X
	public int y; //TILE Y

	public int c; //CATEGORY
	public int i; //INDEX

}