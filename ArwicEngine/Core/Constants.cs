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
        public const int BUTTON_WIDTH = 230;
        public const int BUTTON_HEIGHT = 30;
        public const int ICON_DIM = 30;
        public const int CHECKBOX_DIM = 30;
        public const int FORM_CLOSEBUTTON_DIM = 30;
        public const int FORM_CLOSEBUTTON_PADDING = 5;

        // cursors
        public const string CURSOR_BUSY_PATH = "Core:Cursors/busy";
        public const string CURSOR_HELP_PATH = "Core:Cursors/help";
        public const string CURSOR_LINK_PATH = "Core:Cursors/link";
        public const string CURSOR_NORMAL_PATH = "Core:Cursors/normal";
        public const string CURSOR_TEXT_PATH = "Core:Cursors/text";
        public const string CURSOR_UNAVAILABLE_PATH = "Core:Cursors/unavailable";
        public const string CURSOR_WORKING_PATH = "Core:Cursors/working";

        // fonts
        public const string FONT_CONSOLAS_PATH = "Core:Fonts/Consolas";
        public const string FONT_ARIAL_PATH = "Core:Fonts/Arial";
        public const string FONT_SYMBOL_PATH = "Core:Fonts/ArwicSymbol";

        // resources
        public const string CONTROL_BUTTON = "Core:Textures/Interface/Controls/Button";
        public const string CONTROL_FORM_CLOSE = "Core:Textures/Interface/Controls/Form_Close";
        public const string CONTROL_CHECKBOX_TRUE = "Core:Textures/Interface/Controls/CheckBox_True";
        public const string CONTROL_CHECKBOX_FALSE = "Core:Textures/Interface/Controls/CheckBox_False";
        public const string CONTROL_COMBOBOX_BUTTON = "Core:Textures/Interface/Controls/ComboBox_Button";
        public const string CONTROL_FORM_BACK = "Core:Textures/Interface/Controls/Form_Back";
        public const string CONTROL_PROGRESSBAR_BACK = "Core:Textures/Interface/Controls/ProgressBar_back";
        public const string CONTROL_PROGRESSBAR_FILL = "Core:Textures/Interface/Controls/Progressbar_Fill";
        public const string CONTROL_SCROLLBOX_BACK = "Core:Textures/Interface/Controls/ScrollBox_Back";
        public const string CONTROL_SCROLLBOX_BUTTON = "Core:Textures/Interface/Controls/ScrollBox_Button";
        public const string CONTROL_SCROLLBOX_SELECTED = "Core:Textures/Interface/Controls/ScrollBox_Selected";
        public const string CONTROL_SCROLLBOX_SCRUBBER = "Core:Textures/Interface/Controls/ScrollBox_Scrubber";
        public const string CONTROL_TEXTBOX = "Core:Textures/Interface/Controls/TextBox";
        
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
