using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSOE_COA_Data.model
{
    public class Setting
    {
        /// <summary>
        /// 連接字串
        /// </summary>
        public static string _SQL_ConnectString = @"Data Source=COAGIS\opssqlserver;Initial Catalog=COA_IM;User ID=coagisAP;Password=Portaladmin0000";

        public static string _mailAPI = "https://gis.coa.gov.tw/landtest/api/mailapi/SendMail";//寄送mailAPI

        public static string _ServerPath = "https://coagis.colife.org.tw/OffLineMap/{0}/{1}";//虛擬目錄
        public static string _LocalPath = @"Z:\MobileMap\{0}\{1}";//0:type 1: 縣市

        public static string _StrFullName_GDB_Land = @"z:\Layer\Section\Section_{0}.gdb";
        public static string _StrFeatureClassName_Taiwan = "Section_{0}_Taiwan";
        public static string _StrFeatureClassName_Penghu = "Section_{0}_Penghu";

        public static string _StrShapeFileName_Path = @"Z:\Layer\全國GIS地籍圖供應系統\村里";
        public static string _StrShapeFileName_Village = "VILLAGE";


        /// <summary>
        /// Log XML
        /// </summary>
        public static string _LogDirPath = @"Z:\COA\系統建置\RestSOE_COA_Data\Log\";
    }
}
