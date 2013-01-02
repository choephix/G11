// C# example: A custom Inspector for Transform components.
using UnityEngine;
using UnityEditor;
using LitJson;
using System.IO;

[CustomEditor( typeof( GodOfData ) )]
public class GodOfDataInspector : Editor {

	SerializedObject m_Object;
//	SerializedProperty m_Property;

	string json = "";

	void OnEnable() {
		
		m_Object = new SerializedObject( target );
	//	m_Property = m_Object.FindProperty( "data" );
	//	File.AppendAllText( Application.dataPath + "/loggg.txt", JsonMapper.ToJson( ( target as GodOfData ).data ) );

		json = JsonMapper.ToJson( ( target as GodOfData ).data );
		File.WriteAllText( Application.dataPath + "/data.txt", json );
		Debug.Log( "saved changes" );

		GodOfData.staticData =
		( target as GodOfData ).data2 = JsonMapper.ToObject<SaveData>( json );

	}

	public override void OnInspectorGUI() {

		// Grab the latest data from the object
		m_Object.Update();
		
		// Editor UI for the property
		///EditorGUILayout.PropertyField( m_Property );

		// Apply the property, handle undo
		m_Object.ApplyModifiedProperties();

		base.OnInspectorGUI();

		EditorGUILayout.HelpBox( json, MessageType.None, true );
		
	}

}