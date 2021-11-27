namespace prjDem2Relief {
	partial class frmMain {
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent() {
			this.prb = new System.Windows.Forms.ProgressBar();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cmdInFile = new System.Windows.Forms.Button();
			this.txtInFile = new System.Windows.Forms.TextBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.rdoUnitM = new System.Windows.Forms.RadioButton();
			this.rdoUnitDeg = new System.Windows.Forms.RadioButton();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.txtR = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.chkRel = new System.Windows.Forms.CheckBox();
			this.chkOpu = new System.Windows.Forms.CheckBox();
			this.chkGrad = new System.Windows.Forms.CheckBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.cboPriority = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.cmdGdalDir = new System.Windows.Forms.Button();
			this.txtGdalDir = new System.Windows.Forms.TextBox();
			this.cmdAnalyze = new System.Windows.Forms.Button();
			this.cmdStop = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// prb
			// 
			this.prb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.prb.Location = new System.Drawing.Point(12, 312);
			this.prb.Name = "prb";
			this.prb.Size = new System.Drawing.Size(256, 19);
			this.prb.TabIndex = 1;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.cmdInFile);
			this.groupBox1.Controls.Add(this.txtInFile);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(256, 46);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "入力ファイル";
			// 
			// cmdInFile
			// 
			this.cmdInFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdInFile.Location = new System.Drawing.Point(202, 18);
			this.cmdInFile.Name = "cmdInFile";
			this.cmdInFile.Size = new System.Drawing.Size(48, 19);
			this.cmdInFile.TabIndex = 1;
			this.cmdInFile.Text = "参照";
			this.cmdInFile.UseVisualStyleBackColor = true;
			this.cmdInFile.Click += new System.EventHandler(this.cmdInFile_Click);
			// 
			// txtInFile
			// 
			this.txtInFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtInFile.Location = new System.Drawing.Point(6, 18);
			this.txtInFile.Name = "txtInFile";
			this.txtInFile.Size = new System.Drawing.Size(190, 19);
			this.txtInFile.TabIndex = 0;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.rdoUnitM);
			this.groupBox2.Controls.Add(this.rdoUnitDeg);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.txtR);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.chkRel);
			this.groupBox2.Controls.Add(this.chkOpu);
			this.groupBox2.Controls.Add(this.chkGrad);
			this.groupBox2.Location = new System.Drawing.Point(12, 64);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(256, 130);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "計算設定";
			// 
			// rdoUnitM
			// 
			this.rdoUnitM.AutoSize = true;
			this.rdoUnitM.Location = new System.Drawing.Point(91, 108);
			this.rdoUnitM.Name = "rdoUnitM";
			this.rdoUnitM.Size = new System.Drawing.Size(32, 16);
			this.rdoUnitM.TabIndex = 6;
			this.rdoUnitM.Text = "m";
			this.rdoUnitM.UseVisualStyleBackColor = true;
			// 
			// rdoUnitDeg
			// 
			this.rdoUnitDeg.AutoSize = true;
			this.rdoUnitDeg.Checked = true;
			this.rdoUnitDeg.Location = new System.Drawing.Point(6, 108);
			this.rdoUnitDeg.Name = "rdoUnitDeg";
			this.rdoUnitDeg.Size = new System.Drawing.Size(79, 16);
			this.rdoUnitDeg.TabIndex = 5;
			this.rdoUnitDeg.TabStop = true;
			this.rdoUnitDeg.Text = "度(Degree)";
			this.rdoUnitDeg.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 89);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(65, 12);
			this.label3.TabIndex = 4;
			this.label3.Text = "座標系単位";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(236, 40);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(14, 12);
			this.label2.TabIndex = 4;
			this.label2.Text = "m";
			// 
			// txtR
			// 
			this.txtR.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtR.Location = new System.Drawing.Point(209, 37);
			this.txtR.Name = "txtR";
			this.txtR.Size = new System.Drawing.Size(25, 19);
			this.txtR.TabIndex = 2;
			this.txtR.Text = "50";
			this.txtR.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.txtR.Leave += new System.EventHandler(this.txtR_Leave);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(189, 40);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(19, 12);
			this.label1.TabIndex = 3;
			this.label1.Text = "R=";
			// 
			// chkRel
			// 
			this.chkRel.AutoSize = true;
			this.chkRel.Checked = true;
			this.chkRel.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkRel.Location = new System.Drawing.Point(6, 62);
			this.chkRel.Name = "chkRel";
			this.chkRel.Size = new System.Drawing.Size(192, 16);
			this.chkRel.TabIndex = 3;
			this.chkRel.Text = "レリーフファイルを作成する(*_rel.tiff)";
			this.chkRel.UseVisualStyleBackColor = true;
			// 
			// chkOpu
			// 
			this.chkOpu.AutoSize = true;
			this.chkOpu.Checked = true;
			this.chkOpu.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkOpu.Location = new System.Drawing.Point(6, 40);
			this.chkOpu.Name = "chkOpu";
			this.chkOpu.Size = new System.Drawing.Size(177, 16);
			this.chkOpu.TabIndex = 1;
			this.chkOpu.Text = "地上開度を計算する(*_opu.tiff)";
			this.chkOpu.UseVisualStyleBackColor = true;
			// 
			// chkGrad
			// 
			this.chkGrad.AutoSize = true;
			this.chkGrad.Checked = true;
			this.chkGrad.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkGrad.Location = new System.Drawing.Point(6, 18);
			this.chkGrad.Name = "chkGrad";
			this.chkGrad.Size = new System.Drawing.Size(169, 16);
			this.chkGrad.TabIndex = 0;
			this.chkGrad.Text = "傾斜量を計算する(*_grad.tiff)";
			this.chkGrad.UseVisualStyleBackColor = true;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.cboPriority);
			this.groupBox3.Controls.Add(this.label4);
			this.groupBox3.Controls.Add(this.cmdGdalDir);
			this.groupBox3.Controls.Add(this.txtGdalDir);
			this.groupBox3.Location = new System.Drawing.Point(12, 200);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(256, 72);
			this.groupBox3.TabIndex = 2;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "システム設定";
			// 
			// cboPriority
			// 
			this.cboPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboPriority.FormattingEnabled = true;
			this.cboPriority.Location = new System.Drawing.Point(75, 43);
			this.cboPriority.Name = "cboPriority";
			this.cboPriority.Size = new System.Drawing.Size(121, 20);
			this.cboPriority.TabIndex = 2;
			this.cboPriority.SelectedIndexChanged += new System.EventHandler(this.cboPriority_SelectedIndexChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(7, 46);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(64, 12);
			this.label4.TabIndex = 8;
			this.label4.Text = "CPU優先度";
			// 
			// cmdGdalDir
			// 
			this.cmdGdalDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdGdalDir.Location = new System.Drawing.Point(202, 18);
			this.cmdGdalDir.Name = "cmdGdalDir";
			this.cmdGdalDir.Size = new System.Drawing.Size(48, 19);
			this.cmdGdalDir.TabIndex = 1;
			this.cmdGdalDir.Text = "参照";
			this.cmdGdalDir.UseVisualStyleBackColor = true;
			this.cmdGdalDir.Click += new System.EventHandler(this.cmdGdalDir_Click);
			// 
			// txtGdalDir
			// 
			this.txtGdalDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtGdalDir.Location = new System.Drawing.Point(6, 18);
			this.txtGdalDir.Name = "txtGdalDir";
			this.txtGdalDir.Size = new System.Drawing.Size(190, 19);
			this.txtGdalDir.TabIndex = 0;
			this.txtGdalDir.Text = "C:\\Program Files\\GDAL";
			// 
			// cmdAnalyze
			// 
			this.cmdAnalyze.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdAnalyze.Location = new System.Drawing.Point(12, 278);
			this.cmdAnalyze.Name = "cmdAnalyze";
			this.cmdAnalyze.Size = new System.Drawing.Size(183, 28);
			this.cmdAnalyze.TabIndex = 9;
			this.cmdAnalyze.Text = "実行";
			this.cmdAnalyze.UseVisualStyleBackColor = true;
			this.cmdAnalyze.Click += new System.EventHandler(this.cmdAnalyze_Click);
			// 
			// cmdStop
			// 
			this.cmdStop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdStop.Location = new System.Drawing.Point(203, 278);
			this.cmdStop.Name = "cmdStop";
			this.cmdStop.Size = new System.Drawing.Size(65, 28);
			this.cmdStop.TabIndex = 10;
			this.cmdStop.Text = "停止";
			this.cmdStop.UseVisualStyleBackColor = true;
			this.cmdStop.Click += new System.EventHandler(this.cmdStop_Click);
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 343);
			this.Controls.Add(this.cmdStop);
			this.Controls.Add(this.cmdAnalyze);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.prb);
			this.Name = "frmMain";
			this.Text = "Dem2Relief";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
			this.Load += new System.EventHandler(this.frmMain_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.ProgressBar prb;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button cmdInFile;
		private System.Windows.Forms.TextBox txtInFile;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.CheckBox chkRel;
		private System.Windows.Forms.CheckBox chkOpu;
		private System.Windows.Forms.CheckBox chkGrad;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtR;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Button cmdGdalDir;
		private System.Windows.Forms.TextBox txtGdalDir;
		private System.Windows.Forms.RadioButton rdoUnitM;
		private System.Windows.Forms.RadioButton rdoUnitDeg;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox cboPriority;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button cmdAnalyze;
		private System.Windows.Forms.Button cmdStop;
	}
}

