// Dominion - Copyright (C) Timothy Ings
// SocialPolicyTree.cs
// This file defines classes that define a social policy tree

using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

namespace Dominion.Common.Data
{
    public class SocialPolicyTree
    {
        /// <summary>
        /// The ID of the social policy tree
        /// </summary>
        [Description("The ID of the social policy tree")]
        [DisplayName("ID"), Browsable(true), Category("General")]
        [XmlElement("ID")]
        public string ID { get; set; } = "POLICYTREE_NULL";

        /// <summary>
        /// The name of the social policy in a display ready format
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public string Name { get; set; } = "Null";

        /// <summary>
        /// The key to used to get the background from the background atlas
        /// </summary>
        [Description("The key to used to get the background from the background atlas")]
        [DisplayName("Background Key"), Browsable(true), Category("Graphics")]
        [XmlElement("BackgroundKey")]
        public string BackgroundKey { get; set; } = "POLICYTREE_NULL";

        /// <summary>
        /// The atlas to source the policy tree's background from
        /// </summary>
        [Description("The atlas to source the policy tree's background from")]
        [DisplayName("Background Atlas"), Browsable(true), Category("Graphics")]
        [XmlElement("BackgroundAtlas")]
        public string BackgroundAtlas { get; set; } = "Core:XML/AtlasDefinitions/SocialPolicyTreeAtlasDefinition";

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
}
