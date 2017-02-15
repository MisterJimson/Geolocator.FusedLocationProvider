using Android.App;
using Android.Widget;
using Android.OS;
using GeoFusedLocationProvider;

namespace Sample
{
    [Activity(Label = "Sample", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            var geo = new FusedLocationGeolocator();
            geo.StartListeningAsync(5, 5, false);
        }
    }
}

