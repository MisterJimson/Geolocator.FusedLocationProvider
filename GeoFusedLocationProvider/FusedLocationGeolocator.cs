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
        public bool SupportsHeading => true;
        public bool AllowsBackgroundUpdates { get; set; }
        public bool PausesLocationUpdatesAutomatically { get; set; }

        public bool IsGeolocationAvailable
        {
            get
            {
                if (client != null && client.IsConnected)
                {
                    var avalibility = LocationServices.FusedLocationApi.GetLocationAvailability(client);
                    if (avalibility != null)
                        return avalibility.IsLocationAvailable;
                }

                return IsLocationServicesEnabled();
            }
        }

        public bool IsGeolocationEnabled => IsLocationServicesEnabled();
        

        public event EventHandler<PositionErrorEventArgs> PositionError;
        public event EventHandler<PositionEventArgs> PositionChanged;

        private GoogleCallbacks callbacks;
        private GoogleApiClient client;

        private Position LastPosition;

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

            client.Connect();
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

            LastPosition = position;
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

        public async Task<Position> GetPositionAsync(int timeoutMilliseconds = -1, CancellationToken? cancelToken = null, bool includeHeading = false)
        {
            if (timeoutMilliseconds <= 0 && timeoutMilliseconds != Timeout.Infinite)
                throw new ArgumentOutOfRangeException("timeoutMilliseconds", "timeout must be greater than or equal to 0");

            if (!cancelToken.HasValue)
                cancelToken = CancellationToken.None;

            if (IsListening)
            {
                if (LastPosition != null)
                    return LastPosition;
                else
                    return await NextLocationAsync();
            }
            else
            {
                var nextLocation = NextLocationAsync();
                var startListening = StartListeningAsync(500, 10);

                await startListening;
                var position = await nextLocation;
                await StopListeningAsync();
                return position;
            }
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
            TaskCompletionSource<bool> connectionSource = new TaskCompletionSource<bool>();
            EventHandler<Bundle> handler = null;

            handler = (sender, args) =>
            {
                callbacks.Connected -= handler;
                connectionSource.SetResult(true);
            };

            callbacks.Connected += handler;
            client.Connect();
            return connectionSource.Task;
        }

        private Task<Position> NextLocationAsync()
        {
            TaskCompletionSource<Position> locationSource = new TaskCompletionSource<Position>();
            EventHandler<PositionEventArgs> handler = null;

            handler = (sender, args) =>
            {
                this.PositionChanged -= handler;
                locationSource.SetResult(args.Position);
            };

            this.PositionChanged += handler;
            return locationSource.Task;
        }

        private bool IsLocationServicesEnabled()
        {
            int locationMode = 0;
            string locationProviders;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                try
                {
                    locationMode = Android.Provider.Settings.Secure.GetInt(
                        Application.Context.ContentResolver, 
                        Android.Provider.Settings.Secure.LocationMode);
                }
                catch (Exception e)
                {
                    return false;
                }

                return locationMode != (int)Android.Provider.SecurityLocationMode.Off;
            }
            else
            {
                locationProviders = Android.Provider.Settings.Secure.GetString(
                    Application.Context.ContentResolver,
                    Android.Provider.Settings.Secure.LocationProvidersAllowed);

                return !string.IsNullOrEmpty(locationProviders);
            }
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