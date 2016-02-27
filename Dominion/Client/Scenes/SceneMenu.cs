// Dominion - Copyright (C) Timothy Ings
// SceneMenu.cs
// This file defines classes that define the main menu scene

using ArwicEngine.Audio;
using ArwicEngine.Content;
using ArwicEngine.Core;
using ArwicEngine.Forms;
using ArwicEngine.Graphics;
using ArwicEngine.Net;
using ArwicEngine.Scenes;
using Dominion.Common;
using Dominion.Common.Data;
using Dominion.Common.Entities;
using Dominion.Common.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading;
using static ArwicEngine.Constants;

namespace Dominion.Client.Scenes
{
    public class SceneMenu : BaseScene
    {
        // defines a list item that holds an empire
        private class EmpireListItem : IListItem
        {
            public Button Button { get; set; }
            public RichText Text { get; set; }
            public Empire Empire { get; set; }

            public EmpireListItem(Empire empire)
            {
                // format the text with the empire's colours
                Empire = empire;
                Text = $"<[{Empire.PrimaryColor.ToRichFormat()}]{Empire.Name}>".ToRichText();
            }
        }

        // defines a list item that holds a player
        private class PlayerListItem : IListItem
        {
            public Button Button { get; set; }
            public RichText Text { get; set; }
            public BasicPlayer Player { get; set; }
            public string EmpireName { get; }

            public PlayerListItem(BasicPlayer player, EmpireManager empireManager)
            {
                // format the text with the players name, selected empire and an icon indicating the game host
                Player = player;
                Color empireColor = Color.White;
                if (empireManager != null)
                {
                    EmpireName = Player.EmpireID;
                    empireColor = empireManager.GetEmpire(Player.EmpireID).PrimaryColor;
                }
                else
                    EmpireName = "FACTORY=NULL";

                if (player.InstanceID == 0)
                    Text = $"$(capital) {Player.Name} - <[{empireColor.R},{empireColor.G},{empireColor.B}]{EmpireName}>".ToRichText();
                else
                    Text = new RichText(new RichTextSection($"{Player.Name} - ", Color.White), new RichTextSection(EmpireName, empireColor));
            }
        }

        // defines a list item that holds a string 
        private class StringListItem : IListItem
        {
            public Button Button { get; set; }
            public RichText Text { get; set; }

            public StringListItem(RichText s)
            {
                Text = s;
            }
        }
        
        private object _lock_guiSetUp = new object();

        private SpriteBatch sbGUI;
        private GameManager manager;
        private Canvas canvas;
        private Image background;
        private Form frm_main;
        private Dictionary<string, SoundEffect> empireAnthems;
        private Form frm_lobby;
        private Form frm_lobby_configWindow;
        private ScrollBox lobby_sbPlayers;
        private Button lobby_btnEmpireSelect;
        private Button lobby_btnWorldSize;
        private Button lobby_btnWorldType;
        private Button lobby_btnGameSpeed;
        private Button lobby_btnVictoryTypes;
        private Button lobby_btnOtherOptions;
        private CheckBox[] lobby_cbOtherOptions;
        private CheckBox[] lobby_cbVictoryTypes;
        private Form frm_hostGame;
        private Form frm_joinGame;
        private TextBox tbAddress;
        private TextBox tbPort;
        private TextBox tbUsername;
        private TextBox tbPassword;
        private bool host;
        private CancellationTokenSource getWANcancellationToken;
        private string lastEmpireID = "NULL";

        public SceneMenu(GameManager manager)
            : base ()
        {
            this.manager = manager;
            manager.Client.LobbyStateChanged += Client_LobbyStateChanged;
        }
        
        /// <summary>
        /// Occurs when the scene is entered
        /// </summary>
        public override void Enter()
        {
            // reset the client and the local server
            manager.Client.Dissconnect();
            manager.Server.StopServer();

            //load resources
            sbGUI = new SpriteBatch(GraphicsManager.Instance.Device);
            canvas = new Canvas(GraphicsManager.Instance.Viewport.Bounds);
            ConsoleForm consoleForm = new ConsoleForm(canvas);
            background = new Image(new Rectangle(0, 0, 1920, 1080), new Sprite($"Graphics/Backgrounds/Menu_{RandomHelper.Next(0, 6)}"), null, null);
            empireAnthems = Engine.Instance.Content.LoadListContent<SoundEffect>("Audio/Music/Anthems");
            SetUpMainForm();

            // Play all anthems on shuffle
            //AudioManager.Instance.MusicQueue.Clear();
            //foreach (KeyValuePair<string, SoundEffect> pair in empireAnthems)
            //    AudioManager.Instance.MusicQueue.Enqueue(pair.Value);
            //AudioManager.Instance.PlayerState = MusicPlayerState.Shuffle;
        }

        /// <summary>
        /// Occurs when the scene is left
        /// </summary>
        public override void Leave()
        {
        }

        #region Main Form
        // sets up the main for
        private void SetUpMainForm()
        {
            lock (_lock_guiSetUp)
            {
                // load the form config from file
                FormConfig formConfig = FormConfig.FromFile("Content/Interface/Menu/Main.xml");
                // setup the form
                canvas.RemoveChild(frm_main);
                frm_main = new Form(formConfig, canvas);
                frm_main.CentreControl();

                // get and setup the form elements
                Button btnHost = (Button)frm_main.GetChildByName("btnHost");
                btnHost.MouseClick += Main_BtnHost_MouseClick;
                Button btnJoin = (Button)frm_main.GetChildByName("btnJoin");
                btnJoin.MouseClick += Main_BtnJoin_MouseClick;
                Button btnOptions = (Button)frm_main.GetChildByName("btnOptions");
                btnOptions.MouseClick += Main_BtnOptions_MouseClick;
                Button btnQuit = (Button)frm_main.GetChildByName("btnQuit");
                btnQuit.MouseClick += Main_BtnQuit_MouseClick;
            }
        }
        private void Main_BtnQuit_MouseClick(object sender, MouseEventArgs e)
        {
            Engine.Instance.Exit(EXIT_SUCCESS);
        }
        private void Main_BtnOptions_MouseClick(object sender, MouseEventArgs e)
        {
            // NYI
        }
        private void Main_BtnJoin_MouseClick(object sender, MouseEventArgs e)
        {
            SetUpJoinGameForm();

            if (frm_lobby != null) frm_lobby.Visible = false;
            if (frm_hostGame != null) frm_hostGame.Visible = false;
            frm_main.Visible = false;
            frm_joinGame.Visible = true;
        }
        private void Main_BtnHost_MouseClick(object sender, MouseEventArgs e)
        {
            SetUpHostGameForm();

            if (frm_lobby != null) frm_lobby.Visible = false;
            if (frm_joinGame != null) frm_joinGame.Visible = false;
            frm_main.Visible = false;
            frm_hostGame.Visible = true;
        }
        #endregion

        #region Host Game Form
        // sets up the host game form
        private void SetUpHostGameForm()
        {
            lock (_lock_guiSetUp)
            {
                // load the form config from file
                FormConfig formConfig = FormConfig.FromFile("Content/Interface/Menu/Host.xml");
                // setup the form
                canvas.RemoveChild(frm_hostGame);
                frm_hostGame = new Form(formConfig, canvas);
                frm_hostGame.CentreControl();

                // get and setup the form elements
                tbAddress = (TextBox)frm_hostGame.GetChildByName("tbAddress");
                tbAddress.Text = "Retrieving WAN Address...";
                tbAddress.ToolTip = new ToolTip("This is your public IP address that people can use join to join your game over the internet", 500);
                SetCurrentAddressAsync();
                tbPort = (TextBox)frm_hostGame.GetChildByName("tbPort");
                tbPort.Text = "7894";
                tbPort.ToolTip = new ToolTip("You need to forward this TCP port to play over the internet", 500);
                tbUsername = (TextBox)frm_hostGame.GetChildByName("tbUsername");
                tbUsername.Text = Environment.MachineName;
                tbUsername.ToolTip = new ToolTip("This is the name other players will see you as", 500);
                tbPassword = (TextBox)frm_hostGame.GetChildByName("tbPassword");
                tbPassword.Text = "dog";
                tbPassword.ToolTip = new ToolTip("This is the password people will need to join your lobby", 500);
                Button btnBack = (Button)frm_hostGame.GetChildByName("btnBack");
                btnBack.MouseClick += Host_BtnBack_MouseClick;
                Button btnHost = (Button)frm_hostGame.GetChildByName("btnHost");
                btnHost.MouseClick += Host_BtnHost_MouseClick;
            }
        }
        private void Host_BtnBack_MouseClick(object sender, MouseEventArgs e)
        {
            RemoveAllForms();
            SetUpMainForm();
        }
        private void Host_BtnHost_MouseClick(object sender, MouseEventArgs e)
        {
            if (manager.Server.Running) // check if a server is already running
                return;
            if (tbUsername.Text.Equals("")) // check if the user entered a valid user name
            {
                ConsoleManager.Instance.WriteLine("Invalid username");
                return;
            }
            string address = NetHelper.DnsResolve("localhost");
            int port = 7894;
            try
            {
                port = Convert.ToInt32(tbPort.Text);
            }
            catch (Exception)
            {
                return; // return if the entered port is invalid
            }
            manager.Server.StartServer(port, tbPassword.Text); // start the server
            
            // connect to the server as a client
            bool connected = manager.Client.Connect(tbUsername.Text, address, port, tbPassword.Text);
            if (!connected)
            {
                manager.Client.Dissconnect();
                manager.Client.LobbyStateChanged += Client_LobbyStateChanged;
                manager.Server.StopServer();
                manager.Server = new Server.Server();
                return;
            }
            host = true; // flag this client as the server host

            RemoveAllForms();
        }
        // sets the ip address field of the host game form to the user's current public IP address
        private async void SetCurrentAddressAsync()
        {
            int timeOut = 10000;
            if (getWANcancellationToken != null)
                getWANcancellationToken.Cancel();
            getWANcancellationToken = new CancellationTokenSource(timeOut);
            string address = $"Request timed out ({timeOut}ms)";
            try
            {
                address = await NetHelper.GetPublicIPAsync().WithCancellation(getWANcancellationToken.Token);
            }
            catch { }
            if (tbAddress != null && address != null && !address.Equals(""))
                tbAddress.Text = address;
            else if (tbAddress != null)
                tbAddress.Text = $"Request timed out ({timeOut}ms)";
        }
        #endregion

        #region Join Game Form
        private void SetUpJoinGameForm()
        {
            lock (_lock_guiSetUp)
            {
                if (getWANcancellationToken != null) getWANcancellationToken.Cancel();
                FormConfig formConfig = FormConfig.FromFile("Content/Interface/Menu/Join.xml");
                canvas.RemoveChild(frm_joinGame);
                frm_joinGame = new Form(formConfig, canvas);
                frm_joinGame.Location = new Point(GraphicsManager.Instance.Viewport.Width / 2 - frm_joinGame.Size.Width / 2, GraphicsManager.Instance.Viewport.Height / 2 - frm_joinGame.Size.Height / 2);
                tbAddress = (TextBox)frm_joinGame.GetChildByName("tbAddress");
                tbAddress.Text = ConfigManager.Instance.GetVar(CONFIG_NET_CLIENT_ADDRESS);
                tbUsername = (TextBox)frm_joinGame.GetChildByName("tbUsername");
                tbUsername.Text = Environment.MachineName;
                tbPassword = (TextBox)frm_joinGame.GetChildByName("tbPassword");
                tbPassword.Text = "dog";
                Button btnBack = (Button)frm_joinGame.GetChildByName("btnBack");
                btnBack.MouseClick += Join_BtnBack_MouseClick;
                Button btnJoin = (Button)frm_joinGame.GetChildByName("btnJoin");
                btnJoin.MouseClick += Join_BtnJoin_MouseClick;
            }
        }
        private void Join_BtnBack_MouseClick(object sender, MouseEventArgs e)
        {
            RemoveAllForms();
            SetUpMainForm();
        }
        private void Join_BtnJoin_MouseClick(object sender, MouseEventArgs e)
        {
            if (manager.Client.Running)
                return;
            if (tbUsername.Text.Equals(""))
            {
                ConsoleManager.Instance.WriteLine("Invalid username");
                return;
            }
            string addressPort = tbAddress.Text;
            int port = 7894;
            string[] split = addressPort.Split(':');
            string address = NetHelper.DnsResolve(split[0]);
            if (split.Length > 1)
                try { port = Convert.ToInt32(split[1]); }
                catch (Exception) { }

            bool connected = manager.Client.Connect(tbUsername.Text, address, port, tbPassword.Text);
            if (!connected)
                return;
            host = false;

            RemoveAllForms();
        }
        #endregion

        #region Lobby Form
        // sets up the lobby form
        private void SetUpLobbyForm()
        {
            lock (_lock_guiSetUp)
            {
                // set the audio manager to play the selected empire's anthem over and over
                //AudioManager.Instance.PlayerState = MusicPlayerState.RepeatOne;

                // register events
                manager.Client.LostConnection += Client_LostConnection;

                // load the form config from file
                FormConfig formConfig = FormConfig.FromFile("Content/Interface/Menu/Lobby.xml");

                // setup the form
                canvas.RemoveChild(frm_lobby);
                frm_lobby = new Form(formConfig, canvas);
                frm_lobby.CentreControl();

                // get and setup the for elements
                Button btnStart = (Button)frm_lobby.GetChildByName("btnStart");
                if (host) btnStart.MouseClick += Lobby_BtnStart_MouseClick;
                else btnStart.Enabled = false;

                Button btnBack = (Button)frm_lobby.GetChildByName("btnBack");
                btnBack.MouseClick += Lobby_BtnBack_MouseClick;

                Button btnKick = (Button)frm_lobby.GetChildByName("btnKick");
                if (host) btnKick.MouseClick += Lobby_BtnKick_MouseClick;
                else btnKick.Enabled = false;

                Button btnBan = (Button)frm_lobby.GetChildByName("btnBan");
                if (host) btnBan.MouseClick += Lobby_BtnBan_MouseClick;
                else btnBan.Enabled = false;

                lobby_btnEmpireSelect = (Button)frm_lobby.GetChildByName("btnEmpire");
                lobby_btnEmpireSelect.MouseClick += Lobby_BtnEmpireSelect_MouseClick;

                lobby_btnWorldSize = (Button)frm_lobby.GetChildByName("btnWorldSize");
                if (host) lobby_btnWorldSize.MouseClick += Lobby_BtnWorldSize_MouseClick;
                else lobby_btnWorldSize.Enabled = false;

                lobby_btnWorldType = (Button)frm_lobby.GetChildByName("btnWorldType");
                if (host) lobby_btnWorldType.MouseClick += Lobby_BtnWorldType_MouseClick;
                else lobby_btnWorldType.Enabled = false;

                lobby_btnGameSpeed = (Button)frm_lobby.GetChildByName("btnGameSpeed");
                if (host) lobby_btnGameSpeed.MouseClick += Lobby_BtnGameSpeed_MouseClick;
                else lobby_btnGameSpeed.Enabled = false;

                lobby_btnVictoryTypes = (Button)frm_lobby.GetChildByName("btnVictoryTypes");
                lobby_btnVictoryTypes.MouseClick += Lobby_BtnVictoryTypes_MouseClick;

                lobby_btnOtherOptions = (Button)frm_lobby.GetChildByName("btnOtherOptions");
                lobby_btnOtherOptions.MouseClick += Lobby_BtnOtherOptions_MouseClick;

                lobby_sbPlayers = (ScrollBox)frm_lobby.GetChildByName("sbPlayers");
                lobby_sbPlayers.Items = GetPlayers();

                // fake an event to change the music
                Lobby_BtnEmpireSelect_MouseClick(lobby_btnEmpireSelect, new MouseEventArgs(false, false, false, Point.Zero, 0));
            }
        }
        private void Lobby_BtnBan_MouseClick(object sender, MouseEventArgs e)
        {
            // ban the selected player from the server
            BasicPlayer selectedPlayer = ((PlayerListItem)lobby_sbPlayers.Selected).Player;
            if (selectedPlayer.InstanceID != 0)
                manager.Server.BanPlayer(selectedPlayer.InstanceID);
        }
        private void Lobby_BtnKick_MouseClick(object sender, MouseEventArgs e)
        {
            // kick the selected player from the server
            BasicPlayer selectedPlayer = ((PlayerListItem)lobby_sbPlayers.Selected).Player;
            if (selectedPlayer.InstanceID != 0)
                manager.Server.KickPlayer(selectedPlayer.InstanceID);
        }
        private void Client_LobbyStateChanged(object sender, LobbyStateEventArgs e)
        {
            // update the lobby form's elements when the server sends a new lobby state
            if (e.LobbyState == null)
                return;
            if (frm_lobby == null)
                SetUpLobbyForm();
            lobby_sbPlayers.Items = GetPlayers();
            BasicPlayer myPlayer = e.LobbyState.Players.Find(p => p.InstanceID == manager.Client.Player.InstanceID);
            BasicPlayer hostPlayer = e.LobbyState.Players.Find(p => p.InstanceID == 0);
            frm_lobby.Text = $"{hostPlayer.Name}'s Lobby".ToRichText();
            lobby_btnEmpireSelect.Text = $"Empire: {manager.Client.EmpireManager.GetEmpire(myPlayer.EmpireID).Name}".ToRichText();
            lobby_btnWorldSize.Text = $"World Size: {e.LobbyState.WorldSize}".ToRichText();
            lobby_btnWorldType.Text = $"World Type: {e.LobbyState.WorldType}".ToRichText();
            lobby_btnGameSpeed.Text = $"Game Speed: {e.LobbyState.GameSpeed}".ToRichText();

            if (lobby_cbVictoryTypes != null)
                for (int i = 0; i < lobby_cbVictoryTypes.Length; i++)
                    lobby_cbVictoryTypes[i].Value = manager.Client.LobbyState.VictoryTypes[i];
            if (lobby_cbOtherOptions != null)
                for (int i = 0; i < lobby_cbOtherOptions.Length; i++)
                    lobby_cbOtherOptions[i].Value = manager.Client.LobbyState.OtherOptions[i];

            if (myPlayer.EmpireID != lastEmpireID)
                PlayAnthem(manager.Client.EmpireManager.GetEmpire(myPlayer.EmpireID).Name);
            lastEmpireID = myPlayer.EmpireID;
        }
        private void Client_LostConnection(object sender, EventArgs e)
        {
            // return to the main form
            RemoveAllForms();
            SetUpMainForm();
        }
        private void Lobby_BtnOtherOptions_MouseClick(object sender, MouseEventArgs e)
        {
            // setup the other options pane in the lobby form
            frm_lobby.RemoveChild(frm_lobby_configWindow);
            frm_lobby_configWindow = new Form(new Rectangle(frm_lobby.Bounds.Width - 235, 40, 230, 390), frm_lobby);
            frm_lobby_configWindow.CloseButtonEnabled = false;
            frm_lobby_configWindow.Draggable = false;
            frm_lobby_configWindow.DrawTitlebar = false;
            frm_lobby_configWindow.Text = "Select other options".ToRichText();

            int xOffset = 10;
            int yOffset = 35;
            int width = 20;
            int height = 20;
            int padding = 5;
            string[] otherOptions = Enum.GetNames(typeof(LobbyOtherOption));
            if (lobby_cbOtherOptions == null)
                lobby_cbOtherOptions = new CheckBox[otherOptions.Length];
            for (int i = 0; i < lobby_cbOtherOptions.Length; i++)
            {
                lobby_cbOtherOptions[i] = new CheckBox(new Rectangle(xOffset, yOffset + (height + padding) * i, width, height), frm_lobby_configWindow);
                lobby_cbOtherOptions[i].Text = otherOptions[i].ToRichText();
                lobby_cbOtherOptions[i].Value = manager.Client.LobbyState.OtherOptions[i];
                lobby_cbOtherOptions[i].Enabled = host;
                int locali = i;
                lobby_cbOtherOptions[i].ValueChanged += (s, a) =>
                {
                    if (host)
                    {
                        manager.Server.LobbyState.OtherOptions[locali] = lobby_cbOtherOptions[locali].Value;
                        manager.Server.SendLobbyStateToAll();
                    }
                };
            }
        }
        private void Lobby_BtnVictoryTypes_MouseClick(object sender, MouseEventArgs e)
        {
            // setup the victory type options pane in the lobby form
            frm_lobby.RemoveChild(frm_lobby_configWindow);
            frm_lobby_configWindow = new Form(new Rectangle(frm_lobby.Bounds.Width - 235, 40, 230, 390), frm_lobby);
            frm_lobby_configWindow.CloseButtonEnabled = false;
            frm_lobby_configWindow.Draggable = false;
            frm_lobby_configWindow.DrawTitlebar = false;
            frm_lobby_configWindow.Text = "Select victoy types".ToRichText();
            
            int xOffset = 10;
            int yOffset = 35;
            int width = 20;
            int height = 20;
            int padding = 5;
            string[] victroyTypes = Enum.GetNames(typeof(VictoryType));
            if (lobby_cbVictoryTypes == null)
                lobby_cbVictoryTypes = new CheckBox[victroyTypes.Length];
            for (int i = 0; i < lobby_cbVictoryTypes.Length; i++)
            {
                lobby_cbVictoryTypes[i] = new CheckBox(new Rectangle(xOffset, yOffset + (height + padding) * i, width, height), frm_lobby_configWindow);
                lobby_cbVictoryTypes[i].Text = victroyTypes[i].ToRichText();
                lobby_cbVictoryTypes[i].Value = manager.Client.LobbyState.VictoryTypes[i];
                lobby_cbVictoryTypes[i].Enabled = host;
                int locali = i;
                lobby_cbVictoryTypes[i].ValueChanged += (s, a) =>
                {
                    if (host)
                    {
                        manager.Server.LobbyState.VictoryTypes[locali] = lobby_cbVictoryTypes[locali].Value;
                        manager.Server.SendLobbyStateToAll();
                    }
                };
            }
        }
        private void Lobby_BtnGameSpeed_MouseClick(object sender, MouseEventArgs e)
        {
            // setup the game speed options pane in the lobby form
            frm_lobby.RemoveChild(frm_lobby_configWindow);
            frm_lobby_configWindow = new Form(new Rectangle(frm_lobby.Bounds.Width - 235, 40, 230, 390), frm_lobby);
            frm_lobby_configWindow.CloseButtonEnabled = false;
            frm_lobby_configWindow.Draggable = false;
            frm_lobby_configWindow.DrawTitlebar = false;
            frm_lobby_configWindow.Text = "Select game speed".ToRichText();
            int yOffset = 35;
            ScrollBox sb = new ScrollBox(new Rectangle(5, yOffset + 5, frm_lobby_configWindow.Bounds.Width - 10, frm_lobby_configWindow.Bounds.Height - yOffset - 10), GetEnumStringListItems(typeof(GameSpeed)), frm_lobby_configWindow);
            sb.SelectedIndex = (int)manager.Server.LobbyState.GameSpeed;
            sb.SelectedChanged += (s, a) =>
            {
                manager.Server.LobbyState.GameSpeed = (GameSpeed)sb.SelectedIndex;
                manager.Server.SendLobbyStateToAll();
            };
        }
        private void Lobby_BtnWorldType_MouseClick(object sender, MouseEventArgs e)
        {
            // setup the world tpye options pane in the lobby form
            frm_lobby.RemoveChild(frm_lobby_configWindow);
            frm_lobby_configWindow = new Form(new Rectangle(frm_lobby.Bounds.Width - 235, 40, 230, 390), frm_lobby);
            frm_lobby_configWindow.CloseButtonEnabled = false;
            frm_lobby_configWindow.Draggable = false;
            frm_lobby_configWindow.DrawTitlebar = false;
            frm_lobby_configWindow.Text = "Select world type".ToRichText();
            int yOffset = 35;
            ScrollBox sb = new ScrollBox(new Rectangle(5, yOffset + 5, frm_lobby_configWindow.Bounds.Width - 10, frm_lobby_configWindow.Bounds.Height - yOffset - 10), GetEnumStringListItems(typeof(WorldType)), frm_lobby_configWindow);
            sb.SelectedIndex = (int)manager.Server.LobbyState.WorldType;
            sb.SelectedChanged += (s, a) =>
            {
                manager.Server.LobbyState.WorldType = (WorldType)sb.SelectedIndex;
                manager.Server.SendLobbyStateToAll();
            };
        }
        private void Lobby_BtnWorldSize_MouseClick(object sender, MouseEventArgs e)
        {
            // setup the world size options pane in the lobby form
            frm_lobby.RemoveChild(frm_lobby_configWindow);
            frm_lobby_configWindow = new Form(new Rectangle(frm_lobby.Bounds.Width - 235, 40, 230, 390), frm_lobby);
            frm_lobby_configWindow.CloseButtonEnabled = false;
            frm_lobby_configWindow.Draggable = false;
            frm_lobby_configWindow.DrawTitlebar = false;
            frm_lobby_configWindow.Text = "Select world size".ToRichText();
            int yOffset = 35;
            ScrollBox sb = new ScrollBox(new Rectangle(5, yOffset + 5, frm_lobby_configWindow.Bounds.Width - 10, frm_lobby_configWindow.Bounds.Height - yOffset - 10), GetEnumStringListItems(typeof(WorldSize)), frm_lobby_configWindow);
            sb.SelectedIndex = (int)manager.Server.LobbyState.WorldSize;
            sb.SelectedChanged += (s, a) =>
            {
                manager.Server.LobbyState.WorldSize = (WorldSize)sb.SelectedIndex;
                manager.Server.SendLobbyStateToAll();
            };
        }
        private void Lobby_BtnEmpireSelect_MouseClick(object sender, MouseEventArgs e)
        {
            // setup the empire selection pane in the lobby form
            frm_lobby.RemoveChild(frm_lobby_configWindow);
            frm_lobby_configWindow = new Form(new Rectangle(frm_lobby.Bounds.Width - 235, 40, 230, 390), frm_lobby);
            frm_lobby_configWindow.CloseButtonEnabled = false;
            frm_lobby_configWindow.Draggable = false;
            frm_lobby_configWindow.DrawTitlebar = false;
            frm_lobby_configWindow.Text = "Select an empire".ToRichText();
            int yOffset = 35;
            ScrollBox sb = new ScrollBox(new Rectangle(5, yOffset + 5, frm_lobby_configWindow.Bounds.Width - 10, frm_lobby_configWindow.Bounds.Height - yOffset - 10), GetEmpires(), frm_lobby_configWindow);
            sb.SelectedIndex = manager.Client.EmpireManager.IndexOf(manager.Client.LobbyState.Players.Find(p => p.InstanceID == manager.Client.Player.InstanceID).EmpireID);
            sb.SelectedChanged += (s, a) =>
            {
                EmpireListItem eli = (EmpireListItem)sb.Selected;
                manager.Client.Lobby_SelectNewEmpire(eli.Empire.Name);
            };
        }
        private void Lobby_BtnStart_MouseClick(object sender, MouseEventArgs e)
        {
            // start the game
            manager.Server.StartGame();
        }
        private void Lobby_BtnBack_MouseClick(object sender, MouseEventArgs e)
        {
            // dissconnect from the server
            manager.Client.Dissconnect();
            manager.Client = new Client();
            manager.Client.LobbyStateChanged += Client_LobbyStateChanged;
            if (host) // if this client is the host, also stop and reset the server
            {
                manager.Server.StopServer();
                manager.Server = new Server.Server();
            }
            RemoveAllForms();
            SetUpMainForm();
        }
        #endregion

        // returns all the players in the game as list items
        private List<IListItem> GetPlayers()
        {
            try
            {
                List<IListItem> items = new List<IListItem>();
                foreach (BasicPlayer player in manager.Client.LobbyState.Players)
                    items.Add(new PlayerListItem(player, manager.Client.EmpireManager));
                return items;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // returns all the available empires as list items
        private List<IListItem> GetEmpires()
        {
            if (manager.Client.EmpireManager == null)
                return new List<IListItem>();
            List<IListItem> items = new List<IListItem>();
            foreach (Empire empire in manager.Client.EmpireManager.GetAllEmpires())
                items.Add(new EmpireListItem(empire));
            return items;
        }

        // returns the elements of an enum as string list items
        private List<IListItem> GetEnumStringListItems(Type e)
        {
            List<IListItem> items = new List<IListItem>();
            foreach (string s in Enum.GetNames(e))
                items.Add(new StringListItem(s.ToRichText()));
            return items;
        }

        // plays the anthem of the given empire
        private void PlayAnthem(string empireName)
        {
            //try
            //{
            //    SoundEffect anthem = empireAnthems[empireName];
            //    AudioManager.Instance.PlayMusic(anthem);
            //    AudioManager.Instance.PlayerState = MusicPlayerState.RepeatOne;
            //}
            //catch (Exception)
            //{
            //    ConsoleManager.Instance.WriteLine($"Could not find an anthem for {empireName}", MsgType.Warning);
            //}
        }
        
        // removes and resets all the forms used in this scene
        private void RemoveAllForms()
        {
            if (canvas != null)
            {
                canvas.RemoveChild(frm_hostGame);
                frm_hostGame = null;

                canvas.RemoveChild(frm_joinGame);
                frm_joinGame = null;

                if (frm_lobby != null)
                    frm_lobby.RemoveChild(frm_lobby_configWindow);
                frm_lobby_configWindow = null;
                canvas.RemoveChild(frm_lobby);
                frm_lobby = null;

                canvas.RemoveChild(frm_main);
                frm_main = null;
            }
        }

        /// <summary>
        /// Occurs when the enging updates
        /// </summary>
        public override void Update()
        {
            canvas.Update();
        }

        /// <summary>
        /// Occurs when the engine draws
        /// </summary>
        public override void Draw()
        {
            lock (_lock_guiSetUp)
            {
                try
                {
                    sbGUI.Begin();
                    background.Draw(sbGUI);
                    canvas.Draw(sbGUI);
                    sbGUI.End();
                }
                catch (Exception) { }
            }
        }
    }
}
