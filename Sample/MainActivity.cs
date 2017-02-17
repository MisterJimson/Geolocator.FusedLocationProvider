using System;
using Android.App;
using Android.Widget;
using Android.OS;
using GeoFusedLocationProvider;
using Plugin.Geolocator.Abstractions;

namespace Sample
{
    [Activity(Label = "Sample", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        IGeolocator geolocator = MainApplication.Geolocator;

        private TextView isGeolocationAvailable;
        private TextView isGeolocationEnabled;
        private TextView isListening;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            isGeolocationAvailable = FindViewById<TextView>(Resource.Id.isGeolocationAvailable);
            isGeolocationEnabled = FindViewById<TextView>(Resource.Id.isGeolocationEnabled);
            isListening = FindViewById<TextView>(Resource.Id.isListening);

            FindViewById<Button>(Resource.Id.start).Click += Start;
            FindViewById<Button>(Resource.Id.stop).Click += Stop;
            FindViewById<Button>(Resource.Id.getpos).Click += GetPos;

            geolocator.PositionChanged += GeolocatorOnPositionChanged;

            SetValues();
         }

        private void SetValues()
        {
            isGeolocationAvailable.Text = $"Geolocation Available: {geolocator.IsGeolocationAvailable}";
            isGeolocationEnabled.Text = $"Geolocation Enabled: {geolocator.IsGeolocationEnabled}";
            isListening.Text = $"Listening: {geolocator.IsListening}";
        }

        private async void GetPos(object sender, EventArgs eventArgs)
        {
            await geolocator.GetPositionAsync();
            SetValues();
        }

        private async void Stop(object sender, EventArgs eventArgs)
        {
            await geolocator.StopListeningAsync();
            SetValues();
        }

        private async void Start(object sender, EventArgs eventArgs)
        {
            await geolocator.StartListeningAsync(0, 0);
            SetValues();
        }

        private void GeolocatorOnPositionChanged(object sender, PositionEventArgs positionEventArgs)
        {
            FindViewById<TextView>(Resource.Id.lastPosition).Text = $"Pos: {positionEventArgs.Position.Latitude} {positionEventArgs.Position.Longitude}";
        }
    }
}

