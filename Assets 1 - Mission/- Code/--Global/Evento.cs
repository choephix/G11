



public abstract class Evento {

	

}

public class Evento<T> : Evento {

	public readonly T dispatcher;

	public Evento( T dispatcher ) {
		dispatcher = dispatcher;
	}


}

public class Evento<T1, T2> : Evento<T1> {

	public readonly T1 subject;

	public Evento( T1 dispatcher, T2 subject ) : base( dispatcher) {
		subject = subject;
	}


}