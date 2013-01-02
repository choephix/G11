using UnityEngine;
using System.Collections;

public interface IDamageable {

	//float propHealth { get; }

	void Damage( float amount, DamageType type, Unit attacker = null );

}

public interface ICover {

	float coverValue { get; }

}

public interface ISomethingOnGridTile { //TODO turn to class

	GridTile currentTile { get; set; }

}