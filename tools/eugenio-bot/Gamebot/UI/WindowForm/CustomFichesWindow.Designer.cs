namespace Gamebot.UI.WindowForm
{
		public partial class CustomFichesWindow : global::System.Windows.Forms.Form
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
			this.editFichesOkBtn = new global::System.Windows.Forms.Button();
			this.editFichesCancelBtn = new global::System.Windows.Forms.Button();
			this.editFichesAddBtn = new global::System.Windows.Forms.Button();
			this.customFichesContainerPanel = new global::System.Windows.Forms.Panel();
			this.placeholderPanel12 = new global::System.Windows.Forms.Panel();
			this.placeholderPanel11 = new global::System.Windows.Forms.Panel();
			this.placeholderPanel10 = new global::System.Windows.Forms.Panel();
			this.placeholderPanel09 = new global::System.Windows.Forms.Panel();
			this.placeholderPanel08 = new global::System.Windows.Forms.Panel();
			this.placeholderPanel07 = new global::System.Windows.Forms.Panel();
			this.placeholderPanel06 = new global::System.Windows.Forms.Panel();
			this.placeholderPanel05 = new global::System.Windows.Forms.Panel();
			this.placeholderPanel04 = new global::System.Windows.Forms.Panel();
			this.placeholderPanel03 = new global::System.Windows.Forms.Panel();
			this.placeholderPanel02 = new global::System.Windows.Forms.Panel();
			this.placeholderPanel01 = new global::System.Windows.Forms.Panel();
			this.helpbtn = new global::System.Windows.Forms.Button();
			this.customFichesContainerPanel.SuspendLayout();
			base.SuspendLayout();
			this.editFichesOkBtn.Location = new global::System.Drawing.Point(115, 504);
			this.editFichesOkBtn.Name = "editFichesOkBtn";
			this.editFichesOkBtn.Size = new global::System.Drawing.Size(75, 23);
			this.editFichesOkBtn.TabIndex = 0;
			this.editFichesOkBtn.Text = "OK";
			this.editFichesOkBtn.UseVisualStyleBackColor = true;
			this.editFichesOkBtn.Click += new global::System.EventHandler(this.editFichesOkBtn_Click);
			this.editFichesCancelBtn.DialogResult = global::System.Windows.Forms.DialogResult.Cancel;
			this.editFichesCancelBtn.Location = new global::System.Drawing.Point(219, 504);
			this.editFichesCancelBtn.Name = "editFichesCancelBtn";
			this.editFichesCancelBtn.Size = new global::System.Drawing.Size(75, 23);
			this.editFichesCancelBtn.TabIndex = 1;
			this.editFichesCancelBtn.Text = "Cancel";
			this.editFichesCancelBtn.UseVisualStyleBackColor = true;
			this.editFichesCancelBtn.Click += new global::System.EventHandler(this.editFichesCancelBtn_Click);
			this.editFichesAddBtn.BackColor = global::System.Drawing.Color.Transparent;
			this.editFichesAddBtn.Location = new global::System.Drawing.Point(18, 438);
			this.editFichesAddBtn.Name = "editFichesAddBtn";
			this.editFichesAddBtn.Size = new global::System.Drawing.Size(356, 25);
			this.editFichesAddBtn.TabIndex = 8;
			this.editFichesAddBtn.Tag = "";
			this.editFichesAddBtn.Text = "Aggiungi";
			this.editFichesAddBtn.UseVisualStyleBackColor = true;
			this.editFichesAddBtn.Click += new global::System.EventHandler(this.editFichesAddBtn_Click);
			this.customFichesContainerPanel.Controls.Add(this.placeholderPanel12);
			this.customFichesContainerPanel.Controls.Add(this.placeholderPanel11);
			this.customFichesContainerPanel.Controls.Add(this.placeholderPanel10);
			this.customFichesContainerPanel.Controls.Add(this.placeholderPanel09);
			this.customFichesContainerPanel.Controls.Add(this.placeholderPanel08);
			this.customFichesContainerPanel.Controls.Add(this.placeholderPanel07);
			this.customFichesContainerPanel.Controls.Add(this.placeholderPanel06);
			this.customFichesContainerPanel.Controls.Add(this.placeholderPanel05);
			this.customFichesContainerPanel.Controls.Add(this.placeholderPanel04);
			this.customFichesContainerPanel.Controls.Add(this.placeholderPanel03);
			this.customFichesContainerPanel.Controls.Add(this.placeholderPanel02);
			this.customFichesContainerPanel.Controls.Add(this.placeholderPanel01);
			this.customFichesContainerPanel.Controls.Add(this.editFichesAddBtn);
			this.customFichesContainerPanel.Location = new global::System.Drawing.Point(12, 12);
			this.customFichesContainerPanel.Name = "customFichesContainerPanel";
			this.customFichesContainerPanel.Size = new global::System.Drawing.Size(394, 482);
			this.customFichesContainerPanel.TabIndex = 0;
			this.placeholderPanel12.BackColor = global::System.Drawing.SystemColors.InactiveCaption;
			this.placeholderPanel12.Location = new global::System.Drawing.Point(16, 408);
			this.placeholderPanel12.Name = "placeholderPanel12";
			this.placeholderPanel12.Size = new global::System.Drawing.Size(360, 28);
			this.placeholderPanel12.TabIndex = 38;
			this.placeholderPanel11.BackColor = global::System.Drawing.SystemColors.InactiveCaption;
			this.placeholderPanel11.Location = new global::System.Drawing.Point(16, 372);
			this.placeholderPanel11.Name = "placeholderPanel11";
			this.placeholderPanel11.Size = new global::System.Drawing.Size(360, 28);
			this.placeholderPanel11.TabIndex = 37;
			this.placeholderPanel10.BackColor = global::System.Drawing.SystemColors.InactiveCaption;
			this.placeholderPanel10.Location = new global::System.Drawing.Point(16, 336);
			this.placeholderPanel10.Name = "placeholderPanel10";
			this.placeholderPanel10.Size = new global::System.Drawing.Size(360, 28);
			this.placeholderPanel10.TabIndex = 36;
			this.placeholderPanel09.BackColor = global::System.Drawing.SystemColors.InactiveCaption;
			this.placeholderPanel09.Location = new global::System.Drawing.Point(16, 300);
			this.placeholderPanel09.Name = "placeholderPanel09";
			this.placeholderPanel09.Size = new global::System.Drawing.Size(360, 28);
			this.placeholderPanel09.TabIndex = 35;
			this.placeholderPanel08.BackColor = global::System.Drawing.SystemColors.InactiveCaption;
			this.placeholderPanel08.Location = new global::System.Drawing.Point(16, 264);
			this.placeholderPanel08.Name = "placeholderPanel08";
			this.placeholderPanel08.Size = new global::System.Drawing.Size(360, 28);
			this.placeholderPanel08.TabIndex = 34;
			this.placeholderPanel07.BackColor = global::System.Drawing.SystemColors.InactiveCaption;
			this.placeholderPanel07.Location = new global::System.Drawing.Point(16, 228);
			this.placeholderPanel07.Name = "placeholderPanel07";
			this.placeholderPanel07.Size = new global::System.Drawing.Size(360, 28);
			this.placeholderPanel07.TabIndex = 33;
			this.placeholderPanel06.BackColor = global::System.Drawing.SystemColors.InactiveCaption;
			this.placeholderPanel06.Location = new global::System.Drawing.Point(16, 192);
			this.placeholderPanel06.Name = "placeholderPanel06";
			this.placeholderPanel06.Size = new global::System.Drawing.Size(360, 28);
			this.placeholderPanel06.TabIndex = 32;
			this.placeholderPanel05.BackColor = global::System.Drawing.SystemColors.InactiveCaption;
			this.placeholderPanel05.Location = new global::System.Drawing.Point(16, 156);
			this.placeholderPanel05.Name = "placeholderPanel05";
			this.placeholderPanel05.Size = new global::System.Drawing.Size(360, 28);
			this.placeholderPanel05.TabIndex = 31;
			this.placeholderPanel04.BackColor = global::System.Drawing.SystemColors.InactiveCaption;
			this.placeholderPanel04.Location = new global::System.Drawing.Point(16, 120);
			this.placeholderPanel04.Name = "placeholderPanel04";
			this.placeholderPanel04.Size = new global::System.Drawing.Size(360, 28);
			this.placeholderPanel04.TabIndex = 30;
			this.placeholderPanel03.BackColor = global::System.Drawing.SystemColors.InactiveCaption;
			this.placeholderPanel03.Location = new global::System.Drawing.Point(16, 84);
			this.placeholderPanel03.Name = "placeholderPanel03";
			this.placeholderPanel03.Size = new global::System.Drawing.Size(360, 28);
			this.placeholderPanel03.TabIndex = 29;
			this.placeholderPanel02.BackColor = global::System.Drawing.SystemColors.InactiveCaption;
			this.placeholderPanel02.Location = new global::System.Drawing.Point(16, 48);
			this.placeholderPanel02.Name = "placeholderPanel02";
			this.placeholderPanel02.Size = new global::System.Drawing.Size(360, 28);
			this.placeholderPanel02.TabIndex = 28;
			this.placeholderPanel01.BackColor = global::System.Drawing.SystemColors.InactiveCaption;
			this.placeholderPanel01.Location = new global::System.Drawing.Point(16, 12);
			this.placeholderPanel01.Name = "placeholderPanel01";
			this.placeholderPanel01.Size = new global::System.Drawing.Size(360, 28);
			this.placeholderPanel01.TabIndex = 27;
			this.helpbtn.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Bold, global::System.Drawing.GraphicsUnit.Point, 0);
			this.helpbtn.ForeColor = global::System.Drawing.SystemColors.Highlight;
			this.helpbtn.Location = new global::System.Drawing.Point(349, 504);
			this.helpbtn.Name = "helpbtn";
			this.helpbtn.Size = new global::System.Drawing.Size(37, 23);
			this.helpbtn.TabIndex = 2;
			this.helpbtn.Text = "?";
			this.helpbtn.UseVisualStyleBackColor = true;
			this.helpbtn.Click += new global::System.EventHandler(this.helpbtn_Click);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(418, 539);
			base.Controls.Add(this.helpbtn);
			base.Controls.Add(this.customFichesContainerPanel);
			base.Controls.Add(this.editFichesCancelBtn);
			base.Controls.Add(this.editFichesOkBtn);
			base.MaximizeBox = false;
			base.Name = "EditFichesForm";
			this.Text = "Impostazione Fiches";
			this.customFichesContainerPanel.ResumeLayout(false);
			base.ResumeLayout(false);
		}

	 
		private global::System.ComponentModel.IContainer components;

	 
		private global::System.Windows.Forms.Button editFichesOkBtn;

 
		private global::System.Windows.Forms.Button editFichesCancelBtn;

		 
		private global::System.Windows.Forms.Button editFichesAddBtn;

	 
		private global::System.Windows.Forms.Panel customFichesContainerPanel;
 
		private global::System.Windows.Forms.Panel placeholderPanel01;

		 
		private global::System.Windows.Forms.Panel placeholderPanel12;

		 
		private global::System.Windows.Forms.Panel placeholderPanel11;

		 
		private global::System.Windows.Forms.Panel placeholderPanel10;

	 
		private global::System.Windows.Forms.Panel placeholderPanel09;

		 private global::System.Windows.Forms.Panel placeholderPanel08;

		 private global::System.Windows.Forms.Panel placeholderPanel07;

		 private global::System.Windows.Forms.Panel placeholderPanel06;

		 private global::System.Windows.Forms.Panel placeholderPanel05;

		 private global::System.Windows.Forms.Panel placeholderPanel04;

		 private global::System.Windows.Forms.Panel placeholderPanel03;

		 private global::System.Windows.Forms.Panel placeholderPanel02;

		 private global::System.Windows.Forms.Button helpbtn;
	}
}
