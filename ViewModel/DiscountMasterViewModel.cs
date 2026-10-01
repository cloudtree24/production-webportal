using HCMPortal.Models;

namespace HCMPortal.ViewModel
{
    public class DiscountMasterViewModel: TblDiscountMaster
    {
        public List<PlanMaster>? planMaster { get; set; }
        public List<int>? planIds { get; set; }
        public List<TblSubscriptionMaster>? subscriptionMaster { get; set; }
        public List<int>? subscriptionIds { get; set; }
        public List<PortalCredential>? portalCredential { get; set; }
        public List<int> ApplicableCustomerIds { get; set; } = new List<int>();
        public List<int> ApplicableCompanyTypeIds { get; set; } = new List<int>();
    }
}
