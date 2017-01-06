using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VRLauncherInCsharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String path = Environment.CurrentDirectory;
        String ID, Key;
        public MainWindow()
        {
            InitializeComponent();

            Thread parserWorker = new Thread(Parse_Game_List);
            parserWorker.IsBackground = true;

            Console.WriteLine(path + "\\settings.txt");
            if (File.Exists(path + "\\settings.txt"))
            {
                this.Title = "Vr Game Launcher";

                string[] settinglines = File.ReadAllLines(path + "\\settings.txt");
                ID = settinglines[0];
                Key = settinglines[1];

                first.Visibility = System.Windows.Visibility.Hidden;
                second.Visibility = System.Windows.Visibility.Visible;
                parserWorker.Start();
                //Parse_Game_List();
                //do stuff here if already setup details
            }
            else
            {
                this.Title = "First Time Setup";
                second.Visibility = System.Windows.Visibility.Hidden;
                //ask for steamID and api key
            }
        }

        private void But_Sav_Click(object sender, RoutedEventArgs e)
        {
            Thread parserWorker = new Thread(Parse_Game_List);
            parserWorker.IsBackground = true;

            ID = SteamID.Text;
            Key = API_Key.Text;

            if (ID.Length == 0 && Key.Length == 0) { MessageBox.Show("SteamID and Steam Web API Key fields are empty", "Empty Fields"); }
            else if (ID.Length == 0) { MessageBox.Show("SteamID field is empty", "Empty Field");  }
            else if (Key.Length == 0) { MessageBox.Show("Steam Web API Key is empty", "Empty Field");  }

            TextWriter tw = new StreamWriter(path + "\\settings.txt", true);
            tw.WriteLine(ID);
            tw.WriteLine(Key);
            tw.Close();

            if (File.Exists(path + "\\settings.txt")) { Console.WriteLine("settings.txt created"); }
            else { MessageBox.Show("Settings File not created, Insufficient permission perhaps?", "Failed to create file"); }

            first.Visibility = System.Windows.Visibility.Hidden;
            this.Title = "Vr Game Launcher";
            second.Visibility = System.Windows.Visibility.Visible;
            parserWorker.Start();
            //Parse_Game_List();
            //Put more stuff here to change canvas
        }

        private void Parse_Game_List()
        {
            this.Dispatcher.Invoke((Action)(() => {ParserText.Content = "Reading gamedb";}));
            List<List<String>> gamedb = CSV_Parser(path + "\\gamedb.csv");
            this.Dispatcher.Invoke((Action)(() => { Progress.Value = 16; }));

            this.Dispatcher.Invoke((Action)(() => { ParserText.Content = "Reading node structure"; }));
            List<List<String>> nodeList = CSV_Parser(path + "\\node_struct.csv");
            this.Dispatcher.Invoke((Action)(() => { Progress.Value = 32; }));

            this.Dispatcher.Invoke((Action)(() => { ParserText.Content = "Reading Owned VR Games"; }));
            List<List<String>> rawList = Get_Game_List(gamedb);
            
            Console.WriteLine("done");
        }

        private List<List<String>> Get_Game_List(List<List<String>> gamedb)
        {
            String response = "";
            WebClient client = new WebClient();
            try
            {
                response = client.DownloadString("http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key=" + Key + "&steamid=" + ID + "&format=json");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + " \nMake sure you have the correct SteamID and APIKey", "SteamAPI invalid response");
                System.Environment.Exit(1);
            }

            String[] values = response.Split('\n');  //Delimiter
            List<List<String>> filtered = new List<List<String>>();

            int i = 0;
            int j = values.Length;
            int l = 0;

            this.Dispatcher.Invoke((Action)(() => { ParserText.Content = "Filtering Only VR Games"; }));
            
            while (i < j)
            {
                this.Dispatcher.Invoke((Action)(() => { Progress.Value = 32 + (i / (double)j) * 18; }));
                if (values[i].Contains("appid"))
                {
                    String[] split = values[i].Split(':');
                    String appID = split[1].Substring(1, split[1].Length - 2);

                    int k = 0;
                    while (k < gamedb.Count){
                        if (gamedb[k][0].Equals(appID)){
                            filtered.Add(new List<String>());
                            filtered[l].Add(gamedb[k][0]);  //gameID
                            filtered[l].Add(gamedb[k][1]);  //gameName
                            filtered[l].Add(gamedb[k][2]);  //CustomExecutable
                            filtered[l].Add(gamedb[k][3]);  //Categories
                            filtered[l].Add(gamedb[k][4]);  //Hidden
                            l++;
                            break;
                        }
                        k++;
                    }
                }
                i++;
            }
            Console.WriteLine("done");
            return filtered;
        }

        private List<List<String>> CSV_Parser(String filename)
        {
            StreamReader reader = new StreamReader(File.OpenRead(filename));
            String line = reader.ReadLine();   //Waste reading 1 line
            int i = 0;

            List<List<String>> csv = new List<List<String>>();
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                String[] values = line.Split(',');  //Delimiter
                csv.Add(new List<String>());

                int j = 0;
                while (j < values.Length)
                {
                    csv[i].Add(values[j]);
                    j++;
                }
                i++;
            }

            return csv;
        }
    }
}
