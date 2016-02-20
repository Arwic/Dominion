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
        public const string CURSOR_BUSY_PATH = "Content/Cursors/busy.ani";
        public const string CURSOR_HELP_PATH = "Content/Cursors/help.cur";
        public const string CURSOR_LINK_PATH = "Content/Cursors/link.cur";
        public const string CURSOR_NORMAL_PATH = "Content/Cursors/normal.cur";
        public const string CURSOR_TEXT_PATH = "Content/Cursors/text.cur";
        public const string CURSOR_UNAVAILABLE_PATH = "Content/Cursors/unavailable.cur";
        public const string CURSOR_WORKING_PATH = "Content/Cursors/working.ani";

        // fonts
        public const string FONT_CONSOLAS_PATH = "Fonts/Consolas";
        public const string FONT_ARIAL_PATH = "Fonts/Arial";

        // resources
        public const string DEFAULT_TEXTURE_PATH = "Graphics/Util/Default";
        public const string DEFAULT_MODEL_PATH = "Graphics/Util/DefaultModel";
        public const string CONTROL_BUTTON = "Graphics/Interface/Controls/Button";
        public const string CONTROL_FORM_CLOSE = "Graphics/Interface/Controls/Form_Close";
        public const string CONTROL_CHECKBOX_TRUE = "Graphics/Interface/Controls/CheckBox_True";
        public const string CONTROL_CHECKBOX_FALSE = "Graphics/Interface/Controls/CheckBox_False";
        public const string CONTROL_COMBOBOX_BUTTON = "Graphics/Interface/Controls/ComboBox_Button";
        public const string CONTROL_FORM_BACK = "Graphics/Interface/Controls/Form_Back";
        public const string CONTROL_PROGRESSBAR_BACK = "Graphics/Interface/Controls/ProgressBar_back";
        public const string CONTROL_PROGRESSBAR_FILL = "Graphics/Interface/Controls/Progressbar_Fill";
        public const string CONTROL_SCROLLBOX_BACK = "Graphics/Interface/Controls/ScrollBox_Back";
        public const string CONTROL_SCROLLBOX_BUTTON = "Graphics/Interface/Controls/ScrollBox_Button";
        public const string CONTROL_SCROLLBOX_SELECTED = "Graphics/Interface/Controls/ScrollBox_Selected";
        public const string CONTROL_SCROLLBOX_SCRUBBER = "Graphics/Interface/Controls/ScrollBox_Scrubber";
        public const string CONTROL_TEXTBOX = "Graphics/Interface/Controls/TextBox";
        public const string DATA_BUILFING_LIST = "Content/Data/BuildingList.xml";
        public const string DATA_UNIT_LIST = "Content/Data/UnitList.xml";

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
