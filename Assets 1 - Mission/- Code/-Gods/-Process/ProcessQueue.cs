using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProcessQueue : MissionBaseClass {

	protected readonly List<Process> background = new List<Process>();
	protected readonly List<Process> queue = new List<Process>();

	protected readonly List<Watcher> watchers = new List<Watcher>();

	public bool runningQueue = false;
	public bool runningBackground = false;
	public bool running { get { return runningQueue && runningBackground; } }

	public bool empty { get { return emptyQueue && emptyBackground; } }
	public bool emptyQueue { get { return queue.Count.Equals( 0 ); } }
	public bool emptyBackground { get { return background.Count.Equals( 0 ); } }
	public Process currentProcess { get { return emptyQueue ? null : queue[0]; } }

	void Awake() {

		processQueue = this;

	}

	void Update() {

		if( !emptyQueue ) {

			if( !currentProcess.started ) {

				OnProcessWillStart( currentProcess );

			}

			UpdateProcess( currentProcess ); //TODO New current process will never be checked through OnProcessWillStart!!!!

		} else {

			runningQueue = false;

		}

		if( !emptyBackground ) {

			for( int i = 0 ; i < background.Count ; i++ ) {
				UpdateProcess( background[i] );
			}

		} else {

			runningBackground = false;

		}

	}

	void UpdateProcess( Process process ) {

		runningQueue = true;

		process.Update();

		if( process.ended ) {

			OnProcessEnded( process );

		}

	}

	private void OnProcessWillStart( Process process ) {

		if( process.hasAttached ) {
			background.AddRange( process.attachedPassiveProcesses );
		}

		if( !process.stackable ) {
			int i = 0;
			while( i < background.Count ) {
				if( background[i] != process && background[i].name.Equals( process.name ) ) {
					Remove( background[i] );
				} else {
					i++;
				}
			}
		}

		foreach( Watcher watcher in watchers ) {

			watcher.OnProcessWillStart( process );

			if( watcher.expired ) {

				Debug.Log( "Watcher " + watcher + " expired and will be removed" );

			}

		}

		watchers.RemoveAll( w => w.expired );

	}



	public void Add( Process process, bool inBackground = false ) {

		Debug.Log( "Adding new process " + process + " to the " + ( inBackground ? "background." : "queue" ) );

		if( inBackground ) {

			this.background.Add( process );

		} else {

			queue.Add( process );

		}

	}

	public void JumpAdd( Process process ) {
		queue.Insert( runningQueue ? 1 : 0, process );
	}

	public void Remove( Process process ) {

		foreach( Process p in process.enqueued ) {
			JumpAdd( p );
		}

		background.Remove( process );
		queue.Remove( process );

	}

	public void OnProcessEnded( Process process ) {
		Remove( process );
	}
	


	public void Add( Watcher watcher ) {

		Debug.Log( "Adding new watcher " + watcher + " to the watchers pool" );

		watchers.Add( watcher );

	}



	public void AddDelay( int frames ) { Add( new ProcessBook.Wait( frames ) ); }

	public void AddDelay( float seconds ) { Add( new ProcessBook.WaitSeconds( seconds ) ); }




	public string ToGuiStringWatchers() {

		const string pre = "Process Watchers: ";

		if( watchers.Count == 0 ) {
			return pre + "[empty]";
		}

		string s = watchers.Aggregate( "\n", ( c, w ) => "\n" + w.ToString() + c );
		return pre + s;

	}

	public string ToGuiStringBackground() {

		const string pre = "Background Processes: ";

		if( emptyBackground ) {
			return pre + "[empty]";
		}

		string s = background.Aggregate( "\n", ( c, p ) => "\n" + p.ToString() + c );
		return pre + s;

	}

	public string ToGuiString() {

		const string pre = "Process Queue: ";

		if( emptyQueue ) {
			return pre + "[empty]";
		}

		string s = queue.Aggregate( "\n" , ( current1 , p ) => "\n" + p.ToString() + current1 );
		return pre + s;

	}

	public override string ToString() {

		if( emptyQueue ) {
			return "[empty]";
		}

		return "current process: " + currentProcess.ToString();

	}

	public static ProcessQueue operator +( ProcessQueue pq, Process p ) { pq.Add( p ); return pq; }

	public static ProcessQueue operator +( ProcessQueue pq, Watcher w ) { pq.Add( w ); return pq; }

	//-- -- -- -- -- -- -- -- -- -- -- TREE

	protected readonly List<Process> current = new List<Process>();
	public bool aempty { get { return current.Count == 0; } }
	public bool arunning = false;

	void aUpdate() {

		if( !aempty ) {
			arunning = true;
			for( int i = 0 ; i < current.Count ; i++ ) {
				UpdateProcess( current[i] );
			}
		} else {
			arunning = false;
		}

	}

}
