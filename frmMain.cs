using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml;
using System.Diagnostics;
using GDAL = OSGeo.GDAL;
using Sel = System.Xml.Serialization;
using DEM;

namespace prjDem2Relief {
	public partial class frmMain : Form {

		const string s4612 = "GEOGCS[\"JGD2000\",DATUM[\"Japanese_Geodetic_Datum_2000\",SPHEROID[\"GRS 1980\",6378137,298.257222101,AUTHORITY[\"EPSG\",\"7019\"]],TOWGS84[0,0,0,0,0,0,0],AUTHORITY[\"EPSG\",\"6612\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4612\"]]";

		/// <summary>
		/// 設定事項
		/// </summary>
		cSetting Setting;

		/// <summary>
		/// GeoTiff操作
		/// </summary>
		GDAL.Driver dvGTiff;

		/// <summary>
		/// バックグラウンド処理用
		/// </summary>
		DoWorkEventHandler bgwHandler;

		BackgroundWorker bgwWorking;

		public frmMain() {
			InitializeComponent();
			Setting = new cSetting();
			Setting.ReadXML();
		}

		private void frmMain_Load(object sender, EventArgs e) {
			//Gdal設定
			InitGdal();
			dvGTiff = GDAL.Gdal.GetDriverByName("GTiff");
			txtGdalDir.Text = Setting.GdalPath;

			//CPU優先度
			cboPriority.Items.Add(ProcessPriorityClass.RealTime);
			cboPriority.Items.Add(ProcessPriorityClass.High);
			cboPriority.Items.Add(ProcessPriorityClass.AboveNormal);
			cboPriority.Items.Add(ProcessPriorityClass.Normal);
			cboPriority.Items.Add(ProcessPriorityClass.BelowNormal);
			cboPriority.Items.Add(ProcessPriorityClass.Idle);
			try { cboPriority.SelectedItem = Setting.Priority; }
			catch { cboPriority.SelectedItem = ProcessPriorityClass.Normal; Setting.Priority = ProcessPriorityClass.Normal; }

			Process proc = System.Diagnostics.Process.GetCurrentProcess();
			proc.PriorityClass = Setting.Priority;

			//BackGroundWorker
			bgwWorking = new BackgroundWorker();
			bgwWorking.RunWorkerCompleted += bgwWorking_RunWorkerCompleted;
			bgwWorking.ProgressChanged += bgwWorking_ProgressChanged;
		}


		/// <summary>
		/// 入力ファイル設定
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cmdInFile_Click(object sender, EventArgs e) {
			using(OpenFileDialog ofd = new OpenFileDialog()) {
				if (Directory.Exists(txtInFile.Text)){ ofd.InitialDirectory = txtInFile.Text; }
				ofd.Filter = "GeoTiffファイル(*.tif;*.tiff)|*.tif;*.tiff|すべてのファイル(*.*)|*.*";
				ofd.Title = "傾斜量・地上開度等の計算対象を選択してください。";
				ofd.RestoreDirectory = true;
				if(ofd.ShowDialog() == DialogResult.OK) {
					txtInFile.Text = ofd.FileName;

					#region "CRS取得"
					try {
						using(GDAL.Dataset ds = GDAL.Gdal.Open(txtInFile.Text, GDAL.Access.GA_ReadOnly)) {
							string sPrj = ds.GetProjectionRef();
							if(sPrj == "") { return; }
							if(cAnalyzeDem.GetUnit(sPrj) == cAnalyzeDem.Unit.Degree) { rdoUnitDeg.Checked=true; }
							else { rdoUnitM.Checked = true; }
						}


					} catch(Exception ex) {
						ErrMsg(ex.Message);
					}
					#endregion

				}
			}
		}

		private void cmdGdalDir_Click(object sender, EventArgs e) {
			using(FolderBrowserDialog fbd = new FolderBrowserDialog()) {
				if (Directory.Exists(txtGdalDir.Text)) { fbd.SelectedPath = txtGdalDir.Text; }
				fbd.Description = "GDALのインストールパスを設定してください。";
				if(fbd.ShowDialog() == DialogResult.OK) {
					txtGdalDir.Text = fbd.SelectedPath;
				}
			}

		}

		private void cboPriority_SelectedIndexChanged(object sender, EventArgs e) {
			Setting.Priority = (ProcessPriorityClass)cboPriority.SelectedItem;
		}

		private void frmMain_FormClosed(object sender, FormClosedEventArgs e) {
			try {
				Setting.SaveXML();
			}
			catch (Exception ex) { Debug.Print(ex.Message); }
		}

		private void ErrMsg(string Message) {
			MessageBox.Show(Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

		}

		private bool InitGdal() {
			//GDAL設定
			//http://vipassanaandenvironmentalinformatics.blogspot.jp/2013/03/getting-started-with-c-and-gdal.html
			//
			try {
				if (Directory.Exists(Setting.GdalPath)) {
					string sVal;

					//PATH
					sVal = Environment.GetEnvironmentVariable("PATH") + ";" +
						 Setting.GdalPath + ";" +
						 Path.Combine(Setting.GdalPath, "csharp") + ";" +
						 Path.Combine(Setting.GdalPath, "gdal-data") + ";" +
						 Path.Combine(Setting.GdalPath, "gdalplugins") + ";" +
						 Path.Combine(Setting.GdalPath, "projlib");
					Environment.SetEnvironmentVariable("PATH", sVal);

					//Data
					Environment.SetEnvironmentVariable("GDAL_DATA", Path.Combine(Setting.GdalPath, "gdal-data"));
					GDAL.Gdal.SetConfigOption("GDAL_DATA", Path.Combine(Setting.GdalPath, "gdal-data"));

					//GDAL_DRIVER(Plugins)
					Environment.SetEnvironmentVariable("GDAL_DRIVER_PATH", Path.Combine(Setting.GdalPath, "gdalplugins"));
					GDAL.Gdal.SetConfigOption("GDAL_DRIVER_PATH", Path.Combine(Setting.GdalPath, "gdalplugins"));

					//PROJ_LIB
					Environment.SetEnvironmentVariable("PROJ_LIB", Path.Combine(Setting.GdalPath, "projlib"));
					GDAL.Gdal.SetConfigOption("PROJ_LIB", Path.Combine(Setting.GdalPath, "projlib"));

					GDAL.Gdal.SetConfigOption("GDAL_CACHEMAX", "100000");
					GDAL.Gdal.SetConfigOption("CPL_TMPDIR", @".\");

					GDAL.Gdal.AllRegister();


					return (true);
				}
				else {
					throw (new Exception("GDALのインストール先を正しく設定してください。\n現在の設定:" + Setting.GdalPath));
				}
			}
			catch (Exception ex) { ErrMsg("GDALの設定に失敗しました。\nGeoTiffの作成は出来ません(csv出力をお願いします)。\n" + ex.Message); return (false); }
		}

		private void cmdAnalyze_Click(object sender, EventArgs e) {
			cmdAnalyze.Enabled = false;
			bgwWorking.WorkerReportsProgress = true;
			bgwWorking.WorkerSupportsCancellation = true;
			prb.Value = 0;
			prb.Minimum = 0;
			prb.Maximum = 1000;

			if(chkOpu.Checked || chkRel.Checked) { bgwHandler = (s, ea) => bgwWorking_DoWorkGradOpu(s, ea); }
			else { bgwHandler = (s, ea) => bgwWorking_DoWorkGrad(s, ea); }
			bgwWorking.DoWork += bgwHandler;
			bgwWorking.RunWorkerAsync();
		}

		/// <summary>
		/// 傾斜量のみを計算する
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void bgwWorking_DoWorkGrad(object sender, DoWorkEventArgs e) {
			List<object> lsRes = new List<object>();
			lsRes.Add(-100);

			try {
				using (GDAL.Dataset dsIn = GDAL.Gdal.Open(txtInFile.Text, GDAL.Access.GA_ReadOnly)) {
					//傾斜量格納先
					float[] fGrad = new float[dsIn.RasterXSize * dsIn.RasterYSize];
					for (int i = 0; i < fGrad.Length; i++) { fGrad[i] = float.NaN; }

					//ラスタサイズ
					int iW = dsIn.RasterXSize;
					int iH = dsIn.RasterYSize;

					//神谷ほか(1999) 傾斜量図の作成とその応用，情報地質，Vol.10，No.2，pp.76-79． 
					#region "傾斜量計算"
					using (GDAL.Band bdIn = dsIn.GetRasterBand(1)) {
						//座標系の単位
						string sPrj = dsIn.GetProjectionRef();
						cAnalyzeDem.Unit AxisUnit = cAnalyzeDem.GetUnit(sPrj);

						//1ピクセル当たりの距離
						double[] dX, dY;
						cAnalyzeDem.GetPixcelDist(dsIn, AxisUnit, sPrj, out dX, out dY);

						//標高取得
						double[] dZ = new double[dsIn.RasterXSize * dsIn.RasterYSize];
						bdIn.ReadRaster(0, 0, dsIn.RasterXSize, dsIn.RasterYSize, dZ, dsIn.RasterXSize, dsIn.RasterYSize, 0, 0);

						for (int iY = 1; iY < dsIn.RasterYSize - 1; iY++) {
							bgwWorking.ReportProgress(950 * iY / dsIn.RasterYSize); //進捗報告

							for (int iX = 1; iX < iW - 1; iX++) {
								//変数名は神谷ほか(1999)
								double dH11 = dZ[iX - 1 + iW * (iY + 1)]; double dH12 = dZ[iX + iW * (iY + 1)]; double dH13 = dZ[iX + 1 + iW * (iY + 1)];
								double dH21 = dZ[iX - 1 + iW * iY]; double dH22 = dZ[iX + iW * iY]; double dH23 = dZ[iX + 1 + iW * iY];
								double dH31 = dZ[iX - 1 + iW * (iY - 1)]; double dH32 = dZ[iX + iW * (iY - 1)]; double dH33 = dZ[iX + 1 + iW * (iY - 1)];
								double dSx = (dH11 + dH21 + dH31 - (dH13 + dH23 + dH33)) / (6 * dX[iY]);
								double dSy = (dH11 + dH12 + dH13 - (dH31 + dH32 + dH33)) / (6 * dY[iY]);
								fGrad[iX + iW * iY] = (float)(Math.Atan(Math.Sqrt(dSx * dSx + dSy * dSy)) * 180d / Math.PI);
							}
						}
					}
					#endregion

					#region "傾斜量計算結果保存"
					string sOutF = Path.Combine(Path.GetDirectoryName(txtInFile.Text), string.Format("{0}_grad.tiff", Path.GetFileNameWithoutExtension(txtInFile.Text)));
					GDAL.Driver dvGtiff = GDAL.Gdal.GetDriverByName("GTiff");
					using (GDAL.Dataset dsOut = dvGtiff.Create(sOutF, iW, iH, 1, GDAL.DataType.GDT_Float32, new string[] { "COMPRESS=DEFLATE", "PREDICTOR=2" })) {
						double[] dGeo = new double[6];
						dsIn.GetGeoTransform(dGeo);
						dsOut.SetGeoTransform(dGeo);
						dsOut.SetProjection(dsIn.GetProjection());
						using (GDAL.Band bdOut = dsOut.GetRasterBand(1)) {
							bdOut.WriteRaster(0, 0, iW, iH, fGrad, iW, iH, 0, 0);
						}
					}
					#endregion
				}
				lsRes[0] = 4;
				e.Result = lsRes;
			}
			catch (Exception ex) {
				ErrMsg(ex.Message);
				prb.Value = prb.Minimum;
				cmdAnalyze.Enabled = true;
			}
		}

		
		private void bgwWorking_DoWorkGradOpu(object sender, DoWorkEventArgs e) {
			try {
				List<object> lsRes = new List<object>();
				lsRes.Add(-100);

				double dR;
				if(!double.TryParse(txtR.Text, out dR)) { throw new Exception("半径は数値で入力してください。"); }

				using (GDAL.Dataset dsIn = GDAL.Gdal.Open(txtInFile.Text, GDAL.Access.GA_ReadOnly)) {

					//ラスタサイズ
					int iW = dsIn.RasterXSize;
					int iH = dsIn.RasterYSize;

					//座標系の単位
					string sPrj = dsIn.GetProjectionRef();
					cAnalyzeDem.Unit AxisUnit = cAnalyzeDem.GetUnit(sPrj);

					//1ピクセル当たりの距離
					double[] dX, dY;
					cAnalyzeDem.GetPixcelDist(dsIn, AxisUnit, sPrj, out dX, out dY);

					//標高:傾斜量・地上開度の両方で使用
					double[] dZ = new double[iW * iH];

					//神谷ほか(1999) 傾斜量図の作成とその応用，情報地質，Vol.10，No.2，pp.76-79． 
					float[] fGrad = new float[iW * iH];
					for (int i = 0; i < fGrad.Length; i++) { fGrad[i] = float.NaN; }
					#region "傾斜量計算"
					using (GDAL.Band bdIn = dsIn.GetRasterBand(1)) {

						//標高取得
						bdIn.ReadRaster(0, 0, dsIn.RasterXSize, dsIn.RasterYSize, dZ, dsIn.RasterXSize, dsIn.RasterYSize, 0, 0);

						for (int iY = 1; iY < dsIn.RasterYSize - 1; iY++) {
							bgwWorking.ReportProgress(200 * iY / dsIn.RasterYSize); //進捗報告

							for (int iX = 1; iX < iW - 1; iX++) {
								//変数名は神谷ほか(1999)
								double dH11 = dZ[iX - 1 + iW * (iY + 1)]; double dH12 = dZ[iX + iW * (iY + 1)]; double dH13 = dZ[iX + 1 + iW * (iY + 1)];
								double dH21 = dZ[iX - 1 + iW * iY]; double dH22 = dZ[iX + iW * iY]; double dH23 = dZ[iX + 1 + iW * iY];
								double dH31 = dZ[iX - 1 + iW * (iY - 1)]; double dH32 = dZ[iX + iW * (iY - 1)]; double dH33 = dZ[iX + 1 + iW * (iY - 1)];
								double dSx = (dH11 + dH21 + dH31 - (dH13 + dH23 + dH33)) / (6 * dX[iY]);
								double dSy = (dH11 + dH12 + dH13 - (dH31 + dH32 + dH33)) / (6 * dY[iY]);
								fGrad[iX + iW * iY] = (float)(Math.Atan(Math.Sqrt(dSx * dSx + dSy * dSy)) * 180d / Math.PI);
							}
						}
					}
					#endregion

					//横山ほか(1999) 開度による地形特徴の表示，写真測量とリモートセンシング，38(4)，26-34．定義Ⅱによる地上開度
					float[] fOpu = new float[dsIn.RasterXSize * dsIn.RasterYSize];
					float[] fOpd = new float[dsIn.RasterXSize * dsIn.RasterYSize];
					for (int i = 0; i < fOpu.Length; i++) { fOpu[i] = float.NaN; }
					#region "地上開度計算"

					//X・Yの移動量(0:真北、1:北東、2:東・・・7:北西)
					int[] iSx = new int[] { 0, 1, 1, 1, 0, -1, -1, -1 };
					int[] iSy = new int[] { 1, 1, 0, -1, -1, -1, 0, 1 };

					for (int iY = 1; iY < iH - 1; iY++) {
						bgwWorking.ReportProgress(200 + 780 * iY / dsIn.RasterYSize); //進捗報告

						Parallel.For(1, iW - 1, iX =>//for (int iX = 1; iX < iW - 1; iX++)
						{
							double dZ0 = dZ[iX + iW * iY];

							double[] dOpUm = new double[8];
							double[] dOpDm = new double[8];
							double dSumU = 0d;
							double dSumD = 0d;
							int iC = 0;
							//方位別に地上開度を求める
							for (int i = 0; i < dOpUm.Length; i++) {
								dOpUm[i] = 181d; dOpDm[i] = 181d;//ありえない値
								double dL = 0d;
								int iS = 1;
								while (dL <= dR) {
									//仰角＆方位別開度
									dL = Math.Sqrt(Math.Pow(iS * iSx[i] * dX[iY], 2) + Math.Pow(iS * iSy[i] * dY[iY], 2));
									if (dR < dL) { break; }
									int j = iX + iS * iSx[i] + iW * (iY + (iS * iSy[i]));
									if (j < 0 || dZ.Length <= j) { break; }
									double dZ2 = dZ[j];
									double dDz = dZ2 - dZ0;
									double dAng = 180d * Math.Atan(dDz / dL) / Math.PI; //仰角
									if ((90 - dAng) < dOpUm[i]) { dOpUm[i] = 90 - dAng; } //地上開度
									if ((90 + dAng) < dOpDm[i]) { dOpDm[i] = 90 + dAng; } //地下開度
									iS++;
								}
								if (dOpUm[i] < 180d) {
									dSumU += dOpUm[i];
									dSumD += dOpDm[i];
									iC++;
								}
							}

							//メッシュの地上開度
							if (0 < iC) {
								fOpu[iX + iW * iY] = (float)(dSumU / iC);
								fOpd[iX + iW * iY] = (float)(dSumU / iC);
							}
						});
					}




					#endregion

					GDAL.Driver dvGtiff = GDAL.Gdal.GetDriverByName("GTiff");
					#region "出力①: 傾斜量"
					if (chkGrad.Checked) {
						string sOutF = Path.Combine(Path.GetDirectoryName(txtInFile.Text), string.Format("{0}_grad.tiff", Path.GetFileNameWithoutExtension(txtInFile.Text)));
						using (GDAL.Dataset dsOut = dvGtiff.Create(sOutF, iW, iH, 1, GDAL.DataType.GDT_Float32, new string[] { "COMPRESS=DEFLATE", "PREDICTOR=2" })) {
							double[] dGeo = new double[6];
							dsIn.GetGeoTransform(dGeo);
							dsOut.SetGeoTransform(dGeo);
							dsOut.SetProjection(dsIn.GetProjection());
							using (GDAL.Band bdOut = dsOut.GetRasterBand(1)) {
								bdOut.WriteRaster(0, 0, iW, iH, fGrad, iW, iH, 0, 0);
							}
						}
					}
					#endregion

					#region "出力②: 地上開度"
					if (chkOpu.Checked) {
						string sOutF = Path.Combine(Path.GetDirectoryName(txtInFile.Text), string.Format("{0}_opu.tiff", Path.GetFileNameWithoutExtension(txtInFile.Text)));
						using (GDAL.Dataset dsOut = dvGtiff.Create(sOutF, iW, iH, 1, GDAL.DataType.GDT_Float32, new string[] { "COMPRESS=DEFLATE", "PREDICTOR=2" })) {
							double[] dGeo = new double[6];
							dsIn.GetGeoTransform(dGeo);
							dsOut.SetGeoTransform(dGeo);
							dsOut.SetProjection(dsIn.GetProjection());
							using (GDAL.Band bdOut = dsOut.GetRasterBand(1)) {
								bdOut.WriteRaster(0, 0, iW, iH, fOpu, iW, iH, 0, 0);
							}
						}
					}
					#endregion

					#region "出力③: 陰影指標"
					if (chkRel.Checked) {
						float[] fRel = new float[fGrad.Length];
						for (int i = 0; i < fRel.Length; i++) {
							//傾斜と地上開度の乗算相当
							//参考: 結果色 = 基本色 * 合成色 / 255 →http://creator.dwango.co.jp/11568.html
							float fO = fOpu[i]; if (90f < fO) { fO = 90f; }
							float fG = 90f - fGrad[i];
							fRel[i] = 90f - (fG * fO / 90f);
						}

						string sOutF = Path.Combine(Path.GetDirectoryName(txtInFile.Text), string.Format("{0}_rel.tiff", Path.GetFileNameWithoutExtension(txtInFile.Text)));
						using (GDAL.Dataset dsOut = dvGtiff.Create(sOutF, iW, iH, 1, GDAL.DataType.GDT_Float32, new string[] { "COMPRESS=DEFLATE", "PREDICTOR=2" })) {
							double[] dGeo = new double[6];
							dsIn.GetGeoTransform(dGeo);
							dsOut.SetGeoTransform(dGeo);
							dsOut.SetProjection(dsIn.GetProjection());
							using (GDAL.Band bdOut = dsOut.GetRasterBand(1)) {
								bdOut.WriteRaster(0, 0, iW, iH, fRel, iW, iH, 0, 0);
							}
						}
					}
					#endregion

				}
				lsRes[0] = 4;
				e.Result = lsRes;

			}
			catch (Exception ex) {
				ErrMsg(ex.Message);
				prb.Value = prb.Minimum;
				cmdAnalyze.Enabled = true;
			}
		}

		private void txtR_Leave(object sender, EventArgs e) {
			double dR;
			if (!double.TryParse(txtR.Text, out dR)) {
				ErrMsg(string.Format("半径は数値で入力してください。\n入力値:{0}", txtR.Text));
				txtR.Text = "50";
			}
		}

		/// <summary>
		/// プログレスバーを進める
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void bgwWorking_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			prb.Value = e.ProgressPercentage;
		}


		/// <summary>
		/// 処理完了時
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void bgwWorking_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			try {
				List<object> lsRes = (List<object>)e.Result;
				int iRes = (int)lsRes[0];
				if (iRes < 0) { ErrMsg("ユーザーによりキャンセルされました。"); return; } //キャンセル(-1)orエラー(-100)
				else  {
					prb.Style = ProgressBarStyle.Continuous;
					bgwWorking.DoWork -= bgwHandler;
					prb.Value = prb.Minimum;
					MessageBox.Show("処理が完了しました", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}
			}
			catch (Exception ex) {
				ErrMsg(ex.Message);
				prb.Value = prb.Minimum;
			}
			finally {
				cmdAnalyze.Enabled = true;
			}

		}

		private void cmdStop_Click(object sender, EventArgs e) {
			bgwWorking.CancelAsync();
		}
	}

	#region "動作設定(GDALパス・CPU優先度)"
	public class cSetting {
		const string sXml = "Dem2Relief.xml";
		string _gdalPath;
		ProcessPriorityClass _priority;
		public string GdalPath { get { return (_gdalPath); } set { _gdalPath = value; } }
		public ProcessPriorityClass Priority { get { return (_priority); } set { _priority = value; } }

		#region "コンストラクタ"
		public cSetting() { _gdalPath = ""; _priority = ProcessPriorityClass.Normal; }
		public cSetting(string GdalPath, ProcessPriorityClass Priority) { _gdalPath = GdalPath; _priority = Priority; }
		#endregion

		public void ReadXML() {
			try {
				Sel.XmlSerializer xlSel = new Sel.XmlSerializer(typeof(cSetting));
				using (StreamReader sr = new StreamReader(sXml, Encoding.UTF8)) {
					cSetting cSet;
					cSet = (cSetting)xlSel.Deserialize(sr);
					_gdalPath = cSet._gdalPath;
					_priority = cSet._priority;
				}
				if (_gdalPath == "") { _gdalPath = @"C:\Program Files\GDAL"; }
			}
			catch (Exception ex) {
				System.Diagnostics.Debug.Print(ex.Message);
				//XMLの読み取りに失敗したらとりあえず標準設定する
				_gdalPath = @"C:\Program Files\GDAL";
				_priority = ProcessPriorityClass.Normal;
			}
		}
		public void SaveXML() {
			Sel.XmlSerializer sel = new Sel.XmlSerializer(typeof(cSetting));
			using (StreamWriter sw = new StreamWriter(sXml, false, Encoding.UTF8)) {
				sel.Serialize(sw, this);
			}
		}
	}
	#endregion

}
