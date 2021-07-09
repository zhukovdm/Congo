
namespace Congo.WFA {
	partial class Form1 {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.initOptionLabel = new System.Windows.Forms.Label();
			this.initCongoLabel = new System.Windows.Forms.Label();
			this.initStartButton = new System.Windows.Forms.Button();
			this.initHIvsHIRadioButton = new System.Windows.Forms.RadioButton();
			this.initHIvsAIWhiteRadioButton = new System.Windows.Forms.RadioButton();
			this.initHIvsAIBlackRadioButton = new System.Windows.Forms.RadioButton();
			this.initAIvsAIRadioButton = new System.Windows.Forms.RadioButton();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.SuspendLayout();
			// 
			// initOptionLabel
			// 
			this.initOptionLabel.AutoSize = true;
			this.initOptionLabel.Font = new System.Drawing.Font("Consolas", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.initOptionLabel.ForeColor = System.Drawing.Color.Green;
			this.initOptionLabel.Location = new System.Drawing.Point(53, 101);
			this.initOptionLabel.Name = "initOptionLabel";
			this.initOptionLabel.Size = new System.Drawing.Size(285, 32);
			this.initOptionLabel.TabIndex = 1;
			this.initOptionLabel.Text = "Select game option";
			this.initOptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// initCongoLabel
			// 
			this.initCongoLabel.AutoSize = true;
			this.initCongoLabel.Font = new System.Drawing.Font("Consolas", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.initCongoLabel.ForeColor = System.Drawing.Color.Green;
			this.initCongoLabel.Location = new System.Drawing.Point(94, 9);
			this.initCongoLabel.Name = "initCongoLabel";
			this.initCongoLabel.Size = new System.Drawing.Size(207, 75);
			this.initCongoLabel.TabIndex = 3;
			this.initCongoLabel.Text = "Congo";
			this.initCongoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// initStartButton
			// 
			this.initStartButton.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.initStartButton.Location = new System.Drawing.Point(282, 317);
			this.initStartButton.Name = "initStartButton";
			this.initStartButton.Size = new System.Drawing.Size(90, 32);
			this.initStartButton.TabIndex = 5;
			this.initStartButton.Text = "Start";
			this.initStartButton.UseVisualStyleBackColor = true;
			this.initStartButton.Click += new System.EventHandler(this.initStartButton_Click);
			// 
			// initHIvsHIRadioButton
			// 
			this.initHIvsHIRadioButton.AutoSize = true;
			this.initHIvsHIRadioButton.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.initHIvsHIRadioButton.Location = new System.Drawing.Point(91, 147);
			this.initHIvsHIRadioButton.Name = "initHIvsHIRadioButton";
			this.initHIvsHIRadioButton.Size = new System.Drawing.Size(118, 26);
			this.initHIvsHIRadioButton.TabIndex = 6;
			this.initHIvsHIRadioButton.TabStop = true;
			this.initHIvsHIRadioButton.Text = "HI vs. HI";
			this.initHIvsHIRadioButton.UseVisualStyleBackColor = true;
			// 
			// initHIvsAIWhiteRadioButton
			// 
			this.initHIvsAIWhiteRadioButton.AutoSize = true;
			this.initHIvsAIWhiteRadioButton.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.initHIvsAIWhiteRadioButton.Location = new System.Drawing.Point(91, 179);
			this.initHIvsAIWhiteRadioButton.Name = "initHIvsAIWhiteRadioButton";
			this.initHIvsAIWhiteRadioButton.Size = new System.Drawing.Size(188, 26);
			this.initHIvsAIWhiteRadioButton.TabIndex = 7;
			this.initHIvsAIWhiteRadioButton.TabStop = true;
			this.initHIvsAIWhiteRadioButton.Text = "HI vs. AI, white";
			this.initHIvsAIWhiteRadioButton.UseVisualStyleBackColor = true;
			// 
			// initHIvsAIBlackRadioButton
			// 
			this.initHIvsAIBlackRadioButton.AutoSize = true;
			this.initHIvsAIBlackRadioButton.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.initHIvsAIBlackRadioButton.Location = new System.Drawing.Point(91, 211);
			this.initHIvsAIBlackRadioButton.Name = "initHIvsAIBlackRadioButton";
			this.initHIvsAIBlackRadioButton.Size = new System.Drawing.Size(188, 26);
			this.initHIvsAIBlackRadioButton.TabIndex = 8;
			this.initHIvsAIBlackRadioButton.TabStop = true;
			this.initHIvsAIBlackRadioButton.Text = "HI vs. AI, black";
			this.initHIvsAIBlackRadioButton.UseVisualStyleBackColor = true;
			// 
			// initAIvsAIRadioButton
			// 
			this.initAIvsAIRadioButton.AutoSize = true;
			this.initAIvsAIRadioButton.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.initAIvsAIRadioButton.Location = new System.Drawing.Point(91, 243);
			this.initAIvsAIRadioButton.Name = "initAIvsAIRadioButton";
			this.initAIvsAIRadioButton.Size = new System.Drawing.Size(118, 26);
			this.initAIvsAIRadioButton.TabIndex = 9;
			this.initAIvsAIRadioButton.TabStop = true;
			this.initAIvsAIRadioButton.Text = "AI vs. AI";
			this.initAIvsAIRadioButton.UseVisualStyleBackColor = true;
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(384, 361);
			this.Controls.Add(this.initAIvsAIRadioButton);
			this.Controls.Add(this.initHIvsAIBlackRadioButton);
			this.Controls.Add(this.initHIvsAIWhiteRadioButton);
			this.Controls.Add(this.initHIvsHIRadioButton);
			this.Controls.Add(this.initStartButton);
			this.Controls.Add(this.initCongoLabel);
			this.Controls.Add(this.initOptionLabel);
			this.MaximizeBox = false;
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Congo";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label initOptionLabel;
		private System.Windows.Forms.Label initCongoLabel;
		private System.Windows.Forms.Button initStartButton;
		private System.Windows.Forms.RadioButton initHIvsHIRadioButton;
		private System.Windows.Forms.RadioButton initHIvsAIWhiteRadioButton;
		private System.Windows.Forms.RadioButton initHIvsAIBlackRadioButton;
		private System.Windows.Forms.RadioButton initAIvsAIRadioButton;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
	}
}

