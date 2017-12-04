using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace EliteCMD
{
    public class player
    {

        public delegate void missionEventHandler(object sender, missionEventArgs e);
        public event missionEventHandler OnMissionEvent;

        Timer MissionCountdown = new Timer();
        private bool changed = false;
        private string station = "";
        private string starSystem = "";
        private string docked = "True";
        private string loadFSD = "";
        private string ship = "";
        private string track = "";
        private string gameMode = ""; 
        private string starClass = "";
        private int cargoSpace = 0;
        private int cargo = 0;
        private int passengerCabbin = 0;
        private int passenger = 0;
        public Dictionary<double, mission> Missions = new Dictionary<double, mission>();
        public Dictionary<double, mission> ElapsingMissions = new Dictionary<double, mission>();
        private Dictionary<string, string> Ships = new Dictionary<string, string>();

        public string Station
        {
            get
            {
                return station;
            }

            set
            {
                if(station != value)
                {
                    changed = true;
                }
                station = value;
            }
        } //With changed Flag

        public string StarSystem
        {
            get
            {
                return starSystem;
            }

            set
            {
                if (starSystem != value)
                {
                    changed = true;
                }
                starSystem = value;
            }
        } //With changed Flag

        public string Docked
        {
            get
            {
                return docked;
            }

            set
            {
                if (docked != value)
                {
                    changed = true;
                }
                docked = value;
            }
        } //With changed Flag

        public string LoadFSD
        {
            get
            {
                return loadFSD;
            }

            set
            {
                if (loadFSD != value)
                {
                    changed = true;
                }
                loadFSD = value;
            }
        } //With changed Flag

        public string Ship
        {
            get
            {
                if (Ships.ContainsKey(ship))
                    return Ships[ship];
                return ship;
            }

            set
            {
                if (ship != value)
                {
                    changed = true;
                }
                ship = value;
            }
        } //With changed Flag and Dictionary

        public bool Changed
        {
            get
            {
                bool temp = changed;
                changed = false;
                return temp;
            }

            set
            {
                changed = value;
            }
        } //With changed Flag

        public string Track
        {
            get
            {
                return track;
            }

            set
            {
                if (track != value)
                {
                    changed = true;
                }
                track = value;
            }
        } //With changed Flag

        public int CargoSpace
        {
            get
            {
                return cargoSpace;
            }

            set
            {
                if (cargoSpace != value)
                {
                    changed = true;
                }
                cargoSpace = value;
            }
        } //With changed Flag

        public int Cargo
        {
            get
            {
                return cargo;
            }

            set
            {
                if (cargo != value)
                {
                    changed = true;
                }
                cargo = value;
            }
        } //With changed Flag

        public int PassengerCabbin
        {
            get
            {
                return passengerCabbin;
            }

            set
            {
                if (passengerCabbin != value)
                {
                    changed = true;
                }
                passengerCabbin = value;
            }
        } //With changed Flag

        public int Passenger
        {
            get
            {
                return passenger;
            }

            set
            {
                if (passenger != value)
                {
                    changed = true;
                }
                passenger = value;
            }
        } //With changed Flag

        public string GameMode
        {
            get
            {
                return gameMode;
            }

            set
            {
                if (gameMode != value)
                {
                    changed = true;
                }
                gameMode = value;
            }
        } //With changed Flag

        public string StarClass
        {
            get
            {
                return starClass;
            }

            set
            {
                if (starClass != value)
                {
                    changed = true;
                }
                starClass = value;
            }
        } //With changed Flag

        public player()
        {
            //Ship name dictionary
            Ships.Add("SideWinder",               "SideWinder"    );
            Ships.Add("Eagle",                    "Eagle"         );
            Ships.Add("Hauler",                   "Hauler"        );
            Ships.Add("Adder",                    "Adder"         );
            Ships.Add("Empire_Eagle",             "Imperial Eagle");
            Ships.Add("Viper",                    "Viper MkIII"   );
            Ships.Add("CobraMkIII",               "Cobra MkIII"   );
            Ships.Add("Viper_MkIV",               "Viper MkIV"    );
            Ships.Add("DiamondBack",              "DB Scout"      );
            Ships.Add("Type6",                    "TYPE-6"        );
            Ships.Add("Dolphin",                  "Dolphin"       );
            Ships.Add("DiamondBackXL",            "DB Explorer"   );
            //Imperial Courrier???
            Ships.Add("Independant_Trader",       "Keelback"      );
            Ships.Add("Asp_Scout",                "ASP Scout"     );
            Ships.Add("Vulture",                  "Vulture"       );
            Ships.Add("Asp",                      "ASP Explorer"  );
            Ships.Add("Federation_Dropship",      "Dropship"      );
            Ships.Add("Type7",                    "TYPE-7"        );
            Ships.Add("Cobra_MkIV",               "Cobra MkIV"    );
            Ships.Add("Federation_Dropship_MkII", "Assaultship"   );
            //Imperial Clipper???
            Ships.Add("Federation_Gunship",       "Gunship"       );
            Ships.Add("Orca",                     "Orca"          );
            Ships.Add("FerDeLance",               "Fer-De-Lance"  );
            Ships.Add("Python",                   "Python"        );
            Ships.Add("Type9",                    "TYPE-9"        );
            Ships.Add("BelugaLiner",              "Beluga Liner"  );
            Ships.Add("Anaconda",                 "Anaconda"      );
            Ships.Add("Federation_Corvette",      "Corvette"      );
            //Imperial Cutter???

            //GameMode name correction??
            //Track correction??
            MissionCountdown.AutoReset = true;
            MissionCountdown.Interval = 1000;
            MissionCountdown.Enabled = true;
            MissionCountdown.Elapsed += MissionCountdown_Elapsed;
            MissionCountdown.Start();
        }

        private void MissionCountdown_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dictionary<double, mission> oldM = ElapsingMissions;
            ElapsingMissions = Missions.Where(s => s.Value.Expiry < DateTime.Now.AddMinutes(-5)).ToDictionary(s => s.Key ,s => s.Value);
            if(oldM != ElapsingMissions)
            {
                MissionEvent(ElapsingMissions);
            }
        }
        
        public void MissionEvent(Dictionary<double, mission> em)
        {
            // Make sure someone is listening to event
            if (OnMissionEvent == null) return;

            missionEventArgs args = new missionEventArgs(em);
            OnMissionEvent(this, args);
        }
    }
}
