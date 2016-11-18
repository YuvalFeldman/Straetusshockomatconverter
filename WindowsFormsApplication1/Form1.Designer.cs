namespace Straetusshockomatconverter
{
    partial class Form1
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
            this.Select = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Select
            // 
            this.Select.Location = new System.Drawing.Point(173, 12);
            this.Select.Name = "Select";
            this.Select.Size = new System.Drawing.Size(155, 27);
            this.Select.TabIndex = 0;
            this.Select.Text = "Single";
            this.Select.UseVisualStyleBackColor = true;
            this.Select.Click += new System.EventHandler(this.Select_Click);
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(334, 12);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(155, 27);
            this.Save.TabIndex = 1;
            this.Save.Text = "Batch";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(155, 27);
            this.button1.TabIndex = 2;
            this.button1.Text = "Config";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(496, 45);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.Select);
            this.Name = "Form1";
            this.Text = "Straetus shockomat converter";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Select;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.Button button1;
    }
}

