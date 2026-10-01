using System;
using System.Collections.Generic;

namespace HCMPortal.Models;

public partial class TblTaxMaster
{
    public int TaxId { get; set; }

    public string TaxName { get; set; } = null!;

    public decimal TaxPercentage { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? ModifedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? IsDelete { get; set; }
}
