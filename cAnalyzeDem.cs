using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GDAL = OSGeo.GDAL;
using OGR = OSGeo.OGR;
using OSR = OSGeo.OSR;

namespace DEM
{
	public class cAnalyzeDem {
		const string sGdalDir = @"C:\Program Files\GDAL";

		/// <summary>
		/// 座標の単位(M:平面直角座標、Degree:緯度経度)
		/// </summary>
		public enum Unit { M = 1, Degree = 2 };

		#region "傾斜量"
		/// <summary>
		/// 傾斜量を計算する
		/// </summary>
		/// <param name="ObjDataset">対象とするデータセット</param>
		/// <param name="BandNo">対象とするバンド</param>
		/// <param name="AxisUnit">座標系の単位(平面直角座標/緯度経度)</param>
		/// <param name="OGC_WKT">測地系の定義</param>
		/// <returns>傾斜量の配列</returns>
		public static double[] GetGradient(GDAL.Dataset ObjDataset, int BandNo, Unit AxisUnit, string OGC_WKT) {
			//先に帰り値
			double[] dGrad = new double[ObjDataset.RasterXSize * ObjDataset.RasterYSize]; for (int i = 0; i < dGrad.Length; i++) { dGrad[i] = double.NaN; }

			//神谷ほか(1999) 傾斜量図の作成とその応用，情報地質，Vol.10，No.2，pp.76-79． 
			using (GDAL.Band bd = ObjDataset.GetRasterBand(BandNo)) {
				//1ピクセル当たりの距離
				double[] dX, dY;
				GetPixcelDist(ObjDataset, AxisUnit, OGC_WKT, out dX, out dY);

				//標高取得
				double[] dZ = new double[ObjDataset.RasterXSize * ObjDataset.RasterYSize];
				bd.ReadRaster(0, 0, ObjDataset.RasterXSize, ObjDataset.RasterYSize, dZ, ObjDataset.RasterXSize, ObjDataset.RasterYSize, 0, 0);

				//ラスタの幅
				int iW = ObjDataset.RasterXSize;

				for (int iY = 1; iY < ObjDataset.RasterYSize - 1; iY++) {
					for (int iX = 1; iX < iW - 1; iX++) {
						//変数名は神谷ほか(1999)
						double dH11 = dZ[iX - 1 + iW * (iY + 1)]; double dH12 = dZ[iX + iW * (iY + 1)]; double dH13 = dZ[iX + 1 + iW * (iY + 1)];
						double dH21 = dZ[iX - 1 + iW * iY]; double dH22 = dZ[iX + iW * iY]; double dH23 = dZ[iX + 1 + iW * iY];
						double dH31 = dZ[iX - 1 + iW * (iY - 1)]; double dH32 = dZ[iX + iW * (iY - 1)]; double dH33 = dZ[iX + 1 + iW * (iY - 1)];
						double dSx = (dH11 + dH21 + dH31 - (dH13 + dH23 + dH33)) / (6 * dX[iY]);
						double dSy = (dH11 + dH12 + dH13 - (dH31 + dH32 + dH33)) / (6 * dY[iY]);
						dGrad[iX + iW * iY] = Math.Atan(Math.Sqrt(dSx * dSx + dSy * dSy)) * 180d / Math.PI;
					}
				}
			}
			return (dGrad);
		}
		public static double[] GetGradient(GDAL.Dataset ObjDataset, int BandNo, Unit AxisUnit) {
			string sWkt = ObjDataset.GetProjectionRef();
			return (GetGradient(ObjDataset, BandNo, AxisUnit, sWkt));
		}
		#endregion

		#region "地上開度"
		/// <summary>
		/// 地上開度を計算する
		/// </summary>
		/// <param name="ObjDataset">対象とするデータセット</param>
		/// <param name="BandNo">対象とするバンド</param>
		/// <param name="AxisUnit">座標系の単位(平面直角座標/緯度経度)</param>
		/// <param name="Radius">対象半径(座標系に関わらず単位はm)</param>
		/// <returns>地上開度の配列</returns>
		public static double[] GetOpenUp(GDAL.Dataset ObjDataset, int BandNo, Unit AxisUnit, double Radius) {
			string sWkt = ObjDataset.GetProjectionRef();
			return (GetOpenUp(ObjDataset, BandNo, AxisUnit, sWkt, Radius));
		}

		/// <summary>
		/// 地上開度を計算する
		/// </summary>
		/// <param name="ObjDataset">対象とするデータセット</param>
		/// <param name="BandNo">対象とするバンド</param>
		/// <param name="AxisUnit">座標系の単位(平面直角座標/緯度経度)</param>
		/// <param name="OGC_WKT">測地系の定義</param>
		/// <param name="Radius">対象半径(座標系に関わらず単位はm)</param>
		/// <returns>地上開度の配列</returns>
		public static double[] GetOpenUp(GDAL.Dataset ObjDataset, int BandNo, Unit AxisUnit, string OGC_WKT, double Radius) {
			//横山ほか(1999) 開度による地形特徴の表示，写真測量とリモートセンシング，38(4)，26-34．定義Ⅱによる地上開度
			//先に帰り値
			double[] dOpenU = new double[ObjDataset.RasterXSize * ObjDataset.RasterYSize]; for (int i = 0; i < dOpenU.Length; i++) { dOpenU[i] = double.NaN; }

			//X・Yの移動量(0:真北、1:北東、2:東・・・7:北西)
			int[] iSx = new int[] { 0, 1, 1, 1, 0, -1, -1, -1 };
			int[] iSy = new int[] { 1, 1, 0, -1, -1, -1, 0, 1 };

			//横山ほか(1999) 開度による地形特徴の表示，写真測量とリモートセンシング，38(4)，26-34．定義Ⅱによる地上開度
			using (GDAL.Band bd = ObjDataset.GetRasterBand(BandNo)) {
				//1ピクセル当たりの距離
				double[] dX, dY;
				GetPixcelDist(ObjDataset, AxisUnit, OGC_WKT, out dX, out dY);

				//標高取得
				double[] dZ = new double[ObjDataset.RasterXSize * ObjDataset.RasterYSize];
				bd.ReadRaster(0, 0, ObjDataset.RasterXSize, ObjDataset.RasterYSize, dZ, ObjDataset.RasterXSize, ObjDataset.RasterYSize, 0, 0);

				//ラスタの幅
				int iW = ObjDataset.RasterXSize;

				for (int iY = 1; iY < ObjDataset.RasterYSize - 1; iY++) {
					for (int iX = 1; iX < iW - 1; iX++) {
						double dZ0 = dZ[iX + iW * iY];

						double[] dOp = new double[8];
						double dSum = 0d; int iC = 0;
						//方位別に地上開度を求める
						for (int i = 0; i < dOp.Length; i++) {
							dOp[i] = 181d; //ありえない値
							double dL = 0d;
							int iS = 1;
							while (dL <= Radius) {
								dL = Math.Sqrt(Math.Pow(iS * iSx[i] * dX[iY], 2) + Math.Pow(iS * iSy[i] * dY[iY], 2));
								if (Radius < dL) { break; }
								int j = iX + iS * iSx[i] + iW * (iY + (iS * iSy[i]));
								if (j < 0 || dZ.Length <= j) { break; }
								double dZ2 = dZ[j];
								double dDz = dZ2 - dZ0;
								double dAng = 180d * Math.Atan(dDz / dL) / Math.PI;
								dAng = 90 - dAng;
								if (dAng < dOp[i]) { dOp[i] = dAng; }
								iS++;
							}
							if (dOp[i] < 180d) { dSum += dOp[i]; iC++; }
						}

						//メッシュの地上開度
						if (0 < iC) { dOpenU[iX + iW * iY] = dSum / iC; }
					}
				}
			}
			return (dOpenU);
		}

		/// <summary>
		/// 地上開度を計算する(並列処理)
		/// </summary>
		/// <param name="ObjDataset">対象とするデータセット</param>
		/// <param name="BandNo">対象とするバンド</param>
		/// <param name="AxisUnit">座標系の単位(平面直角座標/緯度経度)</param>
		/// <param name="OGC_WKT">測地系の定義</param>
		/// <param name="Radius">対象半径(座標系に関わらず単位はm)</param>
		/// <returns>地上開度の配列</returns>
		public static double[] GetOpenUpP(GDAL.Dataset ObjDataset, int BandNo, Unit AxisUnit, string OGC_WKT, double Radius, out double[] OpenDn) {
			//横山ほか(1999) 開度による地形特徴の表示，写真測量とリモートセンシング，38(4)，26-34．定義Ⅱによる地上開度

			//先に帰り値
			double[] dOpenU = new double[ObjDataset.RasterXSize * ObjDataset.RasterYSize];
			double[] dOpD = new double[ObjDataset.RasterXSize * ObjDataset.RasterYSize];
			for (int i = 0; i < dOpenU.Length; i++) { dOpenU[i] = double.NaN; dOpD[i] = double.NaN; }

			//X・Yの移動量(0:真北、1:北東、2:東・・・7:北西)
			int[] iSx = new int[] { 0, 1, 1, 1, 0, -1, -1, -1 };
			int[] iSy = new int[] { 1, 1, 0, -1, -1, -1, 0, 1 };

			//横山ほか(1999) 開度による地形特徴の表示，写真測量とリモートセンシング，38(4)，26-34．定義Ⅱによる地上開度
			using (GDAL.Band bd = ObjDataset.GetRasterBand(BandNo)) {
				//1ピクセル当たりの距離
				double[] dX, dY;
				GetPixcelDist(ObjDataset, AxisUnit, OGC_WKT, out dX, out dY);

				//標高取得
				double[] dZ = new double[ObjDataset.RasterXSize * ObjDataset.RasterYSize];
				bd.ReadRaster(0, 0, ObjDataset.RasterXSize, ObjDataset.RasterYSize, dZ, ObjDataset.RasterXSize, ObjDataset.RasterYSize, 0, 0);

				//ラスタの幅
				int iW = ObjDataset.RasterXSize;

				for (int iY = 1; iY < ObjDataset.RasterYSize - 1; iY++) {
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
							while (dL <= Radius) {
									  //仰角＆方位別開度
									  dL = Math.Sqrt(Math.Pow(iS * iSx[i] * dX[iY], 2) + Math.Pow(iS * iSy[i] * dY[iY], 2));
								if (Radius < dL) { break; }
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
							  if (0 < iC) { dOpenU[iX + iW * iY] = dSumU / iC; dOpD[iX + iW * iY] = dSumD / iC; }
					});
				}
			}
			OpenDn = new double[dOpD.Length];
			for (int i = 0; i < dOpD.Length; i++) { OpenDn[i] = dOpD[i]; }
			return (dOpenU);
		}

		/// <summary>
		/// 地上開度(8方位最低)を計算する(並列処理)
		/// </summary>
		/// <param name="ObjDataset">対象とするデータセット</param>
		/// <param name="BandNo">対象とするバンド</param>
		/// <param name="AxisUnit">座標系の単位(平面直角座標/緯度経度)</param>
		/// <param name="OGC_WKT">測地系の定義</param>
		/// <param name="Radius">対象半径(座標系に関わらず単位はm)</param>
		/// <returns>地上開度の配列</returns>
		public static double[] GetOpenUpMinP(GDAL.Dataset ObjDataset, int BandNo, Unit AxisUnit, string OGC_WKT, double Radius, out double[] OpenDn) {
			//横山ほか(1999) 開度による地形特徴の表示，写真測量とリモートセンシング，38(4)，26-34．定義Ⅱによる地上開度
			//先に帰り値
			double[] dOpenU = new double[ObjDataset.RasterXSize * ObjDataset.RasterYSize];
			double[] dOpD = new double[ObjDataset.RasterXSize * ObjDataset.RasterYSize];
			for (int i = 0; i < dOpenU.Length; i++) { dOpenU[i] = double.NaN; dOpD[i] = double.NaN; }

			//X・Yの移動量(0:真北、1:北東、2:東・・・7:北西)
			int[] iSx = new int[] { 0, 1, 1, 1, 0, -1, -1, -1 };
			int[] iSy = new int[] { 1, 1, 0, -1, -1, -1, 0, 1 };

			//横山ほか(1999) 開度による地形特徴の表示，写真測量とリモートセンシング，38(4)，26-34．定義Ⅱによる地上開度
			using (GDAL.Band bd = ObjDataset.GetRasterBand(BandNo)) {
				//1ピクセル当たりの距離
				double[] dX, dY;
				GetPixcelDist(ObjDataset, AxisUnit, OGC_WKT, out dX, out dY);

				//標高取得
				double[] dZ = new double[ObjDataset.RasterXSize * ObjDataset.RasterYSize];
				bd.ReadRaster(0, 0, ObjDataset.RasterXSize, ObjDataset.RasterYSize, dZ, ObjDataset.RasterXSize, ObjDataset.RasterYSize, 0, 0);

				//ラスタの幅
				int iW = ObjDataset.RasterXSize;

				for (int iY = 1; iY < ObjDataset.RasterYSize - 1; iY++) {
					Parallel.For(1, iW - 1, iX =>//for (int iX = 1; iX < iW - 1; iX++)
					{
						double dZ0 = dZ[iX + iW * iY];

						double dOpUm = double.MaxValue;
						double dOpDm = double.MaxValue;
							  //方位別に地上開度を求める
							  for (int i = 0; i < iSx.Length; i++) {
							double dL = 0d;
							int iS = 1;
							while (dL <= Radius) {
									  //仰角＆方位別開度
									  dL = Math.Sqrt(Math.Pow(iS * iSx[i] * dX[iY], 2) + Math.Pow(iS * iSy[i] * dY[iY], 2));
								if (Radius < dL) { break; }
								int j = iX + iS * iSx[i] + iW * (iY + (iS * iSy[i]));
								if (j < 0 || dZ.Length <= j) { break; }
								double dZ2 = dZ[j];
								double dDz = dZ2 - dZ0;
								double dAng = 180d * Math.Atan(dDz / dL) / Math.PI; //仰角
									  if ((90 - dAng) < dOpUm) { dOpUm = 90 - dAng; } //地上開度
									  if ((90 + dAng) < dOpDm) { dOpDm = 90 + dAng; } //地下開度
									  iS++;
							}
						}
						dOpenU[iX + iW * iY] = dOpUm; dOpD[iX + iW * iY] = dOpDm;
					});
				}
			}
			OpenDn = new double[dOpD.Length];
			for (int i = 0; i < dOpD.Length; i++) { OpenDn[i] = dOpD[i]; }
			return (dOpenU);
		}



		#endregion

		#region "範囲内正規化標高"
		/// <summary>
		/// 範囲内規格化標高を計算する
		/// </summary>
		/// <param name="ObjDataset">対象とするデータセット</param>
		/// <param name="BandNo">対象とするバンド</param>
		/// <param name="AxisUnit">座標系の単位(平面直角座標/緯度経度)</param>
		/// <param name="OGC_WKT">測地系の定義</param>
		/// <param name="Radius">対象半径(座標系に関わらず単位はm)</param>
		/// <returns>範囲内規格化標高の配列</returns>
		public static double[] GetNormalized(GDAL.Dataset ObjDataset, int BandNo, Unit AxisUnit, double Radius) {
			string sWkt = ObjDataset.GetProjectionRef();
			return (GetNormalized(ObjDataset, BandNo, AxisUnit, sWkt, Radius));
		}

		/// <summary>
		/// 範囲内規格化標高を計算する
		/// </summary>
		/// <param name="ObjDataset">対象とするデータセット</param>
		/// <param name="BandNo">対象とするバンド</param>
		/// <param name="AxisUnit">座標系の単位(平面直角座標/緯度経度)</param>
		/// <param name="OGC_WKT">測地系の定義</param>
		/// <param name="Radius">対象半径(座標系に関わらず単位はm)</param>
		/// <returns>地上開度の配列</returns>
		public static double[] GetNormalized(GDAL.Dataset ObjDataset, int BandNo, Unit AxisUnit, string OGC_WKT, double Radius) {
			//対象範囲内での規格化標高(半径Radius内のメッシュの標高の平均を引いて標準偏差で割る)
			//先に帰り値
			double[] dNormZ = new double[ObjDataset.RasterXSize * ObjDataset.RasterYSize]; for (int i = 0; i < dNormZ.Length; i++) { dNormZ[i] = double.NaN; }

			using (GDAL.Band bd = ObjDataset.GetRasterBand(BandNo)) {
				//1ピクセル当たりの距離
				double[] dX, dY;
				GetPixcelDist(ObjDataset, AxisUnit, OGC_WKT, out dX, out dY);

				//標高取得
				double[] dZ = new double[ObjDataset.RasterXSize * ObjDataset.RasterYSize];
				bd.ReadRaster(0, 0, ObjDataset.RasterXSize, ObjDataset.RasterYSize, dZ, ObjDataset.RasterXSize, ObjDataset.RasterYSize, 0, 0);

				//ラスタの幅
				int iW = ObjDataset.RasterXSize;

				for (int iY = 1; iY < ObjDataset.RasterYSize - 1; iY++) {
					for (int iX = 1; iX < iW - 1; iX++) {
						double dZ0 = dZ[iX + iW * iY]; //対象メッシュの標高
						int iRx = (int)(Radius / dX[iY]) + 1;
						int iRy = (int)(Radius / dX[iY]) + 1;
						List<double> lsZ = new List<double>();
						for (int iSy = -iRy; iSy <= iRy; iSy++) {
							for (int iSx = -iRx; iSx <= iRx; iSx++) {
								double dD = Math.Sqrt((dX[iY] * iSx) * (dX[iY] * iSx) + (dY[iY] * iSy) * (dY[iY] * iSy));
								int j = (iX + iSx) + iW * (iY + iSy);
								if (Radius < dD || j < 0 || dZ.Length <= j) { continue; }
								lsZ.Add(dZ[j]);
							}
						}
						if (lsZ.Count < 2) { dNormZ[iX + iW * iY] = 0d; }
						else {
							double dMean = lsZ.Sum() / lsZ.Count;
							double dSd = 0d;
							for (int i = 0; i < lsZ.Count; i++) { dSd += (lsZ[i] - dMean) * (lsZ[i] - dMean); }
							dSd = Math.Sqrt(dSd / (lsZ.Count - 1));
							if (dSd == 0d) { dNormZ[iX + iW * iY] = 0d; }
							else { dNormZ[iX + iW * iY] = (dZ0 - dMean) / dSd; }
						}

					}
				}
			}
			return (dNormZ);
		}


		/// <summary>
		/// 範囲内規格化標高を計算する(並列処理)
		/// </summary>
		/// <param name="ObjDataset">対象とするデータセット</param>
		/// <param name="BandNo">対象とするバンド</param>
		/// <param name="AxisUnit">座標系の単位(平面直角座標/緯度経度)</param>
		/// <param name="OGC_WKT">測地系の定義</param>
		/// <param name="Radius">対象半径(座標系に関わらず単位はm)</param>
		/// <returns>地上開度の配列</returns>
		public static double[] GetNormalizedP(GDAL.Dataset ObjDataset, int BandNo, Unit AxisUnit, string OGC_WKT, double Radius) {
			//対象範囲内での規格化標高(半径Radius内のメッシュの標高の平均を引いて標準偏差で割る)
			//先に帰り値
			double[] dNormZ = new double[ObjDataset.RasterXSize * ObjDataset.RasterYSize]; for (int i = 0; i < dNormZ.Length; i++) { dNormZ[i] = double.NaN; }

			using (GDAL.Band bd = ObjDataset.GetRasterBand(BandNo)) {
				//1ピクセル当たりの距離
				double[] dX, dY;
				GetPixcelDist(ObjDataset, AxisUnit, OGC_WKT, out dX, out dY);

				//標高取得
				double[] dZ = new double[ObjDataset.RasterXSize * ObjDataset.RasterYSize];
				bd.ReadRaster(0, 0, ObjDataset.RasterXSize, ObjDataset.RasterYSize, dZ, ObjDataset.RasterXSize, ObjDataset.RasterYSize, 0, 0);

				//ラスタの幅
				int iW = ObjDataset.RasterXSize;

				for (int iY = 1; iY < ObjDataset.RasterYSize - 1; iY++) {
					//Console.WriteLine("{0}/{1}", iY, ObjDataset.RasterYSize - 1);
					Parallel.For(1, iW - 1, iX =>//for (int iX = 1; iX < iW - 1; iX++)
					{
						double dZ0 = dZ[iX + iW * iY]; //対象メッシュの標高
							  int iRx = (int)(Radius / dX[iY]) + 1;
						int iRy = (int)(Radius / dX[iY]) + 1;
						List<double> lsZ = new List<double>();
						for (int iSy = -iRy; iSy <= iRy; iSy++) {
							for (int iSx = -iRx; iSx <= iRx; iSx++) {
								double dD = Math.Sqrt((dX[iY] * iSx) * (dX[iY] * iSx) + (dY[iY] * iSy) * (dY[iY] * iSy));
								int j = (iX + iSx) + iW * (iY + iSy);
								if (Radius < dD || j < 0 || dZ.Length <= j) { continue; }
								lsZ.Add(dZ[j]);
							}
						}
						if (lsZ.Count < 2) { dNormZ[iX + iW * iY] = 0d; }
						else {
							double dMean = lsZ.Sum() / lsZ.Count;
							double dSd = 0d;
							for (int i = 0; i < lsZ.Count; i++) { dSd += (lsZ[i] - dMean) * (lsZ[i] - dMean); }
							dSd = Math.Sqrt(dSd / (lsZ.Count - 1));
							if (dSd == 0d) { dNormZ[iX + iW * iY] = 0d; }
							else { dNormZ[iX + iW * iY] = (dZ0 - dMean) / dSd; }
						}
					});
				}
			}
			return (dNormZ);
		}

		/// <summary>
		/// 平均＆標準偏差も出力する(並列処理)
		/// </summary>
		/// <param name="ObjDataset">対象とするデータセット</param>
		/// <param name="BandNo">対象とするバンド</param>
		/// <param name="AxisUnit">座標系の単位(平面直角座標/緯度経度)</param>
		/// <param name="OGC_WKT">測地系の定義</param>
		/// <param name="Radius">対象半径(座標系に関わらず単位はm)</param>
		/// <returns>正規化標高の配列</returns>
		public static double[] GetNormalizedP(GDAL.Dataset ObjDataset, int BandNo, Unit AxisUnit, string OGC_WKT, double Radius, out double[] Mean, out double[] SD) {
			//対象範囲内での規格化標高(半径Radius内のメッシュの標高の平均を引いて標準偏差で割る)
			//先に帰り値
			double[] dNormZ = new double[ObjDataset.RasterXSize * ObjDataset.RasterYSize];
			Mean = new double[dNormZ.Length]; SD = new double[dNormZ.Length];
			double[] dM = new double[Mean.Length]; //並列計算用
			double[] dS = new double[SD.Length];
			for (int i = 0; i < dNormZ.Length; i++) { dNormZ[i] = double.NaN; dM[i] = double.NaN; dS[i] = double.NaN; }

			using (GDAL.Band bd = ObjDataset.GetRasterBand(BandNo)) {
				//1ピクセル当たりの距離
				double[] dX, dY;
				GetPixcelDist(ObjDataset, AxisUnit, OGC_WKT, out dX, out dY);

				//標高取得
				double[] dZ = new double[ObjDataset.RasterXSize * ObjDataset.RasterYSize];
				bd.ReadRaster(0, 0, ObjDataset.RasterXSize, ObjDataset.RasterYSize, dZ, ObjDataset.RasterXSize, ObjDataset.RasterYSize, 0, 0);

				//ラスタの幅
				int iW = ObjDataset.RasterXSize;

				for (int iY = 1; iY < ObjDataset.RasterYSize - 1; iY++) {
					Console.WriteLine("{0}/{1}", iY, ObjDataset.RasterYSize - 1);
					Parallel.For(1, iW - 1, iX =>//for (int iX = 1; iX < iW - 1; iX++)
					{
						double dZ0 = dZ[iX + iW * iY]; //対象メッシュの標高
							  int iRx = (int)(Radius / dX[iY]) + 1;
						int iRy = (int)(Radius / dX[iY]) + 1;
						List<double> lsZ = new List<double>();
						for (int iSy = -iRy; iSy <= iRy; iSy++) {
							for (int iSx = -iRx; iSx <= iRx; iSx++) {
								double dD = Math.Sqrt((dX[iY] * iSx) * (dX[iY] * iSx) + (dY[iY] * iSy) * (dY[iY] * iSy));
								int j = (iX + iSx) + iW * (iY + iSy);
								if (Radius < dD || j < 0 || dZ.Length <= j) { continue; }
								if (!double.IsNaN(dZ[j])) { lsZ.Add(dZ[j]); }
							}
						}

						if (lsZ.Count == 0) { dNormZ[iX + iW * iY] = double.NaN; dM[iX + iW * iY] = double.NaN; dS[iX + iW * iY] = double.NaN; }
						else if (lsZ.Count < 2) { dNormZ[iX + iW * iY] = 0d; dM[iX + iW * iY] = lsZ[0]; dS[iX + iW * iY] = 0d; }
						else {
							double dMean = lsZ.Sum() / lsZ.Count;
							double dSd = 0d;
							for (int i = 0; i < lsZ.Count; i++) { dSd += (lsZ[i] - dMean) * (lsZ[i] - dMean); }
							dSd = Math.Sqrt(dSd / (lsZ.Count - 1));

							dM[iX + iW * iY] = dMean;
							dS[iX + iW * iY] = dSd;

							if (dSd == 0d) { dNormZ[iX + iW * iY] = 0d; }
							else { dNormZ[iX + iW * iY] = (dZ0 - dMean) / dSd; }
						}
					});
				}
				for (int i = 0; i < dM.Length; i++) {
					Mean[i] = dM[i];
					SD[i] = dS[i];
				}

			}
			return (dNormZ);
		}


		#endregion

		#region "1ピクセル当たりの距離[m]"
		/// <summary>
		/// 1ピクセル当たりの距離を計算する(DataSetに座標系の定義がある場合)
		/// </summary>
		/// <param name="ObjDataset">対象とするデータセット(GDAL)</param>
		/// <param name="AxisUnit">座標系の単位(平面直角座標/緯度経度)</param>
		/// <param name="dX">1ピクセル当たりの距離(東西)[RasterYSize]</param>
		/// <param name="dY">1ピクセル当たりの距離(南北)[RasterYSize]</param>
		public static void GetPixcelDist(GDAL.Dataset ObjDataset, Unit AxisUnit, out double[] dX, out double[] dY) {
			string sPrj = ObjDataset.GetProjectionRef();
			GetPixcelDist(ObjDataset, AxisUnit, out dX, out dY);
		}

		/// <summary>
		/// 1ピクセル当たりの距離を計算する
		/// </summary>
		/// <param name="ObjDataset">対象とするデータセット(GDAL)</param>
		/// <param name="AxisUnit">座標系の単位(平面直角座標/緯度経度)</param>
		/// <param name="dX">1ピクセル当たりの距離(東西)[RasterYSize]</param>
		/// <param name="dY">1ピクセル当たりの距離(南北)[RasterYSize]</param>
		public static void GetPixcelDist(GDAL.Dataset ObjDataset, Unit AxisUnit, string OGC_WKT, out double[] dX, out double[] dY) {
			double[] dGeo = new double[6];
			ObjDataset.GetGeoTransform(dGeo);
			double dXmin = dGeo[0];
			double ddX = dGeo[1];
			double dYmin = dGeo[3];
			double ddY = dGeo[5];
			int iW = ObjDataset.RasterXSize;
			int iH = ObjDataset.RasterYSize;

			dX = new double[ObjDataset.RasterYSize];
			dY = new double[ObjDataset.RasterYSize];

			if (AxisUnit == Unit.M) {
				//平面直角座標系ならそのまま返す
				for (int i = 0; i < ObjDataset.RasterYSize; i++) {
					dX[i] = ddX; dY[i] = ddY;
				}
			}
			else {
				#region "緯度経度の場合→当該ピクセルの幅を返す"
				//赤道半径・離心率(二乗)を取得
				OSR.SpatialReference rf = new OSR.SpatialReference(OGC_WKT);
				double Rx = rf.GetSemiMajor(); //長半径
				double F = 1 / rf.GetInvFlattening();
				double E2 = Math.Pow(F * (2d - F), 2d);
				for (int i = 0; i < ObjDataset.RasterYSize; i++) {
					double dLon = dXmin + ddX * ObjDataset.RasterXSize / 2; //一応中心的な位置とする
					double dLat = dYmin + ddY * i;
					double dA, dD;
					GetDistAngFromLatLon(dLon - ddX, dLat, dLon + ddX, dLat, Rx, E2, out dA, out dD);
					dX[i] = dD / 2d;

					GetDistAngFromLatLon(dLon, dLat - ddY, dLon, dLat + ddY, Rx, E2, out dA, out dD);
					dY[i] = dD / 2d;
				}
				#endregion
			}
		}
		#endregion

		#region "ヒュベニの公式"
		/// <summary>
		/// 緯度経度+距離・方位角から緯度経度を取得する
		/// </summary>
		/// <param name="dLon0">起点の経度</param>
		/// <param name="dLat0">起点の緯度</param>
		/// <param name="dDist">距離(m)</param>
		/// <param name="dAng">方位角</param>
		/// <param name="dLon">目的地の経度</param>
		/// <param name="dLat">目的地の緯度</param>
		static void GetLatLonFromAngDist(double dLon0, double dLat0, double dDist, double dAng, double Rx, double E2, out double dLon, out double dLat) {
			double WT = Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(dLat0 * Math.PI / 180), 2)); //仮のW（第１近似）
			double MT = Rx * (1 - E2) / Math.Pow(WT, 3); //仮のM（第１近似）
			double diT = dDist * Math.Cos(dAng * Math.PI / 180) / MT; //仮のdi（第１近似）
			double i = dLat0 * Math.PI / 180 + diT / 2;

			double W = Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(i), 2));
			double M = Rx * (1 - E2) / Math.Pow(W, 3);
			double N = Rx / W;
			double di = dDist * Math.Cos(dAng * Math.PI / 180) / M;
			double dk = dDist * Math.Sin(dAng * Math.PI / 180) / (N * Math.Cos(i));

			dLat = dLat0 + di * 180 / Math.PI;
			dLon = dLon0 + dk * 180 / Math.PI;
		}

		/// <summary>
		/// ヒュベニの公式を用いて二点の緯度経度から角度と距離を取得する
		/// </summary>
		/// <param name="dLon0">原点経度</param>
		/// <param name="dLat0">原点緯度</param>
		/// <param name="dLon">目的地経度</param>
		/// <param name="dLat">目的地緯度</param>
		/// <param name="dAng">方位角</param>
		/// <param name="dDist">距離</param>
		static void GetDistAngFromLatLon(double dLon0, double dLat0, double dLon, double dLat, double Rx, double E2, out double dAng, out double dDist) {
			//dLat0：出発点緯度（度）
			//dLon0：出発点経度（度）
			//dLat：到達点緯度（度）
			//k2：到達点経度（度）
			//Dist：２点の距離（ｍ）
			double dLatDiff = dLat - dLat0;
			double dLonDiff = dLon - dLon0;
			double dI = (dLat0 + dLat) / 2;

			double dW = Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(dI * Math.PI / 180), 2));
			double dM = Rx * (1 - E2) / Math.Pow(dW, 3);
			double dN = Rx / dW;

			dDist = Math.Sqrt(Math.Pow((dLatDiff * Math.PI / 180 * dM), 2) + Math.Pow((dLonDiff * Math.PI / 180 * dN * Math.Cos(dI * Math.PI / 180)), 2));


			double ddi = dLatDiff * Math.PI / 180 * dM;
			double ddk = dLonDiff * Math.PI / 180 * dN * Math.Cos(dI * Math.PI / 180);

			dAng = (Math.Atan2(ddi, ddk) * 180 / Math.PI + 360) - 90;
			dAng = 360 - dAng;
			if (dAng >= 360) dAng = dAng - 360;
			if (dAng < 0) dAng = dAng + 360;
		}
		#endregion


		public static Unit GetUnit(string OGC_WKT) {
			OSR.SpatialReference rf = new OSGeo.OSR.SpatialReference(OGC_WKT);
			string sUnit = rf.GetAttrValue("UNIT", 0).ToLower();
			if (sUnit == "degree") { return (Unit.Degree); }
			else { return (Unit.M); }
		}

		public static void InitGdal() {
			string sVal;

			//PATH
			sVal = Environment.GetEnvironmentVariable("PATH") + ";" +
			sGdalDir + ";" +
			Path.Combine(sGdalDir, "csharp") + ";" +
			Path.Combine(sGdalDir, "gdal-data") + ";" +
			Path.Combine(sGdalDir, "gdalplugins") + ";" +
			Path.Combine(sGdalDir, "projlib");
			Environment.SetEnvironmentVariable("PATH", sVal);

			//GDAL_DATA
			Environment.SetEnvironmentVariable("GDAL_DATA", Path.Combine(sGdalDir, "gdal-data"));
			GDAL.Gdal.SetConfigOption("GDAL_DATA", Path.Combine(sGdalDir, "gdal-data"));

			//GDAL_DRIVER(Plugins)
			Environment.SetEnvironmentVariable("GDAL_DRIVER_PATH", Path.Combine(sGdalDir, "gdalplugins"));
			GDAL.Gdal.SetConfigOption("GDAL_DRIVER_PATH", Path.Combine(sGdalDir, "gdalplugins"));

			//PROJ_LIB
			Environment.SetEnvironmentVariable("PROJ_LIB", Path.Combine(sGdalDir, "projlib"));
			GDAL.Gdal.SetConfigOption("PROJ_LIB", Path.Combine(sGdalDir, "projlib"));

			GDAL.Gdal.SetConfigOption("GDAL_CACHEMAX", "100000");
			GDAL.Gdal.SetConfigOption("CPL_TMPDIR", @".\");

			GDAL.Gdal.AllRegister();
		}
	}
}
