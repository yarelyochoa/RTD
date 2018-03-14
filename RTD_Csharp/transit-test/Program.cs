using System;
using System.Net;
using System.IO;
using System.Text;
using ProtoBuf;
using transit_realtime;
using System.Collections.Generic;
using System.IO.Ports;

namespace transit_test
{
    class Program
    {
        static void Main(string[] args)
        {
            //set urls for RTD data
            String vehiclePositionUrl = "http://www.rtd-denver.com/google_sync/VehiclePosition.pb";
            String tripUrl = "http://www.rtd-denver.com/google_sync/TripUpdate.pb";
            //get vehicle positions data from RTD.

            FeedMessage vehiclePositionFeed = getData(vehiclePositionUrl);
            FeedMessage tripFeed = getData(tripUrl);

            Stop stop_inst = new Stop();
            Trip trip_inst = new Trip();

            //print the data from vehicle positions.
            //printAllVehiclePositions(vehiclePositionFeed);
            //printTrips(tripFeed);
            //printVP_for_stop(feed2, "23103");
            //ExtraFunctions.VehiclePosition_ByStop(vehiclePositionFeed, "22730");
            String stopNumber = "12850";
            String all = "all";

           
            List<FeedEntity> tripsForStop = ExtraFunctions.StoreTrip_ByStop(tripFeed, stopNumber);

            List<nextTime> nextTime = TimeFunctions.GetAllTimes(tripsForStop,stopNumber);


            ExtraFunctions.PrintTrips_ByStop(tripsForStop, stopNumber);
            Console.WriteLine(ExtraFunctions.getUnixTime());

            String data = TimeFunctions.PrintRouteTimes(nextTime);
            Console.WriteLine(data);
            SerialPort sp = new SerialPort("COM7", 9600, Parity.None, 8, StopBits.One);
            sp.Open();
            sp.Write(data);
            Console.WriteLine("Data Sent!");
            sp.Close();

            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
        }//end of main

        //----------------------------------------------------------------
        //Print trips
        //
        //-----------------------------------------------------------------
        static void printTrips(FeedMessage feed)
        {
            foreach (FeedEntity entity in feed.entity)
            {
                if (entity.trip_update != null)
                {
                    if (entity.trip_update.trip != null)
                    {
                        if (entity.trip_update.stop_time_update != null)
                        {
                            Console.WriteLine("Trip ID  = " + entity.trip_update.trip.trip_id);
                            Console.WriteLine("Schedule Relationship  = " + entity.trip_update.trip.schedule_relationship.ToString());
                            Console.WriteLine("Route ID  = " + entity.trip_update.trip.route_id);
                            Console.WriteLine("Direction ID  = " + entity.trip_update.trip.direction_id);
                            if (entity.trip_update.vehicle != null)
                            {
                                Console.WriteLine("Vehicle ID = " + entity.trip_update.vehicle.id);
                            }
                            foreach (TripUpdate.StopTimeUpdate update in entity.trip_update.stop_time_update)
                            {
                                // StopTimeUpdates *may* have the following data:
                                //  stop_sequence:  uint
                                //  arrival:        StopTimeEvent - see below
                                //  departure:      StopTimeEvent - see below
                                //  stop_id:        string
                                //  schedule_relationsip:  SCHEDULED, SKIPPED, or NO_DATA

                                Console.WriteLine();
                                Console.WriteLine("    Stop Sequence = " + update.stop_sequence.ToString());

                                //  Arrival and Departure are StopTimeEvents, which have three components
                                //  delay:          int
                                //  time:           long
                                //  uncertainty:    int 

                                if (update.arrival != null)
                                {
                                    Console.WriteLine("    Arrival Time = " + UnixTimeStampToDateTime(update.arrival.time).ToString());
                                    //Console.WriteLine("Delay = " + update.arrival.delay.ToString()); // RTD appears to always sends 0
                                }
                                if (update.departure != null)
                                {
                                    Console.WriteLine("    Departure Time = " + UnixTimeStampToDateTime(update.departure.time).ToString());
                                    //Console.WriteLine("Delay = " + update.departure.delay.ToString()); // RTD appears always sends 0
                                }
                                Console.WriteLine("    Stop ID = " + update.stop_id);
                                Console.WriteLine("    Schedule Relationship  = " + update.schedule_relationship.ToString());
                            }
                            Console.WriteLine();
                        }
                    }
                }
            }

        }





        //--------------------------------------------------------------
        //Print all data from vehicle positions
        //
        //-------------------------------------------------------------
        static void printAllVehiclePositions(FeedMessage feed)
        {
            foreach (FeedEntity entity in feed.entity)
            {
                if (entity.vehicle != null)
                {
                    if (entity.vehicle.trip != null)
                    {
                        if (entity.vehicle.trip.route_id != null)
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
        }//end of printAllVehiclePositions()

        //--------------------------------------------------------------------------------------------------
        // This function converts the Unix time provided by RTD to a real date and time
        //
        //---------------------------------------------------------------------------------------
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        //-----------------------------------------------------
        //Get vehicle positions data from RTD
        //
        //-------------------------------------------------------
        static FeedMessage getData(String url)
        {
            Uri myUri = new Uri(url);
            WebRequest myWebRequest = HttpWebRequest.Create(myUri);

            HttpWebRequest myHttpWebRequest = (HttpWebRequest)myWebRequest;

            // This username and password is issued for the IWKS 4120 class. Please DO NOT redistribute.
            NetworkCredential myNetworkCredential = new NetworkCredential("RTDgtfsRT", "realT!m3Feed");  // insert credentials here

            CredentialCache myCredentialCache = new CredentialCache();
            myCredentialCache.Add(myUri, "Basic", myNetworkCredential);

            myHttpWebRequest.PreAuthenticate = true;
            myHttpWebRequest.Credentials = myCredentialCache;

            return Serializer.Deserialize<FeedMessage>(myWebRequest.GetResponse().GetResponseStream());
        }//end of getVehiclePositions()
    }
}