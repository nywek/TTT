﻿using SandboxEditor;

namespace TTT;

[Category( "Ammo" )]
[ClassName( "ttt_ammo_sniper" )]
[EditorModel( "models/ammo/ammo_sniper/ammo_sniper.vmdl" )]
[HammerEntity]
[Title( "Sniper Ammo" )]
public class SniperAmmo : Ammo
{
	protected override AmmoType Type => AmmoType.Sniper;
	protected override int DefaultAmmoCount => 10;
	protected override string WorldModelPath => "models/ammo/ammo_sniper/ammo_sniper.vmdl";
}
