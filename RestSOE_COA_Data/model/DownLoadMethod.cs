using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSOE_COA_Data.model
{
    class DownLoadMethod
    {
        //public enum DownType {Raster_平地, Raster_完整, Vector }
        public enum DownType { Raster_輕量版, Raster_完整版, Vector }

        private string _ServerPath = Setting._ServerPath;//虛擬目錄
        private string _LocalPath = Setting._LocalPath;//0:type 1: 縣市

        public string GetDownURL(DownType argType, string argCity, string argTown, string argVer, out string strMsg)
        {
            string strLocalPath = "";
            string strFileName = "";

            string strResultPath = "";
            strMsg = "";

            strFileName = string.Format("{0}_{1}.zip", argCity, argTown);

            switch (argType)
            {
                //case DownType.Raster_平地:
                //    strLocalPath = string.Format(_LocalPath + @"\{2}\{3}", "Raster", "上線", "平地版", argCity);//Raster\上線\平地版  
                //    break;
                //case DownType.Raster_完整:
                //    strLocalPath = string.Format(_LocalPath + @"\{2}\{3}", "Raster", "上線", "完整版", argCity);//Raster\上線\完整版   Z:\MobileMap\{0}\{1}
                //    break;
                case DownType.Raster_輕量版:
                    strLocalPath = string.Format(_LocalPath + @"\{2}\{3}", "Raster", "上線", "輕量版", argCity);//Raster\上線\平地版  
                    break;
                case DownType.Raster_完整版:
                    strLocalPath = string.Format(_LocalPath + @"\{2}\{3}", "Raster", "上線", "完整版", argCity);//Raster\上線\完整版   Z:\MobileMap\{0}\{1}
                    break;
                case DownType.Vector:
                    //strFileName = string.Format("{0}_{1}.zip", argCity, argTown);
                    strLocalPath = string.Format(_LocalPath, DownType.Vector, strFileName);// @"Z:\MobileMap\{0}\{1}";//0:type 1: 縣市

                    if (string.IsNullOrEmpty(argVer))//not has version
                    {

                        //find latest version that has data
                        for (int i = LandVersionManager.ListVersion.Count - 1; i >= 0 ; i--)
                        {
                            argVer = LandVersionManager.ListVersion[i];

                            string strLocalPath_tmp = strLocalPath.Replace(DownType.Vector.ToString(), DownType.Vector.ToString() + "_" + argVer);

                            if (System.IO.File.Exists(strLocalPath_tmp))//version has data
                            {
                                strLocalPath = strLocalPath_tmp;
                                break;
                            }
                        }

                    }
                    else //(!string.IsNullOrEmpty(argVer))//has version
                    {
                        strLocalPath = strLocalPath.Replace(DownType.Vector.ToString(), DownType.Vector.ToString() + "_" + argVer);
                    }
                   

                    //if (System.IO.File.Exists(strLocalPath))
                    //    strResultPath = string.Format(_ServerPath, DownType.Vector, strFileName);
                    //else
                    //    strMsg = "尚未提供下載檔案!";
                    break;
            }

            
            string strSourceFilePath;
            if (argType == DownType.Raster_輕量版 || argType == DownType.Raster_完整版)
            {
                strSourceFilePath = strLocalPath + @"\" + strFileName;

                #region 壓縮方法 停用 節省時間所以先壓縮

                ////下載檔案編號()
                //string _id = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                ////壓縮加密檔案名稱(.zip)
                //strFileName = _id + ".zip";
                ////壓縮檔儲存位置_目錄
                //string strDate = DateTime.Now.ToString("yyyyMMdd");
                //string ZipFileToCreate = string.Format(_LocalPath, "download", strDate);
                ////欲壓縮的原始檔案位置_目錄
                //string DirectoryToZip = strLocalPath;

                ////檢查要壓縮目錄位置是否存在
                //if (System.IO.Directory.Exists(DirectoryToZip) == false)
                //{
                //    strMsg = "尚未提供縣市下載檔案!";
                //    return strResultPath;
                //}
                ////檢查下載檔案位置目錄
                //if (System.IO.Directory.Exists(ZipFileToCreate) == false)
                //    System.IO.Directory.CreateDirectory(ZipFileToCreate);

                //bool ishaveFile = false;
                //using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(System.Text.Encoding.Default))
                //{
                //    foreach (var item in System.IO.Directory.GetFiles(strLocalPath))
                //    {
                //        if (item.Contains(argTown))
                //        {
                //            zip.AddFile(item, "");
                //            ishaveFile = true;
                //        }
                //    }

                //    if (ishaveFile)
                //    {
                //        zip.Save(ZipFileToCreate + "\\" + strFileName);//ZipFileToCreate + _FileName
                //        strResultPath = string.Format(_ServerPath, "download/" + strDate + "/", strFileName);
                //    }
                //}

                #endregion

            }
            else
                strSourceFilePath = strLocalPath;
            

            //先檢查來源檔案
            if (System.IO.File.Exists(strSourceFilePath) == false)
            {
                strMsg = string.Format("{0}_{1}尚未提供下載檔案!", argCity, argTown);
                return strResultPath;
            }

            string strDate = DateTime.Now.ToString("yyyyMMdd");
            string FileToCreate = string.Format(_LocalPath, "download", strDate);
            //下載檔案名稱
            string _id = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string copyFileName = _id +"_"+argVer+ ".zip";
            string strFileToCreateFilePath = FileToCreate + @"\" + copyFileName;

            //檢查下載檔案位置目錄
            if (System.IO.Directory.Exists(FileToCreate) == false)
                System.IO.Directory.CreateDirectory(FileToCreate);

            //不可覆寫相同檔名
            System.IO.File.Copy(strSourceFilePath, strFileToCreateFilePath);


            if (System.IO.File.Exists(strFileToCreateFilePath))
                strResultPath = string.Format(_ServerPath, "download/" + strDate + "/", copyFileName);
            else
                strMsg = "尚未提供下載檔案!";

            return strResultPath;
        }

    }
}
