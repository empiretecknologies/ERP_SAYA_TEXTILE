using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire_ERP.Core.Entities
{
    public class QCJobAssignment
    {


        public int? T_SIZES { get; set; }
        public int? PICK_ID { get; set; }


        public int? CreditAccount { get; set; }
        public int? DebitAccount { get; set; }
        public int? PickId { get; set; }
        public string? RBCODE { get; set; }
        public string? RPERIOD_ID { get; set; }
        public string? REMARKS { get; set; }
        public string? PAGE_TYPE { get; set; }
        public int? TJV_TRANID { get; set; }
    }

    public class QCJobAssignmentViewModel
    {
        public string? Amount { get; set; }
        public string? ToBranch { get; set; }
        public string? ToBranchAddress { get; set; }
        public string? FromBranch { get; set; }
        public string? FromBranchAddress { get; set; }
        public string? FromBranchPhone { get; set; }
        public string? ToBranchPhone{ get; set; }
        
    }

    public class CustomQCJobAssignment
    {
        public List<QCJobAssignment>? Master { get; set; }
    }
}
