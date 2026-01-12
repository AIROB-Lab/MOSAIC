using System;

namespace MosaicLibary
{
    partial class cpPythonNetPCA
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false..</param>
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
            this.inputField_Learning = new System.Windows.Forms.TextBox();
            this.Apply = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.Model_ch = new System.Windows.Forms.ComboBox();
            this.LDA_Label = new System.Windows.Forms.Label();
            this.LDA = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // inputField_Learning
            // 
            this.inputField_Learning.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.inputField_Learning.Location = new System.Drawing.Point(21, 273);
            this.inputField_Learning.Name = "inputField_Learning";
            this.inputField_Learning.Size = new System.Drawing.Size(145, 50);
            this.inputField_Learning.TabIndex = 2;
            // 
            // Apply
            // 
            this.Apply.BackColor = System.Drawing.SystemColors.Info;
            this.Apply.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Apply.Location = new System.Drawing.Point(21, 362);
            this.Apply.Name = "Apply";
            this.Apply.Size = new System.Drawing.Size(572, 129);
            this.Apply.TabIndex = 3;
            this.Apply.Text = "Apply Model";
            this.Apply.UseVisualStyleBackColor = false;
            this.Apply.Click += new System.EventHandler(this.Fit_Model);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(13, 205);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(522, 44);
            this.label4.TabIndex = 9;
            this.label4.Text = "Number of the Components:";
            this.label4.Click += new System.EventHandler(this.label4_Click_3);
            // 
            // Model_ch
            // 
            this.Model_ch.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Model_ch.FormattingEnabled = true;
            this.Model_ch.Items.AddRange(new object[] {
            "PCA",
            "ICA",
            "LDA Transform",
            "LDA Predict"});
            this.Model_ch.Location = new System.Drawing.Point(21, 122);
            this.Model_ch.Name = "Model_ch";
            this.Model_ch.Size = new System.Drawing.Size(435, 56);
            this.Model_ch.TabIndex = 10;
            this.Model_ch.Text = "Choose Model";
            // 
            // LDA_Label
            // 
            this.LDA_Label.AutoSize = true;
            this.LDA_Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold);
            this.LDA_Label.Location = new System.Drawing.Point(647, 130);
            this.LDA_Label.Name = "LDA_Label";
            this.LDA_Label.Size = new System.Drawing.Size(235, 48);
            this.LDA_Label.TabIndex = 11;
            this.LDA_Label.Text = "LDA_Label";
            // 
            // LDA
            // 
            this.LDA.AutoSize = true;
            this.LDA.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold);
            this.LDA.Location = new System.Drawing.Point(647, 55);
            this.LDA.Name = "LDA";
            this.LDA.Size = new System.Drawing.Size(236, 48);
            this.LDA.TabIndex = 12;
            this.LDA.Text = "LDA Label:";
            // 
            // cpPythonNetPCA
            // 
            this.ClientSize = new System.Drawing.Size(968, 583);
            this.Controls.Add(this.LDA);
            this.Controls.Add(this.LDA_Label);
            this.Controls.Add(this.Model_ch);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Apply);
            this.Controls.Add(this.inputField_Learning);
            this.Name = "cpPythonNetPCA";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void label4_Click_3(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label_a;
        private System.Windows.Forms.TextBox inputField_Learning;
        private System.Windows.Forms.Button Apply;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox Model_ch;
        private System.Windows.Forms.Label LDA_Label;
        private System.Windows.Forms.Label LDA;
    }
}
