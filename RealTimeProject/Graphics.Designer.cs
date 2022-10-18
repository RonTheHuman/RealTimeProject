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
            this.Player1Label = new System.Windows.Forms.Label();
            this.Player2Label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // SocketTimer
            // 
            this.SocketTimer.Enabled = true;
            this.SocketTimer.Interval = 17;
            this.SocketTimer.Tick += new System.EventHandler(this.SocketTimer_Tick);
            // 
            // Player1Label
            // 
            this.Player1Label.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Player1Label.Location = new System.Drawing.Point(12, 349);
            this.Player1Label.Name = "Player1Label";
            this.Player1Label.Size = new System.Drawing.Size(56, 55);
            this.Player1Label.TabIndex = 0;
            // 
            // Player2Label
            // 
            this.Player2Label.BackColor = System.Drawing.SystemColors.Desktop;
            this.Player2Label.Location = new System.Drawing.Point(732, 42);
            this.Player2Label.Name = "Player2Label";
            this.Player2Label.Size = new System.Drawing.Size(56, 55);
            this.Player2Label.TabIndex = 1;
            // 
            // Graphics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.Player2Label);
            this.Controls.Add(this.Player1Label);
            this.Name = "Graphics";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Graphics_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Graphics_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Graphics_KeyUp);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer SocketTimer;
        private Label Player1Label;
        private Label Player2Label;
    }
}