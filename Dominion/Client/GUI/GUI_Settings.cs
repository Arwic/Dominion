using ArwicEngine.Audio;
using ArwicEngine.Core;
using ArwicEngine.Forms;
using ArwicEngine.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ArwicEngine.Constants;

namespace Dominion.Client.GUI
{
    public class GUI_Settings : IGUIElement
    {
        private class StringListItem : IListItem
        {
            public Button Button { get; set; }

            public RichText Text { get; set; }

            public ToolTip ToolTip { get; set; }

            public StringListItem(string str)
            {
                Text = str.ToRichText();
            }

            public void OnDraw(object sender, DrawEventArgs e)
            {
                
            }
        }

        private Canvas canvas;

        private FormConfig configGameSettings;
        private FormConfig configVideoSettings;
        private FormConfig configAudioSettings;

        private Form frmGameSettings;
        private Form frmVideoSettings;
        private Form frmAudioSettings;

        public event EventHandler FormClosed;

        protected virtual void OnFormClosed(EventArgs args)
        {
            if (FormClosed != null)
                FormClosed(this, args);
        }

        public GUI_Settings(Canvas canvas)
        {
            this.canvas = canvas;
            configGameSettings = FormConfig.FromStream(Engine.Instance.Content.GetAsset<Stream>("Core:XML/Interface/Menu/Settings_GameSettings"));
            configVideoSettings = FormConfig.FromStream(Engine.Instance.Content.GetAsset<Stream>("Core:XML/Interface/Menu/Settings_VideoSettings"));
            configAudioSettings = FormConfig.FromStream(Engine.Instance.Content.GetAsset<Stream>("Core:XML/Interface/Menu/Settings_AudioSettings"));
        }

        public void Show()
        {
            ShowGameSettings();
        }
        
        // hides all the "tabs"
        private void HideAllForms()
        {
            if (frmAudioSettings != null)
                frmAudioSettings.Visible = false;
            if (frmGameSettings != null)
                frmGameSettings.Visible = false;
            if (frmVideoSettings != null)
                frmVideoSettings.Visible = false;
        }

        private void SetCheckBoxValue(CheckBox cb, string varName, bool defaultVal)
        {
            try
            {
                cb.Value = Convert.ToBoolean(Convert.ToInt32(ConfigManager.Instance.GetVar(varName)));
            }
            catch (Exception)
            {
                cb.Value = defaultVal;
            }
        }

        private void ShowGameSettings()
        {
            HideAllForms();

            // form
            canvas.RemoveChild(frmGameSettings);
            frmGameSettings = new Form(configGameSettings, canvas);
            frmGameSettings.CentreControl();
            frmGameSettings.Visible = true;

            // tabs
            Button btnTabGameSettings = (Button)frmGameSettings.GetChildByName("btnTabGameSettings");
            btnTabGameSettings.MouseClick += (s, a) => ShowGameSettings();

            Button btnTabVideoSettings = (Button)frmGameSettings.GetChildByName("btnTabVideoSettings");
            btnTabVideoSettings.MouseClick += (s, a) => ShowVideoSettings();

            Button btnTabAudioSettings = (Button)frmGameSettings.GetChildByName("btnTabAudioSettings");
            btnTabAudioSettings.MouseClick += (s, a) => ShowAudioSettings();

            // settings
            CheckBox cbAutoWorkerDontReplaceImprovements = (CheckBox)frmGameSettings.GetChildByName("cbAutoWorkerDontReplaceImprovements");
            SetCheckBoxValue(cbAutoWorkerDontReplaceImprovements, CONFIG_GAME_AUTO_WORKER_DONT_REPLACE_IMPROVEMENTS, true);

            CheckBox cbAutoWorkerDontRemoveFeatures = (CheckBox)frmGameSettings.GetChildByName("cbAutoWorkerDontRemoveFeatures");
            SetCheckBoxValue(cbAutoWorkerDontRemoveFeatures, CONFIG_GAME_AUTO_WORKER_DONT_REMOVE_FEATURES, true);

            CheckBox cbShowRewardPopups = (CheckBox)frmGameSettings.GetChildByName("cbShowRewardPopups");
            SetCheckBoxValue(cbShowRewardPopups, CONFIG_GAME_SHOW_REWARD_POPUPS, true);

            CheckBox cbShowTileReccomendations = (CheckBox)frmGameSettings.GetChildByName("cbShowTileReccomendations");
            SetCheckBoxValue(cbShowTileReccomendations, CONFIG_GAME_SHOW_TILE_RECOMMENDATIONS, true);

            CheckBox cbAutoUnitCycle = (CheckBox)frmGameSettings.GetChildByName("cbAutoUnitCycle");
            SetCheckBoxValue(cbAutoUnitCycle, CONFIG_GAME_AUTO_UNIT_CYCLE, false);

            CheckBox cbSinglePlayerAutoEndTurn = (CheckBox)frmGameSettings.GetChildByName("cbSinglePlayerAutoEndTurn");
            SetCheckBoxValue(cbSinglePlayerAutoEndTurn, CONFIG_GAME_SP_AUTO_END_TURN, false);

            CheckBox cbMultiplayerAutoEndTurn = (CheckBox)frmGameSettings.GetChildByName("cbMultiplayerAutoEndTurn");
            SetCheckBoxValue(cbMultiplayerAutoEndTurn, CONFIG_GAME_MP_AUTO_END_TURN, false);

            CheckBox cbSinglePlayerQuickCombat = (CheckBox)frmGameSettings.GetChildByName("cbSinglePlayerQuickCombat");
            SetCheckBoxValue(cbSinglePlayerQuickCombat, CONFIG_GAME_SP_QUICK_COMBAT, false);

            CheckBox cbMultiplayerQuickCombat = (CheckBox)frmGameSettings.GetChildByName("cbMultiplayerQuickCombat");
            SetCheckBoxValue(cbMultiplayerQuickCombat, CONFIG_GAME_MP_QUICK_COMBAT, false);

            CheckBox cbSinglePlayerQuickMovement = (CheckBox)frmGameSettings.GetChildByName("cbSinglePlayerQuickMovement");
            SetCheckBoxValue(cbSinglePlayerQuickMovement, CONFIG_GAME_SP_QUICK_MOVEMENT, false);

            CheckBox cbMultiplayerQuickMovement = (CheckBox)frmGameSettings.GetChildByName("cbMultiplayerQuickMovement");
            SetCheckBoxValue(cbMultiplayerQuickMovement, CONFIG_GAME_MP_QUICK_MOVEMENT, false);

            CheckBox cbShowAllPolicyInfo = (CheckBox)frmGameSettings.GetChildByName("cbShowAllPolicyInfo");
            SetCheckBoxValue(cbShowAllPolicyInfo, CONFIG_GAME_SHOW_ALL_POLICY_INFO, false);

            CheckBox cbSinglePlayerScoreList = (CheckBox)frmGameSettings.GetChildByName("cbSinglePlayerScoreList");
            SetCheckBoxValue(cbSinglePlayerScoreList, CONFIG_GAME_SP_SCORE_LIST, true);

            CheckBox cbMultiplayerScoreList = (CheckBox)frmGameSettings.GetChildByName("cbMultiplayerScoreList");
            SetCheckBoxValue(cbMultiplayerScoreList, CONFIG_GAME_MP_SCORE_LIST, true);

            CheckBox cbAutosave = (CheckBox)frmGameSettings.GetChildByName("cbAutosave");
            SetCheckBoxValue(cbAutosave, CONFIG_GAME_AUTOSAVE_ENABLED, true);

            TextBox tbTurnsBetweenAutosaves = (TextBox)frmGameSettings.GetChildByName("tbTurnsBetweenAutosaves");
            tbTurnsBetweenAutosaves.Text = ConfigManager.Instance.GetVar(CONFIG_GAME_TURNS_BETWEEN_AUTOSAVES);

            TextBox tbAutosavesToKeep = (TextBox)frmGameSettings.GetChildByName("tbAutosavesToKeep");
            tbAutosavesToKeep.Text = ConfigManager.Instance.GetVar(CONFIG_GAME_AUTOSAVES_TO_KEEP);

            // Apply - Defaults - Back
            Button btnApply = (Button)frmGameSettings.GetChildByName("btnApply");
            btnApply.MouseClick += (s, a) =>
            {
                ConfigManager.Instance.SetVar(CONFIG_GAME_AUTO_WORKER_DONT_REPLACE_IMPROVEMENTS, Convert.ToInt32(cbAutoWorkerDontReplaceImprovements.Value).ToString());
                ConfigManager.Instance.SetVar(CONFIG_GAME_AUTO_WORKER_DONT_REMOVE_FEATURES, Convert.ToInt32(cbAutoWorkerDontRemoveFeatures.Value).ToString());
                ConfigManager.Instance.SetVar(CONFIG_GAME_SHOW_REWARD_POPUPS, Convert.ToInt32(cbShowRewardPopups.Value).ToString());
                ConfigManager.Instance.SetVar(CONFIG_GAME_SHOW_TILE_RECOMMENDATIONS, Convert.ToInt32(cbShowTileReccomendations.Value).ToString());
                ConfigManager.Instance.SetVar(CONFIG_GAME_AUTO_UNIT_CYCLE, Convert.ToInt32(cbAutoUnitCycle.Value).ToString());
                ConfigManager.Instance.SetVar(CONFIG_GAME_SP_AUTO_END_TURN, Convert.ToInt32(cbSinglePlayerAutoEndTurn.Value).ToString());
                ConfigManager.Instance.SetVar(CONFIG_GAME_MP_AUTO_END_TURN, Convert.ToInt32(cbMultiplayerAutoEndTurn.Value).ToString());
                ConfigManager.Instance.SetVar(CONFIG_GAME_SP_QUICK_COMBAT, Convert.ToInt32(cbSinglePlayerQuickCombat.Value).ToString());
                ConfigManager.Instance.SetVar(CONFIG_GAME_MP_QUICK_COMBAT, Convert.ToInt32(cbMultiplayerQuickCombat.Value).ToString());
                ConfigManager.Instance.SetVar(CONFIG_GAME_SP_QUICK_MOVEMENT, Convert.ToInt32(cbSinglePlayerQuickMovement.Value).ToString());
                ConfigManager.Instance.SetVar(CONFIG_GAME_MP_QUICK_MOVEMENT, Convert.ToInt32(cbMultiplayerQuickMovement.Value).ToString());
                ConfigManager.Instance.SetVar(CONFIG_GAME_SHOW_ALL_POLICY_INFO, Convert.ToInt32(cbShowAllPolicyInfo.Value).ToString());
                ConfigManager.Instance.SetVar(CONFIG_GAME_SP_SCORE_LIST, Convert.ToInt32(cbSinglePlayerScoreList.Value).ToString());
                ConfigManager.Instance.SetVar(CONFIG_GAME_MP_SCORE_LIST, Convert.ToInt32(cbMultiplayerScoreList.Value).ToString());
                ConfigManager.Instance.SetVar(CONFIG_GAME_AUTOSAVE_ENABLED, Convert.ToInt32(cbAutosave.Value).ToString());
                try
                {
                    ConfigManager.Instance.SetVar(CONFIG_GAME_TURNS_BETWEEN_AUTOSAVES, Convert.ToInt32(tbTurnsBetweenAutosaves.Text).ToString());
                    ConfigManager.Instance.SetVar(CONFIG_GAME_AUTOSAVES_TO_KEEP, Convert.ToInt32(tbAutosavesToKeep.Text).ToString());
                }
                catch (Exception)
                {
                    // TODO use an in engine system when one is implemented
                    System.Windows.Forms.MessageBox.Show("Value must be an integer", "Error applying settings", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }

                ConfigManager.Instance.Write(CONFIG_PATH);
            };

            Button btnDefaults = (Button)frmGameSettings.GetChildByName("btnDefaults");
            btnDefaults.MouseClick += (s, a) => SetDefaultGameSettings();

            Button btnBack = (Button)frmGameSettings.GetChildByName("btnBack");
            btnBack.MouseClick += (s, a) => Hide();

        }

        private void ShowVideoSettings()
        {
            HideAllForms();

            // form
            canvas.RemoveChild(frmVideoSettings);
            frmVideoSettings = new Form(configVideoSettings, canvas);
            frmVideoSettings.CentreControl();
            frmVideoSettings.Visible = true;

            // tabs
            Button btnTabGameSettings = (Button)frmVideoSettings.GetChildByName("btnTabGameSettings");
            btnTabGameSettings.MouseClick += (s, a) => ShowGameSettings();

            Button btnTabVideoSettings = (Button)frmVideoSettings.GetChildByName("btnTabVideoSettings");
            btnTabVideoSettings.MouseClick += (s, a) => ShowVideoSettings();

            Button btnTabAudioSettings = (Button)frmVideoSettings.GetChildByName("btnTabAudioSettings");
            btnTabAudioSettings.MouseClick += (s, a) => ShowAudioSettings();

            // settings
            //ComboBox cbResolution = (ComboBox)frmVideoSettings.GetChildByName("cbResolution"); // combo boxes are borken in the interface editor
            List<IListItem> resItems = new List<IListItem>();
            string[] supportedRes =
            {
                "3840x2160",
                "2560x1440",
                "1920x1080",
                "1600x900",
                "1366x768",
                "1280x720",
                "640x360"
            };
            foreach (string res in supportedRes)
            {
                StringListItem item = new StringListItem(res);
                resItems.Add(item);
            }
            ComboBox cbResolution = new ComboBox(new Rectangle(126, 95, 230, 30), resItems, frmVideoSettings);

            string resString = ConfigManager.Instance.GetVar(CONFIG_GFX_RESOLUTION);
            bool foundRes = false;
            for (int i = 0; i < cbResolution.Items.Length; i++)
            {
                if (cbResolution.Items[i].Text.Text == resString)
                {
                    cbResolution.SelectedIndex = i;
                    foundRes = true;
                    break;
                }
            }

            if (!foundRes) // if we didnt find the res from the presets, add the custom on to the combo box and select it
            {
                StringListItem item = new StringListItem(resString);
                cbResolution.AddItem(item);
                cbResolution.SelectedIndex = cbResolution.Items.Length;
            }

            // Apply - Defaults - Back
            Button btnApply = (Button)frmVideoSettings.GetChildByName("btnApply");
            btnApply.MouseClick += (s, a) =>
            {
                ConfigManager.Instance.SetVar(CONFIG_GFX_RESOLUTION, ((StringListItem)cbResolution.Selected).Text.Text);

                GraphicsManager.Instance.Apply();
                ConfigManager.Instance.Write(CONFIG_PATH);
            };

            Button btnDefaults = (Button)frmVideoSettings.GetChildByName("btnDefaults");
            btnDefaults.MouseClick += (s, a) => SetDefaultVideoSettings();

            Button btnBack = (Button)frmVideoSettings.GetChildByName("btnBack");
            btnBack.MouseClick += (s, a) => Hide();
        }

        private void ShowAudioSettings()
        {
            HideAllForms();

            // form
            canvas.RemoveChild(frmAudioSettings);
            frmAudioSettings = new Form(configAudioSettings, canvas);
            frmAudioSettings.CentreControl();
            frmAudioSettings.Visible = true;

            // tabs
            Button btnTabGameSettings = (Button)frmAudioSettings.GetChildByName("btnTabGameSettings");
            btnTabGameSettings.MouseClick += (s, a) => ShowGameSettings();

            Button btnTabVideoSettings = (Button)frmAudioSettings.GetChildByName("btnTabVideoSettings");
            btnTabVideoSettings.MouseClick += (s, a) => ShowVideoSettings();

            Button btnTabAudioSettings = (Button)frmAudioSettings.GetChildByName("btnTabAudioSettings");
            btnTabAudioSettings.MouseClick += (s, a) => ShowAudioSettings();

            // settings
            CheckBox cbMusic = (CheckBox)frmAudioSettings.GetChildByName("cbMusic");
            SetCheckBoxValue(cbMusic, CONFIG_AUD_MUSIC_ENABLED, true);

            TextBox tbMusicVolume = (TextBox)frmAudioSettings.GetChildByName("tbMusicVolume");
            try { tbMusicVolume.Text = (Convert.ToInt32(ConfigManager.Instance.GetVar(CONFIG_AUD_MUSIC)) * 100).ToString(); }
            catch (Exception) { tbMusicVolume.Text = "100"; }

            CheckBox cbSoundEffects = (CheckBox)frmAudioSettings.GetChildByName("cbSoundEffects");
            SetCheckBoxValue(cbSoundEffects, CONFIG_AUD_SFX_ENABLED, true);

            TextBox tbSoundEffectsVolume = (TextBox)frmAudioSettings.GetChildByName("tbSoundEffectsVolume");
            try { tbSoundEffectsVolume.Text = (Convert.ToInt32(ConfigManager.Instance.GetVar(CONFIG_AUD_SFX)) * 100).ToString(); }
            catch (Exception) { tbSoundEffectsVolume.Text = "100"; }

            // Apply - Defaults - Back
            Button btnApply = (Button)frmAudioSettings.GetChildByName("btnApply");
            btnApply.MouseClick += (s, a) =>
            {
                ConfigManager.Instance.SetVar(CONFIG_AUD_MUSIC_ENABLED, Convert.ToInt32(cbMusic.Value).ToString());
                ConfigManager.Instance.SetVar(CONFIG_AUD_SFX_ENABLED, Convert.ToInt32(cbSoundEffects.Value).ToString());
                try
                {
                    ConfigManager.Instance.SetVar(CONFIG_AUD_MUSIC, (Convert.ToSingle(tbMusicVolume.Text) / 100).ToString());
                    ConfigManager.Instance.SetVar(CONFIG_AUD_SFX, (Convert.ToSingle(tbSoundEffectsVolume.Text) / 100).ToString());
                }
                catch (Exception)
                {
                    // TODO use an in engine system when one is implemented
                    System.Windows.Forms.MessageBox.Show("Value must be an integer", "Error applying settings", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }

                AudioManager.Instance.Apply();
                ConfigManager.Instance.Write(CONFIG_PATH);
            };

            Button btnDefaults = (Button)frmAudioSettings.GetChildByName("btnDefaults");
            btnDefaults.MouseClick += (s, a) => SetDefaultAduioSettings();

            Button btnBack = (Button)frmAudioSettings.GetChildByName("btnBack");
            btnBack.MouseClick += (s, a) => Hide();
        }

        private void SetDefaultGameSettings()
        {
            ConfigManager.Instance.SetVar(CONFIG_GAME_AUTO_WORKER_DONT_REPLACE_IMPROVEMENTS, "0");
            ConfigManager.Instance.SetVar(CONFIG_GAME_AUTO_WORKER_DONT_REMOVE_FEATURES, "0");
            ConfigManager.Instance.SetVar(CONFIG_GAME_SHOW_REWARD_POPUPS, "1");
            ConfigManager.Instance.SetVar(CONFIG_GAME_SHOW_TILE_RECOMMENDATIONS, "1");
            ConfigManager.Instance.SetVar(CONFIG_GAME_AUTO_UNIT_CYCLE, "0");
            ConfigManager.Instance.SetVar(CONFIG_GAME_SP_AUTO_END_TURN, "0");
            ConfigManager.Instance.SetVar(CONFIG_GAME_MP_AUTO_END_TURN, "0");
            ConfigManager.Instance.SetVar(CONFIG_GAME_SP_QUICK_COMBAT, "0");
            ConfigManager.Instance.SetVar(CONFIG_GAME_MP_QUICK_COMBAT, "0");
            ConfigManager.Instance.SetVar(CONFIG_GAME_SP_QUICK_MOVEMENT, "0");
            ConfigManager.Instance.SetVar(CONFIG_GAME_MP_QUICK_MOVEMENT, "0");
            ConfigManager.Instance.SetVar(CONFIG_GAME_SHOW_ALL_POLICY_INFO, "0");
            ConfigManager.Instance.SetVar(CONFIG_GAME_SP_SCORE_LIST, "1");
            ConfigManager.Instance.SetVar(CONFIG_GAME_MP_SCORE_LIST, "1");
            ConfigManager.Instance.SetVar(CONFIG_GAME_AUTOSAVE_ENABLED, "1");
            ConfigManager.Instance.SetVar(CONFIG_GAME_TURNS_BETWEEN_AUTOSAVES, "10");
            ConfigManager.Instance.SetVar(CONFIG_GAME_AUTOSAVES_TO_KEEP, "10");

            ShowGameSettings();
        }

        private void SetDefaultVideoSettings()
        {
            ConfigManager.Instance.SetVar(CONFIG_GFX_RESOLUTION, "1920x1080");
            ConfigManager.Instance.SetVar(CONFIG_GFX_VSYNC, "0");
            ConfigManager.Instance.SetVar(CONFIG_GFX_DISPLAY_MODE, "0");

            GraphicsManager.Instance.Apply();
            ShowVideoSettings();
        }

        private void SetDefaultAduioSettings()
        {
            ConfigManager.Instance.SetVar(CONFIG_AUD_MUSIC_ENABLED, "1");
            ConfigManager.Instance.SetVar(CONFIG_AUD_MUSIC, "1");
            ConfigManager.Instance.SetVar(CONFIG_AUD_SFX_ENABLED, "1");
            ConfigManager.Instance.SetVar(CONFIG_AUD_SFX, "1");

            AudioManager.Instance.Apply();
            ShowAudioSettings();
        }

        public void Hide()
        {
            HideAllForms();

            OnFormClosed(EventArgs.Empty);
        }
    }
}
