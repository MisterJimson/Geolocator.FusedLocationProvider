using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Locations;
using Android.OS;
using Plugin.Geolocator.Abstractions;

namespace GeoFusedLocationProvider
{
    public class FusedLocationGeolocator : IGeolocator, IDisposable
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

        private GoogleCallbacks callbacks;
        private GoogleApiClient client;

        public FusedLocationGeolocator()
        {
            DesiredAccuracy = 100;

            callbacks = new GoogleCallbacks();
            callbacks.Connected += Connected;
            callbacks.ConnectionFailed += ConnectionFailed;
            callbacks.ConnectionSuspended += ConnectionSuspended;
            callbacks.LocationChanged += LocationChanged;

            client = 
                new GoogleApiClient.Builder(Application.Context, callbacks, callbacks)
                .AddApi(LocationServices.API)
                .Build();
        }

        private void LocationChanged(object sender, Location location)
        {
            var position = new Position();
            position.Latitude = location.Latitude;
            position.Longitude = location.Longitude;
            position.Accuracy = location.Accuracy;
            position.AltitudeAccuracy = location.Accuracy;
            position.Altitude = location.Altitude;
            position.Heading = location.Bearing;
            position.Speed = location.Speed;
            position.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(location.Time);

            PositionChanged?.Invoke(this, new PositionEventArgs(position));
        }

        private void ConnectionSuspended(object sender, int i)
        {
        }

        private void ConnectionFailed(object sender, ConnectionResult connectionResult)
        {
        }

        private void Connected(object sender, Bundle bundle)
        {
        }

        public Task<Position> GetPositionAsync(int timeoutMilliseconds = -1, CancellationToken? token = null, bool includeHeading = false)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> StartListeningAsync(int minTime, double minDistance, bool includeHeading = false)
        {
            if (!client.IsConnected)
                client.Connect();

            if (!client.IsConnected)
                return await Task.FromResult(false);

            var locationRequest = new LocationRequest();
            locationRequest.SetSmallestDisplacement(Convert.ToInt64(minDistance))
                .SetFastestInterval(minTime)
                .SetInterval(minTime * 3)
                .SetMaxWaitTime(minTime * 6)
                .SetPriority(GetPriority());

            var result = await LocationServices.FusedLocationApi
                .RequestLocationUpdatesAsync(client, locationRequest, callbacks);

            return result.IsSuccess;
        }

        public async Task<bool> StopListeningAsync()
        {
            var result = await LocationServices.FusedLocationApi.RemoveLocationUpdatesAsync(client, callbacks);
            return result.IsSuccess;
        }

        private int GetPriority()
        {
            if (DesiredAccuracy < 50)
                return LocationRequest.PriorityHighAccuracy;

            if (DesiredAccuracy < 100)
                return LocationRequest.PriorityBalancedPowerAccuracy;

            if (DesiredAccuracy < 200)
                return LocationRequest.PriorityLowPower;

            return LocationRequest.PriorityNoPower;
        }

        public void Dispose()
        {
            if (client.IsConnected)
                client.Disconnect();

            callbacks?.Dispose();
            client?.Dispose();
        }
    }
}