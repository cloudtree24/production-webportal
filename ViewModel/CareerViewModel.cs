using HCMPortal.Models;
namespace HCMPortal.ViewModel
{
    public class CareerViewModel
    {
        public List<TblJobPostDetail>? JobList { get; set; }

        public TblCandiateApplication? CandidateApplication { get; set; }
        public string? SelectedJobTitle { get; set; }
    }
}
