using Sandbox;

namespace TTT;

[ClassName( "ttt_entity_decoy" )]
[EditorModel( "models/decoy/decoy.vmdl" )]
[Title( "Decoy" )]
public partial class DecoyEntity : Prop, IEntityHint, IUse
{
	private static readonly Model WorldModel = Model.Load( "models/decoy/decoy.vmdl" );

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		Health = 100f;
	}

	protected override void OnDestroy()
	{
		Owner?.Components.RemoveAny<DecoyComponent>();
		base.OnDestroy();
	}

	bool IUse.OnUse( Entity user )
	{
		var player = user as Player;
		player.Inventory.Add( new Decoy() );
		Delete();

		return false;
	}

	bool IUse.IsUsable( Entity user )
	{
		return user is Player && (Owner is null || user == Owner);
	}
}
