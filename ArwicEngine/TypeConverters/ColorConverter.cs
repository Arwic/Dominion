// Dominion - Copyright (C) Timothy Ings
// ColorConverter.cs
// This file contains classes that define a type converter for xna color

using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ArwicEngine.TypeConverters
{
    public class ColorEditor : UITypeEditor
    {
        private IWindowsFormsEditorService service;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            // This tells it to show the [...] button which is clickable firing off EditValue below.
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
                service = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            if (service != null)
            {
                // This is the code you want to run when the [...] is clicked and after it has been verified.

                // Get our currently selected color.
                Color color = (Color)value;

                // Create a new instance of the ColorDialog.
                ColorDialog selectionControl = new ColorDialog();
                selectionControl.FullOpen = true;

                // Set the selected color in the dialog.
                selectionControl.Color = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

                // Show the dialog.
                selectionControl.ShowDialog();

                // Return the newly selected color.
                value = new Color(selectionControl.Color.R, selectionControl.Color.G, selectionControl.Color.B, selectionControl.Color.A);
            }

            return value;
        }
    }

    public class ColorConverter : TypeConverter
    {
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
            if (value is string)
            {
                string[] v = ((string)value).Split(',');
                return new Color(int.Parse(v[0]), int.Parse(v[1]), int.Parse(v[2]), int.Parse(v[3]));
            }
            return base.ConvertFrom(context, culture, value);
        }
        // Overrides the ConvertTo method of TypeConverter.
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
                return $"{((Color)value).R}, {((Color)value).G}, {((Color)value).B}, {((Color)value).A}";
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
