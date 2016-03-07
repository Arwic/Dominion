// Dominion - Copyright (C) Timothy Ings
// Constants.cs
// This file defines constants that are used throughout the engine
// The functionality of this file probabbly could be implemented better somewhere else

namespace ArwicEngine
{
    public static class Constants
    {
        // engine
        public const int EXIT_FAILURE = 1;
        public const int EXIT_SUCCESS = 0;
        public const string CONFIG_PATH = "Config.cfg";
        public const string ENGINE_NAME = "ArwicEngine";

        // graphics
        public const int PROP_BUTTON_WIDTH = 230;
        public const int PROP_BUTTON_HEIGHT = 30;
        public const int PROP_ICON_DIM = 30;
        public const int PROP_CHECKBOX_DIM = 30;
        public const int PROP_FORM_CLOSE_BUTTON_DIM = 30;
        public const int PROP_FORM_CLOSE_BUTTON_PADDING = 5;

        // cursors
        public const string ASSET_CURSOR_BUSY = "Core:Cursors/busy";
        public const string ASSET_CURSOR_HELP = "Core:Cursors/help";
        public const string ASSET_CURSOR_LINK = "Core:Cursors/link";
        public const string ASSET_CURSOR_NORMAL = "Core:Cursors/normal";
        public const string ASSET_CURSOR_TEXT = "Core:Cursors/text";
        public const string ASSET_CURSOR_UNAVAILABLE = "Core:Cursors/unavailable";
        public const string ASSET_CURSOR_WORKING = "Core:Cursors/working";

        // fonts
        public const string ASSET_FONT_CONSOLE = "Core:Fonts/Consolas";
        public const string ASSET_FONT_STANDARD = "Core:Fonts/Arial";
        public const string ASSET_FONT_SYMBOL = "Core:Fonts/ArwicSymbol";

        // resources
        public const string ASSET_CONTROL_BUTTON = "Core:Textures/Interface/Controls/Button";
        public const string ASSET_CONTROL_FORM_CLOSE = "Core:Textures/Interface/Controls/Form_Close";
        public const string ASSET_CONTROL_CHECKBOX_TRUE = "Core:Textures/Interface/Controls/CheckBox_True";
        public const string ASSET_CONTROL_CHECKBOX_FALSE = "Core:Textures/Interface/Controls/CheckBox_False";
        public const string ASSET_CONTROL_COMBOBOX_BUTTON = "Core:Textures/Interface/Controls/ComboBox_Button";
        public const string ASSET_CONTROL_FORM_BACK = "Core:Textures/Interface/Controls/Form_Back";
        public const string ASSET_CONTROL_PROGRESSBAR_BACK = "Core:Textures/Interface/Controls/ProgressBar_back";
        public const string ASSET_CONTROL_PROGRESSBAR_FILL = "Core:Textures/Interface/Controls/Progressbar_Fill";
        public const string ASSET_CONTROL_SCROLLBOX_BACK = "Core:Textures/Interface/Controls/ScrollBox_Back";
        public const string ASSET_CONTROL_SCROLLBOX_BUTTON = "Core:Textures/Interface/Controls/ScrollBox_Button";
        public const string ASSET_CONTROL_SCROLLBOX_SELECTED = "Core:Textures/Interface/Controls/ScrollBox_Selected";
        public const string ASSET_CONTROL_SCROLLBOX_SCRUBBER = "Core:Textures/Interface/Controls/ScrollBox_Scrubber";
        public const string ASSET_CONTROL_TEXTBOX = "Core:Textures/Interface/Controls/TextBox";

        // config vars
        public const string CONFIG_GFX_RESOLUTION = "gfx_resolution";
        public const string CONFIG_GFX_VSYNC = "gfx_vsync";
        public const string CONFIG_GFX_DISPLAY_MODE = "gfx_display_mode";
        public const string CONFIG_AUD_MUSIC_ENABLED = "aud_music_enabled";
        public const string CONFIG_AUD_MUSIC = "aud_music";
        public const string CONFIG_AUD_SFX_ENABLED = "aud_sfx_enabled";
        public const string CONFIG_AUD_SFX = "aud_sfx";
        public const string CONFIG_NET_SERVER_PORT = "net_server_port";
        public const string CONFIG_NET_SERVER_TIMEOUT = "net_server_timeout";
        public const string CONFIG_NET_CLIENT_PORT = "net_client_port";
        public const string CONFIG_NET_CLIENT_TIMEOUT = "net_client_timeout";
        public const string CONFIG_NET_CLIENT_ADDRESS = "net_client_address";
        public const string CONFIG_GAME_AUTO_WORKER_DONT_REPLACE_IMPROVEMENTS = "game_auto_worker_dont_replace_improvements";
        public const string CONFIG_GAME_AUTO_WORKER_DONT_REMOVE_FEATURES = "game_auto_worker_dont_remove_features";
        public const string CONFIG_GAME_SHOW_REWARD_POPUPS = "game_show_reward_popups";
        public const string CONFIG_GAME_SHOW_TILE_RECOMMENDATIONS = "game_show_tile_recommendations";
        public const string CONFIG_GAME_AUTO_UNIT_CYCLE = "game_auto_unit_cycle";
        public const string CONFIG_GAME_SP_AUTO_END_TURN = "game_sp_auto_end_turn";
        public const string CONFIG_GAME_MP_AUTO_END_TURN = "game_mp_auto_end_turn";
        public const string CONFIG_GAME_SP_QUICK_COMBAT = "game_sp_quick_combat";
        public const string CONFIG_GAME_MP_QUICK_COMBAT = "game_mp_quick_combat";
        public const string CONFIG_GAME_SP_QUICK_MOVEMENT = "game_sp_quick_movement";
        public const string CONFIG_GAME_MP_QUICK_MOVEMENT = "game_mp_quick_movement";
        public const string CONFIG_GAME_SHOW_ALL_POLICY_INFO = "game_show_all_policy_info";
        public const string CONFIG_GAME_SP_SCORE_LIST = "game_sp_score_list";
        public const string CONFIG_GAME_MP_SCORE_LIST = "game_mp_score_list";
        public const string CONFIG_GAME_AUTOSAVE_ENABLED = "game_autosave_enabled";
        public const string CONFIG_GAME_TURNS_BETWEEN_AUTOSAVES = "game_turns_between_auotsaves";
        public const string CONFIG_GAME_AUTOSAVES_TO_KEEP = "game_autosaves_to_keep";
    }
}
