namespace RealTimeProject
{
    partial class Graphics
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SocketTimer = new System.Windows.Forms.Timer(this.components);
            this.TestLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // SocketTimer
            // 
            this.SocketTimer.Enabled = true;
            this.SocketTimer.Tick += new System.EventHandler(this.SocketTimer_Tick);
            // 
            // TestLabel
            // 
            this.TestLabel.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.TestLabel.Location = new System.Drawing.Point(127, 163);
            this.TestLabel.Name = "TestLabel";
            this.TestLabel.Size = new System.Drawing.Size(90, 86);
            this.TestLabel.TabIndex = 0;
            // 
            // Graphics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.TestLabel);
            this.Name = "Graphics";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Graphics_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Graphics_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Graphics_KeyUp);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer SocketTimer;
        private Label TestLabel;
    }
}