using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTATrainTrackerConsole
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;

    namespace CTATrainTrackerConsole
    {
        class Logic
        {
            static void Main(string[] args)
            {
                int count;
                string trainKey;
                string station;
                string stationName;
                string route;
                string destination;
                string direction;
                string estArrivalTime;
                string runNumber;
                int isDelayed;
                int isArriving;
                List<Train> trains = new List<Train>();

                //Add your CTA API key here
                trainKey = null;

                //Add station here, CTA dev documentation has full list
                station = "30109";

                string type;

                if (station.Substring(0, 1) == "4")
                    type = "mapid";
                else
                    type = "stpid";

                //Imports XML data.
                XmlDocument APIDoc = null;
                XmlTextReader reader = new XmlTextReader("http://lapi.transitchicago.com/api/1.0/ttarrivals.aspx?key=" + trainKey + "&" + type + "=" + station);
                APIDoc = new XmlDocument();
                APIDoc.Load(reader);
                count = APIDoc.ChildNodes[1].SelectNodes("eta").Count;
                reader.Close();

                //Extracts data from XML and stroes in a Train object
                for (int i = 3; i < count + 2; i++)
                {
                    stationName = APIDoc.ChildNodes[1].ChildNodes[i].ChildNodes[2].ChildNodes[0].Value;
                    route = APIDoc.ChildNodes[1].ChildNodes[i].ChildNodes[5].ChildNodes[0].Value;
                    destination = APIDoc.ChildNodes[1].ChildNodes[i].ChildNodes[3].ChildNodes[0].Value;
                    direction = APIDoc.ChildNodes[1].ChildNodes[i].ChildNodes[8].ChildNodes[0].Value;
                    estArrivalTime = APIDoc.ChildNodes[1].ChildNodes[i].ChildNodes[10].ChildNodes[0].Value;
                    runNumber = APIDoc.ChildNodes[1].ChildNodes[i].ChildNodes[4].ChildNodes[0].Value;
                    isDelayed = Convert.ToInt32(APIDoc.ChildNodes[1].ChildNodes[i].ChildNodes[13].ChildNodes[0].Value);
                    isArriving = Convert.ToInt32(APIDoc.ChildNodes[1].ChildNodes[i].ChildNodes[11].ChildNodes[0].Value);

                    Train train = new Train(stationName, route, destination, direction, runNumber);

                    train.Route = train.RouteNameConversion(route);
                    train.IsDelayed = train.IsTrainDelayed(isDelayed);
                    train.IsArriving = train.IsTrainArriving(isArriving);
                    train.ArrivalInMinutes = train.CalculateMinutesToArrival(estArrivalTime);
                    train.PrintString = train.PrintData();

                    trains.Add(train);
                }

                WriteListToConsole(trains);

                //WriteNextTrainToConsole(trains);

                Console.ReadLine();


            }

            //Gets the next 2 arriving trains
            public static void WriteNextTrainToConsole(List<Train> trains)
            {
                if (trains.Count > 1)
                    for (int i = 0; i < 2; i++)
                    {
                        Console.WriteLine(trains[i].PrintString);
                    }

                else
                    Console.WriteLine(trains[0].PrintString);
            }

            //Gets all arriving trains
            public static void WriteListToConsole(List<Train> trains)
            {
                foreach (Train train in trains)
                {
                    Console.WriteLine(train.PrintString);
                }
            }

        }

        public class Train
        {
            public string Station { get; set; }
            public string Route { get; set; }
            public string Destination { get; set; }
            public string Direction { get; set; }
            public string RunNumber { get; set; }
            public int ArrivalInMinutes { get; set; }
            public bool IsDelayed { get; set; }
            public bool IsArriving { get; set; }
            public string PrintString { get; set; }

            //Constructor
            public Train(string station, string route, string destination, string direction, string runNumber)
            {
                Station = station;
                Route = route;
                Destination = destination;
                Direction = direction;
                RunNumber = runNumber;
                ArrivalInMinutes = 0;
                IsDelayed = false;
                IsArriving = false;
            }

            //Calculates the estimated minutes to train arrival
            public int CalculateMinutesToArrival(string estArrivalTime)
            {
                int estHour;
                int estMinute;
                int curHour;
                int curMinute;

                //Separates hour and minute
                DateTime now = DateTime.Now;
                curHour = Convert.ToInt32(now.ToString("HH"));
                curMinute = now.Minute;

                estHour = Convert.ToInt32(estArrivalTime.Substring(9, 2));
                estMinute = Convert.ToInt32(estArrivalTime.Substring(12, 2));

                if (estHour > curHour)
                    estMinute = +60;

                return estMinute - curMinute;
            }

            //Checks if train is delayed
            public bool IsTrainDelayed(int isDelayed)
            {
                if (isDelayed == 1)
                    return true;
                return false;
            }

            //Checks if train is arriving
            public bool IsTrainArriving(int isArriving)
            {
                if (isArriving == 1)
                    return true;
                return false;
            }

            //Creates a string of train data to display
            public string PrintData()
            {
                if (IsDelayed)
                    return Station + '\n' + Route + "-" + RunNumber + '\n' +
                             Destination + '\n' + RunNumber + "Is delayed." + '\n';
                else
                {
                    if (IsArriving)
                        return Station + '\n' + Route + "-" + RunNumber + '\n' +
                                 Destination + '\n' + RunNumber + "Is arriving." + '\n';
                    else
                        return Station + '\n' + Route + "-" + RunNumber + '\n' +
                                 Destination + '\n' + "Will arrive in " + ArrivalInMinutes + " minutes" + '\n';
                }
            }

            //Converts CTA route codes to full names
            public string RouteNameConversion(string name)
            {
                string output = null;

                string[,] colors = {{"Red","Red Line"},{"Blue", "Blue Line"},{"Brn","Brown Line"},{"G","Green Line"},
            {"Org","Orange Line"},{"P" , "Purple Line"},{"Pink","Pink Line"},{"Y","Yellow Line"}};

                for (int i = 0; i < colors.Length; i++)
                {
                    if (name == colors[i, 0])
                    {
                        output = colors[i, 1];
                        break;
                    }
                }
                return output;

            }


        }
    }

}
