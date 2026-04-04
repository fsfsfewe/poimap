using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Navigation;
using Android.Gms.Maps;

namespace poimap
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback
    {
        private GoogleMap? _map;
        private BottomNavigationView? _bottomNavigation;
        private HorizontalScrollView? _categoryBar;

        private SupportMapFragment? _mapFragment;
        private ProfileFragment? _profileFragment;
        private ToursFragment? _toursFragment;

        // CHỖ SỬA 1: Khai báo đích danh họ tên đầy đủ của Fragment
        private AndroidX.Fragment.App.Fragment? _activeFragment;

        private const int RequestLocationId = 1;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _bottomNavigation = FindViewById<BottomNavigationView>(Resource.Id.bottom_navigation);
            _categoryBar = FindViewById<HorizontalScrollView>(Resource.Id.category_bar);

            if (_bottomNavigation != null)
            {
                _bottomNavigation.ItemSelected += BottomNavigation_ItemSelected;
            }

            SetupFragments();
        }

        private void SetupFragments()
        {
            _mapFragment = SupportMapFragment.NewInstance();
            _profileFragment = new ProfileFragment();
            _toursFragment = new ToursFragment();

            SupportFragmentManager.BeginTransaction()
                .Add(Resource.Id.map_container, _toursFragment, "tours").Hide(_toursFragment)
                .Add(Resource.Id.map_container, _profileFragment, "profile").Hide(_profileFragment)
                .Add(Resource.Id.map_container, _mapFragment, "map")
                .Commit();

            _activeFragment = _mapFragment;
            _mapFragment.GetMapAsync(this);
        }

        private void BottomNavigation_ItemSelected(object? sender, NavigationBarView.ItemSelectedEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.navigation_map:
                    SwitchFragment(_mapFragment);
                    if (_categoryBar != null) _categoryBar.Visibility = ViewStates.Visible;
                    break;

                case Resource.Id.navigation_tours:
                    SwitchFragment(_toursFragment);
                    if (_categoryBar != null) _categoryBar.Visibility = ViewStates.Gone;
                    break;

                case Resource.Id.navigation_profile:
                    SwitchFragment(_profileFragment);
                    if (_categoryBar != null) _categoryBar.Visibility = ViewStates.Gone;
                    break;
            }
            e.Handled = true;
        }

        // CHỖ SỬA 2: Đổi tham số truyền vào thành tên đầy đủ
        private void SwitchFragment(AndroidX.Fragment.App.Fragment? targetFragment)
        {
            if (targetFragment == null || targetFragment == _activeFragment) return;

            SupportFragmentManager.BeginTransaction()
                .Hide(_activeFragment!)
                .Show(targetFragment)
                .Commit();

            _activeFragment = targetFragment;
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            _map = googleMap;
            if (_map != null)
            {
                _map.UiSettings.ZoomControlsEnabled = true;
            }
            CheckAndRequestLocationPermission();
        }

        private void CheckAndRequestLocationPermission()
        {
            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessFineLocation) == (int)Permission.Granted)
            {
                if (_map != null) { _map.MyLocationEnabled = true; _map.UiSettings.MyLocationButtonEnabled = true; }
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new string[] { Android.Manifest.Permission.AccessFineLocation }, RequestLocationId);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == RequestLocationId && grantResults.Length > 0 && grantResults[0] == Permission.Granted)
            {
                if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessFineLocation) == (int)Permission.Granted)
                {
                    if (_map != null) { _map.MyLocationEnabled = true; _map.UiSettings.MyLocationButtonEnabled = true; }
                }
            }
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}