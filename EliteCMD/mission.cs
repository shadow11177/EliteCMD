using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteCMD
{
    public class mission
    {
        private int count = 0;
        private string locName = "";
        private string type = "";
        private string destSys = "";
        private string destStat = "";

        public int Count
        {
            get
            {
                return count;
            }

            set
            {
                count = value;
            }
        }

        public string DestSys
        {
            get
            {
                return destSys;
            }

            set
            {
                destSys = value;
            }
        }

        public string DestStat
        {
            get
            {
                return destStat;
            }

            set
            {
                destStat = value;
            }
        }

        public string LocName
        {
            get
            {
                return locName;
            }

            set
            {
                locName = value;
            }
        }

        public string Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

        public mission()
        {

        }
    }
}
