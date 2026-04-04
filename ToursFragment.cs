using Android.OS;
using Android.Views;

namespace poimap
{
    public class ToursFragment : AndroidX.Fragment.App.Fragment
    {
        public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.layout_tours, container, false);
        }
    }
}