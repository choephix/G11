using UnityEngine;
using System.Collections;

public class BuilderGuiText : MonoBehaviour {

	private GUIText txt;
	private GUIText txtAnim;

	void Awake() {

		txt = transform.Find( "txt" ).GetComponent<GUIText>();
		txtAnim = transform.Find( "txtAnim" ).GetComponent<GUIText>();
	
	}
	
	public void UpdateText ( string text ) {

		txt.text = text;

	}

	void Update() {
	}

	public void UpdateBars( int objIndex , int objCount , int flrIndex , int flrCount ) {

		string s = "";

		for( int i = 0 ; i < objCount ; i++ ) {
			s += i == objIndex ? "/ " : "- ";
		}

		s += "\n";

		for( int i = 0 ; i < flrCount ; i++ ) {
			s += i == flrIndex ? "/ " : "- ";
		}

		txtAnim.text = s;

	}

}
