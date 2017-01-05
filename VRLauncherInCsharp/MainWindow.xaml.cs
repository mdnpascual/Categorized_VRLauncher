using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Console.WriteLine(path + "\\settings.txt");
            if (File.Exists(path + "\\settings.txt"))
            {
                this.Title = "Vr Game Launcher";

                string[] settinglines = File.ReadAllLines(path + "\\settings.txt");
                ID = settinglines[0];
                Key = settinglines[1];

                first.Visibility = System.Windows.Visibility.Hidden;
                second.Visibility = System.Windows.Visibility.Visible;
                Parse_Game_List();
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
            Parse_Game_List();
            //Put more stuff here to change canvas
        }

        private void Parse_Game_List()
        {
            String[] rawList = Get_Game_List();
            List<List<String>> gamedb = CSV_Parser(path + "\\gamedb.csv");
            List<List<String>> nodeList = CSV_Parser(path + "\\node_struct.csv");
            Console.WriteLine("done");
        }

        private String[] Get_Game_List()
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
            Console.WriteLine("done");
            return values;
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
