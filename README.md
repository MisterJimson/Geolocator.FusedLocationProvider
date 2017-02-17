# Geolocator.FusedLocationProvider

This is a replacement for the default Android Geolocator implementation in the [GeolocatorPlugin](https://github.com/jamesmontemagno/GeolocatorPlugin).

This implementation is backed by the [Fused Location Provider Api](https://developers.google.com/android/reference/com/google/android/gms/location/FusedLocationProviderApi) instead of using the standard Android [Location Api](https://developer.android.com/reference/android/location/package-summary.html).

Using this Api requires the device to have Google Play Services avilable, so this library will fallback to the standard implementation if Google Play Services cannot be found.

Refer to the [GeolocatorPlugin](https://github.com/jamesmontemagno/GeolocatorPlugin) docs for Api usage info.

To use this library instantiate a FusedLocationGeolocator for your IGeolocator implementation in your Android.
