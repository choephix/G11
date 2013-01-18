using System.Collections.Generic;
using UnityEngine;

public abstract class Process {

	public readonly string name;
	public readonly bool stackable;

	public bool started;
	public bool ended;
	public event EventHandler eventStarted = delegate { };
	public event EventHandler eventEnded = delegate { };

	public bool hasAttached { get { return attachedPassiveProcesses.Count > 0; } }
	public readonly List<Process> attachedPassiveProcesses = new List<Process>();

	public readonly bool strictlyBackground = false; //TODO replace the process queues with ProcessTree and deprecate this shit

	protected Process( string name, bool stackable = true ) {
		this.name = name;
		this.stackable = stackable;
	}

	public virtual void Update() {

		if( !started ) {
			Start();
		}

		if( !ended ) {
			_Update();
		}

	}

	protected virtual void _Update() { }

	protected void Start() {

		started = true;
		eventStarted.Invoke();
		Events.processStarted.Invoke( this );

		Debug.Log( "process " + this + " just started" );

		_Start();

	}

	protected virtual void _Start() { }

	protected void End() {

		_End();

		Debug.Log( "process " + this + " just ended" );

		ended = true;
		eventEnded.Invoke();
		Events.processFinished.Invoke( this );

	}

	protected virtual void _End() { }

	public Process AttachPassive( Process process ) {

		attachedPassiveProcesses.Add( process );

		return process;

	}

	public override string ToString() {

		return name;

	}

	//-- -- -- -- -- -- -- -- -- -- -- TREE

	public readonly List<Process> enqueued = new List<Process>();

	public Process Enqueue( Process process ) {
		enqueued.Add( process );
		return process;
	}

}