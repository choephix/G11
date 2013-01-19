using UnityEngine;
using System.Collections;

public class TestBehaviour : BaseClass {

	private Texture2D tex;

	[ SerializeField ] 
	private GUIText[] t;

	private GodOfProcesses pm;

	void Start() {

		pm = GetComponent<GodOfProcesses>();

		renderer.material.mainTexture = tex = new Texture2D( 32, 32 );

		GUI_TextureFactory.PaintPercentBar( tex, 0 );

	}

	void Update() {

		t[0].text = pm.ToGuiString();
		t[1].text = pm.ToGuiStringWatchers();

	}


	public void OnMouseUp() {

		GUI_TextureFactory.PaintPercentBar( tex, rand * 100 );

		Process p;

		p = PDelay( .5f );

		p.Enqueue( PDelay( .5f ) )
			.Enqueue( PDelay( .1f ) )
			.Enqueue( PDelay( .1f ) )
			.Enqueue( PDelay( .1f ) )
			.Enqueue( PDelay( .1f ) )
			.Enqueue( PDelay( .1f ) )
			.Enqueue( PDelay( .1f ) )
			.Enqueue( PDelay( .1f ) )
			.Enqueue( PDelay( .1f ) ).Enqueue( PDelay( 50 ) ).Attach( PDelay( .25f ) );

		pm.Add( p );

		Watcher<ProcessBook.Wait> w = new Watcher<ProcessBook.Wait>();

		w.eventWillStart += delegate( Process p1 ) {
			Process p2 = PDelay( 50 );
			p2.Attach( PDelay( 50 ) ).Attach( PDelay( 50 ) ).Attach( PDelay( 50 ) );
			p2.Enqueue( PDelay( 50 ) ).Enqueue( PDelay( 50 ) ).Enqueue( PDelay( 50 ) );
			pm.OvertakeAdd( p2, p1 );
			Debug.LogWarning( "BAZINGA! >> " + p1 );
		};

		pm.Add( w );

	}

	public static Process PDelay( int time ) { return new ProcessBook.Wait( time ); }
	public static Process PDelay( float time ) { return new ProcessBook.WaitForSeconds( time ); }

}