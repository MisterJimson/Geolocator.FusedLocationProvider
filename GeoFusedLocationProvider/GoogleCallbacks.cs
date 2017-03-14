using System;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Locations;
using Android.OS;
using Android.Runtime;

namespace GeoFusedLocationProvider
{
    public class GoogleCallbacks : Java.Lang.Object,
                                   GoogleApiClient.IConnectionCallbacks,
                                   GoogleApiClient.IOnConnectionFailedListener, 
                                   Android.Gms.Location.ILocationListener
    {
        public EventHandler<Bundle> Connected;
        public EventHandler<ConnectionResult> ConnectionFailed;
        public EventHandler<int> ConnectionSuspended;
        public EventHandler<Location> LocationChanged;

        public GoogleCallbacks()
        {
            
        }

        public GoogleCallbacks(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
            
        }

        public void OnConnected(Bundle connectionHint)
        {
            Connected?.Invoke(this, connectionHint);
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            ConnectionFailed?.Invoke(this, result);
        }

        public void OnConnectionSuspended(int cause)
        {
            ConnectionSuspended?.Invoke(this, cause);
        }

        public void OnLocationChanged(Location location)
        {
            if (!(location.Latitude == 0.0 && location.Longitude == 0.0))
                LocationChanged?.Invoke(this, location);
        }
    }
}