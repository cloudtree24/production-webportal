using HCMPortal.Models;

namespace HCMPortal.ViewModel
{
    public class SubscriptionPageViewModel
    {
        public List<PlanMaster>? Plans { get; set; }
        public List<SubscriptionDetailsViewModel>? SubscriptionDetails { get; set; }
        public List<WebBanner>? bannerList { get; set; }
        public List<TestimonialModel>? testimonialList { get; set; }// newly added for testimonial on subscription page -- Daitari 12.11.2025

        public List<TblHomeFeatureMaster>? featureList { get; set; }
    }
}
