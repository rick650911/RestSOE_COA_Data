using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSOE_COA_Data.model
{
    class QueryMethod
    {
        /// <summary>
        /// 利用案件編號查詢相片品質屬性
        /// </summary>
        /// <param name="refResult"></param>
        /// <param name="argFC"></param>
        /// <param name="argCaseID"></param>
        public void QueryCaseIDValue(ref ResultImageQueryQuality refResult,IFeatureClass argFC,long argCaseID)
        {
            //
            IQueryFilter queryFilter = new QueryFilter();
            queryFilter.AddField("*");
            queryFilter.WhereClause = "CaseID=" + argCaseID;

            IFeatureCursor resultsFeatureCursor = argFC.Search(queryFilter, true);
            //

            //
            Type objType = typeof(ResultImageQueryQuality);
            IFeature resultsFeature = null;
            List<string> listFields = new List<string>()
            { "CadAddr", "Direction", "Pitchangle", "Brightness", "Quality", "Distance", "TargetScale" };
            while ((resultsFeature = resultsFeatureCursor.NextFeature()) != null)
            {
                foreach (var itemField in listFields)
                {
                    int classFieldIndex = argFC.FindField(itemField);

                    System.Reflection.PropertyInfo prop = objType.GetProperty(itemField.ToLower());


                    object temp = resultsFeature.get_Value(classFieldIndex);

                    if (prop.PropertyType == typeof(double))
                        temp = (double)temp;
                    prop.SetValue(refResult, temp, null);
                }

                IPoint point = resultsFeature.Shape as IPoint;
                refResult.gps = new double[] { point.X, point.Y }; 

                refResult.status = true;
            }

            if (refResult.status == false)
            {
                refResult.msg = "查無CaseID資料!";
            }
        }


        public void QueryCity(IFeatureClass argFC,ref List<Cluster> listCluster)
        {
            string where = "";
            foreach (var item in listCluster)
            {
                if (where != "") where += ',';
                where += string.Format("'{0}'", item.City);
            }
            string strField = "CountyName";
            //
            IQueryFilter queryFilter = new QueryFilter();
            queryFilter.AddField(strField);
            queryFilter.WhereClause = string.Format( "{0} in ({1})", strField, where);

            IFeatureCursor resultsFeatureCursor = argFC.Search(queryFilter, true);
            //

            //
            IFeature resultsFeature = null;

            while ((resultsFeature = resultsFeatureCursor.NextFeature()) != null)
            {
                int classFieldIndex = argFC.FindField(strField);
                object temp = resultsFeature.get_Value(classFieldIndex);

                foreach (var item in listCluster)
                {
                    if (temp.ToString() == item.City)
                    {
                        IArea area = resultsFeature.Shape as IArea;
                        IPoint point = area.Centroid;

                        item.centerPoint = new double[] { point.X, point.Y };
                        break;
                    }                    
                }

                
                
            }

            
        }

        public void QueryCityTown(IFeatureClass argFC, ref List<Cluster> listCluster)
        {

            //針對 鄉鎮 台 字，一律用 臺 處理
            string strold = "台";
            string strnew = "臺";
            IDictionary<string, string> citysTowns = new Dictionary<string, string>();
            foreach (var item in listCluster)
            {
                string strCity = item.City;
                if (citysTowns.ContainsKey(strCity))
                {
                    string strTowns = citysTowns[strCity];
                    strTowns += ',';
                    strTowns += string.Format("'{0}'", item.Town.Replace(strold, strnew));
                    citysTowns[strCity] = strTowns;
                }
                else
                {
                    citysTowns.Add(strCity, string.Format("'{0}'", item.Town.Replace(strold, strnew)));
                }
            }

            string strFieldCounty = "CountyName";
            string strFieldTown = "TownName";

            string where = "";
            foreach (var citys in citysTowns)
            {
                if (where != "")
                    where += " or ";

                if (true)
                {

                }
                where += string.Format("( {0}='{1}' and {2} in ({3}) )", strFieldCounty, citys.Key, strFieldTown, citys.Value);
            }

            //
            IQueryFilter queryFilter = new QueryFilter();
            queryFilter.AddField(strFieldCounty);
            queryFilter.AddField(strFieldTown);
            queryFilter.WhereClause = where;

            IFeatureCursor resultsFeatureCursor = argFC.Search(queryFilter, true);
            //

            //
            IFeature resultsFeature = null;

            while ((resultsFeature = resultsFeatureCursor.NextFeature()) != null)
            {
                int classFieldIndex_County = argFC.FindField(strFieldCounty);
                object temp_County = resultsFeature.get_Value(classFieldIndex_County);

                int classFieldIndex_Town = argFC.FindField(strFieldTown);
                object temp_Town = resultsFeature.get_Value(classFieldIndex_Town);

                foreach (var item in listCluster)
                {
                    if (temp_County.ToString() == item.City && temp_Town.ToString().Replace(strold, strnew) == item.Town.Replace(strold, strnew))
                    {
                        IArea area = resultsFeature.Shape as IArea;
                        IPoint point = area.Centroid;
                        
                        item.centerPoint = new double[] { point.X, point.Y };
                        break;
                    }
                }



            }


        }

        /// <summary>
        /// 動態群聚 先找範圍內地籍址再找統計
        /// </summary>
        /// <param name="argFC"></param>
        /// <param name="argStrExtent"></param>
        public void Query相片位置(IFeatureClass argFC, string argStrExtent, out List<Cluster> listCluster,out string argMsg)
        {
            listCluster = new List<Cluster>();
            argMsg = "";
            string[] extent = argStrExtent.Split(',');
            double dbl_xmin, dbl_ymin, dbl_xmax, dbl_ymax;
            double.TryParse(extent[0], out dbl_xmin);
            double.TryParse(extent[1], out dbl_ymin);
            double.TryParse(extent[2], out dbl_xmax);
            double.TryParse(extent[3], out dbl_ymax);

            double dbllimit = 0.1;//範圍不要超過  Degrees
            if (dbl_xmax- dbl_xmin > dbllimit || dbl_ymax - dbl_ymin > dbllimit)
            {
                argMsg = "範圍過大";
                return;
            }

            IEnvelope env = new EnvelopeClass() as IEnvelope;
            env.PutCoords(dbl_xmin, dbl_ymin, dbl_xmax, dbl_ymax);
            
            env.SpatialReference = SpWGS84();

            ISpatialFilter queryFilter = new SpatialFilter();
            string strField = "CadAddr";
            queryFilter.AddField(strField);
            queryFilter.Geometry = env;
            queryFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelEnvelopeIntersects;
            
            IFeatureCursor resultsFeatureCursor = argFC.Search(queryFilter, true);
            
            //

            //先取得範圍extent的相片地籍址
            IFeature resultsFeature = null;
            Cluster cluster;
            while ((resultsFeature = resultsFeatureCursor.NextFeature()) != null)
            {
                int classFieldIndex_CadAddr = argFC.FindField(strField);
                object temp_CadAddr = resultsFeature.get_Value(classFieldIndex_CadAddr);

                cluster = new Cluster();
                cluster.CadAddr = temp_CadAddr.ToString();
                IPoint point = resultsFeature.Shape as IPoint;
                cluster.centerPoint = new double[] { point.X, point.Y };
                listCluster.Add(cluster);
            }

            //再取得該地籍址的count
            DataMethod daMet = new DataMethod();
            List<Cluster> listClusterCount;
            listClusterCount = daMet.GetClusterCount_CadAddrs(listCluster);
            foreach (var itemCaddr in listClusterCount)
            {
                var getCadAddr = from p in listCluster
                                 where p.CadAddr == itemCaddr.CadAddr
                                 select p;

                if (getCadAddr.Any())
                    itemCaddr.centerPoint = getCadAddr.FirstOrDefault().centerPoint;
            }

            //從暫存轉換為處理各地籍址計算
            listCluster = listClusterCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argFC"></param>
        /// <param name="argStrExtent"></param>
        /// <param name="listCluster"></param>
        /// <param name="argMsg"></param>
        public void Query_Sects(IFeatureClass argFC, IEnvelope argExtent, string argAppName, string argStartTime, string argStopTime, out List<Cluster> listCluster)
        {
            listCluster = new List<Cluster>();
            //query範圍地籍fc
            ISpatialFilter queryFilter = new SpatialFilter();
            string strField = "SCNAME";
            queryFilter.AddField(strField);
            queryFilter.Geometry = argExtent;
            queryFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelEnvelopeIntersects;

            IFeatureCursor resultsFeatureCursor = argFC.Search(queryFilter, true);

            //取得範圍extent的地段-取得中心位置
            IFeature resultsFeature = null;
            Cluster cluster;
            CityTown citytown;
            while ((resultsFeature = resultsFeatureCursor.NextFeature()) != null)
            {
                int classFieldIndex = argFC.FindField(strField);
                object temp = resultsFeature.get_Value(classFieldIndex);

                int classFieldIndex_CTY = argFC.FindField("CTY");
                object temp_CTY = resultsFeature.get_Value(classFieldIndex_CTY);
                int classFieldIndex_TOWN = argFC.FindField("TOWN");
                object temp_TOWN = resultsFeature.get_Value(classFieldIndex_TOWN);

                //利用代碼對應縣市鄉鎮
                citytown = DataMethod.CityTownCode.Find(ct => ct.TOWNID == temp_CTY.ToString() + temp_TOWN.ToString());

                IArea area = resultsFeature.Shape as IArea;
                IPoint point = area.Centroid;
                point.Project(SpWGS84());//回傳為經緯度

                cluster = new Cluster()
                {
                    centerPoint = new double[] { point.X, point.Y },
                    Sect = temp.ToString(),
                    City = citytown != null ? citytown.COUNTYNAME : "",
                    Town = citytown != null ? citytown.TOWNNAME : "",
                };
                listCluster.Add(cluster);
            }

            DataMethod daMet = new DataMethod();
            List<Cluster> listClusterCount;
            listClusterCount = daMet.GetClusterCount_Sect(listCluster,argAppName,argStartTime,argStopTime);//利用地段查詢
            List<Cluster> resultList = new List<Cluster>();
            foreach (var item in listClusterCount)
            {
                var getCadAddr = from p in listCluster
                                 where p.Sect == item.Sect && p.City == item.City && p.Town == item.Town
                                 select p;

                if (getCadAddr.Any())
                {
                    item.centerPoint = getCadAddr.FirstOrDefault().centerPoint;
                    resultList.Add(item);//有geometry，才回傳
                }
            }

            //從暫存轉換為處理各地籍址計算
            listCluster = resultList;
        }


        public void Query_Villages(IFeatureClass argFC, IEnvelope argExtent, string argAppName, string argStartTime, string argStopTime, out List<Cluster> listCluster)
        {
            listCluster = new List<Cluster>();
            //query範圍地籍fc
            ISpatialFilter queryFilter = new SpatialFilter();
            string strField_VILLNAME = "VILLNAME";
            string strField_TOWNNAME = "TOWNNAME";
            string strField_COUNTYNAME = "COUNTYNAME";
            queryFilter.AddField(strField_VILLNAME);
            queryFilter.AddField(strField_TOWNNAME);
            queryFilter.AddField(strField_COUNTYNAME);

            int classFieldIndex = argFC.FindField(strField_VILLNAME);
            int classFieldIndex_CN = argFC.FindField(strField_COUNTYNAME);
            int classFieldIndex_TN = argFC.FindField(strField_TOWNNAME);

            queryFilter.Geometry = argExtent;
            queryFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelEnvelopeIntersects;

            IFeatureCursor resultsFeatureCursor = argFC.Search(queryFilter, true);

            //取得範圍extent的村里-取得中心位置
            IFeature resultsFeature = null;
            Cluster cluster;
            while ((resultsFeature = resultsFeatureCursor.NextFeature()) != null)
            {
               
                object temp = resultsFeature.get_Value(classFieldIndex);

                object temp_CTY = resultsFeature.get_Value(classFieldIndex_CN);
                object temp_TOWN = resultsFeature.get_Value(classFieldIndex_TN);

                IArea area = resultsFeature.Shape as IArea;
                IPoint point = area.Centroid;
                

                cluster = new Cluster()
                {
                    centerPoint = new double[] { point.X, point.Y },
                    Village = temp.ToString(),
                    City = temp_CTY.ToString(),
                    Town = temp_TOWN.ToString()
                };
                listCluster.Add(cluster);
            }

            DataMethod daMet = new DataMethod();
            List<Cluster> listClusterCount;
            listClusterCount = daMet.GetClusterCount_Village(listCluster, argAppName, argStartTime, argStopTime);//利用村里查詢
            List<Cluster> resultList = new List<Cluster>();
            foreach (var item in listClusterCount)
            {
                var getCadAddr = from p in listCluster
                                 where p.Village == item.Village && p.City == item.City && p.Town == item.Town
                                 select p;

                if (getCadAddr.Any())
                {
                    item.centerPoint = getCadAddr.FirstOrDefault().centerPoint;
                    resultList.Add(item);//有geometry，才回傳
                }
            }

            //從暫存轉換為處理各村里計算
            listCluster = resultList;
        }


        public IEnvelope GetSpExtent(string argStrExtent,out IFeatureClass outFC, out string argMsg)
        {
            argMsg = string.Empty;
            IEnvelope env = new EnvelopeClass() as IEnvelope;
            outFC = null;

            string[] extent = argStrExtent.Split(',');
            double dbl_xmin, dbl_ymin, dbl_xmax, dbl_ymax;
            double.TryParse(extent[0], out dbl_xmin);
            double.TryParse(extent[1], out dbl_ymin);
            double.TryParse(extent[2], out dbl_xmax);
            double.TryParse(extent[3], out dbl_ymax);

            double dbllimit = 0.5;//範圍不要超過  Degrees 0.5 => 10公里
            if (dbl_xmax - dbl_xmin > dbllimit || dbl_ymax - dbl_ymin > dbllimit)
            {
                argMsg = "範圍過大";
                return env;
            }
            //輸入為經緯
            
            env.PutCoords(dbl_xmin, dbl_ymin, dbl_xmax, dbl_ymax);
            env.SpatialReference = SpWGS84();

            //轉換為twd97並判斷Penghu、Taiwan
            if (dbl_xmax < 120 || dbl_ymin > 25.4)
            {
                env.Project(SpTWD97_Penghu());
                outFC = FeatureDataMethod.OpenGDBFeatureClass_Sect("106Q4", 0);
            }
            else
            {
                env.Project(SpTWD97_Taiwan());
                outFC = FeatureDataMethod.OpenGDBFeatureClass_Sect("106Q4", 1);
            }


            return env;
        }

        public IEnvelope GetSpExtentAndVillageFC(string argStrExtent, out IFeatureClass outFC, out string argMsg)
        {
            argMsg = string.Empty;
            IEnvelope env = new EnvelopeClass() as IEnvelope;
            outFC = null;

            string[] extent = argStrExtent.Split(',');
            double dbl_xmin, dbl_ymin, dbl_xmax, dbl_ymax;
            double.TryParse(extent[0], out dbl_xmin);
            double.TryParse(extent[1], out dbl_ymin);
            double.TryParse(extent[2], out dbl_xmax);
            double.TryParse(extent[3], out dbl_ymax);

            double dbllimit = 0.5;//範圍不要超過  Degrees 0.5 => 10公里
            if (dbl_xmax - dbl_xmin > dbllimit || dbl_ymax - dbl_ymin > dbllimit)
            {
                argMsg = "範圍過大";
                return env;
            }
            //輸入為經緯

            env.PutCoords(dbl_xmin, dbl_ymin, dbl_xmax, dbl_ymax);
            env.SpatialReference = SpWGS84();

            outFC = FeatureDataMethod.OpenFeatureClass_Shapefile(Setting._StrShapeFileName_Path, Setting._StrShapeFileName_Village);


            return env;
        }


        private ISpatialReference SpWGS84()
        {
            ISpatialReferenceFactory spF = new SpatialReferenceEnvironmentClass();
            ISpatialReference spWGS84 = spF.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);

            return spWGS84;
        }

        private ISpatialReference SpTWD97_Taiwan()
        {
            ISpatialReferenceFactory spF = new SpatialReferenceEnvironmentClass();
            ISpatialReference spWGS84 = spF.CreateProjectedCoordinateSystem((int)esriSRProjCS3Type.esriSRProjCS_TWD1997TM_Taiwan);

            return spWGS84;
        }

        private ISpatialReference SpTWD97_Penghu()
        {
            ISpatialReferenceFactory spF = new SpatialReferenceEnvironmentClass();
            ISpatialReference spWGS84 = spF.CreateProjectedCoordinateSystem((int)esriSRProjCS3Type.esriSRProjCS_TWD1997TMPenghu);

            return spWGS84;
        }
    }
}
