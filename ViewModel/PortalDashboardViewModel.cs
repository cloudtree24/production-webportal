using HCMPortal.Models;

namespace HCMPortal.ViewModel
{
    public class PortalDashboardViewModel
    {
        //pending invoice count----Billing
        public int pendingBillingCount { get; set; }
        //Training modules added count---Training Modules
        public int trainingModuleCount { get; set; }
        //open tickets count---Support
        public int openTicketCount { get; set; }
        //company created List
        public List<TblCompanySetUp> CreatedCompany { get; set; }
        // maximum employee alloted vs remaining--Resource usage
        public int employeeRegisteredCount { get; set; }
        public int totalAllotedemployeeRegisteredCount { get; set; }
        public string? EmpEmail { get; set; }
        //added for showing the company setup link for HCM in the customer dashboard--Anshuman 03.03.02026
        public bool isCompanySetupPending { get; set; }
        //// Employee recruitment vs exit for currentmonth then filter data upon requirement-----Employee Recruitment vs Exit
        ////can get from the Hires & Exits Analysis portlet in the dashboard
        //public int totalRecruitment { get; set; }
        //public int totalExit { get; set; }


        ////Employee attendance bar graph----Employee Attendance---can get from this public async Task<IActionResult> GetEmployeeAttendancePortlet() in the hcm frontend
        ////in the commondashboard controller
        //public List<EmployeeAttendanceViewModel> employeeAttendanceList { get; set; }


    }
}
