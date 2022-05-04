using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSOE_COA_Data.model
{
 
    public class XYZ
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    /// <summary>
    ///群聚計算
    /// </summary>
    public class Cluster
    {
        public string City { get; set; }
        public string Town { get; set; }

        public string Sect { get; set; }

        public string Village { get; set; }

        public string CadAddr { get; set; }
        public string AppName { get; set; }
        public int count { get; set; }
        public double[] centerPoint { get; set; }
    }

    public class CityTown
    {
        public string COUNTYNAME { get; set; }
        public string TOWNNAME { get; set; }

        public string COUNTYID { get; set; }
        public string TOWNID { get; set; }
    }

    /// <summary>
    /// 回傳
    /// </summary>
    public class Result
    {
        private bool _status = false;
        
        public string msg { get; set; }

        public bool status
        {
            get
            {
                return _status;
            }

            set
            {
                _status = value;
            }
        }
    }

    public class ResultCalCadAddr : Result
    {
        public string cadaddr { get; set; }
    }

    public class ResultImageQueryQuality : Result
    {
        public double[] gps { get; set; }

        public string cadaddr { get; set; }

        public double direction { get; set; }

        public double pitchangle { get; set; }

        public string brightness { get; set; }

        public string quality { get; set; }

        public double distance { get; set; }

        public double targetscale { get; set; }
    }

    public class ResultGetOffLineMap : Result
    {
        public string fileUrl { get; set; }
    }

    public class ResultCluster : Result
    {
        public List<Cluster> Clusters { get; set; }
    }

}
