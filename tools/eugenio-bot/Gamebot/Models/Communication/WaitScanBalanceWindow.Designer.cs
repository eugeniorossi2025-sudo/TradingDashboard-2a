namespace Gamebot.Models.Communication
{
	 
	public partial class WaitScanBalanceWindow : global::System.Windows.Forms.Form
	{ 
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}
		 
		private void InitializeComponent()
		{
			this.label1 = new global::System.Windows.Forms.Label();
			this.progressBar1 = new global::System.Windows.Forms.ProgressBar();
			base.SuspendLayout();
			this.label1.AutoSize = true;
			this.label1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12.25f);
			this.label1.Location = new global::System.Drawing.Point(190, 88);
			this.label1.Name = "label1";
			this.label1.Size = new global::System.Drawing.Size(235, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "Attendere la lettura del saldo...";
			this.progressBar1.Location = new global::System.Drawing.Point(194, 148);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new global::System.Drawing.Size(231, 23);
			this.progressBar1.Style = global::System.Windows.Forms.ProgressBarStyle.Marquee;
			this.progressBar1.TabIndex = 1;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.SystemColors.ActiveCaption;
			base.ClientSize = new global::System.Drawing.Size(608, 272);
			base.Controls.Add(this.progressBar1);
			base.Controls.Add(this.label1);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.None;
			base.Name = "frmWait";
			base.Opacity = 0.85;
			base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "frmWait";
			base.ResumeLayout(false);
			base.PerformLayout();
		}
		 
		private global::System.ComponentModel.IContainer components;
		 
		private global::System.Windows.Forms.Label label1;
		 
		private global::System.Windows.Forms.ProgressBar progressBar1;
	}
}
