using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;


public class SuperInspector : EditorWindow {

	string dataFilePath = Application.dataPath + "/data.json";
	string dataFileText = "";
	string dataFileTextBox = "";

	string json = "";
	SaveData data;

	SerializedObject m_Object;
	SerializedProperty m_Property;

	[MenuItem( "Window/Super Inspector" )]
	public static void ShowWindow()	{
		EditorWindow.GetWindow( typeof( SuperInspector ) );
	}

	void OnEnable() {
		UpdateFileText();
	}

	void OnGUI() {

		if( GUI.Button( new Rect( 8, 8, 100, 16 ), "Save To Disc" ) ) {
			DataSave();
		}

		if( GUI.Button( new Rect( 110, 8, 100, 16 ), "Load From Disc" ) ) {
			DataLoad();
		}

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		if( m_Object!=null ) {
			m_Object.Update();
			EditorGUILayout.PropertyField( m_Property, true );
			m_Object.ApplyModifiedProperties();
		}

		EditorGUILayout.HelpBox( dataFileTextBox, MessageType.None, true );

		EditorGUILayout.HelpBox( GetBugboxText(), MessageType.None, true );

		Config.DEV_UNIT_MOVE_RANGE =
			EditorGUILayout.IntSlider( "DEV_UNIT_MOVE_RANGE", Config.DEV_UNIT_MOVE_RANGE, 0, 16 );

		//Config.BASE_UNIT_ACTIONS = (byte)
		//    EditorGUILayout.IntSlider( "BASE_UNIT_ACTIONS", Config.BASE_UNIT_ACTIONS, 1, 16 );

		Config.OVERRIDE_HIT_CHANCE_ACCURACY =
			GUILayout.Toggle( Config.OVERRIDE_HIT_CHANCE_ACCURACY, "OVERRIDE_HIT_CHANCE_ACCURACY" );

		Config.OVERRIDE_HIT_CHANCE_DISTANCE =
			GUILayout.Toggle( Config.OVERRIDE_HIT_CHANCE_DISTANCE, "OVERRIDE_HIT_CHANCE_DISTANCE" );

		Config.OVERRIDE_HIT_CHANCE_COVER =
			GUILayout.Toggle( Config.OVERRIDE_HIT_CHANCE_COVER, "OVERRIDE_HIT_CHANCE_COVER" );

		Config.OVERRIDE_HIT_CHANCE_UNIT_SIZE =
			GUILayout.Toggle( Config.OVERRIDE_HIT_CHANCE_UNIT_SIZE, "OVERRIDE_HIT_CHANCE_COVER_UNIT_SIZE" );

		//GUILayout.Label ("Base Settings", EditorStyles.boldLabel);
		//myString = EditorGUILayout.TextField ("Text Field", myString);

		//groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
		//    myBool = EditorGUILayout.Toggle ("Toggle", myBool);
		//    myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
		//EditorGUILayout.EndToggleGroup ();
	}

	private string GetBugboxText() {
		string s ="";
		if(God.selectedUnit) {
			s += "SELECTED UNIT:\n";
			s += God.selectedUnit.props.ToString();
		}
		return s;
	}

	private void UpdateFileText() {
		dataFileText = File.ReadAllText( dataFilePath );
		dataFileTextBox = 
			DateTime.Now.ToString( "HH:mm:ss tt" ) +
			" " + dataFilePath + ":\n\n" +
			dataFileText;
	}

	private void DataLoad() {
		Debug.Log( "loading data" );
		UpdateFileText();
		GodOfData.staticData =
		GodOfData.me.data = JsonMapper.ToObject<SaveData>( dataFileText );
		m_Object = new SerializedObject( GodOfData.me );
		m_Property = m_Object.FindProperty( "data" );
	}

	private void DataSave() {
		json = JsonMapper.ToJson( GodOfData.me.data );
		File.WriteAllText( dataFilePath, json );
		UpdateFileText();
		Debug.Log( "saved changes" );
	}

}