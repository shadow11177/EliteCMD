using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Codeplex.Data;
using System.Windows;
using System.Threading;

namespace EliteCMD
{
    class EliteAPI
    {

        public delegate void apiEventHandler(object sender, apiEventArgs e);
        public event apiEventHandler OnApiEvent;

        Thread wrk;

        private string pathToLog = Environment.GetEnvironmentVariable("userprofile") + "\\Saved Games\\Frontier Developments\\Elite Dangerous";
        private FileInfo currentLog;
        private player pl = new player();
        //private string plStation = "";
        //private string plStarSystem = "";
        //private string plDocked = "True";
        //private string plLoadFSD = "False";
        //private string plShip = "";
        private bool run = true;
        private long currPos = 0;


        public EliteAPI()
        {
            
        }

        public void Run()
        {
            wrk = new Thread(work);
            wrk.Start();
        }

        public void Stop()
        {
            run = false;
        }

        private void work()
        {
            refresh();
            while (run)
            {
                Thread.Sleep(2000);
                //currentLog.Length
                DirectoryInfo dir = new DirectoryInfo(pathToLog);
                FileInfo fi = dir.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
                if(currentLog.FullName == fi.FullName)
                {
                   
                        currentLog = fi;
                        refresh();
                }
                else
                {
                    currPos = 0;
                    refresh();
                }
            }
        }

        private void refresh()
        {
            bool newevent = false;
            DirectoryInfo dir = new DirectoryInfo(pathToLog);
            currentLog = dir.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
            Stream fs = new FileStream(currentLog.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fs.Position = currPos;
            currPos = fs.Length;
            StreamReader file = new StreamReader(fs);
            while (!file.EndOfStream)
            {
                string line = file.ReadLine();
                if (line.Length > 10)
                {
                    if (parseLine(line))
                    {
                        newevent = true;
                    }
                }
            }

            if (newevent)
            {
                ApiEvent(pl);
            }
        }

        public void ApiEvent(player pl)
        {
            // Make sure someone is listening to event
            if (OnApiEvent == null) return;

            apiEventArgs args = new apiEventArgs(pl);
            OnApiEvent(this, args);
        }

        private bool parseLine(string Line)
        {
            dynamic json = DynamicJson.Parse(Line);
            if (json["event"] == "Docked")                                          //Docked
            {
                //{ "timestamp":"2017-11-29T16:06:06Z", "event":"Docked", "StationName":"Chelbin Service Station", "StationType":"Orbis", "StarSystem":"Wolf 397", "StationFaction":"Wolf 397 Independents", "StationGovernment":"$government_Democracy;", "StationGovernment_Localised":"Demokratie", "StationAllegiance":"Alliance", "StationServices":[ "Dock", "Autodock", "BlackMarket", "Commodities", "Contacts", "Exploration", "Missions", "Outfitting", "CrewLounge", "Rearm", "Refuel", "Repair", "Shipyard", "Tuning", "MissionsGenerated", "FlightController", "StationOperations", "Powerplay", "SearchAndRescue" ], "StationEconomy":"$economy_Agri;", "StationEconomy_Localised":"Landwirtschaft", "DistFromStarLS":135.359375 }
                pl.Station = json.StationName;
                pl.StarSystem = json.StarSystem;
                pl.Docked = "docked";
                pl.Track = "";
            }
            else if (json["event"] == "Undocked")                                   //Undocked
            {
                //{ "timestamp":"2017-11-29T16:01:22Z", "event":"Undocked", "StationName":"Trophy Camp", "StationType":"SurfaceStation" }
                pl.Docked = "undocked";
            }
            else if (json["event"] == "StartJump")                                  //Start Jump
            {
                //{ "timestamp":"2017-11-29T16:01:57Z", "event":"StartJump", "JumpType":"Supercruise" }
                //{ "timestamp":"2017-11-29T16:49:29Z", "event":"StartJump", "JumpType":"Hyperspace", "StarSystem":"EGGR 431", "StarClass":"F" }
                pl.LoadFSD = "LoadFSD";
                if (json.IsDefined("StarClass"))
                {
                    pl.StarClass = json.StarClass + "-Star";
                    pl.StarSystem = json.StarSystem;
                }
            }
            else if (json["event"] == "FSDJump")                                    //FSD Jump
            {
                //{ "timestamp":"2017-11-29T16:49:47Z", "event":"FSDJump", "StarSystem":"EGGR 431", "StarPos":[31.688,36.750,-28.406], "SystemAllegiance":"Federation", "SystemEconomy":"$economy_HighTech;", "SystemEconomy_Localised":"Hightech", "SystemGovernment":"$government_Corporate;", "SystemGovernment_Localised":"Konzernpolitik", "SystemSecurity":"$SYSTEM_SECURITY_high;", "SystemSecurity_Localised":"Hohe Sicherheit", "Population":38932317, "Powers":[ "Felicia Winters" ], "PowerplayState":"Exploited", "JumpDist":46.869, "FuelUsed":4.846728, "FuelLevel":27.153273, "Factions":[ { "Name":"United EGGR 431 Free", "FactionState":"Boom", "Government":"Democracy", "Influence":0.170170, "Allegiance":"Federation" }, { "Name":"Pilots Federation Local Branch", "FactionState":"None", "Government":"Democracy", "Influence":0.000000, "Allegiance":"PilotsFederation" }, { "Name":"Olorun Republic Party", "FactionState":"None", "Government":"Democracy", "Influence":0.103103, "Allegiance":"Federation", "PendingStates":[ { "State":"Boom", "Trend":0 } ] }, { "Name":"Workers of AZ Cancri Democrats", "FactionState":"None", "Government":"Democracy", "Influence":0.090090, "Allegiance":"Federation" }, { "Name":"Bureau of EGGR 431", "FactionState":"Boom", "Government":"Dictatorship", "Influence":0.064064, "Allegiance":"Independent", "RecoveringStates":[ { "State":"Outbreak", "Trend":0 } ] }, { "Name":"EGGR 431 Systems", "FactionState":"Boom", "Government":"Corporate", "Influence":0.405405, "Allegiance":"Federation" }, { "Name":"EGGR 431 Silver Drug Empire", "FactionState":"Boom", "Government":"Anarchy", "Influence":0.048048, "Allegiance":"Independent", "PendingStates":[ { "State":"Outbreak", "Trend":0 } ] }, { "Name":"eclipse empire", "FactionState":"Boom", "Government":"Democracy", "Influence":0.119119, "Allegiance":"Federation" } ], "SystemFaction":"EGGR 431 Systems", "FactionState":"Boom" }
                pl.LoadFSD = "";
                pl.Station = "";
                pl.StarSystem = json.StarSystem;
            }
            else if (json["event"] == "SupercruiseExit")                            //Supercruise Exit
            {
                //{ "timestamp":"2017-11-29T16:02:25Z", "event":"SupercruiseExit", "StarSystem":"Wolf 397", "Body":"Trus Madi", "BodyType":"Planet" }
                //{ "timestamp":"2017-11-29T16:05:04Z", "event":"SupercruiseExit", "StarSystem":"Wolf 397", "Body":"Chelbin Service Station", "BodyType":"Station" }
                pl.Station = "";
                pl.StarSystem = json.StarSystem;
                pl.Station = json.Body;
            }
            else if (json["event"] == "SupercruiseEntry")                           //Supercruise Entry
            {
                //{ "timestamp":"2017-11-29T17:10:08Z", "event":"SupercruiseEntry", "StarSystem":"Muang" }
                pl.LoadFSD = "";
                pl.Station = "";
                pl.StarSystem = json.StarSystem;
            }
            else if (json["event"] == "Loadout")                                    //Loadout
            {
                //{ "timestamp":"2017-11-29T15:56:53Z", "event":"Loadout", "Ship":"Federation_Corvette", "ShipID":9, "ShipName":"", "ShipIdent":"", "Modules":[ { "Slot":"HugeHardpoint1", "Item":"Hpt_BeamLaser_Fixed_Huge", "On":true, "Priority":0, "Health":1.000000, "Value":2102631 }, { "Slot":"HugeHardpoint2", "Item":"Hpt_BeamLaser_Fixed_Huge", "On":true, "Priority":0, "Health":1.000000, "Value":2102631 }, { "Slot":"LargeHardpoint1", "Item":"Hpt_BeamLaser_Fixed_Large", "On":true, "Priority":0, "Health":1.000000, "Value":1033344 }, { "Slot":"MediumHardpoint1", "Item":"Hpt_MultiCannon_Gimbal_Medium", "On":true, "Priority":0, "AmmoInClip":90, "AmmoInHopper":2100, "Health":1.000000, "Value":50018, "EngineerBlueprint":"Weapon_Efficient", "EngineerLevel":2 }, { "Slot":"MediumHardpoint2", "Item":"Hpt_MultiCannon_Gimbal_Medium", "On":true, "Priority":0, "AmmoInClip":90, "AmmoInHopper":2100, "Health":1.000000, "Value":50018, "EngineerBlueprint":"Weapon_Sturdy", "EngineerLevel":2 }, { "Slot":"SmallHardpoint1", "Item":"Hpt_MultiCannon_Gimbal_Small", "On":true, "Priority":0, "AmmoInClip":90, "AmmoInHopper":2100, "Health":1.000000, "Value":12505 }, { "Slot":"SmallHardpoint2", "Item":"Hpt_MultiCannon_Gimbal_Small", "On":true, "Priority":0, "AmmoInClip":90, "AmmoInHopper":2100, "Health":1.000000, "Value":12505 }, { "Slot":"TinyHardpoint1", "Item":"Hpt_ShieldBooster_Size0_Class5", "On":true, "Priority":0, "Health":1.000000, "Value":246578 }, { "Slot":"TinyHardpoint2", "Item":"Hpt_ShieldBooster_Size0_Class5", "On":true, "Priority":0, "Health":1.000000, "Value":246578 }, { "Slot":"TinyHardpoint3", "Item":"Hpt_ShieldBooster_Size0_Class5", "On":true, "Priority":0, "Health":1.000000, "Value":246578 }, { "Slot":"TinyHardpoint4", "Item":"Hpt_ShieldBooster_Size0_Class5", "On":true, "Priority":0, "Health":1.000000, "Value":246578 }, { "Slot":"TinyHardpoint5", "Item":"Hpt_PlasmaPointDefence_Turret_Tiny", "On":true, "Priority":0, "AmmoInClip":12, "AmmoInHopper":10000, "Health":1.000000, "Value":16275 }, { "Slot":"TinyHardpoint6", "Item":"Hpt_PlasmaPointDefence_Turret_Tiny", "On":true, "Priority":0, "AmmoInClip":12, "AmmoInHopper":10000, "Health":1.000000, "Value":16275 }, { "Slot":"TinyHardpoint7", "Item":"Hpt_PlasmaPointDefence_Turret_Tiny", "On":true, "Priority":0, "AmmoInClip":12, "AmmoInHopper":10000, "Health":1.000000, "Value":16275 }, { "Slot":"TinyHardpoint8", "Item":"Hpt_PlasmaPointDefence_Turret_Tiny", "On":true, "Priority":0, "AmmoInClip":12, "AmmoInHopper":10000, "Health":1.000000, "Value":16275 }, { "Slot":"Armour", "Item":"Federation_Corvette_Armour_Grade2", "On":true, "Priority":1, "Health":1.000000, "Value":65977277, "EngineerBlueprint":"Armour_Advanced", "EngineerLevel":1 }, { "Slot":"PaintJob", "Item":"PaintJob_Federation_Corvette_BlackFriday_01", "On":true, "Priority":1, "Health":1.000000, "Value":0 }, { "Slot":"PowerPlant", "Item":"Int_Powerplant_Size8_Class5", "On":true, "Priority":1, "Health":1.000000, "Value":142669643 }, { "Slot":"MainEngines", "Item":"Int_Engine_Size7_Class5", "On":true, "Priority":0, "Health":1.000000, "Value":45006196 }, { "Slot":"FrameShiftDrive", "Item":"Int_Hyperdrive_Size6_Class5", "On":true, "Priority":0, "Health":1.000000, "Value":14197539 }, { "Slot":"LifeSupport", "Item":"Int_LifeSupport_Size5_Class5", "On":true, "Priority":0, "Health":1.000000, "Value":1089257 }, { "Slot":"PowerDistributor", "Item":"Int_PowerDistributor_Size8_Class5", "On":true, "Priority":0, "Health":1.000000, "Value":23911341 }, { "Slot":"Radar", "Item":"Int_Sensors_Size8_Class2", "On":true, "Priority":0, "Health":1.000000, "Value":1530326 }, { "Slot":"FuelTank", "Item":"Int_FuelTank_Size5_Class3", "On":true, "Priority":1, "Health":1.000000, "Value":85779 }, { "Slot":"Slot01_Size7", "Item":"Int_ShieldGenerator_Size7_Class3_Fast", "On":true, "Priority":0, "Health":1.000000, "Value":7501033 }, { "Slot":"Slot02_Size7", "Item":"Int_FighterBay_Size7_Class1", "On":true, "Priority":0, "Health":1.000000, "Value":2079079 }, { "Slot":"Slot03_Size7", "Item":"Int_CargoRack_Size7_Class1", "On":true, "Priority":1, "Health":1.000000, "Value":1034064 }, { "Slot":"Slot04_Size6", "Item":"Int_ShieldCellBank_Size6_Class3", "On":true, "Priority":0, "AmmoInClip":1, "AmmoInHopper":4, "Health":1.000000, "Value":487987 }, { "Slot":"Slot05_Size6", "Item":"Int_FSDInterdictor_Size4_Class5", "On":true, "Priority":0, "Health":1.000000, "Value":18723520 }, { "Slot":"Slot06_Size5", "Item":"Int_BuggyBay_Size4_Class2", "On":true, "Priority":1, "Health":1.000000, "Value":75816 }, { "Slot":"Slot07_Size5", "Item":"Int_Repairer_Size5_Class5", "On":true, "Priority":1, "AmmoInClip":6700, "Health":1.000000, "Value":7461433 }, { "Slot":"Slot08_Size4", "Item":"Int_FuelScoop_Size4_Class5", "On":true, "Priority":1, "Health":1.000000, "Value":2790805 }, { "Slot":"Slot09_Size4", "Item":"Int_StellarBodyDiscoveryScanner_Advanced", "On":true, "Priority":1, "Health":1.000000, "Value":1355738 }, { "Slot":"Slot10_Size3", "Item":"Int_DetailedSurfaceScanner_Tiny", "On":true, "Priority":1, "Health":1.000000, "Value":219375 }, { "Slot":"Military01", "Item":"Int_HullReinforcement_Size5_Class2", "On":true, "Priority":1, "Health":1.000000, "Value":394875, "EngineerBlueprint":"HullReinforcement_Advanced", "EngineerLevel":1 }, { "Slot":"Military02", "Item":"Int_HullReinforcement_Size5_Class2", "On":true, "Priority":1, "Health":1.000000, "Value":394875, "EngineerBlueprint":"HullReinforcement_Advanced", "EngineerLevel":1 }, { "Slot":"Decal1", "Item":"Decal_Trade_Elite", "On":true, "Priority":1, "Health":1.000000, "Value":0 }, { "Slot":"Decal2", "Item":"Decal_Explorer_Pathfinder", "On":true, "Priority":1, "Health":1.000000, "Value":0 }, { "Slot":"Decal3", "Item":"Decal_Combat_Competent", "On":true, "Priority":1, "Health":1.000000, "Value":0 }, { "Slot":"PlanetaryApproachSuite", "Item":"Int_PlanetApproachSuite", "On":true, "Priority":1, "Health":1.000000, "Value":438 }, { "Slot":"ShipCockpit", "Item":"Federation_Corvette_Cockpit", "On":true, "Priority":1, "Health":1.000000, "Value":0 }, { "Slot":"CargoHatch", "Item":"ModularCargoBayDoor", "On":true, "Priority":1, "Health":1.000000, "Value":0 } ] }
                pl.Ship = json.Ship;
                int cargospace = 0;
                int passengerCabbin = 0;
                foreach (dynamic mod in json.Modules)
                {
                    if (mod.Item.Contains("Int_CargoRack_"))
                    {
                        int mul = Convert.ToInt32(mod.Item.Substring(18, 1));
                        cargospace += (int)Math.Pow(2, mul);
                    } else if (mod.Item.Contains("PassengerCabin"))
                    {
                        passengerCabbin += mod.AmmoInClip;
                    }
                }
                pl.CargoSpace = cargospace;
                pl.PassengerCabbin = passengerCabbin;
            }
            else if (json["event"] == "Cargo")                                      //Cargo
            {
                //{ "timestamp":"2017-11-29T16:24:40Z", "event":"Cargo", "Inventory":[  ] }
                //{ "timestamp":"2017-10-12T11:27:06Z", "event":"Cargo", "Inventory":[ { "Name":"bromellite", "Count":4 } ] }
                int cargo = 0;
                foreach (dynamic mod in json.Inventory)
                {
                    cargo += Convert.ToInt32(mod.Count);
                }
                pl.Cargo = cargo;
            }
            else if (json["event"] == "MarketSell")                                 //Market Sell
            {
                //{ "timestamp":"2017-11-29T18:32:12Z", "event":"MarketSell", "Type":"fujintea", "Count":10, "SellPrice":16775, "TotalSale":167750, "AvgPricePaid":1146 }
                pl.Cargo -= json["Count"];
            }
            else if (json["event"] == "MarketBuy")                                  //Market Buy
            {
                //{ "timestamp":"2017-10-09T09:36:33Z", "event":"MarketBuy", "Type":"lithium", "Count":77, "BuyPrice":1201, "TotalCost":92477 }
                pl.Cargo += json["Count"];
            }
            else if (json["event"] == "MissionAccepted")                            //Mission Accepted
            {
                //{ "timestamp":"2017-11-16T13:39:30Z", "event":"MissionAccepted", "Faction":"Upsilon Aquarii Services", "Name":"Mission_Altruism", "LocalisedName":"54 Einheiten spenden: Halbleiter", "Commodity":"$Semiconductors_Name;", "Commodity_Localised":"Halbleiter", "Count":54, "DestinationSystem":"Upsilon Aquarii", "DestinationStation":"Allen Hub", "Expiry":"2017-11-18T04:09:59Z", "Influence":"Low", "Reputation":"High", "MissionID":246921122 }
                //{ "timestamp":"2017-11-23T11:49:51Z", "event":"MissionAccepted", "Faction":"Workers of HIP 67882 Democrats", "Name":"Chain_HelpFinishTheOrder", "LocalisedName":"2 Einheiten liefern: Silber", "Commodity":"$Silver_Name;", "Commodity_Localised":"Silber", "Count":2, "DestinationSystem":"Ochosi", "DestinationStation":"Sheckley Holdings", "Expiry":"2017-11-24T11:49:14Z", "Influence":"Low", "Reputation":"Low", "Reward":289475, "MissionID":250996387 }
                //{ "timestamp":"2017-11-23T11:58:36Z", "event":"MissionAccepted", "Faction":"Chakpa Purple Travel Exchange", "Name":"Mission_AltruismCredits_CivilUnrest", "LocalisedName":"1.000.000 CR Beschaffen, um zivile Unruhen einzudämmen", "Expiry":"2017-11-23T15:39:24Z", "Influence":"Low", "Reputation":"High", "MissionID":250998729 }
                //{ "timestamp":"2017-11-17T10:35:23Z", "event":"MissionAccepted", "Faction":"Upsilon Aquarii Services", "Name":"Mission_AltruismCredits", "LocalisedName":"1.000.000 CR für die Sache spenden", "Expiry":"2017-11-17T13:39:02Z", "Influence":"Low", "Reputation":"High", "MissionID":247404125 }
                //{ "timestamp":"2017-11-23T11:52:05Z", "event":"MissionAccepted", "Faction":"Chakpa Purple Travel Exchange", "Name":"Mission_Courier_RankFed", "LocalisedName":"Kurierauftrag der föderalen Flotte verfügbar", "DestinationSystem":"Ochosi", "DestinationStation":"Perry's Folly", "Expiry":"2017-11-24T11:51:44Z", "Influence":"Low", "Reputation":"Med", "Reward":84736, "MissionID":250997004 }
                //{ "timestamp":"2017-11-16T13:41:18Z", "event":"MissionAccepted", "Faction":"Upsilon Aquarii PLC", "Name":"Mission_Massacre_Conflict_War", "LocalisedName":"Schiffe der Upsilon Aquarii Nationalists-Fraktion massakrieren", "TargetFaction":"Upsilon Aquarii Nationalists", "KillCount":4, "DestinationSystem":"Upsilon Aquarii", "DestinationStation":"Allen Hub", "Expiry":"2017-11-17T09:47:13Z", "Influence":"High", "Reputation":"Med", "Reward":133226, "MissionID":246921671 }
                //{ "timestamp":"2017-11-07T11:18:50Z", "event":"MissionAccepted", "Faction":"Upsilon Aquarii Free", "Name":"Mission_PassengerBulk_REFUGEE_LEAVING", "LocalisedName":"9 Flüchtlinge will nach  ", "DestinationSystem":"LTT 9360", "DestinationStation":"Smeaton Orbital", "Expiry":"2017-11-07T13:52:20Z", "Influence":"High", "Reputation":"Low", "Reward":5674436, "PassengerCount":9, "PassengerVIPs":false, "PassengerWanted":false, "PassengerType":"Refugee", "MissionID":241744417 }
                //{ "timestamp":"2017-10-12T11:46:06Z", "event":"MissionAccepted", "Faction":"Traditional Rhea Front", "Name":"Mission_PassengerVIP", "LocalisedName":"Transport Jordin Hinton", "Commodity":"$ConsumerTechnology_Name;", "Commodity_Localised":"Unterhaltungselektronik", "Count":3, "DestinationSystem":"LQ Hydrae", "DestinationStation":"Wohler Horizons", "Expiry":"2017-10-12T14:10:29Z", "Influence":"Med", "Reputation":"High", "Reward":452992, "PassengerCount":6, "PassengerVIPs":true, "PassengerWanted":false, "PassengerType":"Tourist", "MissionID":225273391 }
                double id = json.MissionID;
                mission ms = new mission();
                
                ms.Faction = json.Faction;
                ms.Name = json.Name;
                ms.Reputation = json.Reputation;
                ms.Influence = json.Influence;
                ms.LocalisedName = json.LocalisedName;
                if (json.IsDefined("Reward"))
                {
                    ms.Reward = json.Reward;
                }
                if (json.IsDefined("Commodity"))     //Cargo count 
                {
                    if(json.Name != "Mission_Altruism" && json.Name != "Mission_PassengerVIP")
                    {
                        pl.Cargo += json["Count"];
                        ms.Type = "cargo";
                    }
                    ms.Commodity = json.Commodity;
                    ms.Count = (int)json["Count"];
                    ms.Commodity_Localised = json.Commodity_Localised;
                }
                if (json.IsDefined("PassengerCount"))
                {
                    pl.Passenger += json["PassengerCount"];
                    ms.Count = (int)json["PassengerCount"];
                    ms.Type = "passenger";
                }
                if (json.IsDefined("DestinationStation"))
                {
                    ms.DestStat = json["DestinationStation"];
                }
                if (json.IsDefined("DestinationSystem"))
                {
                    ms.DestSys = json["DestinationSystem"];
                }
                pl.Missions.Add(id, ms);
            }
            else if (json["event"] == "MissionCompleted")                           //Mission Completed
            {
                //{ "timestamp":"2017-11-23T11:47:01Z", "event":"MissionCompleted", "Faction":"Partnership of HIP 67086", "Name":"Mission_AltruismCredits_name", "MissionID":250995436, "Donation":1000000 }
                //{ "timestamp":"2017-11-07T13:14:10Z", "event":"MissionCompleted", "Faction":"Partnership of LTT 9315", "Name":"Mission_PassengerBulk_POLPRISONER_ARRIVING_name", "MissionID":241759887, "DestinationSystem":"LTT 9360", "DestinationStation":"Smeaton Orbital", "Reward":22094226 }
                //{ "timestamp":"2017-11-23T11:59:19Z", "event":"MissionCompleted", "Faction":"Partnership of HIP 67086", "Name":"Chain_HelpFinishTheOrder_name", "MissionID":250997069, "Commodity":"$Explosives_Name;", "Commodity_Localised":"Sprengstoffe", "Count":3, "DestinationSystem":"Ochosi", "DestinationStation":"Perry's Folly", "Reward":564393 }
                double id = json.MissionID;
                
                if(pl.Missions.ContainsKey(id)) //Passenger Missions dont give Leaving passengers count
                {
                    if(pl.Missions[id].Type == "cargo")
                    {
                        pl.Passenger -= pl.Missions[id].Count;
                    }
                    else if(pl.Missions[id].Type == "passenger")
                    {
                        pl.Cargo -= json["Count"];
                    }
                    pl.Missions.Remove(id);
                }

                if (json.IsDefined("CommodityReward")) //Added cargo as Mission reward
                {
                    foreach(dynamic com in json["CommodityReward"])
                        pl.Cargo += com["Count"];
                }
            }
            else if (json["event"] == "DockingGranted")                             //Docking Granted
            {
                //{ "timestamp":"2017-11-29T16:05:29Z", "event":"DockingGranted", "LandingPad":9, "StationName":"Chelbin Service Station" }
                pl.Docked = "docking Granted";
                pl.Track = "Pad: " + json.LandingPad;
            }
            else if (json["event"] == "Music")                                      //Music (Activity)
            {
                //{ "timestamp":"2017-11-29T16:05:05Z", "event":"Music", "MusicTrack":"DestinationFromSupercruise" }
                //{ "timestamp":"2017-11-29T16:05:10Z", "event":"Music", "MusicTrack":"NoTrack" }
                //{ "timestamp":"2017-11-29T17:10:08Z", "event":"Music", "MusicTrack":"Supercruise" }
                if (json.MusicTrack == "NoTrack")
                {
                    pl.Track = "";
                }
                else
                {
                    pl.Track = json.MusicTrack;
                    if(pl.Track == "MainMenu")
                    {
                        pl.GameMode = "Offline";
                    }
                }
            }
            else if (json["event"] == "DockingRequested")                           //Docking Requested
            {
                //{ "timestamp":"2017-11-29T16:05:27Z", "event":"DockingRequested", "StationName":"Chelbin Service Station" }
                pl.Track = "Requested";
                pl.Station = json.StationName;
                pl.Docked = "docking";
            }
            else if (json["event"] == "Passengers")                                 //Passengers
            {
                //{ "timestamp":"2017-11-07T11:19:37Z", "event":"Passengers", "Manifest":[ { "MissionID":241744417, "Type":"Refugee", "VIP":false, "Wanted":false, "Count":9 }, { "MissionID":241744483, "Type":"Refugee", "VIP":false, "Wanted":false, "Count":11 } ] }
                int passengers = 0;
                foreach (dynamic mod in json.Manifest)
                {
                    passengers += mod.Count;
                    double id = mod.MissionID;

                    if (!pl.Missions.ContainsKey(id))                                //save Passenger Missions for count
                    {
                        mission ms = new mission();
                        ms.Type = "passenger";
                        ms.Count = (int)mod.Count;
                        pl.Missions.Add(id, ms);
                    }
                }
                pl.Passenger = passengers;
            }
            else if (json["event"] == "MissionAbandoned")                           //Mission Abandoned
            {
                //{ "timestamp":"2017-11-23T10:23:23Z", "event":"MissionAbandoned", "Name":"Mission_Scan_CivilUnrest_name", "MissionID":250974655 }
                double id = json.MissionID;
                if (pl.Missions.ContainsKey(id)) //Passenger Missions dont give Leaving passengers
                {
                    if (pl.Missions[id].Type == "passenger")
                    {
                        pl.Passenger -= pl.Missions[id].Count;
                    }
                    pl.Missions.Remove(id);
                }
            }
            else if (json["event"] == "LoadGame")                                  //Load Game
            {
                //{ "timestamp":"2017-11-29T15:56:53Z", "event":"LoadGame", "Commander":"Shadow1117", "Ship":"Federation_Corvette", "ShipID":9, "ShipName":"", "ShipIdent":"", "FuelLevel":32.000000, "FuelCapacity":32.000000, "GameMode":"Open", "Credits":107193064, "Loan":0 }
                pl.GameMode = json.GameMode;
            }
            else if (json["event"] == "ModuleBuy")                                  //Module Buy
            {
                //{ "timestamp":"2017-11-29T18:30:16Z", "event":"ModuleBuy", "Slot":"Slot04_Size3", "BuyItem":"$int_buggybay_size2_class1_name;", "BuyItem_Localised":"Planetenfahrzeug-Hangar", "BuyPrice":17550, "Ship":"asp", "ShipID":5 }
                //{ "timestamp":"2017-11-28T17:20:56Z", "event":"ModuleBuy", "Slot":"Slot05_Size6", "StoredItem":"$int_fsdinterdictor_size4_class5_name;", "StoredItem_Localised":"FSA-Unterbrecher", "BuyItem":"$int_cargorack_size6_class1_name;", "BuyItem_Localised":"Frachtgestell", "BuyPrice":353527, "Ship":"federation_corvette", "ShipID":9 }
                if (json.BuyItem.Contains("Int_CargoRack_"))
                {
                    int mul = Convert.ToInt32(json.BuyItem.Substring(18, 1));
                    pl.CargoSpace += (int)Math.Pow(2, mul);
                }
                else if (json.BuyItem.Contains("PassengerCabin"))
                {
                    pl.PassengerCabbin += json.AmmoInClip;
                }
                if (json.IsDefined("StoredItem"))
                {
                    if (json.StoredItem.Contains("Int_CargoRack_"))
                    {
                        int mul = Convert.ToInt32(json.StoredItem.Substring(18, 1));
                        pl.CargoSpace += (int)Math.Pow(2, mul);
                    }
                    else if (json.StoredItem.Contains("PassengerCabin"))
                    {
                        pl.PassengerCabbin -= json.StoredItem_AmmoInClip;
                    }
                }
            }
            else if (json["event"] == "ModuleSell")                                  //Module Sell
            {
                //{ "timestamp":"2017-11-29T18:32:28Z", "event":"ModuleSell", "Slot":"Slot02_Size5", "SellItem":"$int_cargorack_size5_class1_name;", "SellItem_Localised":"Frachtgestell", "SellPrice":108777, "Ship":"asp", "ShipID":5 }
                //{ "timestamp":"2017-11-28T17:31:16Z", "event":"ModuleSell", "Slot":"Slot05_Size6", "SellItem":"$int_cargorack_size6_class1_name;", "SellItem_Localised":"Frachtgestell", "SellPrice":353527, "Ship":"federation_corvette", "ShipID":9 }
                if (json.SellItem.Contains("Int_CargoRack_"))
                {
                    int mul = Convert.ToInt32(json.SellItem.Substring(18, 1));
                    pl.CargoSpace += (int)Math.Pow(2, mul);
                }
                else if (json.SellItem.Contains("PassengerCabin"))
                {
                    pl.PassengerCabbin += json.AmmoInClip;
                }
            }
            else if (json["event"] == "EngineerContribution")                                 //Engineer Contribution
            {
                //{ "timestamp":"2017-11-28T17:30:12Z", "event":"EngineerContribution", "Engineer":"Liz Ryder", "Type":"Commodity", "Commodity":"landmines", "Quantity":200, "TotalQuantity":200 }
                if(json.Type == "Commodity")
                {
                    pl.Cargo -= json.Quantity;
                }
            }
            else if (json["event"] == "MissionFailed")                                 //Engineer Contribution
            {
                //{ "timestamp":"2017-11-06T10:55:43Z", "event":"MissionFailed", "Name":"Mission_PassengerBulk_REFUGEE_LEAVING_name", "MissionID":241111985 }
                double id = json.MissionID;
                if (pl.Missions.ContainsKey(id)) //Passenger Missions dont give Leaving passengers
                {
                    if (pl.Docked == "docked")
                    {
                        pl.Passenger -= pl.Missions[id].Count;
                    }
                    pl.Missions.Remove(id);
                }
            }


            //{ "timestamp":"2017-11-06T10:55:54Z", "event":"PayFines", "Amount":500 }
            //{ "timestamp":"2017-11-06T10:56:03Z", "event":"RedeemVoucher", "Type":"bounty", "Amount":179564, "Factions":[ { "Faction":"", "Amount":281 }, { "Faction":"", "Amount":59841 }, { "Faction":"", "Amount":60138 }, { "Faction":"", "Amount":59304 } ], "BrokerPercentage":25.000000 }
            //{ "timestamp":"2017-11-29T18:32:41Z", "event":"ModuleRetrieve", "Slot":"Slot02_Size5", "RetrievedItem":"$int_fueltank_size5_class3_name;", "RetrievedItem_Localised":"Treibstofftank", "Ship":"asp", "ShipID":5 }
            //{ "timestamp":"2017-11-29T15:56:53Z", "event":"Materials", "Raw":[ { "Name":"selenium", "Count":15 }, { "Name":"manganese", "Count":19 }, { "Name":"nickel", "Count":27 }, { "Name":"sulphur", "Count":23 }, { "Name":"iron", "Count":28 }, { "Name":"ruthenium", "Count":3 }, { "Name":"carbon", "Count":24 }, { "Name":"phosphorus", "Count":15 }, { "Name":"cadmium", "Count":5 }, { "Name":"zirconium", "Count":3 }, { "Name":"zinc", "Count":6 }, { "Name":"chromium", "Count":17 } ], "Manufactured":[ { "Name":"mechanicalcomponents", "Count":12 }, { "Name":"configurablecomponents", "Count":6 }, { "Name":"heatexchangers", "Count":2 }, { "Name":"mechanicalscrap", "Count":14 }, { "Name":"refinedfocuscrystals", "Count":16 }, { "Name":"phasealloys", "Count":18 }, { "Name":"highdensitycomposites", "Count":18 }, { "Name":"hybridcapacitors", "Count":3 }, { "Name":"wornshieldemitters", "Count":12 }, { "Name":"focuscrystals", "Count":8 }, { "Name":"shieldingsensors", "Count":9 }, { "Name":"salvagedalloys", "Count":19 }, { "Name":"shieldemitters", "Count":15 }, { "Name":"mechanicalequipment", "Count":14 }, { "Name":"gridresistors", "Count":4 }, { "Name":"galvanisingalloys", "Count":9 }, { "Name":"precipitatedalloys", "Count":7 }, { "Name":"protolightalloys", "Count":6 }, { "Name":"chemicalprocessors", "Count":7 }, { "Name":"militarysupercapacitors", "Count":27 }, { "Name":"fedcorecomposites", "Count":9 }, { "Name":"fedproprietarycomposites", "Count":18 }, { "Name":"compoundshielding", "Count":6 }, { "Name":"biotechconductors", "Count":45 }, { "Name":"thermicalloys", "Count":2 }, { "Name":"heatconductionwiring", "Count":1 }, { "Name":"conductivecomponents", "Count":6 }, { "Name":"heatvanes", "Count":3 }, { "Name":"conductivepolymers", "Count":8 }, { "Name":"exquisitefocuscrystals", "Count":26 } ], "Encoded":[ { "Name":"decodedemissiondata", "Count":32 }, { "Name":"emissiondata", "Count":40 }, { "Name":"disruptedwakeechoes", "Count":12 }, { "Name":"scrambledemissiondata", "Count":5 }, { "Name":"fsdtelemetry", "Count":6 }, { "Name":"scandatabanks", "Count":19 }, { "Name":"legacyfirmware", "Count":10 }, { "Name":"bulkscandata", "Count":39 }, { "Name":"shielddensityreports", "Count":40 }, { "Name":"encryptedfiles", "Count":3 }, { "Name":"shieldpatternanalysis", "Count":40 }, { "Name":"consumerfirmware", "Count":9 }, { "Name":"embeddedfirmware", "Count":26 }, { "Name":"shieldcyclerecordings", "Count":40 }, { "Name":"industrialfirmware", "Count":10 }, { "Name":"shieldsoakanalysis", "Count":40 }, { "Name":"scanarchives", "Count":6 }, { "Name":"encryptionarchives", "Count":3 }, { "Name":"classifiedscandata", "Count":3 }, { "Name":"encodedscandata", "Count":3 } ] }
            //{ "timestamp":"2017-11-29T15:58:45Z", "event":"EngineerProgress", "Engineer":"Tod 'The Blaster' McQuinn", "Rank":4 }
            //{ "timestamp":"2017-11-29T15:56:53Z", "event":"Rank", "Combat":4, "Trade":8, "Explore":5, "Empire":2, "Federation":12, "CQC":0 }
            //{ "timestamp":"2017-11-29T15:56:53Z", "event":"Progress", "Combat":11, "Trade":100, "Explore":5, "Empire":39, "Federation":3, "CQC":0 }
            //{ "timestamp":"2017-11-07T13:14:18Z", "event":"MaterialCollected", "Category":"Manufactured", "Name":"biotechconductors", "Count":1 }

            return pl.Changed; 
        }
    }

    
    
    public class apiEventArgs : EventArgs
    {
        public player Player { get; private set; } 

        public apiEventArgs(player pl)
        {
            Player = pl;
        }
    }
}
