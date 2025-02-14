﻿using Sandbox;
using Sandbox.UI;

namespace TTT.UI;

public enum Channel
{
	All,
	Team,
	Spectator
}

[UseTemplate]
public partial class ChatBox : Panel
{
	private static readonly Color _allChatColor = PlayerStatus.Alive.GetColor();
	private static readonly Color _spectatorChatColor = PlayerStatus.Spectator.GetColor();

	public static ChatBox Instance { get; private set; }

	public Panel EntryCanvas { get; set; }
	public TabTextEntry Input { get; set; }
	public Channel CurrentChannel { get; private set; } = Channel.All;

	public bool IsOpen
	{
		get => HasClass( "open" );
		set
		{
			SetClass( "open", value );
			if ( value )
			{
				Input.Focus();
				Input.Text = string.Empty;
				Input.Label.SetCaretPosition( 0 );
			}
		}
	}

	public ChatBox()
	{
		Instance = this;

		Sandbox.Hooks.Chat.OnOpenChat += () =>
		{
			IsOpen = !IsOpen;
		};

		EntryCanvas.PreferScrollToBottom = true;
		EntryCanvas.TryScrollToBottom();

		Input.AddEventListener( "onsubmit", Submit );
		Input.AddEventListener( "onblur", () => IsOpen = false );
		Input.OnTabPressed += OnTabPressed;
	}

	public override void Tick()
	{
		base.Tick();

		var player = Local.Pawn as Player;

		if ( !IsOpen )
			return;

		if ( !player.IsAlive() )
			CurrentChannel = Channel.Spectator;
		else if ( !player.Role.CanTeamChat )
			CurrentChannel = Channel.All;

		switch ( CurrentChannel )
		{
			case Channel.All:
				Input.Style.BorderColor = _allChatColor;
				return;
			case Channel.Spectator:
				Input.Style.BorderColor = _spectatorChatColor;
				return;
			case Channel.Team:
				Input.Style.BorderColor = player.Role.Color;
				return;
		}

		Input.Placeholder = string.Empty;
	}

	public void AddEntry( string name, string message, string c = "" )
	{
		var entry = new ChatEntry( name, message );
		if ( !string.IsNullOrEmpty( c ) )
			entry.AddClass( c );
		EntryCanvas.AddChild( entry );
	}

	public void AddEntry( string name, string message, Color? color )
	{
		var entry = new ChatEntry( name, message, color );
		EntryCanvas.AddChild( entry );
	}

	private void Submit()
	{
		if ( string.IsNullOrWhiteSpace( Input.Text ) )
			return;

		if ( Input.Text == Strings.RTVCommand )
		{
			if ( Local.Client.GetValue<bool>( Strings.HasRockedTheVote ) )
			{
				AddInfo( "You have already rocked the vote!" );
				return;
			}
		}

		SendChat( Input.Text, CurrentChannel );
	}

	[ConCmd.Server]
	public static void SendChat( string message, Channel channel = Channel.All )
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
			return;

		if ( message == Strings.RTVCommand )
		{
			Game.RockTheVote();
			return;
		}

		if ( !player.IsAlive() )
		{
			var clients = Game.Current.State is InProgress ? Utils.GetDeadClients() : Client.All;
			AddChat( To.Multiple( clients ), player.Client.Name, message, Channel.Spectator );
			return;
		}

		if ( channel == Channel.All )
			AddChat( To.Everyone, player.Client.Name, message, channel, player.IsRoleKnown ? player.Role.Info.ResourceId : -1 );
		else if ( channel == Channel.Team && player.Role.CanTeamChat )
			AddChat( player.Team.ToClients(), player.Client.Name, message, channel, player.Role.Info.ResourceId );
	}

	[ConCmd.Client( "chat_add", CanBeCalledFromServer = true )]
	public static void AddChat( string name, string message, Channel channel, int roleId = -1 )
	{
		switch ( channel )
		{
			case Channel.All:
				Instance?.AddEntry( name, message, roleId != -1 ? ResourceLibrary.Get<RoleInfo>( roleId ).Color : _allChatColor );
				return;
			case Channel.Team:
				Instance?.AddEntry( $"(TEAM) {name}", message, ResourceLibrary.Get<RoleInfo>( roleId ).Color );
				return;
			case Channel.Spectator:
				Instance?.AddEntry( name, message, _spectatorChatColor );
				return;
		}
	}

	[ConCmd.Client( "chat_add_info", CanBeCalledFromServer = true )]
	public static void AddInfo( string message )
	{
		Instance?.AddEntry( message, "", "info" );
	}

	private void OnTabPressed()
	{
		if ( Local.Pawn is not Player player || !player.IsAlive() )
			return;

		if ( player.Role.CanTeamChat )
			CurrentChannel = CurrentChannel == Channel.All ? Channel.Team : Channel.All;
	}
}

