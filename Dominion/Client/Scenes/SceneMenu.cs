using ArwicEngine.Audio;
using ArwicEngine.Core;
using ArwicEngine.Forms;
using ArwicEngine.Graphics;
using ArwicEngine.Net;
using ArwicEngine.Scenes;
using Dominion.Common;
using Dominion.Common.Entities;
using Dominion.Common.Factories;
using Dominion.Server;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Threading;
using static ArwicEngine.Constants;

namespace Dominion.Client.Scenes
{
    public class SceneMenu : BaseScene
    {
        private class EmpireListItem : IListItem
        {
            public Button Button { get; set; }
            public RichText Text { get; set; }
            public Empire Empire { get; set; }

            public EmpireListItem(Empire empire)
            {
                Empire = empire;
                Text = $"<[{Empire.PrimaryColor.ToRichFormat()}]{Empire.Name}>".ToRichText();
            }
        }

        private class PlayerListItem : IListItem
        {
            public Button Button { get; set; }
            public RichText Text { get; set; }
            public BasicPlayer Player { get; set; }
            public string EmpireName { get; }

            public PlayerListItem(BasicPlayer player, EmpireFactory empireFactory)
            {
                Player = player;
                Color empireColor = Color.White;
                if (empireFactory != null)
                {
                    EmpireName = empireFactory.GetEmpire(Player.EmpireID).Name;
                    empireColor = empireFactory.GetEmpire(Player.EmpireID).PrimaryColor;
                }
                else
                    EmpireName = "FACTORY=NULL";

                if (player.PlayerID == 0)
                    Text = $"$(capital) {Player.Name} - <[{empireColor.R},{empireColor.G},{empireColor.B}]{EmpireName}>".ToRichText();
                else
                    Text = new RichText(new RichTextSection($"{Player.Name} - ", Color.White), new RichTextSection(EmpireName, empireColor));
            }
        }

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
        private int lastEmpireID;

        public SceneMenu(GameManager manager)
            : base ()
        {
            this.manager = manager;
            manager.Client.LobbyStateChanged += Client_LobbyStateChanged;
            lastEmpireID = -1;
        }
        
        public override void Enter()
        {
            manager.Client.Dissconnect();
            manager.Server.StopServer();

            sbGUI = new SpriteBatch(GraphicsManager.Instance.Device);

            canvas = new Canvas(GraphicsManager.Instance.Viewport.Bounds);
            ConsoleForm consoleForm = new ConsoleForm(canvas);
            background = new Image(new Rectangle(0, 0, 1920, 1080), new Sprite($"Graphics/Backgrounds/Menu_{RandomHelper.Next(0, 6)}"), null, null);
            empireAnthems = Engine.Instance.Content.LoadListContent<SoundEffect>("Audio/Music/Anthems");
            SetUpMainForm();

            // Play all anthems on shuffle
            AudioManager.Instance.MusicQueue.Clear();
            foreach (KeyValuePair<string, SoundEffect> pair in empireAnthems)
                AudioManager.Instance.MusicQueue.Enqueue(pair.Value);
            AudioManager.Instance.PlayerState = MusicPlayerState.Shuffle;
        }

        public override void Leave()
        {
        }

        #region Main Form
        private void SetUpMainForm()
        {
            lock (_lock_guiSetUp)
            {
                FormConfig formConfig = FormConfig.FromFile("Content/Interface/Menu/Main.xml");
                canvas.RemoveChild(frm_main);
                frm_main = new Form(formConfig, canvas);
                frm_main.CentreControl();
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
        private void SetUpHostGameForm()
        {
            lock (_lock_guiSetUp)
            {
                FormConfig formConfig = FormConfig.FromFile("Content/Interface/Menu/Host.xml");
                canvas.RemoveChild(frm_hostGame);
                frm_hostGame = new Form(formConfig, canvas);
                frm_hostGame.CentreControl();
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
            if (manager.Server.Running)
                return;
            if (tbUsername.Text.Equals(""))
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
                return;
            }
            manager.Server.StartServer(port, tbPassword.Text);
            
            bool connected = manager.Client.Connect(tbUsername.Text, address, port, tbPassword.Text);
            if (!connected)
            {
                manager.Client.Dissconnect();
                manager.Client.LobbyStateChanged += Client_LobbyStateChanged;
                manager.Server.StopServer();
                manager.Server = new Server.Server();
                return;
            }
            host = true;

            RemoveAllForms();
        }
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
        private void SetUpLobbyForm()
        {
            lock (_lock_guiSetUp)
            {
                AudioManager.Instance.PlayerState = MusicPlayerState.RepeatOne;
                manager.Client.LostConnection += Client_LostConnection;

                canvas.RemoveChild(frm_lobby);
                FormConfig formConfig = FormConfig.FromFile("Content/Interface/Menu/Lobby.xml");
                frm_lobby = new Form(formConfig, canvas);
                frm_lobby.CentreControl();

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

                Lobby_BtnEmpireSelect_MouseClick(lobby_btnEmpireSelect, new MouseEventArgs(false, false, false, Point.Zero, 0));
            }
        }
        private void Lobby_BtnBan_MouseClick(object sender, MouseEventArgs e)
        {
            BasicPlayer selectedPlayer = ((PlayerListItem)lobby_sbPlayers.Selected).Player;
            if (selectedPlayer.PlayerID != 0)
                manager.Server.BanPlayer(selectedPlayer.PlayerID);
        }
        private void Lobby_BtnKick_MouseClick(object sender, MouseEventArgs e)
        {
            BasicPlayer selectedPlayer = ((PlayerListItem)lobby_sbPlayers.Selected).Player;
            if (selectedPlayer.PlayerID != 0)
                manager.Server.KickPlayer(selectedPlayer.PlayerID);
        }
        private void Client_LobbyStateChanged(object sender, LobbyStateEventArgs e)
        {
            if (e.LobbyState == null)
                return;
            if (frm_lobby == null)
                SetUpLobbyForm();
            lobby_sbPlayers.Items = GetPlayers();
            BasicPlayer myPlayer = e.LobbyState.Players.Find(p => p.PlayerID == manager.Client.Player.InstanceID);
            BasicPlayer hostPlayer = e.LobbyState.Players.Find(p => p.PlayerID == 0);
            frm_lobby.Text = $"{hostPlayer.Name}'s Lobby".ToRichText();
            lobby_btnEmpireSelect.Text = $"Empire: {manager.Client.EmpireFactory.GetEmpire(myPlayer.EmpireID).Name}".ToRichText();
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
                PlayAnthem(manager.Client.EmpireFactory.GetEmpire(myPlayer.EmpireID).Name);
            lastEmpireID = myPlayer.EmpireID;
        }
        private void Client_LostConnection(object sender, EventArgs e)
        {
            RemoveAllForms();
            SetUpMainForm();
        }
        private void Lobby_BtnOtherOptions_MouseClick(object sender, MouseEventArgs e)
        {
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
            frm_lobby.RemoveChild(frm_lobby_configWindow);
            frm_lobby_configWindow = new Form(new Rectangle(frm_lobby.Bounds.Width - 235, 40, 230, 390), frm_lobby);
            frm_lobby_configWindow.CloseButtonEnabled = false;
            frm_lobby_configWindow.Draggable = false;
            frm_lobby_configWindow.DrawTitlebar = false;
            frm_lobby_configWindow.Text = "Select an empire".ToRichText();
            int yOffset = 35;
            ScrollBox sb = new ScrollBox(new Rectangle(5, yOffset + 5, frm_lobby_configWindow.Bounds.Width - 10, frm_lobby_configWindow.Bounds.Height - yOffset - 10), GetEmpires(), frm_lobby_configWindow);
            sb.SelectedIndex = manager.Client.LobbyState.Players.Find(p => p.PlayerID == manager.Client.Player.InstanceID).EmpireID;
            sb.SelectedChanged += (s, a) =>
            {
                manager.Client.Lobby_SelectNewEmpire(sb.SelectedIndex);
            };
        }
        private void Lobby_BtnStart_MouseClick(object sender, MouseEventArgs e)
        {
            manager.Server.StartGame();
        }
        private void Lobby_BtnBack_MouseClick(object sender, MouseEventArgs e)
        {
            manager.Client.Dissconnect();
            manager.Client = new Client();
            manager.Client.LobbyStateChanged += Client_LobbyStateChanged;
            if (host)
            {
                manager.Server.StopServer();
                manager.Server = new Server.Server();
            }
            RemoveAllForms();
            SetUpMainForm();
        }
        #endregion

        private List<IListItem> GetPlayers()
        {
            try
            {
                List<IListItem> items = new List<IListItem>();
                foreach (BasicPlayer player in manager.Client.LobbyState.Players)
                    items.Add(new PlayerListItem(player, manager.Client.EmpireFactory));
                return items;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private List<IListItem> GetEmpires()
        {
            if (manager.Client.EmpireFactory.Empires == null)
                return new List<IListItem>();
            List<IListItem> items = new List<IListItem>();
            foreach (Empire empire in manager.Client.EmpireFactory.Empires)
                items.Add(new EmpireListItem(empire));
            return items;
        }
        private List<IListItem> GetEnumStringListItems(Type e)
        {
            List<IListItem> items = new List<IListItem>();
            foreach (string s in Enum.GetNames(e))
                items.Add(new StringListItem(s.ToRichText()));
            return items;
        }

        private void PlayAnthem(string empireName)
        {
            try
            {
                SoundEffect anthem = empireAnthems[empireName];
                AudioManager.Instance.PlayMusic(anthem);
                AudioManager.Instance.PlayerState = MusicPlayerState.RepeatOne;
            }
            catch (Exception)
            {
                ConsoleManager.Instance.WriteLine($"Could not find an anthem for {empireName}", MsgType.Warning);
            }
        }

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

        public override void Update()
        {
            canvas.Update();
        }

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
