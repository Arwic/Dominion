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
        public const string CONFIG_RESOLUTION = "gfx_resolution";
        public const string CONFIG_VSYNC = "gfx_vsync";
        public const string CONFIG_DISPLAYMODE = "gfx_displaymode";
        public const string CONFIG_SOUNDVOLUME = "aud_sound";
        public const string CONFIG_MUSICVOLUME = "aud_music";
        public const string CONFIG_NET_SERVER_PORT = "net_server_port";
        public const string CONFIG_NET_SERVER_TIMEOUT = "net_server_timeout";
        public const string CONFIG_NET_CLIENT_PORT = "net_client_port";
        public const string CONFIG_NET_CLIENT_TIMEOUT = "net_client_timeout";
        public const string CONFIG_NET_CLIENT_ADDRESS = "net_client_address";
    }
}
