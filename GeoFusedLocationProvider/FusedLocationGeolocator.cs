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
        public bool IsListening { get; private set; }
        public bool SupportsHeading { get; private set; }
        public bool AllowsBackgroundUpdates { get; set; }
        public bool PausesLocationUpdatesAutomatically { get; set; }

        public bool IsGeolocationAvailable =>
            LocationServices.FusedLocationApi.GetLocationAvailability(client).IsLocationAvailable;

        public bool IsGeolocationEnabled =>
            LocationServices.FusedLocationApi.GetLocationAvailability(client).IsLocationAvailable;

        public event EventHandler<PositionErrorEventArgs> PositionError;
        public event EventHandler<PositionEventArgs> PositionChanged;

        private GoogleCallbacks callbacks;
        private GoogleApiClient client;

        public FusedLocationGeolocator()
        {
            DesiredAccuracy = 90;

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
            var position = new Position
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Accuracy = location.Accuracy,
                AltitudeAccuracy = location.Accuracy,
                Altitude = location.Altitude,
                Heading = location.Bearing,
                Speed = location.Speed,
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(location.Time)
            };

            System.Diagnostics.Debug.WriteLine($"PositionChanged: {position.Latitude} {position.Longitude}");
            PositionChanged?.Invoke(this, new PositionEventArgs(position));
        }

        private void ConnectionSuspended(object sender, int i)
        {
            System.Diagnostics.Debug.WriteLine($"ConnectionSuspended: {i}");
        }

        private void ConnectionFailed(object sender, ConnectionResult connectionResult)
        {
            System.Diagnostics.Debug.WriteLine($"ConnectionFailed: {connectionResult.ErrorMessage}");
        }

        private void Connected(object sender, Bundle bundle)
        {
            System.Diagnostics.Debug.WriteLine("Connected");
        }

        public Task<Position> GetPositionAsync(int timeoutMilliseconds = -1, CancellationToken? token = null, bool includeHeading = false)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> StartListeningAsync(int minTime, double minDistance, bool includeHeading = false)
        {
            if (!client.IsConnected)
                await ConnectAsync();

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

            if (result.IsSuccess)
                IsListening = true;

            return result.IsSuccess;
        }

        public async Task<bool> StopListeningAsync()
        {
            var result = await LocationServices.FusedLocationApi.RemoveLocationUpdatesAsync(client, callbacks);

            if (result.IsSuccess)
                IsListening = false;

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

        private Task ConnectAsync()
        {
            TaskCompletionSource<bool> resultTaskCompletionSource = new TaskCompletionSource<bool>();
            callbacks.Connected += delegate
            {
                resultTaskCompletionSource.SetResult(true);
            };
            client.Connect();
            return resultTaskCompletionSource.Task;
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