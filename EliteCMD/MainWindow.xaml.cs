using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EliteCMD
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort COM = new SerialPort();
        private EliteAPI EA = new EliteAPI();
        private player pl = new player();
        private string[] lines = new string[4];

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            COM.BaudRate = 250000;
            foreach (string Port in SerialPort.GetPortNames())
                cboPort.Items.Add(Port);
            EA.OnApiEvent += EA_OnApiEvent;
            EA.OnMissionEvent += EA_OnMissionEvent;
            EA.Run();
        }

        private void EA_OnMissionEvent(object sender, missionEventArgs e)
        {
            this.Dispatcher.Invoke(delegate
            {

                lstElapsingMissions.Items.Clear();
                foreach (double id in e.ElapsingMissions.Keys)
                {
                    lstElapsingMissions.Items.Add(e.ElapsingMissions[id].Expiry.ToShortTimeString() + " - " + e.ElapsingMissions[id].LocalisedName);
                }
            });
        }

        private void EA_OnApiEvent(object sender, apiEventArgs e)
        {
            /*
             *  plStation = "";
                plStarSystem = "";
                plDocked = "True";
                plShip = "";
                plLoadFSD = "False";*/

            this.Dispatcher.Invoke(delegate
            {
                refScreen(e.Player);
            });
        }


        private void refScreen(player pla)
        {
            lblStation.Content = pla.Station;
            lblStarSystem.Content = pla.StarSystem;
            lblDocked.Content = pla.Docked;
            lblShip.Content = pla.Ship;
            lblLoadFSD.Content = pla.LoadFSD;
            lblTrack.Content = pla.Track;
            lblGameMode.Content = pla.GameMode;
            string cargo = pla.Cargo + "/" + pla.CargoSpace;
            lblCargo.Content = cargo;
            string pass = pla.Passenger + "/" + pla.PassengerCabbin;
            lblPassengers.Content = pass;


            if (COM.IsOpen)
            {
                string line = "";
                //pla.StarSystem                    pla.Station
                int space = 20 - (pla.StarSystem.Length + pla.Station.Length);
                string spacer = " - ";
                if(space > 3)
                {
                    spacer = "";
                    for (int i = 0; i < space; i++)
                    {
                        spacer += i == space / 2 ? "-" : " ";
                    }
                }
                if (pla.Station == "")
                {
                    line = pla.StarSystem.PadRight(20);
                }
                else
                    line = pla.StarSystem + spacer + pla.Station;

                if(line != lines[0])
                {
                    lines[0] = line;
                    COM.WriteLine("0" + line);
                }

                string type = ((pla.Docked == "undocked") && (pla.Track == "Supercruise")) || (pla.LoadFSD == "LoadFSD") ? pla.StarClass : pla.Docked;
                line = (type + " ").PadRight(20 - (pla.Track.Length > 19 ? 19 : pla.Track.Length)) + pla.Track;

                if (line != lines[1])
                {
                    Thread.Sleep(50);
                    lines[1] = line;
                    COM.WriteLine("1" + line);
                }

                string right = pla.LoadFSD == "" ? pla.GameMode : pla.LoadFSD;
                line = (pla.Ship + " ").PadRight(20 - (pla.LoadFSD.Length > 19 ? 19 : right.Length)) + right;

                if (line != lines[2])
                {
                    Thread.Sleep(50);
                    lines[2] = line;
                    COM.WriteLine("2" + line);
                }

                string passeng = "";
                if (pla.PassengerCabbin > 0)
                    passeng = pass;
                line = (passeng + " ").PadRight(20 - cargo.Length) + cargo;

                if (line != lines[3])
                {
                    Thread.Sleep(50);
                    lines[3] = line;
                    COM.WriteLine("3" + line);
                }
            }
            pl = pla;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (COM.IsOpen)
            {
                COM.Close();
            }
            EA.Stop();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if(cboPort.SelectedIndex >= 0)
            {
                COM.PortName = cboPort.SelectedItem.ToString();
                btnConnect.Content = COM.PortName + " Trennen";
                COM.Open();
                cboPort.SelectedIndex = -1;
                cboPort.Items.Clear();
                refScreen(pl);
            }
            else
            {
                btnConnect.Content = "Verbinden";
                COM.Close();
                foreach(string Port in SerialPort.GetPortNames())
                    cboPort.Items.Add(Port);
            }
        }
    }
}
