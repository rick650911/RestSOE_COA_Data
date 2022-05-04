using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RestSOE_COA_Data.model
{
    public enum ErrorType { 地籍查詢服務 }

    class ConnectMethod
    {
        public bool ClientRequest(string argUrl, string argPara, out string outStr)
        {
            //連接mapserver服務
            outStr = "";
            try
            {
                var encode = Encoding.UTF8;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(argUrl);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                //
                byte[] byteArray = encode.GetBytes(argPara);
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(byteArray, 0, byteArray.Length);
                }
                //
                string statusCode = "";
                using (WebResponse response = request.GetResponse())
                {
                    HttpWebResponse httpresponse = (HttpWebResponse)response;
                    statusCode = httpresponse.StatusCode.ToString();
                    using (StreamReader sr = new StreamReader(response.GetResponseStream(), encode))
                    {
                        outStr = sr.ReadToEnd();
                    }//end using 

                    httpresponse.Close();
                }
                
                if (statusCode == "OK")
                    return true;
                else
                    return false;

            }
            catch (Exception ex)
            {
                outStr = ex.Message;
            }

            return false;
        }


        //連接寄送通知錯誤

        public void ErrorMail(string argTitle, string argMsg, string argTag)
        {
            try
            {
                string str;
                string url = Setting._mailAPI;
                string para = string.Format("Title={0}&Msg={1}&Tag={2}",
                    argTitle,
                    string.Format("{0}<p>{1}</p>", argMsg, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    argTag);
                bool isConnt = ClientRequest(url, para, out str);
            }
            catch (Exception)
            {

            }


        }
    }
}
