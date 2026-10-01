using System;
using System.Collections.Generic;

namespace HCMPortal.Models;

public partial class TblLeadSourceMaster
{
    public int Lsid { get; set; }

    public string? SourceName { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? IsDelete { get; set; }
}
