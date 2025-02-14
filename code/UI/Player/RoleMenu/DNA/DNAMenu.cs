using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

namespace TTT.UI;

[UseTemplate]
public partial class DNAMenu : Panel
{
	private readonly Dictionary<DNA, DNASample> _entries = new();
	private DNAScanner _dnaScanner;

	private Panel SampleContainer { get; init; }
	private Panel Empty { get; init; }
	private Label Charge { get; init; }
	private Label ChargeStatus { get; init; }
	private Checkbox AutoScan { get; init; }

	public override void Tick()
	{
		if ( !IsVisible || Local.Pawn is not Player player )
			return;

		_dnaScanner ??= player.Inventory.Find<DNAScanner>();
		if ( !_dnaScanner.IsValid() )
			return;

		ChargeStatus.Text = _dnaScanner.IsCharging ? "CHARGING" : "READY";

		if ( _dnaScanner.AutoScan != AutoScan.Checked )
			SetAutoScan( AutoScan.Checked );

		Charge.Text = _dnaScanner.SlotText;

		foreach ( var dna in _dnaScanner.DNACollected )
		{
			if ( !_entries.ContainsKey( dna ) )
				_entries[dna] = AddDNASample( dna );
		}

		foreach ( var dnaPanel in _entries.Values )
		{
			if ( !_dnaScanner.DNACollected.Contains( dnaPanel.DNA ) )
			{
				_entries.Remove( dnaPanel.DNA );
				dnaPanel?.Delete();
			}

			dnaPanel.SetClass( "selected", _dnaScanner?.SelectedId == dnaPanel.DNA.Id );
		}

		Empty.Enabled( !_entries.Any() );
	}

	private DNASample AddDNASample( DNA dna )
	{
		var panel = new DNASample( dna );
		SampleContainer.AddChild( panel );
		return panel;
	}

	public class DNASample : Panel
	{
		public DNA DNA { get; private set; }

		public DNASample( DNA dna )
		{
			DNA = dna;

			var deleteButton = Add.Icon( "cancel", "delete-button" );
			deleteButton.AddEventListener( "onclick", () => { DeleteSample( dna.Id ); } );

			Add.Button( $"#{dna.Id} - {dna.SourceName}", () => { SetActiveSample( dna.Id ); } );
		}
	}

	[ConCmd.Server]
	public static void SetActiveSample( int id )
	{
		Player player = ConsoleSystem.Caller.Pawn as Player;
		if ( !player.IsValid() )
			return;

		var scanner = player.Inventory.Find<DNAScanner>();
		if ( !scanner.IsValid() )
			return;

		foreach ( var dna in scanner.DNACollected )
		{
			if ( dna.Id == id )
			{
				scanner.SelectedId = id;
				return;
			}
		}
	}

	[ConCmd.Server]
	public static void DeleteSample( int id )
	{
		Player player = ConsoleSystem.Caller.Pawn as Player;
		if ( !player.IsValid() )
			return;

		var scanner = player.Inventory.Find<DNAScanner>();
		if ( !scanner.IsValid() )
			return;

		foreach ( var dna in scanner.DNACollected )
		{
			if ( dna.Id == id )
			{
				scanner.RemoveDNA( dna );
				return;
			}
		}
	}

	[ConCmd.Server]
	public static void SetAutoScan( bool enabled )
	{
		Player player = ConsoleSystem.Caller.Pawn as Player;
		if ( !player.IsValid() )
			return;

		var scanner = player.Inventory.Find<DNAScanner>();
		if ( !scanner.IsValid() )
			return;

		scanner.AutoScan = enabled;
	}
}
