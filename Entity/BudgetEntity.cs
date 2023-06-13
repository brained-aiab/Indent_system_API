using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class BudgetEntity
    {
        public object Style;
        public string BussinessLevel { get; set; }
        public string LocationLevel { get; set; }
        public string Remark { get; set; }
        public string Type { get; set; }
        public string file { get; set; }
        public string LocationId { get; set; }
        public string UserId { get; set; }
        public string EncryptedPassword { get; set; }
        public string UserName { get; set; }
        public string Main_Group { get; set; }
        public string Subgroup { get; set; }
        public string Department { get; set; }
        public string Role { get; set; }
        public string Branch { get; set; }
        public string Mega_Zone { get; set; }
        public string Zone { get; set; }
        public string Region { get; set; }
        public string Status { get; set; }
        public string Budget { get; set; }
        public string Budgetyear { get; set; }
        public string Buffer { get; set; }
        public string ID { get; set; }
        public string Year { get; set; }
        public string CurrentYear { get; set; }
        public string NextYear { get; set; }
        public string Date { get; set; }

        public int stauscode { get; set; }
        public string message { get; set; }
        public object Attachments { get; set; }

        public object Grade { get; set; }
        public object RowNumber { get; set; }

        public object RowSize { get; set; }
        public object Designation { get; set; }
        public object Location { get; set; }
        public object Reason { get; set; }
        public object EmpID { get; set; }
        public object RAId { get; set; }
        public object DeptShortCode { get; set; }
        public object RefBMId { get; set; }
        public object RefEMPID { get; set; }
        public object OFFSET { get; set; }
        public object LIMIT { get; set; }
        public object Emp_Count { get; set; }
        public object CreatedBy { get; set; }
        public object IndentType { get; set; }
        public object HrsContactID { get; set; }
        public object Worksheets { get; set; }

        public string FileName { get; set; }
        public DataTable Table { get; set; }
        public object RefIndentId { get; set; }
        public object ModifyBy { get; set; }
        public object RefId { get; set; }
        // public object Data { get; set; }
        public object Jobcode { get; set; }
        public object RA { get; set; }
        public object DescrShort { get; set; }

        public object Description { get; set; }
        public object RequestorName { get; set; }
        public object FromDate { get; set; }
        public object ToDate { get; set; }
        public object User_Code { get; set; }
        // public object Password { get; set; }
        public object EmpName { get; set; }
        public object Action { get; set; }
        public object DeletedBy { get; set; }

        public object Search { get; set; }
        public object SearchOn { get; set; }
        public object BookmarkName { get; set; }
        public string PId { get; set; }
        public DataTable datatable { get; set; }
        public string Password { get; set; }
        public string Tokens { get; set; }
        public string Data { get; set; }

    }



}
