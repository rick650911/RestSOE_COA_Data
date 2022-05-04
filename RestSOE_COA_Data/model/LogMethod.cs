using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RestSOE_COA_Data.model
{
    [Serializable]
    public class LogMethod
    {
        string _DirPath = Setting._LogDirPath;

        private List<string> listError;

        public List<string> ListError
        {
            get
            {
                if (listError == null)
                    listError = new List<string>();
                return listError;
            }

            set
            {
                listError = value;
            }
        }

        private string _methodName;

        public LogMethod()
        { }
        public LogMethod(string argName)
        {
            _methodName = argName;
        }

        public void Add(string argStrMsg)
        {
            ListError.Add(argStrMsg);
        }

        public void Save()
        {
            Save(_DirPath, this, typeof(LogMethod));
        }

        private void Save(string argPath, Object argObject, Type argType)
        {
            try
            {
                if (Directory.Exists(argPath) != true)
                {
                    Directory.CreateDirectory(argPath);
                }
                FileStream fs = new FileStream(argPath + _methodName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xml", FileMode.Create);

                XmlSerializer xs = new XmlSerializer(argType);

                xs.Serialize(fs, argObject);

                fs.Close();
            }
            catch (Exception )
            {

            }
        }

    }
}
