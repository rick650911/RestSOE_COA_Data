using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSOE_COA_Data.model
{
    class CalMethod
    {
        string _Proxy;
        string _Url_Server;

        
        public CalMethod(string argProxy, string argUrl_Server)
        {
            _Proxy = argProxy;
            _Url_Server = argUrl_Server;
        }


        /// <summary>
        /// 利用座標、方位角、攝距，計算位置
        /// </summary>
        /// <param name="argLon"></param>
        /// <param name="argLat"></param>
        /// <param name="argDirection"></param>
        /// <param name="argDistance"></param>
        /// <returns></returns>
        public IPoint CalPosition(double argLon,double argLat,double argDirection,double argDistance)
        {
            IPoint point = new PointClass();
            point.SpatialReference = SpWGS84();
            point.X = argLon;
            point.Y = argLat;

            IPoint resultPoint = GetAngleDisPoint(point, argDirection, argDistance);

            return resultPoint;
        }

        private ISpatialReference SpWGS84()
        {
            ISpatialReferenceFactory spF = new SpatialReferenceEnvironmentClass();
            ISpatialReference spWGS84 = spF.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);

            return spWGS84;
        }

        /// <summary>
        /// (球體座標WGS84)利用座標、方位角、攝距，計算位置
        /// </summary>
        /// <param name="argBasePoint"></param>
        /// <param name="intAngle"></param>
        /// <param name="dblDis"></param>
        /// <returns></returns>
        private static IPoint GetAngleDisPoint(IPoint argBasePoint, double intAngle, double dblDis)
        {
            ILinearUnit linerUnit = ConvertUnitToLinearUnit(esriUnits.esriMeters);

            IPolyline polyline = new PolylineClass();
            IConstructGeodetic pGeoline = (IConstructGeodetic)polyline;

            pGeoline.ConstructGeodeticLineFromDistance(esriGeodeticType.esriGeodeticTypeGeodesic, argBasePoint, linerUnit, dblDis, intAngle, esriCurveDensifyMethod.esriCurveDensifyByLength, 0);

            IClone pClone = polyline.ToPoint as IClone;
            return pClone.Clone() as IPoint;
        }

        private static ILinearUnit ConvertUnitToLinearUnit(esriUnits esriunit)
        {
            Type factoryType = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
            System.Object obj = Activator.CreateInstance(factoryType);
            ISpatialReferenceFactory3 spatialReferenceFactory = obj as ISpatialReferenceFactory3;

            ILinearUnit linearUnitobj = null;
            switch (esriunit)
            {
                //case esriUnits.esriMiles:
                //    linearUnitobj = (ILinearUnit)spatialReferenceFactory.CreateUnit((int)esriSRUnitType.esriSRUnit_SurveyMile);
                //    break;
                //case esriUnits.esriNauticalMiles:
                //    linearUnitobj = (ILinearUnit)spatialReferenceFactory.CreateUnit((int)esriSRUnitType.esriSRUnit_NauticalMile);
                //    break;
                case esriUnits.esriMeters:
                    linearUnitobj = (ILinearUnit)spatialReferenceFactory.CreateUnit((int)esriSRUnitType.esriSRUnit_Meter);
                    break;
                case esriUnits.esriKilometers:
                    linearUnitobj = (ILinearUnit)spatialReferenceFactory.CreateUnit((int)esriSRUnitType.esriSRUnit_Kilometer);
                    break;

            }
            return linearUnitobj;
        }

        /// <summary>
        /// 查詢地籍址
        /// </summary>
        /// <param name="parmVer"></param>
        /// <param name="XY"></param>
        /// <param name="index"></param>
        /// <param name="argWKID"></param>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        public bool CalAddr(string parmVer, string XY, int index, int argWKID, out string strMsg)
        {
            strMsg = "";

            ConnectMethod connt = new ConnectMethod();
            string str;

            var url = this._Proxy + "?" + this._Url_Server + "CadastralMap/CadastralMap_Tiled_" + parmVer + "/MapServer/" + index + "/query";//
            string para = string.Format("geometry={0}&geometryType=esriGeometryPoint&spatialRel=esriSpatialRelIntersects&outFields=*&returnGeometry=false&f=pjson&inSR={1}",
                XY, argWKID);
            bool isConnt = connt.ClientRequest(url, para, out str);

            if (!isConnt)
            {
                strMsg = "地籍圖問題 \r\n" + str;
                return false;
            }

            JObject json = JObject.Parse(str);
            var features = json["features"];

            if (features.Count() == 0)
            {
                strMsg = "查無地籍資料!" ;
                return false;
            }

            foreach (var item in features)
            {
                strMsg = GetSectFromAttributes(item["attributes"]);//因為是點，理論上只有一組
            }

            return true;
        }

        public bool CalAddr(string parmVer, string XY, int index, int argWKID, out string strMsg, out string strCity,out string strTown)
        {
            strMsg = ""; strCity = ""; strTown = "";

            ConnectMethod connt = new ConnectMethod();
            string str;

            var url = this._Proxy + "?" + this._Url_Server + "CadastralMap/CadastralMap_Tiled_" + parmVer + "/MapServer/" + index + "/query";//
            string para = string.Format("geometry={0}&geometryType=esriGeometryPoint&spatialRel=esriSpatialRelIntersects&outFields=*&returnGeometry=false&f=pjson&inSR={1}",
                XY, argWKID);
            bool isConnt = connt.ClientRequest(url, para, out str);

            if (!isConnt)
            {
                strMsg = "地籍圖問題 \r\n" + str;
                return false;
            }

            JObject json = JObject.Parse(str);
            var features = json["features"];

            if (features.Count() == 0)
            {
                strMsg = "查無地籍資料!";
                return false;
            }

            foreach (var item in features)
            {
                strMsg = GetSectFromAttributes(item["attributes"]);//因為是點，理論上只有一組
                strCity = item["attributes"]["縣市"].ToString();
                strTown = item["attributes"]["鄉鎮"].ToString();
            }

            return true;
        }


        /// <summary>
        /// 欄位建置地籍 回傳欄位地籍
        /// </summary>
        /// <param name="jt"></param>
        /// <returns></returns>
        private string GetSectFromAttributes(JToken jt)
        {
            string str = "";

            try
            {
                //先處理子母地號
                int 宗地母號; int.TryParse(jt["宗地母號"].ToString(), out 宗地母號);
                int 宗地子號; int.TryParse(jt["宗地子號"].ToString(), out 宗地子號);
                string strNum = 宗地母號 + "";
                if (宗地子號 != 0)
                    strNum += "-" + 宗地子號;

                //str = string.Format("{0}{1}({2}){3}段{4}小段{5}地號",
                //jt["縣市"].ToString(), jt["鄉鎮"].ToString(), jt["段碼"].ToString(), jt["段"].ToString(), jt["小段"].ToString(), strNum);

                //
                string str小段 = string.IsNullOrEmpty(jt["小段"].ToString()) ? "" : jt["小段"].ToString()+ "小段";
                str = string.Format("{0}{1}{2}段{3}{4}地號",
                jt["縣市"].ToString(), jt["鄉鎮"].ToString(), jt["段"].ToString(), str小段, strNum);

            }
            catch (Exception)
            {
                str = "";
            }

            return str;
        }

        #region 檢核功能會放在排程程式

        public enum APP_OS { android, IOS }

        public double CalPitch(APP_OS argOS, XYZ argXYZ)
        {
            double rotation = 0;
            rotation = argXYZ.Z * 9;
            switch (argOS)
            {
                case APP_OS.android:

                    //可用xy值代表直拿與橫拿
                    if (Math.Abs(argXYZ.X) - Math.Abs(argXYZ.Y) > 1)
                    {
                        //代表橫拿，角度需加 90度
                        if (argXYZ.X > 0)
                            rotation += 90;
                        else
                            rotation -= 90;
                        //橫拿時，上下拍會影響角度，直拿不會，故針對橫拿做修正
                        if (argXYZ.Z < 0)
                            rotation += 180;
                    }
                    break;
                case APP_OS.IOS:
                    rotation = argXYZ.Z * 9;

                    break;
                default:
                    break;
            }
            return rotation;
        }

        #endregion

    }
}
