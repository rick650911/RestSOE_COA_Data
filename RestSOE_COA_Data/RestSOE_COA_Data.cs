using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.Specialized;
using System.Runtime.InteropServices;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Server;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SOESupport;
using RestSOE_COA_Data.model;
using System.Net;


//TODO: sign the project (project properties > signing tab > sign the assembly)
//      this is strongly suggested if the dll will be registered using regasm.exe <your>.dll /codebase


namespace RestSOE_COA_Data
{
    [ComVisible(true)]
    [Guid("0b6ec752-ebd4-459e-a3a8-241ca1ba847e")]
    [ClassInterface(ClassInterfaceType.None)]
    [ServerObjectExtension("MapServer",//use "MapServer" if SOE extends a Map service and "ImageServer" if it extends an Image service.
        AllCapabilities = "",
        DefaultCapabilities = "",
        Description = "",
        DisplayName = "RestSOE_COA_Data",
        Properties = "Proxy=https://coagis.colife.org.tw/proxy/proxy.ashx;Url_Server=https://coagis.colife.org.tw/arcgis/rest/services/;",
        SupportsREST = true,
        SupportsSOAP = false)]
    public class RestSOE_COA_Data : IServerObjectExtension, IObjectConstruct, IRESTRequestHandler
    {
        private string soe_name;

        private IPropertySet configProps;
        private IServerObjectHelper serverObjectHelper;
        private ServerLogger logger;
        private IRESTRequestHandler reqHandler;

        private string _Proxy = "";
        private string _Url_Server = "";

        private string _mapName;
        private IMapServerDataAccess _dataAccess;
        private IFeatureClass _FeatureClass;
        private IFeatureClass _FeatureClass_City;
        private IFeatureClass _FeatureClass_Town;
        private int _layerId = -1;

        //private IWorkspaceEdit _workspaceEdit;
        public RestSOE_COA_Data()
        {
            soe_name = this.GetType().Name;
            logger = new ServerLogger();
            reqHandler = new SoeRestImpl(soe_name, CreateRestSchema()) as IRESTRequestHandler;
        }

        #region IServerObjectExtension Members

        public void Init(IServerObjectHelper pSOH)
        {
            serverObjectHelper = pSOH;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;
        }

        public void Shutdown()
        {
        }

        #endregion

        #region IObjectConstruct Members

        public void Construct(IPropertySet props)
        {
            configProps = props;
            this._Proxy = (string)props.GetProperty("Proxy");
            this._Url_Server = (string)props.GetProperty("Url_Server");

            LogMethod log = new LogMethod("Construct");
            try
            {
                IMapServer3 mapServer = (IMapServer3)serverObjectHelper.ServerObject;
                _mapName = mapServer.DefaultMapName;
                IMapLayerInfos layerInfos = mapServer.GetServerInfo(_mapName).MapLayerInfos;
                _dataAccess = (IMapServerDataAccess)mapServer;

                this._FeatureClass = (IFeatureClass)_dataAccess.GetDataSource(_mapName, 0);
                this._FeatureClass_Town = (IFeatureClass)_dataAccess.GetDataSource(_mapName, 1);
                this._FeatureClass_City = (IFeatureClass)_dataAccess.GetDataSource(_mapName, 2);

                log.Add("start:" + this._FeatureClass.AliasName);//初始化可以
                log.Add("start:" + this._FeatureClass_City.AliasName);
                log.Add("start:" + this._FeatureClass_Town.AliasName);
            }
            catch (Exception ex)
            {

                log.Add(ex.Message);
                log.Add(ex.StackTrace);

            }
            log.Save();
        }

        #endregion

        #region IRESTRequestHandler Members

        public string GetSchema()
        {
            return reqHandler.GetSchema();
        }

        public byte[] HandleRESTRequest(string Capabilities, string resourceName, string operationName, string operationInput, string outputFormat, string requestProperties, out string responseProperties)
        {
            return reqHandler.HandleRESTRequest(Capabilities, resourceName, operationName, operationInput, outputFormat, requestProperties, out responseProperties);
        }

        #endregion

        private RestResource CreateRestSchema()
        {
            RestResource rootRes = new RestResource(soe_name, false, RootResHandler);

            RestOperation sampleOper = new RestOperation("sampleOperation",
                                                      new string[] { "parm1", "parm2" },
                                                      new string[] { "json" },
                                                      SampleOperHandler);

            //20220504-APP無使用此功能，將此功能下架
            //RestOperation CalCadAddrOper = new RestOperation("CalCadAddr",
            //                                          new string[] { "Ver","Lon","Lat", "Direction", "Distance" },
            //                                          new string[] { "json" },
            //                                          CalCadAddrHandler);

            //RestOperation GetOffLineMapOper = new RestOperation("GetOffLineMap",
            //                                          new string[] { "Type", "City", "Town", },
            //                                          new string[] { "json" },
            //                                          GetOffLineMapHandler);

            RestOperation GetOffLineMapOper_ver = new RestOperation("GetOffLineMap_ver",
                                                      new string[] { "Type", "City","Town", "Land_Version", "scale_level" },
                                                      new string[] { "json" },
                                                      GetOffLineMapHandler_ver);

            RestOperation ImageQueryQualityOper = new RestOperation("ImageQueryQuality",
                                                      new string[] { "CaseID" },
                                                      new string[] { "json" },
                                                      ImageQueryQualityHandler);

            RestOperation ClusterOper = new RestOperation("GetCluster",
                                                      new string[] { "Citys","CityTowns", "Extent", "AppName", "StartTime", "StopTime" },
                                                      new string[] { "json" },
                                                      ClusterHandler);

            rootRes.operations.Add(sampleOper);

            //rootRes.operations.Add(CalCadAddrOper);

           // rootRes.operations.Add(GetOffLineMapOper);

            rootRes.operations.Add(GetOffLineMapOper_ver);

            rootRes.operations.Add(ImageQueryQualityOper);

            rootRes.operations.Add(ClusterOper);

            return rootRes;
        }

        private byte[] RootResHandler(NameValueCollection boundVariables, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = null;

            JsonObject result = new JsonObject();
            result.AddString("UploadPhoto", "上傳json資料");
            result.AddString("CalCadAddr", "計算地籍");

            result.AddString("GetCluster", "計算群聚");

            return Encoding.UTF8.GetBytes(result.ToJson());
        }

        private byte[] SampleOperHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            string parm1Value;
            bool found = operationInput.TryGetString("parm1", out parm1Value);
            if (!found || string.IsNullOrEmpty(parm1Value))
                throw new ArgumentNullException("parm1");

            string parm2Value;
            found = operationInput.TryGetString("parm2", out parm2Value);
            if (!found || string.IsNullOrEmpty(parm2Value))
                throw new ArgumentNullException("parm2");

            JsonObject result = new JsonObject();
            result.AddString("Proxy", this._Proxy);
            result.AddString("Url_Server", this._Url_Server);

            return Encoding.UTF8.GetBytes(result.ToJson());
        }


  

        private byte[] CalCadAddrHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            string strMsg ="";

            string parmver;
            bool found = operationInput.TryGetString("Ver", out parmver);
            if (!found || string.IsNullOrEmpty(parmver))
                parmver = "106Q4";//預設106Q4

            double? parmLon;
            found = operationInput.TryGetAsDouble("Lon", out parmLon);
            if (!found || parmLon.HasValue == false)
                strMsg = "Lon(經):請輸入數值 ";

            double? parmLat;
            found = operationInput.TryGetAsDouble("Lat", out parmLat);
            if (!found || parmLat.HasValue == false)
                strMsg += "Lat(緯):請輸入數值 ";

            double? parmDirection;
            found = operationInput.TryGetAsDouble("Direction", out parmDirection);
            if (!found || parmDirection.HasValue == false)
                strMsg += "Direction(方位):請輸入數值 ";

            double? parmDistance;
            found = operationInput.TryGetAsDouble("Distance", out parmDistance);
            if (!found || parmDistance.HasValue == false)
                strMsg += "Distance(攝距):請輸入數值 ";


            JsonObject result = new JsonObject();
            ResultCalCadAddr resultClass = new ResultCalCadAddr();

            if (!string.IsNullOrEmpty(strMsg))
            {
                resultClass.msg = strMsg;
                result.AddObject("result", resultClass);
                return Encoding.UTF8.GetBytes(result.ToJson());
            }
            LogMethod log = new LogMethod("CalCadAddr");
            IPoint point = null;
            CalMethod calm = new CalMethod(this._Proxy, this._Url_Server);

            try
            {
                //計算攝距方法
                point = calm.CalPosition(parmLon.Value, parmLat.Value, parmDirection.Value, parmDistance.Value);
            }
            catch (Exception ex) 
            {
                strMsg = "計算攝距錯誤!";
                log.Add(ex.Message);
                log.Add(ex.StackTrace);
                log.Save();
                resultClass.msg = strMsg;
                result.AddObject("result", resultClass);
                return Encoding.UTF8.GetBytes(result.ToJson());
            }

            bool ishave = false;
            string strAddr;

            //取得地籍方法
            int index = 1;
            if (point.X < 120)
                index = 0;

            ishave = calm.CalAddr(parmver, point.X + "," + point.Y, index, 4326, out strAddr);

            if (ishave == false)
            {
                strMsg = strAddr;
                log.Add(strAddr);
                log.Save();
                resultClass.msg = strMsg;

                ConnectMethod con = new ConnectMethod();
                con.ErrorMail(ErrorType.地籍查詢服務+ "CalCadAddr", "<p>使用地籍查詢錯誤問題</p><p>" + this._Proxy + "</p><p>" + this._Url_Server+ "</p>", "CalCadAddr");

                result.AddObject("result", resultClass);
                return Encoding.UTF8.GetBytes(result.ToJson());
            }


            
            resultClass.cadaddr = strAddr;
            resultClass.status = true;
            resultClass.msg = strMsg;
            result.AddObject("result", resultClass);
            result.AddDouble("X", point.X);
            result.AddDouble("Y", point.Y);
            return Encoding.UTF8.GetBytes(result.ToJson());
        }



        



        //private byte[] GetOffLineMapHandler(NameValueCollection boundVariables,
        //                                          JsonObject operationInput,
        //                                              string outputFormat,
        //                                              string requestProperties,
        //                                          out string responseProperties)
        //{
        //    responseProperties = null;

        //    string strMsg="";

        //    string parmType;
        //    bool found = operationInput.TryGetString("Type", out parmType);
        //    if (!found || string.IsNullOrEmpty(parmType))
        //        strMsg = "Type:請輸入地圖類型 ";

        //    string parmCity;
        //    found = operationInput.TryGetString("City", out parmCity);
        //    if (!found || string.IsNullOrEmpty(parmCity))
        //        strMsg += "City:請輸入縣市 ";

        //    string parmTown;
        //    found = operationInput.TryGetString("Town", out parmTown);
        //    if (!found || string.IsNullOrEmpty(parmTown))
        //        strMsg += "Town:請輸入鄉鎮 ";


        //    JsonObject result = new JsonObject();
        //    ResultGetOffLineMap resultClass = new ResultGetOffLineMap();
        //    if (!string.IsNullOrEmpty(strMsg))
        //    {
        //        resultClass.msg = strMsg;
        //        result.AddObject("result", resultClass);
        //        return Encoding.UTF8.GetBytes(result.ToJson());
        //    }

        //    LogMethod log = new LogMethod("GetOffLineMap");
        //    string strURL = "";
        //    try
        //    {
               
        //        DownLoadMethod down = new DownLoadMethod();
                
        //        DownLoadMethod.DownType type;
        //        string strInput = parmType.ToLower();
        //        if (strInput.Contains("vector"))
        //            type = DownLoadMethod.DownType.Vector;
        //        else if (strInput.Contains("raster") && strInput.Contains("輕量"))
        //            type = DownLoadMethod.DownType.Raster_輕量版;
        //        else if (strInput.Contains("raster") && strInput.Contains("完整"))
        //            type = DownLoadMethod.DownType.Raster_完整版;
        //        else
        //        {
        //            resultClass.msg = "請輸入 地圖類型 vector or raster-平地 or raster-完整";
        //            result.AddObject("result", resultClass);
        //            return Encoding.UTF8.GetBytes(result.ToJson());
        //        }

        //        strURL = down.GetDownURL(type, parmCity, parmTown,null,out strMsg);

        //    }
        //    catch (Exception ex)
        //    {
        //        strMsg = "檔案路徑錯誤!";
        //        log.Add(ex.Message);
        //        log.Add(ex.StackTrace);
        //        log.Save();
        //    }

        //    if (!string.IsNullOrEmpty(strMsg))
        //    {
        //        resultClass.msg = strMsg;
        //        result.AddObject("result", resultClass);
        //        return Encoding.UTF8.GetBytes(result.ToJson());
        //    }

        //    resultClass.fileUrl = strURL;
        //    resultClass.status = true;
        //    resultClass.msg = strMsg;
        //    result.AddObject("result", resultClass);
        //    return Encoding.UTF8.GetBytes(result.ToJson());
        //}


        private byte[] GetOffLineMapHandler_ver(NameValueCollection boundVariables,
                                                 JsonObject operationInput,
                                                     string outputFormat,
                                                     string requestProperties,
                                                 out string responseProperties)
        {
            responseProperties = null;

            string strMsg = "";

            string parmType;
            bool found = operationInput.TryGetString("Type", out parmType);
            if (!found || string.IsNullOrEmpty(parmType))
                strMsg = "Type:請輸入地圖類型 ";

            string parmCity;
            found = operationInput.TryGetString("City", out parmCity);
            if (!found || string.IsNullOrEmpty(parmCity))
                strMsg += "City:請輸入縣市 ";

            string parmTown;
            found = operationInput.TryGetString("Town", out parmTown);
            if (!found || string.IsNullOrEmpty(parmTown))
                strMsg += "Town:請輸入鄉鎮 ";

            string parmVer;
            found = operationInput.TryGetString("Land_Version", out parmVer);
            //if (!found || string.IsNullOrEmpty(parmVer))  
            //    strMsg += "Land_Version:請輸入地籍圖版次 ";

            string parm_scale_level;
            found = operationInput.TryGetString("scale_level", out parm_scale_level);

            if (!found || string.IsNullOrEmpty(parm_scale_level))
                parm_scale_level= "輕量版";

            JsonObject result = new JsonObject();
            ResultGetOffLineMap resultClass = new ResultGetOffLineMap();
            if (!string.IsNullOrEmpty(strMsg))
            {
                resultClass.msg = strMsg;
                result.AddObject("result", resultClass);
                return Encoding.UTF8.GetBytes(result.ToJson());
            }

            LogMethod log = new LogMethod("GetOffLineMap");
            string strURL = "";
            try
            {

                DownLoadMethod down = new DownLoadMethod();

                DownLoadMethod.DownType type;
                string strInput = parmType.ToLower();
                if (strInput.Contains("vector"))
                    type = DownLoadMethod.DownType.Vector;
                else if (strInput.Contains("raster") && parm_scale_level.Contains("輕量"))
                    type = DownLoadMethod.DownType.Raster_輕量版;
                else if (strInput.Contains("raster") && parm_scale_level.Contains("完整"))
                    type = DownLoadMethod.DownType.Raster_完整版;
                else
                {
                    resultClass.msg = "請輸入 地圖類型 vector or raster, 圖磚比例尺層級 輕量版 or 完整版";
                    result.AddObject("result", resultClass);
                    return Encoding.UTF8.GetBytes(result.ToJson());
                }

                strURL = down.GetDownURL(type, parmCity, parmTown, parmVer, out strMsg);

                //檔案不存在的話，再用同義字看看
                if (strMsg.Contains(parmCity+ "_"+ parmTown + "尚未提供下載檔案!"))
                {
                    bool blFound = false;

                    string parmCity_in= parmCity;
                    string parmTown_in= parmTown;

                    //縣市同義字
                    foreach (KeyValuePair<string, List<string>> item in LandVersionManager.DicSameWord_city )
                    {
                        string str_word_sys = item.Key;

                        List<string> list_str_word_same = item.Value;

                        foreach (string str_word_same in list_str_word_same)
                        {
                            parmCity = parmCity.Replace(str_word_same, str_word_sys);

                            strURL = down.GetDownURL(type, parmCity, parmTown, parmVer, out strMsg);
                            
                            if (!strMsg.Contains(parmCity + "_" + parmTown + "尚未提供下載檔案!")) //找到檔案存在
                            {
                                blFound = true;                                
                            }

                            if (blFound) 
                            {
                                break; 
                            }

                            parmCity = parmCity_in;
                        }

                        if (blFound)
                        {
                            break;
                        }
                    }// foreach (KeyValuePair<string, List<string>> item in LandVersionManager.DicSameWord_city )

                    //鄉鎮同義字
                    if (!blFound)
                    {
                        parmCity = parmCity_in;

                        foreach (KeyValuePair<string, List<string>> item in LandVersionManager.DicSameWord_town)
                        {
                            string str_word_sys = item.Key;

                            List<string> list_str_word_same = item.Value;

                            foreach (string str_word_same in list_str_word_same)
                            {
                                parmTown = parmTown.Replace(str_word_same, str_word_sys);

                                strURL = down.GetDownURL(type, parmCity, parmTown, parmVer, out strMsg);

                                if (!strMsg.Contains(parmCity + "_" + parmTown + "尚未提供下載檔案!")) //找到檔案存在
                                {
                                    blFound = true;
                                }

                                if (blFound)
                                {
                                    break;
                                }

                                parmTown = parmTown_in;
                            }

                            if (blFound)
                            {
                                break;
                            }
                        }//foreach (KeyValuePair<string, List<string>> item in LandVersionManager.DicSameWord_town)
                    }

                }// if (strMsg.Contains(parmCity+ "_"+ parmTown + "尚未提供下載檔案!"))

            }
            catch (Exception ex)
            {
                strMsg = "檔案路徑錯誤!";
                log.Add(ex.Message);
                log.Add(ex.StackTrace);
                log.Save();
            }

            if (!string.IsNullOrEmpty(strMsg))
            {
                resultClass.msg = strMsg;
                result.AddObject("result", resultClass);
                return Encoding.UTF8.GetBytes(result.ToJson());
            }

            resultClass.fileUrl = strURL;
            resultClass.status = true;
            resultClass.msg = strMsg;
            result.AddObject("result", resultClass);
            return Encoding.UTF8.GetBytes(result.ToJson());
        }

        private byte[] ImageQueryQualityHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            JsonObject result = new JsonObject();
            ResultImageQueryQuality resultClass = new ResultImageQueryQuality();

            LogMethod log = new LogMethod("ImageQueryQuality");

            long? parmCaseid;
            bool found = operationInput.TryGetAsLong("CaseID", out parmCaseid);
            if (!found || parmCaseid.HasValue == false)
            {
                resultClass.msg = "請輸入 CaseID";
                result.AddObject("result", resultClass);
                return Encoding.UTF8.GetBytes(result.ToJson());
            }

            try
            {
                QueryMethod query = new QueryMethod();
                query.QueryCaseIDValue(ref resultClass, _FeatureClass, parmCaseid.Value);
            }
            catch (Exception ex)
            {
                log.Add(ex.Message);
                log.Add(ex.StackTrace);
                log.Save();
                resultClass.status = false;
                result.AddObject("result", resultClass);
                return Encoding.UTF8.GetBytes(result.ToJson());
            }
            
            
            result.AddObject("result", resultClass);
            return Encoding.UTF8.GetBytes(result.ToJson());
        }



        private byte[] ClusterHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            #region 參數

            string parm_Citys;//縣市-是否
            bool found = operationInput.TryGetString("Citys", out parm_Citys);
            if (!found || string.IsNullOrEmpty(parm_Citys))
                parm_Citys = "";//輸入 all 目前全部取

            string parm_CityTowns;//鄉鎮-是否
            found = operationInput.TryGetString("CityTowns", out parm_CityTowns);
            if (!found || string.IsNullOrEmpty(parm_CityTowns))
                parm_CityTowns = "";//輸入 all 目前全部取

            string parm_Extent;//Extent範圍
            found = operationInput.TryGetString("Extent", out parm_Extent);
            if (!found || string.IsNullOrEmpty(parm_Extent))
                parm_Extent = "";

            string parm_AppName;//APP名稱
            found = operationInput.TryGetString("AppName", out parm_AppName);
            if (!found || string.IsNullOrEmpty(parm_AppName))
                parm_AppName = "";

            string parm_StartTime;//時間範圍-起始
            found = operationInput.TryGetString("StartTime", out parm_StartTime);
            if (!found || string.IsNullOrEmpty(parm_StartTime))
                parm_StartTime = "2018/08/01";

            string parm_StopTime;//時間範圍-結束
            found = operationInput.TryGetString("StopTime", out parm_StopTime);
            if (!found || string.IsNullOrEmpty(parm_StopTime))
                parm_StopTime = "2018/08/30";

            #endregion

            ResultCluster resultClass = new ResultCluster();

            DataMethod daMet = new DataMethod();

            JsonObject result = new JsonObject();

            QueryMethod queryMet = new QueryMethod();

            List<Cluster> listCluster;
            try
            {
                if (string.IsNullOrEmpty(parm_Extent) && string.IsNullOrEmpty(parm_CityTowns) && string.IsNullOrEmpty(parm_Citys))
                {
                    resultClass.msg = "Citys or CityTowns or Extent";
                }
                else if (string.IsNullOrEmpty(parm_Citys) == false)
                {
                    listCluster = daMet.GetClusterCount_Citys(parm_AppName,parm_StartTime, parm_StopTime);
                    queryMet.QueryCity(this._FeatureClass_City,ref listCluster);
                    resultClass.Clusters = listCluster;
                    resultClass.status = true;
                }
                else if (string.IsNullOrEmpty(parm_CityTowns) == false)
                {
                    listCluster = daMet.GetClusterCount_CityTowns(parm_AppName, parm_StartTime, parm_StopTime);
                    queryMet.QueryCityTown(this._FeatureClass_Town, ref listCluster);
                    resultClass.Clusters = listCluster;
                    resultClass.status = true;
                }
                //else if (string.IsNullOrEmpty(parm_Extent) == false)
                //{
                //    //動態取得，先取得範圍內相片，再取得該地段的count
                //    //注意範圍控制在鄉鎮以下0.15 degree
                //    IFeatureClass outFC;
                //    string strMsg;
                //    IEnvelope extent =  queryMet.GetSpExtent(parm_Extent, out outFC, out strMsg);
                //    if (outFC != null)
                //    {
                //        queryMet.Query_Sects(outFC, extent, parm_AppName, parm_StartTime, parm_StopTime, out listCluster);
                //        resultClass.Clusters = listCluster;
                //    }
                //    resultClass.msg = strMsg;
                //    resultClass.status = true;
                //}
                else if (string.IsNullOrEmpty(parm_Extent) == false)
                {
                    //動態取得，先取得範圍內相片，再取得該村里的count
                    IFeatureClass outFC;
                    string strMsg;
                    IEnvelope extent = queryMet.GetSpExtentAndVillageFC(parm_Extent, out outFC, out strMsg);
                    if (outFC != null)
                    {
                        queryMet.Query_Villages(outFC, extent, parm_AppName, parm_StartTime, parm_StopTime, out listCluster);
                        resultClass.Clusters = listCluster;
                    }
                    resultClass.msg = strMsg;
                    resultClass.status = true;
                }
            }
            catch (Exception ex)
            {
                resultClass.msg = ex.Message;
                resultClass.msg += ex.StackTrace;
            }

            


            result.AddObject("result", resultClass);
            return Encoding.UTF8.GetBytes(result.ToJson());
        }


    }
}
