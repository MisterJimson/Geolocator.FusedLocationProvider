using Android.App;
using Android.Gms.Common;

namespace GeoFusedLocationProvider
{
    internal class Util
    {
        public static bool IsPlayServicesAvailable()
        {
            var result = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(Application.Context);
            return result == ConnectionResult.Success;
        }
    }
}