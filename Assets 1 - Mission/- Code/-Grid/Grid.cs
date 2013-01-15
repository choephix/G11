using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {
	
	public GridTile SampleTile;
	protected Int2D size;
	
	protected GridTile[,] tiles;
	
	void Start () {

	}

	internal void Build( Int2D size ) {

		this.size = size;
		
		float tileSize = 1f;
		tiles = new GridTile[size.x, size.y];
		for( short i=0; i<size.y; i++ ) {
			for( short j=0; j<size.x; j++ ) {
				tiles[j, i] = Instantiate( SampleTile, 
					new Vector3( ( j - size.x / 2 ) * tileSize, 0, ( i - size.y ) * tileSize ),
					SampleTile.transform.rotation ) 
					as GridTile;
				tiles[j, i].location = new Int2D( j, i );
				tiles[j, i].transform.parent = transform;
			}
		}

		ResetTiles();

		God.OnReady_Grid();
		
	}

	internal void ResetTiles() {

		print( "Resetting Tile Grid" );

		foreach( GridTile tile in tiles ) {
			tile.Reset();
		}

	}

	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * */

	public bool HasTile( int x, int y ) {
		return ( ( x >= 0 ) && ( x < size.x ) && ( y >= 0 ) && ( y < size.y ) );
	}

	public bool HasTile( Int2D v ) {
		return HasTile( v.x, v.y );
	}


	public GridTile GetTileSafely( int x, int y ) {
		x = x.ClipMaxMinInt( size.x - 1 );
		y = y.ClipMaxMinInt( size.y - 1 );
		return GetTile( x, y );
	}
	public GridTile GetTile( int x, int y ) {
		if( HasTile( x, y ) ) {
			return tiles[x, y];
		} else {
			Logger.Error( "NO TILE AT " + x + "x" + y );
			return null;
		}
	}
	public GridTile GetTile( Int2D v ) {
		return GetTile( v.x, v.y );
	}

	public GridTile[,] GetAllTiles() {
		return tiles;
	}
	

}
