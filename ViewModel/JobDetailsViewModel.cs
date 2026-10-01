using HCMPortal.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HCMPortal.ViewModel
{
    public class JobDetailsViewModel
    {
        [ValidateNever]
        public TblJobPostDetail? tblJobPostDetail { get; set; } 
        public TblCandiateApplication? CandidateApplication { get; set; }
        public string? SelectedJobTitle { get; set; }
    }
}
