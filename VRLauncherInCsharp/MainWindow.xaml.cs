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
using System.Windows.Media.Animation;

namespace VRLauncherInCsharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String path = Environment.CurrentDirectory;
        String ID, Key;
        TreeNode<List<String>> filteredStruct;
        Button[] butList;
        Line[] L;
        Canvas customCanvas = new Canvas();
        Boolean Start_clicked = false;
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
            List<List<String>> filteredList = Get_Game_List(gamedb);

            this.Dispatcher.Invoke((Action)(() => { ParserText.Content = "Generating Tree Structure"; }));
            TreeNode<List<String>> treeStruct = List_to_Tree(nodeList);

            this.Dispatcher.Invoke((Action)(() => { ParserText.Content = "Adding Games to Tree Structure"; }));
            filteredStruct = Add_Games_to_Tree(treeStruct, filteredList);

            ///////////MOVE THIS CODE BLOCK///////////
            int i = 1;
            int j = filteredStruct.ElementsIndex.Count;

            while (i < j)
            {
                if (filteredStruct.ElementAt(i).Data.Count == 4)
                {
                    Console.WriteLine(filteredStruct.ElementAt(i).Data[2]);
                }
                else
                {
                    Console.WriteLine(filteredStruct.ElementAt(i).Data[1]);
                }
                i++;
            }
            ///////////MOVE THIS CODE BLOCK///////////

            Graphics_main(filteredStruct);

            this.Dispatcher.Invoke((Action)(() => { mainWindow.Width = 1920; }));
            this.Dispatcher.Invoke((Action)(() => { mainWindow.Height = 1020; }));
            this.Dispatcher.Invoke((Action)(() => { mainWindow.Top = 0; }));
            this.Dispatcher.Invoke((Action)(() => { mainWindow.Left = 0; }));             
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

        private TreeNode<List<String>> List_to_Tree(List<List<String>> nodeList)
        {
            TreeNode<List<String>> tree = new TreeNode<List<String>>(new List<String>());
            
            int i = 0;
            int j = nodeList.Count;

            while (i < j)
            {
                this.Dispatcher.Invoke((Action)(() => { Progress.Value = 50 + (i / (double)j) * 10; }));
                List<String> node = nodeList[i];
                if (node[1].Equals("\\N")){
                    tree.AddChild(node);
                }
                else
                {
                    TreeNode<List<String>> found = tree.FindTreeNode(link => link.Data.Count != 0 && link.Data[0].Equals(node[1]));
                    found.AddChild(node);
                }
                i++;
            }   
            return tree;
        }

        private TreeNode<List<String>> Add_Games_to_Tree(TreeNode<List<String>> treeStruct, List<List<String>> filteredList)
        {
            int i = 0;
            int j = filteredList.Count;

            while (i < j)
            {
                this.Dispatcher.Invoke((Action)(() => { Progress.Value = 60 + (i / (double)j) * 15; }));
                List<String> node = filteredList[i];
                String categories = node[3];
                String[] category = categories.Split('|');
                int k = 0;
                int l = category.Length;

                while (k < l)
                {
                    TreeNode<List<String>> found = treeStruct.FindTreeNode(link => link.Data.Count != 0 && link.Data[0].Equals(category[k]));
                    found.AddChild(node);
                    k++;
                }
                i++;
            }
            return treeStruct;
        }

        private void Game_Execute(String command)
        {

        }

        private void Node_Click(object sender, RoutedEventArgs e)
        {
            Button node = (Button)sender;
            int count = 0;

            if (node.Content.Equals("Start"))
            {
                if (!Start_clicked)
                {
                    Start_clicked = true;
                    count = filteredStruct.Children.Count;
                    double degree = 360.0 / count;
                    double x = 0, y = 0;

                    int i = 1;
                    int j = filteredStruct.ElementsIndex.Count;
                    int k = 0;
                    int padding = 200;

                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        while (i < j)
                        {
                            if (filteredStruct.ElementAt(i).Data[1].Equals("\\N"))
                            {
                                switch (k % 8)
                                {
                                    case 0:
                                        x = butList[i].Margin.Left + (Double)((175 + ((padding - 100) * Math.Floor(k / 8.0))));
                                        y = 0;
                                        animateLine(L[i], customCanvas.Width / 2, customCanvas.Height / 2, (customCanvas.Width / 2) + x, (customCanvas.Height / 2) + y);
                                        break;
                                    case 1:
                                        x = butList[i].Margin.Left + (Double)((175 + ((padding - 100) * Math.Floor(k / 8.0))) * Math.Cos((Math.PI / 180) * 45));
                                        y = butList[i].Margin.Top + (Double)((175 + ((padding - 100) * Math.Floor(k / 8.0))) * Math.Sin((Math.PI / 180) * 45));
                                        animateLine(L[i], customCanvas.Width / 2, customCanvas.Height / 2, (customCanvas.Width / 2) + x, (customCanvas.Height / 2) + y);
                                        break;
                                    case 2:
                                        x = 0;
                                        y = butList[i].Margin.Top + (Double)((150 + ((padding - 100) * Math.Floor(k / 8.0))));
                                        animateLine(L[i], customCanvas.Width / 2, customCanvas.Height / 2, (customCanvas.Width / 2) + x, (customCanvas.Height / 2) + y);
                                        break;
                                    case 3:
                                        x = butList[i].Margin.Left - (Double)((200 + ((padding - 75) * Math.Floor(k / 8.0))) * Math.Cos((Math.PI / 180) * 45));
                                        y = butList[i].Margin.Top + (Double)((200 + ((padding - 75) * Math.Floor(k / 8.0))) * Math.Sin((Math.PI / 180) * 45));
                                        animateLine(L[i], customCanvas.Width / 2, customCanvas.Height / 2, (customCanvas.Width / 2) + x + (25 * Math.Ceiling(k / 8.0)), (customCanvas.Height / 2) + y);
                                        break;
                                    case 4:
                                        x = butList[i].Margin.Left - (Double)((200 + (padding * Math.Floor(k / 8.0))));
                                        y = 0;
                                        animateLine(L[i], customCanvas.Width / 2, customCanvas.Height / 2, (customCanvas.Width / 2) + x + (150 * Math.Ceiling(k/8.0)), (customCanvas.Height / 2) + y);
                                        break;
                                    case 5:
                                        x = butList[i].Margin.Left - (Double)((200 + (padding * Math.Floor(k / 8.0))) * Math.Cos((Math.PI / 180) * 45));
                                        y = butList[i].Margin.Top - (Double)((200 + (padding * Math.Floor(k / 8.0))) * Math.Sin((Math.PI / 180) * 45));
                                        animateLine(L[i], customCanvas.Width / 2, customCanvas.Height / 2, (customCanvas.Width / 2) + x + 100, (customCanvas.Height / 2) + y + 100);
                                        break;
                                    case 6:
                                        x = 0;
                                        y = butList[i].Margin.Top - (Double)((200 + (padding * Math.Floor(k / 8.0))));
                                        animateLine(L[i], customCanvas.Width / 2, customCanvas.Height / 2, (customCanvas.Width / 2) + x, (customCanvas.Height / 2) + y + 50);
                                        break;
                                    case 7:
                                        x = butList[i].Margin.Left + (Double)((200 + ((padding - 75) * Math.Floor(k / 8.0))) * Math.Cos((Math.PI / 180) * 45));
                                        y = butList[i].Margin.Top - (Double)((200 + ((padding - 75) * Math.Floor(k / 8.0))) * Math.Sin((Math.PI / 180) * 45));
                                        animateLine(L[i], customCanvas.Width / 2, customCanvas.Height / 2, (customCanvas.Width / 2) + x, (customCanvas.Height / 2) + y+25);
                                        break;
                                }
                                k++;
                                animateNode(butList[i].Margin.Left + x, butList[i].Margin.Top + y, butList[i]);
                                
                                
                                Console.WriteLine(node.Content);
                            }
                            i++;
                        }
                    }));
                }
                else
                {
                    Start_clicked = false;
                    int i = 1;
                    int j = filteredStruct.ElementsIndex.Count;

                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        while (i < j)
                        {
                            animateLine(L[i], L[i].X1, L[i].Y1, customCanvas.Width / 2, customCanvas.Height / 2);
                            animateNode(0, 0, butList[i]);
                            i++;
                        }
                    }));
                }
                //butList[0].Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                String butName = node.Content.ToString();
            }
            
            Console.WriteLine(node.Content);
        }

        private void animateNode(double newX, double newY, Button toAnimate)
        {
            ThicknessAnimation slide = new ThicknessAnimation();
            slide.From = toAnimate.Margin;                                 // get current grid location
            slide.To = new Thickness(newX, newY, 0, 0);                    // set new grid location

            slide.Duration = new Duration(TimeSpan.FromSeconds(0.5));     // set translation time
            toAnimate.BeginAnimation(Button.MarginProperty, slide);          // animate movement
        }

        private void animateLine(Line toAnimate, Double origX, Double origY, Double destX, Double destY)
        {
            /*
            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = toAnimate.Opacity;                                 // get current grid location
            opacityAnimation.To = 1;                    // set new grid location
            opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(.5));     // set translation time
            toAnimate.BeginAnimation(Line.OpacityProperty, opacityAnimation);          // animate movement
            */
            toAnimate.X1 = origX;
            toAnimate.Y1 = origY;

            Storyboard sb = new Storyboard();
            DoubleAnimation da1 = new DoubleAnimation(toAnimate.X2, destX, new Duration(TimeSpan.FromSeconds(.5)));
            DoubleAnimation da = new DoubleAnimation(toAnimate.Y2, destY, new Duration(TimeSpan.FromSeconds(.5)));
            Storyboard.SetTargetProperty(da, new PropertyPath("(Line.Y2)"));
            Storyboard.SetTargetProperty(da1, new PropertyPath("(Line.X2)"));
            sb.Children.Add(da);
            sb.Children.Add(da1);

            //MessageBox.Show("HOY");
            toAnimate.BeginStoryboard(sb);


        }

        private void Graphics_main(TreeNode<List<String>> filteredStruct)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                second.Visibility = System.Windows.Visibility.Hidden;

                Style style2 = new Style(typeof(Line));
                style2.Setters.Add(new Setter(Line.StrokeThicknessProperty, 1.0));
                style2.Setters.Add(new Setter(Line.StrokeProperty, Brushes.Black));
                style2.Setters.Add(new Setter(Line.VerticalAlignmentProperty, System.Windows.VerticalAlignment.Top));
                style2.Setters.Add(new Setter(Line.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Left));
                Resources.Add(typeof(Line), style2);

                customCanvas = new Canvas();
                customCanvas.Width = 1900;
                customCanvas.Height = 1000;
                customCanvas.Background = Brushes.LightSteelBlue;

                int i = 1;
                int j = filteredStruct.ElementsIndex.Count;

                butList = new Button[j];
                L = new Line[j];

                while (i < j)
                {
                    butList[i] = new Button();
   
                    if (filteredStruct.ElementAt(i).Data.Count == 4)    //Nodes
                    {
                        butList[i].Style = (Style)FindResource("NodeOrange");
                        butList[i].FontSize = 16;
                        butList[i].Height = 100;
                        butList[i].Width = 100;
                        butList[i].Content = filteredStruct.ElementAt(i).Data[2];
                        butList[i].ToolTip = butList[i].Content;
                    }
                    else
                    {
                        butList[i].Style = (Style)FindResource("NodeYellow");
                        butList[i].FontSize = 16;
                        butList[i].Height = 80;
                        butList[i].Width = 80;
                        butList[i].Content = filteredStruct.ElementAt(i).Data[1];
                        butList[i].ToolTip = butList[i].Content;
                    }

                    Canvas.SetLeft(butList[i], (customCanvas.Width / 2) - butList[i].Width / 2);
                    Canvas.SetTop(butList[i], (customCanvas.Height / 2) - butList[i].Height / 2);
                    customCanvas.Children.Add(butList[i]);

                    L[i] = new Line();
                    //L[i].StrokeThickness = 1;
                    //L[i].Stroke = Brushes.Black;
                    //L[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    //L[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    //Canvas.SetZIndex(L[i], -1);
                    customCanvas.Children.Add(L[i]);
                    //L[i].X1 = customCanvas.Width / 2.0;
                    //L[i].Y1 = customCanvas.Height / 2.0;
                    //L[i].X2 = customCanvas.Width / 2.0;
                    //L[i].Y2 = customCanvas.Height / 2.0;
                    
                    //Panel.SetZIndex(L, -1);
                    i++;
                }

                //0th Button
                butList[0] = new Button();
                butList[0].Style = (Style)FindResource("NodeGreen");
                butList[0].FontSize = 16;
                butList[0].Height = 120;
                butList[0].Width = 120;
                butList[0].Content = "Start";
                butList[0].ToolTip = butList[0].Content;

                Canvas.SetLeft(butList[0], (customCanvas.Width / 2) - butList[0].Width / 2);
                Canvas.SetTop(butList[0], (customCanvas.Height / 2) - butList[0].Height / 2);
                customCanvas.Children.Add(butList[0]);

                //Initializes event handlers for all nodes
                for (i = 0; i < j; i++)
                {
                    butList[i].Click += Node_Click;
                }

                mainWindow.Content = customCanvas;
            }));
        }
    }
}
