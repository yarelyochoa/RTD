using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using transit_realtime;

namespace transit_test
{
    class TimeFunctions
    {
        //--------------------------------------------------------------
        // GetAllTimes
        // Stores the route and departure time for each bus heading to that stop
        // Precondition: List of trips is sent in
        // Postcondition: List of trips created for that stop
        // Returns: Nothing
        //-----------------------------------------------------------------
        public static void GetAllTimes(List<FeedEntity> list, string stop_id)
        {
            List<Route_Times> tripsByStop = new List<Route_Times>();

            foreach (FeedEntity item in list)
            {
                Route_Times newRoute = new Route_Times();

                    newRoute.setRoute(item.trip_update.trip.route_id);                                  //set route name    

                foreach (TripUpdate.StopTimeUpdate update in item.trip_update.stop_time_update)
                {
                    if (update.stop_id == stop_id || stop_id == "all")
                    {
                       if (update.departure != null)
                       {
                            newRoute.setTime(update.departure.time);
                        }
                    }
                }

                tripsByStop.Add(newRoute);
            }

            GetNextTime(tripsByStop);

        }

        //--------------------------------------------------------------
        // GetNextTime
        // Stores the times for each route for object specific to that route
        // Precondition: List of trips for that stop
        // Postcondition: List of routes with all of next times
        // Returns: Nothing
        //-----------------------------------------------------------------
        public static void GetNextTime(List<Route_Times> list)
        {
            List<nextTime> tripsByRoute = new List<nextTime>();

            foreach (Route_Times item2 in list)
            {
                bool exists = false;

                string first1 = item2.getRoute();
                //check if the route exists already
                foreach (nextTime item in tripsByRoute)
                {
                    string second1 = item.getRoute();
                    if (item.getRoute() == item2.getRoute())
                    {
                        exists = true;                                      //if exists just add the time to the array.
                        item.times.Add(item2.getTime());
                    }

                }

                if (exists == false)
                {
                    //if it doesnt, create trips by route object

                    nextTime newRouteForStop = new nextTime();
                    newRouteForStop.setRoute(item2.getRoute());
                    newRouteForStop.times.Add(item2.getTime());
                    tripsByRoute.Add(newRouteForStop);


                }
            }

           PrintRouteTimes(tripsByRoute);

        }


        //--------------------------------------------------------------
        // PrintRouteTimes
        // Displays the times for the next arrival for each route to the console
        // Precondition: List of routes with all of next times
        // Postcondition: none
        // Returns: Nothing
        //-----------------------------------------------------------------
        public static void PrintRouteTimes(List<nextTime> list)
        {

            foreach (nextTime item in list)
            {
                item.setTime();
                Console.WriteLine("The route " + item.getRoute() + " will be here in " + item.getTime() + " minutes.");
            }

        }
    }



    class Route_Times
    {
        private string route;
        private long departureTime;

        public void setTime(long depart_time)
        {
            departureTime = depart_time;
        }
        public long getTime()
        {
            return this.departureTime;
        }
        public void setRoute(string routeName)
        {
            route = routeName;
        }
        public string getRoute()
        {
            return this.route;
        }

    }

    class nextTime
    {
        private string routeName;
        private long minutes;
        public List<long> times = new List<long>();


        public void setTime()
        {
            
            long smallestTime = times.Min();                                         //get the smallest time
            minutes = (smallestTime - ExtraFunctions.getUnixTime())/ 60;                 //calculate minutes until next

           
        }
        public long getTime()
        {
            return this.minutes;
        }
        public void setRoute(string route_name)
        {
            routeName = route_name;
        }
        public string getRoute()
        {
            return this.routeName;
        }
    }

}
