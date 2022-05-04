using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSOE_COA_Data.model
{
    public static class FeatureDataMethod
    {

        private static IFeatureWorkspace _FeatureClassWorkspace;

        public static IFeatureWorkspace FeatureClassWorkspace
        {
            get
            {
                return _FeatureClassWorkspace;
            }

            set
            {
                _FeatureClassWorkspace = value;
            }
        }

        public static IFeatureClass OpenGDBFeatureClass_Sect(string argVer, int index)
        {

            string argStrFeatureClassName_Land =
                index == 0 ? string.Format(Setting._StrFeatureClassName_Penghu, argVer) : string.Format(Setting._StrFeatureClassName_Taiwan, argVer);

            try
            {
                
                //只有106Q4版本
                if (FeatureClassWorkspace == null)
                {
                    string argStrFullName_GDB_Land = string.Format(Setting._StrFullName_GDB_Land, argVer);

                    IWorkspaceName pWorkspaceName_GDB_Land = (IWorkspaceName)new WorkspaceName();

                    pWorkspaceName_GDB_Land.PathName = argStrFullName_GDB_Land;
                    pWorkspaceName_GDB_Land.WorkspaceFactoryProgID = "esriDataSourcesFile.FileGDBWorkspaceFactory.1";

                    IWorkspace pWorkspace_GDB_Land;

                    pWorkspace_GDB_Land = pWorkspaceName_GDB_Land.WorkspaceFactory.OpenFromFile(argStrFullName_GDB_Land, 0);

                    //IFeatureWorkspace pFeatureClassWorkspace_GDB_Land;
                    _FeatureClassWorkspace = (IFeatureWorkspace)pWorkspace_GDB_Land;
                }


                IFeatureClass pFeatureClass_Land = FeatureClassWorkspace.OpenFeatureClass(argStrFeatureClassName_Land);
                return pFeatureClass_Land;
            }
            catch (Exception ex)
            {
                Console.WriteLine("開啟地籍圖發生錯誤!");

                return null;
            }


        }

        public static IFeatureClass OpenFeatureClass_Shapefile(string dataPath, string nameOfShapefile)
        {
            IWorkspaceFactory workspaceFactory = new ESRI.ArcGIS.DataSourcesFile.ShapefileWorkspaceFactory();
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspaceFactory.OpenFromFile(dataPath, 0);
            IFeatureClass featureClass = featureWorkspace.OpenFeatureClass(nameOfShapefile);

            return featureClass;
        }


    }
}
