using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using static System.Net.WebRequestMethods;


namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {//Primary window for Program
            InitializeComponent();
        }

        private void btnOpenFiles_Click(object sender, RoutedEventArgs e)
        {//Event caused bt clicking on New under the File menu on main screen.
            double x,y,f,l = 0;// Varaible used to track various numbers needed in the method.
            string fName = " ";// Holds the file directory.

            OpenFileDialog openFileDialog = new OpenFileDialog();// Opens a file browser window.
            openFileDialog.Filter = "CSV Files (*.csv)|*.csv";// Filters search to CSV files.

            if (openFileDialog.ShowDialog() == true)
            {// Attempts to open the file
                fName = openFileDialog.FileName;// Gets and Stores the file Directory.
                Driver[] dArr = new Driver[1];// Creates an Array of Driver objects.
                lapObj[] lArr = lapObjArr(fName);// Creates an array of lapObj objects.
                dArr[0] = new Driver();// initializes a generic instance of Driver as the first item in the array.
            
                string[] s = fName.Split('\\', '_', '.');// Splits the Directory to find the Session and Event titles.
                EventName.Text = s[(s.Length - 3)];// Outputs the Event.
                SeshName.Text = s[(s.Length - 2)];// Outputs the Seassion

                double[,] ranking = getRanking(lArr);// Creates an array that lists each lap index per fastest time.
                y = 1;// Variable for the number of individual drivers.

                for (int i = 0; i < lArr.Length; i++)
                {// Loop runs through each lap recorded
                    x = 0;// Variable for the number of laps per individual.
                    if (!lArr[(int)ranking[i, 1]].proc)
                    {// If the car number has not been processed yet, continue.

                        f = ranking[i, 0];// Variable for the fastest lap assigned to this Car.

                        for (int j = 0; j < lArr.Length; j++)
                        {// Loop runs through Each Lap recorded.
                            if (lArr[(int)ranking[i, 1]].CarNumber == lArr[j].CarNumber)
                            {// if the car number of the current indexed lap is the same as the car number currently being processed
                                lArr[j].proc = true;// set this lap instance as being processed.
                                l = lArr[j].Time;// set this lap time as being the last lap.
                                x = x + 1;// Increment the total number of laps.
                            }
                        }
                        Array.Resize<Driver>(ref dArr, (int)y);// Increments the number of elements in the Array of Drivers.
                        dArr[(int)y - 1] = new Driver(dArr.Length, lArr[(int)ranking[i, 1]].CarNumber, lArr[(int)ranking[i, 1]].LastName, f, l, lArr[(int)ranking[i, 1]].Lap, (int)x);
                        
                        y = y + 1;// Increments the number of drivers.
                    }
                }
                displayRankings(dArr);// Displays the finalized rankings per car.
                }
        }

        public lapObj[] lapObjArr(string fileN)
        {// Creates an Array of lapObj Objects based off of the info found in the provided CSV file.
            int numI = 1;//Stores the size of the Array.
            lapObj[] lapRec = new lapObj[numI];
            lapRec[0] = new lapObj();// Sets the first index as a generic Lap
            int CarNumber, Lap;
            string LastName, ShortName, Flag, EntryTOD;
            double Time, EntryTime, ExitTime;

            using (TextFieldParser csvParser = new TextFieldParser(fileN))
            {// Parses the data found in the file.

                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();


                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    CarNumber = (int)isNum(fields[0]);
                    Lap = (int)isNum(fields[6]);
                    LastName = fields[1];
                    ShortName = fields[2];
                    Flag = fields[7];
                    EntryTOD = fields[8];
                    Time = isNum(fields[3]);
                    EntryTime = isNum(fields[4]);
                    ExitTime = isNum(fields[5]);

                    lapObj lapRecN = new lapObj(CarNumber, Lap, LastName, ShortName, Flag, EntryTOD, Time, EntryTime, ExitTime);
                    Array.Resize<lapObj>(ref lapRec, numI);
                    lapRec[numI - 1] = lapRecN;
                    numI++;
                }
            }
            return lapRec;
        }

        public DataTable dt;// Initializes the table used in the Window
        public DataRow dr;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {// Further assigns and sets up the values used by the table
            dt = new DataTable("emp");

            DataColumn dc1 = new DataColumn("Rank", typeof(int));
            DataColumn dc2 = new DataColumn("Car", typeof(int));
            DataColumn dc3 = new DataColumn("Driver Name", typeof(string));
            DataColumn dc4 = new DataColumn("Fast Lap Time", typeof(double));
            DataColumn dc5 = new DataColumn("Last Lap Time", typeof(double)); 
            DataColumn dc6 = new DataColumn("Fast Lap Number", typeof(int));
            DataColumn dc7 = new DataColumn("Total Laps", typeof(int));

            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);
            dt.Columns.Add(dc3);
            dt.Columns.Add(dc4);
            dt.Columns.Add(dc5);
            dt.Columns.Add(dc6);
            dt.Columns.Add(dc7);

            dataGrid1.ItemsSource = dt.DefaultView;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {// Close data action
            dt.Rows.Clear();
        }

        private void displayRankings(Driver[] dRanking)
        {// Displays the ranking results onto the table.
            dt.Rows.Clear();
            for (int i = 0; i < dRanking.Length; i++)
            {
                dr = dt.NewRow();

                dr[0] = dRanking[i].Rank;
                dr[1] = dRanking[i].Car;
                dr[2] = dRanking[i].LastName;
                dr[3] = dRanking[i].FastLapTime;
                dr[4] = dRanking[i].LastLapTime;
                dr[5] = dRanking[i].FastLapNum;
                dr[6] = dRanking[i].TotLap;

                dt.Rows.Add(dr);
                dataGrid1.ItemsSource = dt.DefaultView;
            }
        }
        public double[,] getRanking(lapObj[] lArr)
        {// Orders the Laps by the fastest time and returns the times and indeces
            double[,] rank = new double[lArr.Length, 2];
            for (int j = 0; j < lArr.Length; j++)
            {// Initial population of Array
                rank[j, 0] = 10000;
                rank[j, 1] = 0;
            }

            for (int j = 0; j < lArr.Length; j++)
            {// Finds best lap
                if (lArr[j].Time < rank[0, 0])
                {
                    rank[0, 0] = lArr[j].Time;
                    rank[0, 1] = j;
                }
            }

            for (int i = 1; i < lArr.Length; i++)
            {// Finds and orders entries by the fastest lap Times.
                for(int j = 0; j < lArr.Length; j++)
                {
                    if ((lArr[j].Time < rank[i,0]) && (lArr[j].Time > rank[(i-1), 0]))
                    {
                        rank[i, 0] = lArr[j].Time;
                        rank[i, 1] = j;
                    }
                }
            }
            return rank;
        }

        public double isNum(string x)
        {// Finds if a string can be converted to a double.
            double num = -1;

            if (double.TryParse(x, out double j))
            {
                num = double.Parse(x);
            }

            return num;
        }
    }

    public class lapObj
    {// Class to generate an Object for each Lap recorded.
        public int CarNumber, Lap;
        public string LastName, ShortName, Flag, EntryTOD;
        public double Time, EntryTime, ExitTime;
        public Boolean proc;

        public lapObj()
        {// Generic Lap Object.
            this.CarNumber = 0;
            this.Lap = 0;
            this.LastName = "Null";
            this.ShortName = "Null";
            this.Flag = "A";
            this.EntryTOD = "0";
            this.Time = 0;
            this.EntryTime = 0;
            this.ExitTime = 0;
            this.proc = false;
        }
        public lapObj(int CarNumberI, int LapI, string LastNameI, string ShortNameI, string FlagI, string EntryTODI, double TimeI, double EntryTimeI, double ExitTimeI)
        {// Lap Object generated by specific variables
            this.CarNumber = CarNumberI;
            this.Lap = LapI;
            this.LastName = LastNameI;
            this.ShortName = ShortNameI;
            this.Flag = FlagI;
            this.EntryTOD = EntryTODI;
            this.Time = TimeI;
            this.EntryTime = EntryTimeI;
            this.ExitTime = ExitTimeI;
            this.proc = false;
        }
    }

    public class Driver
    {// Class to create an Object for each individual Driver
        public int Rank, Car, FastLapNum, TotLap;
        public string LastName;
        public double FastLapTime, LastLapTime;

        public Driver()
        {// Generic instance of Driver
            this.Rank = 0;
            this.Car = 0;
            this.FastLapNum = 0;
            this.TotLap = 0;
            this.LastName = "A";
            this.FastLapTime = 0;
            this.LastLapTime = 0;
        }
        public Driver(int RankI, int CarI, string LastNameI, double FastLapTimeI, double LastLapTimeI, int FastLapNumI, int TotLapI)
        {// Driver Object based of specifit variables.
            this.Rank = RankI;
            this.Car = CarI;
            this.FastLapNum = FastLapNumI;
            this.TotLap = TotLapI;
            this.LastName = LastNameI;
            this.FastLapTime = FastLapTimeI;
            this.LastLapTime = LastLapTimeI;
        }
    }
}
