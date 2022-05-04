using ESRI.ArcGIS.SOESupport;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RestSOE_COA_Data.model
{
    class DataMethod
    {

        #region 取得 群聚 計算view 

        static string _IMAGEPOSITION_Table = "[dbo].[IMAGEPOSITION]";

        //0:分類欄位 1:計算欄位 2:來源 
        static string _view_Count = "SELECT {0}, COUNT({1}) AS count FROM {2} GROUP BY {0} HAVING (COUNT({1}) > 0)";
        //0:回傳欄位 1:來源 2:大於起始時間 3:小於起始時間
        static string _view_Filter_Time = "(SELECT {0} FROM {1} WHERE PhotoTime > '{2}' and PhotoTime < '{3}') as tab";


        static string _IMAGEPOSITION_CadAddr_Table = "[dbo].[IMAGEPOSITION_CadAddr]";



        public List<Cluster> GetClusterCount_Citys(string argAppName,string argStartTime,string argStopTime)
        {
            string strFields = "City, AppName";
            string strField_Count = "City";

            if (string.IsNullOrEmpty(argAppName))
                strFields = "City";

            List<Cluster> list = new List<Cluster>();
            using (clsDB db = new clsDB())
            {
                string _sql = string.Empty;

                string strFilterTime = string.Format(_view_Filter_Time, strFields, _IMAGEPOSITION_Table, argStartTime, argStopTime);
                string strViewCount = string.Format(_view_Count, strFields, strField_Count, strFilterTime);

                if (string.IsNullOrEmpty(argAppName))
                    _sql = string.Format("select * from ({0}) as t ", strViewCount);
                else
                    _sql = string.Format("select * from ({0}) as t where AppName='{1}' ", strViewCount, argAppName);


                DataTable _dt = db.ToDataTable(_sql);
                list = DataTableExtensions.ToList<Cluster>(_dt).ToList();

            }

            
            return list;
        }

        public List<Cluster> GetClusterCount_CityTowns(string argAppName, string argStartTime, string argStopTime)
        {
            string strFields = "City,Town, AppName";
            string strField_Count = "Town";

            if (string.IsNullOrEmpty(argAppName))
                strFields = "City,Town";

            List<Cluster> list = new List<Cluster>();
            using (clsDB db = new clsDB())
            {
                string _sql = string.Empty;
                string strFilterTime = string.Format(_view_Filter_Time, strFields, _IMAGEPOSITION_Table, argStartTime, argStopTime);
                string strViewCount = string.Format(_view_Count, strFields, strField_Count, strFilterTime);

                if (string.IsNullOrEmpty(argAppName))
                    _sql = string.Format("select * from ({0}) as t where Town <>''", strViewCount);
                else
                    _sql = string.Format("select * from ({0}) as t where AppName='{1}' and Town <>''", strViewCount, argAppName);

                DataTable _dt = db.ToDataTable(_sql);
                list = DataTableExtensions.ToList<Cluster>(_dt).ToList();

            }
            return list;
        }

        public List<Cluster> GetClusterCount_Sect(List<Cluster> argCluster, string argAppName, string argStartTime, string argStopTime)
        {
            if (argCluster.Count == 0)
            {
                return new List<Cluster>();
            }
            string where = "";
            foreach (var item in argCluster)
            {
                if (where != "") where += ',';
                where += string.Format("'{0}'", item.Sect);
            }

            string strFields = "City,Town,Sect, AppName";
            string strField_Count = "Sect";

            if (string.IsNullOrEmpty(argAppName))
                strFields = "City,Town,Sect";

            List<Cluster> list = new List<Cluster>();
            using (clsDB db = new clsDB())
            {
                string _sql = string.Empty;
                string strFilterTime = string.Format(_view_Filter_Time, strFields, _IMAGEPOSITION_Table, argStartTime, argStopTime);
                string strViewCount = string.Format(_view_Count, strFields, strField_Count, strFilterTime);

                if (string.IsNullOrEmpty(argAppName))
                    _sql = string.Format("select * from ({0}) as t where Sect in ({1}) and Town <>''", strViewCount, where);
                else
                    _sql = string.Format("select * from ({0}) as t where AppName='{1}' and Sect in ({2}) and Town <>''", strViewCount, argAppName, where);

                DataTable _dt = db.ToDataTable(_sql);
                list = DataTableExtensions.ToList<Cluster>(_dt).ToList();

            }
            return list;
        }

        public List<Cluster> GetClusterCount_Village(List<Cluster> argCluster, string argAppName, string argStartTime, string argStopTime)
        {
            if (argCluster.Count == 0)
            {
                return new List<Cluster>();
            }
            string where = "";
            foreach (var item in argCluster)
            {
                if (where != "") where += ',';
                where += string.Format("'{0}'", item.Village);
            }

            string strFields = "City,Town,Village, AppName";
            string strField_Count = "Village";

            if (string.IsNullOrEmpty(argAppName))
                strFields = "City,Town,Village";

            List<Cluster> list = new List<Cluster>();
            using (clsDB db = new clsDB())
            {
                string _sql = string.Empty;
                string strFilterTime = string.Format(_view_Filter_Time, strFields, _IMAGEPOSITION_Table, argStartTime, argStopTime);
                string strViewCount = string.Format(_view_Count, strFields, strField_Count, strFilterTime);

                if (string.IsNullOrEmpty(argAppName))
                    _sql = string.Format("select * from ({0}) as t where Village in ({1}) and Town <>''", strViewCount, where);
                else
                    _sql = string.Format("select * from ({0}) as t where AppName='{1}' and Village in ({2}) and Town <>''", strViewCount, argAppName, where);

                DataTable _dt = db.ToDataTable(_sql);
                list = DataTableExtensions.ToList<Cluster>(_dt).ToList();

            }
            return list;
        }


        public List<Cluster> GetClusterCount_CadAddrs(string argCadAddrs)
        {
            string[] strArray = argCadAddrs.Split(',');
            string where = "";
            foreach (var item in strArray)
            {
                if (where != "") where += ',';
                where += string.Format("'{0}'", item);
            }

            List <Cluster> list = new List<Cluster>();
            using (clsDB db = new clsDB())
            {
                string _sql = string.Empty;
                _sql = string.Format("select * from {0} where CadAddr in ({1})", _IMAGEPOSITION_CadAddr_Table, where);

                DataTable _dt = db.ToDataTable(_sql);
                list = DataTableExtensions.ToList<Cluster>(_dt).ToList();

            }
            return list;
        }


        public List<Cluster> GetClusterCount_CadAddrs(List<Cluster> argCadAddrs)
        {
            string where = "";
            foreach (var item in argCadAddrs)
            {
                if (where != "") where += ',';
                where += string.Format("'{0}'", item.CadAddr);
            }

            List<Cluster> list = new List<Cluster>();
            using (clsDB db = new clsDB())
            {
                string _sql = string.Empty;
                _sql = string.Format("select * from {0} where CadAddr in ({1})", _IMAGEPOSITION_CadAddr_Table, where);

                DataTable _dt = db.ToDataTable(_sql);
                list = DataTableExtensions.ToList<Cluster>(_dt).ToList();

            }
            return list;
        }


        #endregion

        static string _縣市鄉鎮代碼_Table = "[dbo].[縣市鄉鎮代碼]";

        private static List<CityTown> _CityTownCode;

        public static List<CityTown> CityTownCode
        {
            get
            {
                if (_CityTownCode == null)
                {
                    _CityTownCode = GetCityTownCode();
                }
                return _CityTownCode;
            }

            set
            {
                _CityTownCode = value;
            }
        }

        /// <summary>
        /// 取得縣市鄉鎮ID對應表 配合地段使用
        /// </summary>
        /// <returns></returns>
        private static List<CityTown> GetCityTownCode()
        {
            List<CityTown> list = new List<CityTown>();
            using (clsDB db = new clsDB())
            {
                string _sql = string.Empty;

                _sql = string.Format("select * from {0} ", _縣市鄉鎮代碼_Table);

                DataTable _dt = db.ToDataTable(_sql);
                list = DataTableExtensions.ToList<CityTown>(_dt).ToList();
            }

            return list;
        }


    }
}
