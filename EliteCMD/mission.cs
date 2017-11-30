﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteCMD
{
    public class mission
    {
        private int count = 0;
        private string localisedName = "";
        private string name = "";
        private string faction = ""; 
        private string type = "";
        private string destSys = "";
        private string destStat = "";
        private string commodity = "";
        private string commodity_Localised = "";
        private string influence = "";
        private string reputation = "";
        private string reward = "";


        public int Count { get => count; set => count = value; }
        public string LocalisedName { get => localisedName; set => localisedName = value; }
        public string Type { get => type; set => type = value; }
        public string DestSys { get => destSys; set => destSys = value; }
        public string DestStat { get => destStat; set => destStat = value; }
        public string Name { get => name; set => name = value; }
        public string Faction { get => faction; set => faction = value; }
        public string Commodity { get => commodity; set => commodity = value; }
        public string Commodity_Localised { get => commodity_Localised; set => commodity_Localised = value; }
        public string Influence { get => influence; set => influence = value; }
        public string Reputation { get => reputation; set => reputation = value; }
        public string Reward { get => reward; set => reward = value; }

        public mission()
        {

        }
    }
}
