public class TestMod : Equippable {

	public override void Init( Unit owner ) {

		base.Init( owner );

		owner.actions.Add( new ActionsBook.Stomp( owner, this ) );

	}

}
