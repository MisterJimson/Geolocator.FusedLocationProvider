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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            FindViewById<TextView>(Resource.Id.isGeolocationAvailable).Text = geolocator.IsGeolocationAvailable.ToString();
            FindViewById<TextView>(Resource.Id.isGeolocationEnabled).Text = geolocator.IsGeolocationEnabled.ToString();
            FindViewById<Button>(Resource.Id.start).Click += Start;
            FindViewById<Button>(Resource.Id.stop).Click += Stop;
            FindViewById<Button>(Resource.Id.getpos).Click += GetPos;

            geolocator.PositionChanged += GeolocatorOnPositionChanged;
        }

        private void GetPos(object sender, EventArgs eventArgs)
        {
            geolocator.GetPositionAsync();
        }

        private void Stop(object sender, EventArgs eventArgs)
        {
            geolocator.StopListeningAsync();
        }

        private void Start(object sender, EventArgs eventArgs)
        {
            geolocator.StartListeningAsync(500, 100);
        }

        private void GeolocatorOnPositionChanged(object sender, PositionEventArgs positionEventArgs)
        {
            FindViewById<TextView>(Resource.Id.lastPosition).Text = $"Pos: {positionEventArgs.Position.Latitude} {positionEventArgs.Position.Longitude}";
        }
    }
}

