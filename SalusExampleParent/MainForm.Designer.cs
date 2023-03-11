namespace SalusExampleParent
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
            Grid = new DataGridView();
            AddButton = new Button();
            RefreshButton = new Button();
            Id = new DataGridViewTextBoxColumn();
            Data1 = new DataGridViewTextBoxColumn();
            Data2 = new DataGridViewTextBoxColumn();
            Edit = new DataGridViewButtonColumn();
            Delete = new DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)Grid).BeginInit();
            SuspendLayout();
            // 
            // Grid
            // 
            Grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Grid.Columns.AddRange(new DataGridViewColumn[] { Id, Data1, Data2, Edit, Delete });
            Grid.Location = new Point(12, 12);
            Grid.Name = "Grid";
            Grid.RowTemplate.Height = 25;
            Grid.Size = new Size(776, 392);
            Grid.TabIndex = 0;
            Grid.CellContentClick += Grid_CellContentClick;
            // 
            // AddButton
            // 
            AddButton.Location = new Point(713, 410);
            AddButton.Name = "AddButton";
            AddButton.Size = new Size(75, 28);
            AddButton.TabIndex = 1;
            AddButton.Text = "Add";
            AddButton.UseVisualStyleBackColor = true;
            AddButton.Click += AddButton_Click;
            // 
            // RefreshButton
            // 
            RefreshButton.Location = new Point(632, 410);
            RefreshButton.Name = "RefreshButton";
            RefreshButton.Size = new Size(75, 28);
            RefreshButton.TabIndex = 2;
            RefreshButton.Text = "Refresh";
            RefreshButton.UseVisualStyleBackColor = true;
            RefreshButton.Click += RefreshButton_Click;
            // 
            // Id
            // 
            Id.HeaderText = "Id";
            Id.Name = "Id";
            Id.ReadOnly = true;
            // 
            // Data1
            // 
            Data1.HeaderText = "Data 1";
            Data1.Name = "Data1";
            Data1.ReadOnly = true;
            // 
            // Data2
            // 
            Data2.HeaderText = "Data 2 (child does not import this)";
            Data2.Name = "Data2";
            Data2.ReadOnly = true;
            // 
            // Edit
            // 
            Edit.HeaderText = "Edit";
            Edit.Name = "Edit";
            // 
            // Delete
            // 
            Delete.HeaderText = "Delete";
            Delete.Name = "Delete";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(RefreshButton);
            Controls.Add(AddButton);
            Controls.Add(Grid);
            Name = "MainForm";
            Text = "MainForm";
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)Grid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView Grid;
        private Button AddButton;
        private Button RefreshButton;
        private DataGridViewTextBoxColumn Id;
        private DataGridViewTextBoxColumn Data1;
        private DataGridViewTextBoxColumn Data2;
        private DataGridViewButtonColumn Edit;
        private DataGridViewButtonColumn Delete;
    }
}