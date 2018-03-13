using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using transit_realtime;

namespace transit_test
{
    class ExtraFunctions
    {
        //-------------------------------------------------------------------------------
        // getUnixTime()
        // Gets the current Unix time
        // Precondition: None
        // Postcondition: Unix time is calculated
        // Returns: Int32 containing the current Unix time. 
        //---------------------------------------------------------------------------------
        public static Int32 getUnixTime()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        //------------------------------------------------------------------------------
        // StoreTrip_ByStop
        // This gets stores all of the Trip objects for a given bus stop in a list
        // Precondition: FeedMessage from RTD is passed in, and desired bus stop is passed in as a string
        // Postcondition: All of the trips for that bus stop are stored in a List
        // Returns: List of stops. 
        //----------------------------------------------------------------------------------
        public static List<FeedEntity> StoreTrip_ByStop(FeedMessage feed, String bus_stop)
        {
            List<FeedEntity> tripsForStop = new List<FeedEntity>();
            foreach (FeedEntity entity in feed.entity)
            {
                 foreach (TripUpdate.StopTimeUpdate update in entity.trip_update.stop_time_update)
                 {
                    if (update.stop_id != null && update.stop_id == bus_stop)
                    {
                        tripsForStop.Add(entity);
                    }
                 }//end of inner loop
            }//end of outer loop
            return tripsForStop;
        }//end of function

        //--------------------------------------------------------------
        // PrintTrips_ByStop
        // Prints the data in the list of trips
        // Precondition: List of trips is sent in
        // Postcondition: List of trips is printed to console
        // Returns: Nothing
        //-----------------------------------------------------------------
        public static void PrintTrips_ByStop(List<FeedEntity> list, String stop_id)
        {
            foreach(FeedEntity item in list)
            {
                //Print the trip data
                if(item.trip_update.trip.trip_id != null)
                    Console.WriteLine("Trip ID  = " + item.trip_update.trip.trip_id);
                if(item.trip_update.trip.schedule_relationship.ToString() != null)
                    Console.WriteLine("Schedule Relationship  = " + item.trip_update.trip.schedule_relationship.ToString());
                if(item.trip_update.trip.route_id != null)
                    Console.WriteLine("Route ID  = " + item.trip_update.trip.route_id);
                Console.WriteLine("Direction ID  = " + item.trip_update.trip.direction_id);
                if(item.trip_update.vehicle != null)
                    Console.WriteLine("Vehicle ID = " + item.trip_update.vehicle.id);
                //print all the stop data
                foreach (TripUpdate.StopTimeUpdate update in item.trip_update.stop_time_update)
                {
                    if (update.stop_id == stop_id || stop_id == "all")
                    {
                        if (update.stop_sequence.ToString() != null)
                            Console.WriteLine("    Stop Sequence = " + update.stop_sequence.ToString());
                        if (update.arrival != null)
                        {
                            Console.WriteLine("    Arrival Time = " + update.arrival.time);
                            Console.WriteLine("    Arrival Time = " + Program.UnixTimeStampToDateTime(update.arrival.time).ToString());
                        }
                        if (update.departure != null)
                        {
                            Console.WriteLine("    Departure Time = " + update.departure.time);
                            Console.WriteLine("    Departure Time = " + Program.UnixTimeStampToDateTime(update.departure.time).ToString());
                        }
                        if (update.stop_id != null)
                            Console.WriteLine("    Stop ID = " + update.stop_id);
                        Console.WriteLine("    Schedule Relationship  = " + update.schedule_relationship.ToString());
                        Console.WriteLine();
                    }
                }
                    Console.WriteLine();
            }
        }

        public static void VehiclePosition_ByStop(FeedMessage feed, String bus_stop)
        {
            foreach (FeedEntity entity in feed.entity)
            {
                if (entity.vehicle != null)
                {
                    if (entity.vehicle.trip != null)
                    {
                        if (entity.vehicle.trip.route_id != null && entity.vehicle.stop_id == bus_stop)
                        {
                            Console.WriteLine("Vehicle ID = " + entity.vehicle.vehicle.id);
                            Console.WriteLine("Current Position Information:");
                            Console.WriteLine("Current Latitude = " + entity.vehicle.position.latitude);
                            Console.WriteLine("Current Longitude = " + entity.vehicle.position.longitude);
                            Console.WriteLine("Current Bearing = " + entity.vehicle.position.bearing);
                            Console.WriteLine("Current Status = " + entity.vehicle.current_status + " StopID: " + entity.vehicle.stop_id);
                            if (Stop.stops.ContainsKey(entity.vehicle.stop_id))
                            {
                                Console.WriteLine("The name of this StopID is \"" + Stop.stops[entity.vehicle.stop_id].stop_name + "\"");
                                Console.WriteLine("The Latitude of this StopID is \"" + Stop.stops[entity.vehicle.stop_id].stop_lat + "\"");
                                Console.WriteLine("The Longitude of this StopID is \"" + Stop.stops[entity.vehicle.stop_id].stop_long + "\"");
                                string wheelChairOK = "IS NOT";
                                if (Stop.stops[entity.vehicle.stop_id].wheelchair_access)
                                {
                                    wheelChairOK = "IS";
                                }
                                Console.WriteLine("This stop is " + wheelChairOK + " wheelchair accessible");
                            }

                            Console.WriteLine("Trip ID = " + entity.vehicle.trip.trip_id);
                            if (Trip.trips.ContainsKey(entity.vehicle.trip.trip_id))
                            {
                                if (entity.vehicle.current_status.ToString() == "IN_TRANSIT_TO")
                                {
                                    if (Stop.stops.ContainsKey(entity.vehicle.stop_id))
                                    {
                                        Console.WriteLine("Vehicle in transit to: " + Stop.stops[entity.vehicle.stop_id].stop_name);
                                        Trip.trip_t trip = Trip.trips[entity.vehicle.trip.trip_id];
                                        foreach (Trip.trip_stops_t stop in trip.tripStops)
                                        {
                                            if (stop.stop_id == entity.vehicle.stop_id)
                                            {
                                                Console.WriteLine(".. and is scheduled to arrive there at " + stop.arrive_time);
                                                break;
                                            }
                                        }
                                    }
                                }
                                Console.WriteLine();
                            }
                        }
                    }
                }
            }
        }
    }
}
