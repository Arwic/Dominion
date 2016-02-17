namespace ArwicInterfaceDesigner
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Button");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("CheckBox");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("ComboBox");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("ContextMenu");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Image");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Label");
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("ProgressBar");
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("ScrollBox");
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("SpinButton");
            System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("TextBox");
            System.Windows.Forms.TreeNode treeNode11 = new System.Windows.Forms.TreeNode("TextLog");
            System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("ToolTip");
            System.Windows.Forms.TreeNode treeNode13 = new System.Windows.Forms.TreeNode("Controls", new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4,
            treeNode5,
            treeNode6,
            treeNode7,
            treeNode8,
            treeNode9,
            treeNode10,
            treeNode11,
            treeNode12});
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.pb_drawSurface = new System.Windows.Forms.PictureBox();
            this.pg_properties = new System.Windows.Forms.PropertyGrid();
            this.gb_properties = new System.Windows.Forms.GroupBox();
            this.gb_toolbox = new System.Windows.Forms.GroupBox();
            this.btn_deleteSelected = new System.Windows.Forms.Button();
            this.btn_load = new System.Windows.Forms.Button();
            this.btn_save = new System.Windows.Forms.Button();
            this.btn_new = new System.Windows.Forms.Button();
            this.tv_toolBox = new System.Windows.Forms.TreeView();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.pb_drawSurface)).BeginInit();
            this.gb_properties.SuspendLayout();
            this.gb_toolbox.SuspendLayout();
            this.SuspendLayout();
            // 
            // pb_drawSurface
            // 
            this.pb_drawSurface.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pb_drawSurface.Location = new System.Drawing.Point(0, 0);
            this.pb_drawSurface.Name = "pb_drawSurface";
            this.pb_drawSurface.Size = new System.Drawing.Size(1249, 624);
            this.pb_drawSurface.TabIndex = 0;
            this.pb_drawSurface.TabStop = false;
            this.pb_drawSurface.SizeChanged += new System.EventHandler(this.pb_drawSurface_SizeChanged);
            this.pb_drawSurface.Click += new System.EventHandler(this.pb_drawSurface_Click);
            this.pb_drawSurface.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pb_drawSurface_MouseDown);
            this.pb_drawSurface.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pb_drawSurface_MouseMove);
            this.pb_drawSurface.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pb_drawSurface_MouseUp);
            // 
            // pg_properties
            // 
            this.pg_properties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pg_properties.Location = new System.Drawing.Point(3, 16);
            this.pg_properties.Name = "pg_properties";
            this.pg_properties.Size = new System.Drawing.Size(393, 605);
            this.pg_properties.TabIndex = 4;
            // 
            // gb_properties
            // 
            this.gb_properties.Controls.Add(this.pg_properties);
            this.gb_properties.Dock = System.Windows.Forms.DockStyle.Right;
            this.gb_properties.Location = new System.Drawing.Point(850, 0);
            this.gb_properties.Name = "gb_properties";
            this.gb_properties.Size = new System.Drawing.Size(399, 624);
            this.gb_properties.TabIndex = 5;
            this.gb_properties.TabStop = false;
            this.gb_properties.Text = "Properties";
            // 
            // gb_toolbox
            // 
            this.gb_toolbox.Controls.Add(this.btn_deleteSelected);
            this.gb_toolbox.Controls.Add(this.btn_load);
            this.gb_toolbox.Controls.Add(this.btn_save);
            this.gb_toolbox.Controls.Add(this.btn_new);
            this.gb_toolbox.Controls.Add(this.tv_toolBox);
            this.gb_toolbox.Dock = System.Windows.Forms.DockStyle.Right;
            this.gb_toolbox.Location = new System.Drawing.Point(700, 0);
            this.gb_toolbox.Name = "gb_toolbox";
            this.gb_toolbox.Size = new System.Drawing.Size(150, 624);
            this.gb_toolbox.TabIndex = 6;
            this.gb_toolbox.TabStop = false;
            this.gb_toolbox.Text = "Toolbox";
            // 
            // btn_deleteSelected
            // 
            this.btn_deleteSelected.Location = new System.Drawing.Point(7, 126);
            this.btn_deleteSelected.Name = "btn_deleteSelected";
            this.btn_deleteSelected.Size = new System.Drawing.Size(137, 23);
            this.btn_deleteSelected.TabIndex = 4;
            this.btn_deleteSelected.Text = "Delete Selected";
            this.btn_deleteSelected.UseVisualStyleBackColor = true;
            this.btn_deleteSelected.Click += new System.EventHandler(this.btn_deleteSelected_Click);
            // 
            // btn_load
            // 
            this.btn_load.Location = new System.Drawing.Point(7, 78);
            this.btn_load.Name = "btn_load";
            this.btn_load.Size = new System.Drawing.Size(137, 23);
            this.btn_load.TabIndex = 3;
            this.btn_load.Text = "Load";
            this.btn_load.UseVisualStyleBackColor = true;
            this.btn_load.Click += new System.EventHandler(this.btn_load_Click);
            // 
            // btn_save
            // 
            this.btn_save.Location = new System.Drawing.Point(7, 49);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(137, 23);
            this.btn_save.TabIndex = 2;
            this.btn_save.Text = "Save";
            this.btn_save.UseVisualStyleBackColor = true;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // btn_new
            // 
            this.btn_new.Location = new System.Drawing.Point(7, 20);
            this.btn_new.Name = "btn_new";
            this.btn_new.Size = new System.Drawing.Size(137, 23);
            this.btn_new.TabIndex = 1;
            this.btn_new.Text = "New";
            this.btn_new.UseVisualStyleBackColor = true;
            this.btn_new.Click += new System.EventHandler(this.btn_new_Click);
            // 
            // tv_toolBox
            // 
            this.tv_toolBox.Location = new System.Drawing.Point(7, 155);
            this.tv_toolBox.Name = "tv_toolBox";
            treeNode1.Name = "node_button";
            treeNode1.Text = "Button";
            treeNode2.Name = "node_checkBox";
            treeNode2.Text = "CheckBox";
            treeNode3.Name = "node_comboBox";
            treeNode3.Text = "ComboBox";
            treeNode4.Name = "node_contextMenu";
            treeNode4.Text = "ContextMenu";
            treeNode5.Name = "node_image";
            treeNode5.Text = "Image";
            treeNode6.Name = "node_label";
            treeNode6.Text = "Label";
            treeNode7.Name = "node_progressBar";
            treeNode7.Text = "ProgressBar";
            treeNode8.Name = "node_scrollBox";
            treeNode8.Text = "ScrollBox";
            treeNode9.Name = "node_spinButton";
            treeNode9.Text = "SpinButton";
            treeNode10.Name = "node_textBox";
            treeNode10.Text = "TextBox";
            treeNode11.Name = "node_textLog";
            treeNode11.Text = "TextLog";
            treeNode12.Name = "node_toolTip";
            treeNode12.Text = "ToolTip";
            treeNode13.Name = "node_controls";
            treeNode13.Text = "Controls";
            this.tv_toolBox.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode13});
            this.tv_toolBox.Size = new System.Drawing.Size(137, 441);
            this.tv_toolBox.TabIndex = 0;
            this.tv_toolBox.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tv_toolBox_NodeMouseClick);
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "xml";
            this.openFileDialog.FileName = "form.xml";
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.FileName = "form.xml";
            this.saveFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog_FileOk);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1249, 624);
            this.Controls.Add(this.gb_toolbox);
            this.Controls.Add(this.gb_properties);
            this.Controls.Add(this.pb_drawSurface);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "Arwic Interface Designer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_Close);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainForm_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.pb_drawSurface)).EndInit();
            this.gb_properties.ResumeLayout(false);
            this.gb_toolbox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pb_drawSurface;
        private System.Windows.Forms.PropertyGrid pg_properties;
        private System.Windows.Forms.GroupBox gb_properties;
        private System.Windows.Forms.GroupBox gb_toolbox;
        private System.Windows.Forms.TreeView tv_toolBox;
        private System.Windows.Forms.Button btn_load;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.Button btn_new;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Button btn_deleteSelected;
    }
}