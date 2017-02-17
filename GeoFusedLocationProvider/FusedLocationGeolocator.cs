using System;
using System.Threading;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;

namespace GeoFusedLocationProvider
{
    public class FusedLocationGeolocator : IGeolocator
    {
        private IGeolocator bestGeolocator;

        public FusedLocationGeolocator()
        {
            if (Util.IsPlayServicesAvailable())
            {
                bestGeolocator = new FusedLocationGeolocatorInternal();
            }
            else
            {
                bestGeolocator = new GeolocatorImplementation();
            }
        }

        public Task<Position> GetPositionAsync(int timeoutMilliseconds = -1, CancellationToken? token = null, bool includeHeading = false)
        {
            return bestGeolocator.GetPositionAsync(timeoutMilliseconds, token, includeHeading);
        }

        public Task<bool> StartListeningAsync(int minTime, double minDistance, bool includeHeading = false)
        {
            return bestGeolocator.StartListeningAsync(minTime, minDistance, includeHeading);
        }

        public Task<bool> StopListeningAsync()
        {
            return bestGeolocator.StopListeningAsync();
        }

        public double DesiredAccuracy
        {
            get { return bestGeolocator.DesiredAccuracy; }
            set { bestGeolocator.DesiredAccuracy = value; }
        }

        public bool IsListening
        {
            get { return bestGeolocator.IsListening; }
        }

        public bool SupportsHeading
        {
            get { return bestGeolocator.SupportsHeading; }
        }

        public bool AllowsBackgroundUpdates
        {
            get { return bestGeolocator.AllowsBackgroundUpdates; }
            set { bestGeolocator.AllowsBackgroundUpdates = value; }
        }

        public bool PausesLocationUpdatesAutomatically
        {
            get { return bestGeolocator.PausesLocationUpdatesAutomatically; }
            set { bestGeolocator.PausesLocationUpdatesAutomatically = value; }
        }

        public bool IsGeolocationAvailable
        {
            get { return bestGeolocator.IsGeolocationAvailable; }
        }

        public bool IsGeolocationEnabled
        {
            get { return bestGeolocator.IsGeolocationEnabled; }
        }

        public event EventHandler<PositionErrorEventArgs> PositionError
        {
            add { bestGeolocator.PositionError += value; }
            remove { bestGeolocator.PositionError -= value; }
        }
        public event EventHandler<PositionEventArgs> PositionChanged
        {
            add { bestGeolocator.PositionChanged += value; }
            remove { bestGeolocator.PositionChanged -= value; }
        }
    }
}