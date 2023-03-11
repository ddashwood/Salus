namespace SalusExampleParent
{
    partial class AddEditForm
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
            Data1TextBox = new TextBox();
            Data2TextBox = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            SaveButton = new Button();
            CancelAddButton = new Button();
            SuspendLayout();
            // 
            // Data1Textbox
            // 
            Data1TextBox.Location = new Point(148, 12);
            Data1TextBox.Multiline = true;
            Data1TextBox.Name = "Data1Textbox";
            Data1TextBox.Size = new Size(640, 162);
            Data1TextBox.TabIndex = 0;
            // 
            // Data2TextBox
            // 
            Data2TextBox.Location = new Point(148, 180);
            Data2TextBox.Multiline = true;
            Data2TextBox.Name = "Data2TextBox";
            Data2TextBox.Size = new Size(640, 162);
            Data2TextBox.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 12);
            label1.Name = "label1";
            label1.Size = new Size(40, 15);
            label1.TabIndex = 2;
            label1.Text = "Data 1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 180);
            label2.Name = "label2";
            label2.Size = new Size(40, 15);
            label2.TabIndex = 3;
            label2.Text = "Data 2";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 207);
            label3.Name = "label3";
            label3.Size = new Size(87, 30);
            label3.TabIndex = 4;
            label3.Text = "Client does not\r\nimport Data 2";
            // 
            // SaveButton
            // 
            SaveButton.Location = new Point(713, 348);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new Size(75, 40);
            SaveButton.TabIndex = 5;
            SaveButton.Text = "Save";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            // 
            // CancelAddButton
            // 
            CancelAddButton.Location = new Point(632, 348);
            CancelAddButton.Name = "CancelAddButton";
            CancelAddButton.Size = new Size(75, 40);
            CancelAddButton.TabIndex = 6;
            CancelAddButton.Text = "Cancel";
            CancelAddButton.UseVisualStyleBackColor = true;
            CancelAddButton.Click += CancelButton_Click;
            // 
            // AddForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 400);
            Controls.Add(CancelAddButton);
            Controls.Add(SaveButton);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(Data2TextBox);
            Controls.Add(Data1TextBox);
            Name = "AddForm";
            Text = "AddForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox Data1TextBox;
        private TextBox Data2TextBox;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button SaveButton;
        private Button CancelAddButton;
    }
}