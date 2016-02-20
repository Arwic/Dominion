// Dominion - Copyright (C) Timothy Ings
// SpriteConverter.cs
// This file contains classes that define a type converter for sprites

using ArwicEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.ComponentModel;
using System.Globalization;

namespace ArwicEngine.TypeConverters
{
    public class SpriteConverter : TypeConverter
    {
        public static ContentManager Content { get; set; }

        // Overrides the CanConvertFrom method of TypeConverter.
        // The ITypeDescriptorContext interface provides the context for the
        // conversion. Typically, this interface is used at design time to 
        // provide information about the design-time container.
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }
        // Overrides the ConvertFrom method of TypeConverter.
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (Content == null)
                throw new InvalidOperationException("SpriteConverter.Content must be assigned to use this function");
            if (value is string)
            {
                return new Sprite(Content, (string)value);
            }
            return base.ConvertFrom(context, culture, value);
        }
        // Overrides the ConvertTo method of TypeConverter.
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
                return ((Sprite)value).Path;
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
