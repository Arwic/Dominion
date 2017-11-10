// Dominion - Copyright (C) Timothy Ings
// SocialPolicy.cs
// This file defines classes that define a social policy

using ArwicEngine.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Xml.Serialization;

namespace Dominion.Common.Data
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    public class SocialPolicyTree
    {
        public class Editor : CollectionEditor
        {
            public Editor(Type type) : base(type) { }

            protected override string GetDisplayText(object value)
            {
                if (value is SocialPolicyTree)
                    return ((SocialPolicyTree)value).ID;
                return value.ToString();
            }
        }

        /// <summary>
        /// The ID of the social policy tree
        /// </summary>
        [Description("The ID of the social policy tree")]
        [DisplayName("ID"), Browsable(true), Category("General")]
        [XmlElement("ID")]
        public string ID { get; set; } = "POLICYTREE_NULL";

        /// <summary>
        /// The name of the social policy tree in a display ready format
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public string Name { get; set; } = "Null";

        /// <summary>
        /// The description of the social policy
        /// </summary>
        [Description("The description of the social policy tree")]
        [DisplayName("Description"), Browsable(true), Category("General")]
        [XmlElement("Description")]
        public string Description { get; set; } = "New Social Policy Tree";

        /// <summary>
        /// The index of the social policy tree in the list
        /// </summary>
        [Description("The x position of the social policy in its tree")]
        [DisplayName("List Index"), Browsable(true), Category("Graphics")]
        [XmlElement("ListIndex")]
        public int ListIndex { get; set; } = 0;

        /// <summary>
        /// The key to used to get the icon from the icon atlas
        /// </summary>
        [Description("The key to used to get the icon from the icon atlas")]
        [DisplayName("Icon Key"), Browsable(true), Category("Graphics")]
        [XmlElement("IconKey")]
        public string IconKey { get; set; } = "POLICYTREE_NULL";

        /// <summary>
        /// The icon atlas to source the social policy's icon from
        /// </summary>
        [Description("The icon atlas to source the social policy tree's icon from")]
        [DisplayName("Icon Atlas"), Browsable(true), Category("Graphics")]
        [XmlElement("IconAtlas")]
        public string IconAtlas { get; set; } = "Core:XML/AtlasDefinitions/SocialPolicyAtlasDefinition";

        public SocialPolicyTree() { }

        /// <summary>
        /// Converts a policy tree name to a presentable form
        /// I.e. converts "POLICYTREE_MY_NAME" to "My Name"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FormatName(string name)
        {
            string prefix = "POLICYTREE_";

            // check if the string is valid
            if (!name.Contains(prefix))
                return name;

            // strip "POLICYTREE_"
            // "POLICYTREE_MY_NAME" -> "MY_NAME"
            name = name.Remove(0, prefix.Length);

            // replace all "_"
            // "MY_NAME" -> "MY NAME"
            name = name.Replace('_', ' ');

            // convert to lower case
            // "MY NAME" -> "my name"
            name = name.ToLowerInvariant();

            // convert to title case
            // "my name" -> "My Name"
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            name = textInfo.ToTitleCase(name);

            return name;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    public class SocialPolicy
    {
        public class Editor : CollectionEditor
        {
            public Editor(Type type) : base(type) { }

            protected override string GetDisplayText(object value)
            {
                if (value is SocialPolicy)
                    return ((SocialPolicy)value).ID;
                return value.ToString();
            }
        }

        /// <summary>
        /// The ID of the social policy
        /// </summary>
        [Description("The ID of the social policy")]
        [DisplayName("ID"), Browsable(true), Category("General")]
        [XmlElement("ID")]
        public string ID { get; set; } = "POLICY_NULL";

        /// <summary>
        /// The name of the social policy in a display ready format
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public string Name { get; set; } = "Null";

        /// <summary>
        /// The Tree ID of the social policy
        /// </summary>
        [Description("The Tree ID of the social policy")]
        [DisplayName("TreeID"), Browsable(true), Category("General")]
        [XmlElement("TreeID")]
        public string TreeID { get; set; } = "POLICYTREE_NULL";

        /// <summary>
        /// The descriptions of the social policy
        /// </summary>
        [Description("The descriptions of the social policy")]
        [DisplayName("Description"), Browsable(true), Category("General")]
        [XmlElement("Description")]
        public string Description { get; set; } = "New Social Policy";

        /// <summary>
        /// The x position of the social policy in its tree
        /// </summary>
        [Description("The x position of the social policy in its tree")]
        [DisplayName("Grid X"), Browsable(true), Category("Graphics")]
        [XmlElement("GridX")]
        public int GridX { get; set; } = 0;

        /// <summary>
        /// The y position of the social policy in its tree
        /// </summary>
        [Description("The y position of the social policy in its tree")]
        [DisplayName("Grid Y"), Browsable(true), Category("Graphics")]
        [XmlElement("GridY")]
        public int GridY { get; set; } = 0;

        /// <summary>
        /// The key to used to get the icon from the icon atlas
        /// </summary>
        [Description("The key to used to get the icon from the icon atlas")]
        [DisplayName("Icon Key"), Browsable(true), Category("Graphics")]
        [XmlElement("IconKey")]
        public string IconKey { get; set; } = "POLICY_NULL";

        /// <summary>
        /// The icon atlas to source the social policy's icon from
        /// </summary>
        [Description("The icon atlas to source the social policy's icon from")]
        [DisplayName("Icon Atlas"), Browsable(true), Category("Graphics")]
        [XmlElement("IconAtlas")]
        public string IconAtlas { get; set; } = "Core:XML/AtlasDefinitions/SocialPolicyAtlasDefinition";

        /// <summary>
        /// A list of social policies that have to be unlocked in order to unlock this node
        /// </summary>
        [Description("A list of social policies that have to be unlocked in order to unlock this node")]
        [DisplayName("Prerequisite Policies"), Browsable(true), Category("Game")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ListConverter))]
        [XmlArray("Prerequisites"), XmlArrayItem(typeof(string), ElementName = "Prerequisite")]
        public List<string> Prerequisites { get; set; } = new List<string>();

        /// <summary>
        /// Indicates whether this policy is unlocked
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public bool Unlocked { get; set; } = false;

        public SocialPolicy() { }

        /// <summary>
        /// Converts a policy name to a presentable form
        /// I.e. converts "POLICY_MY_NAME" to "My Name"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FormatName(string name)
        {
            string prefix = "POLICY_";

            // check if the string is valid
            if (!name.Contains(prefix))
                return name;

            // strip "POLICY_"
            // "POLICY_MY_NAME" -> "MY_NAME"
            name = name.Remove(0, prefix.Length);

            // replace all "_"
            // "MY_NAME" -> "MY NAME"
            name = name.Replace('_', ' ');

            // convert to lower case
            // "MY NAME" -> "my name"
            name = name.ToLowerInvariant();

            // convert to title case
            // "my name" -> "My Name"
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            name = textInfo.ToTitleCase(name);

            return name;
        }
    }
}
