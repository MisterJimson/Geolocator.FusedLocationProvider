using System;
using System.Threading;
using System.Threading.Tasks;
using Plugin.Geolocator.Abstractions;

namespace GeoFusedLocationProvider
{
    public class FusedLocationGeolocator : IGeolocator
    {
        public double DesiredAccuracy { get; set; }
        public bool IsListening { get; }
        public bool SupportsHeading { get; }
        public bool AllowsBackgroundUpdates { get; set; }
        public bool PausesLocationUpdatesAutomatically { get; set; }
        public bool IsGeolocationAvailable { get; }
        public bool IsGeolocationEnabled { get; }
        public event EventHandler<PositionErrorEventArgs> PositionError;
        public event EventHandler<PositionEventArgs> PositionChanged;

        public Task<Position> GetPositionAsync(int timeoutMilliseconds = -1, CancellationToken? token = null, bool includeHeading = false)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StartListeningAsync(int minTime, double minDistance, bool includeHeading = false)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StopListeningAsync()
        {
            throw new NotImplementedException();
        }
    }
}