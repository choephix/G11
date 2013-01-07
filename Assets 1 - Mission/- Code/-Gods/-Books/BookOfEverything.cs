using UnityEngine;
using System.Collections;
using System.IO;

public class BookOfEverything : MonoBehaviour {

	public static BookOfEverything me;

	public TeamInitProps[] teams;

	public UnitInitProps[] genericUnits;

	public TempObject[] gfx;

	void Start() {

		me = this;

	}

}

public enum Teams {
	H12 = 0,
	Di = 1,
	Street = 2,
	DirtyCorp = 3,
	ParaMili = 4,
	Monsters = 5
}