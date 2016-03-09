using ArwicEngine.Core;
using Dominion.Common.Entities;
using Dominion.Common.Managers;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace ArwicXmlEditor
{
    public partial class Form1 : Form
    {
        private object xmlObject;
        private string version = Engine.RetrieveLinkerTimestamp().ToString("dd MMM yyyy HH:mm");

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Text = $"Arwic Xml Editor ({version})";

            cb_xmlType.Items.Add(typeof(EmpireDataPack));
            cb_xmlType.Items.Add(typeof(UnitDataPack));
            cb_xmlType.Items.Add(typeof(BuildingDataPack));
            cb_xmlType.Items.Add(typeof(TechnologyDataPack));
            cb_xmlType.Items.Add(typeof(SocialPolicyDataPack));
            cb_xmlType.SelectedIndex = 0;
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void btn_open_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void XmlSerialize(string file, object obj, Type t)
        {
            XmlSerializer xmls = new XmlSerializer(t);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            using (FileStream fs = new FileStream(file, FileMode.Create))
            using (XmlWriter writer = XmlWriter.Create(fs, settings))
            {
                writer.WriteStartDocument();
                writer.WriteComment($"Generated with ArwicXmlEditor ({version}) by Tim Ings");
                xmls.Serialize(writer, obj);
                writer.WriteEndDocument();
            }
        }

        private object XmlDeserialize(string file, Type t)
        {
            XmlSerializer xmls = new XmlSerializer(t);
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return xmls.Deserialize(fs);
            }
        }

        private Type GetXmlType()
        {
            return (Type)cb_xmlType.SelectedItem;
        }

        private void SelectObject()
        {
            pg_xmlView.SelectedObject = xmlObject;
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                XmlSerialize(saveFileDialog1.FileName, xmlObject, GetXmlType());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to serialize that file, check that you have selected the correct xml type.\nException: {ex.Message}");
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                xmlObject = XmlDeserialize(openFileDialog1.FileName, GetXmlType());
                SelectObject();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to deserialise that file, check that you have selected the correct xml type.\nException: {ex.Message}");
            }
        }

        private void btn_new_Click(object sender, EventArgs e)
        {
            xmlObject = Activator.CreateInstance(GetXmlType());
            SelectObject();
        }
    }
}
