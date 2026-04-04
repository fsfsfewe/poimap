using Android.OS;
using Android.Views;

namespace poimap
{
    // Đã chỉ định rõ họ tên đầy đủ của Fragment
    public class ProfileFragment : AndroidX.Fragment.App.Fragment
    {
        public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.layout_profile, container, false);
        }
    }
}