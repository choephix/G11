using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class UnitBuffs {

	private readonly List<Buff> list = new List<Buff>();

	public void OnTurnStart() {

		int i = 0;
		Buff buff;
		bool expired;

		while( i < list.Count ) {

			buff = list[i];
			expired = false;

			if( buff.terminationCondition == BuffTerminationCondition.Timeout ) {

				buff.duration--;

				if( buff.duration <= 0 ) {

					Debug.Log( "Buff " + buff + " timed out and will be removed." );

					expired = true;

				}

			}

			if( expired ) {

				Remove( buff );

			} else {

				i++;

				buff.OnTurnStart();

			}

		}

	}

	//-- -- -- -- -- 

	public void Add( Buff buff ) {

		if( !buff.stackable ) {
			RemoveDuplicates( buff );
		}

		list.Add( buff );
		buff.OnApplied();
		buff.TerminatedEvent += () => Remove( buff );

	}

	private void Remove( Buff buff ) {
		buff.OnRemoved();
		list.Remove( buff );
	}

	public void Remove( string name ) {
		list.ForEach( delegate( Buff b ) { if( b.name.Equals( name ) ) Remove( b ); } );
	}

	private void RemoveDuplicates( Buff buff ) {
		Remove( buff.name );
	}

	private void RemoveOneDuplicate( Buff buff ) { //TODO DO
		Remove( buff.name );
	}

	//private void RemoveDuplicates( string name ) {

	//	//List<Buff> dups = list.FindAll( b => b.name.Equals( name ) );
	//	//dups.ForEach( Remove );

	//}

	public bool HasDuplicates( Buff buff ) {
		return HasBuff( buff.name );
	}
	public bool HasBuff( string name ) {

		List<Buff> dups = list.FindAll( b => b.name.Equals( name ) );
		return dups.Count > 0;

	}

	//-- -- -- -- -- 

	public bool GetFlagProp( BuffPropFlag flag ) {
		return list.Aggregate( false, ( current, buff ) => current | buff[flag] );
	}

	public float GetFlagMult( BuffPropMult mul ) {
		return list.Aggregate( 1f, ( current, buff ) => current * buff[mul] );
	}

	//-- -- -- -- -- 

	public override string ToString() {
		return list.Aggregate( "", ( current, buff ) => current + ( "{" + buff + '}' ) );
	}

	public bool this[BuffPropFlag flag] {
		get { return GetFlagProp( flag ); }
	}

	public float this[BuffPropMult mul] {
		get { return GetFlagMult( mul ); }
	}

	public static UnitBuffs operator +( UnitBuffs bm, Buff b ) { bm.Add( b ); return bm; }

}












