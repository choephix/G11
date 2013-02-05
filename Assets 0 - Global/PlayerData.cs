using UnityEngine;
using System.Collections;

public class PlayerData : MonoBehaviour {

	public Equippable[] equipmentAll;
	public Equippable[] equipmentArmory;
	public Equippable[] equipmentTrunk;

	public Unit[] unitsAll;
	public Unit[] unitsUsable;
	public Unit[] unitsSquad;

}

[System.Serializable]
public class PlayerDataJsonReady : MonoBehaviour {

	public JEquippable[] equipment;
	public JEquippable[] units;



	public class JEquippable {

		public int item;
		public int owner;
		public bool trunked;

	}

	public class JUnit {

		public int model;

		public int skin;



	}

}
