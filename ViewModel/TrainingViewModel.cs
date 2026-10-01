using System.ComponentModel.DataAnnotations;
using HCMPortal.Models;

namespace HCMPortal.ViewModel
{
    public class TrainingViewModel: TrainingModule
    {
        public string? CreatedByUser { get; set; }
        [Required(ErrorMessage = "Required")]
        public int ModuleId { get; set; }
        public string? ModuleName { get; set; }
        public string? FeatureName { get; set; }
    }
}
