using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL
{
   public class UtilityClass
    {
        public static void ActivityLog(string message)
        {
            //Server.MapPath(@"~/ActivityLog/ActivityLog.txt");
            string filePath = System.Web.HttpContext.Current.Server.MapPath(@"~/ActivityLog/ActivityLog.txt");
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
            {
                file.WriteLine(message + System.Environment.NewLine);
            }
        }
        //public class Utility
        //{
        //    public static void ActivityLog(string message)
        //    {
        //        //Server.MapPath(@"~/ActivityLog/ActivityLog.txt");
        //        string filePath = System.Web.HttpContext.Current.Server.MapPath(@"~/ActivityLog/ActivityLog.txt");
        //        using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
        //        {
        //            file.WriteLine(message + System.Environment.NewLine);
        //        }
        //    }
        //    public string decodeString(string encoded_string = "")
        //    {
        //        byte[] data = Convert.FromBase64String(encoded_string);
        //        string decodedString = System.Text.Encoding.UTF8.GetString(data);
        //        return decodedString;
        //    }

        //    //public static void InsertError(Exception e, string TokenID)
        //    //{
        //    //    Account_BAL objBal = new Account_BAL();
        //    //    Account_Entity objParam = new Account_Entity();
        //    //    objParam.TokenID = TokenID;
        //    //    //objParam.CreatedDate = DateTime.Now.ToString();
        //    //    //objParam.ErrorOccuredIN = methodName;
        //    //    //objParam.ERROR = e.ToString();
        //    //    //objParam.StackTrace = e.StackTrace.ToString();
        //    //    //objParam.Message = e.Message.ToString();
        //    //    objParam.Message = "Error Message: " + e.Message.ToString() + "Error print trace: " + e.StackTrace.ToString();
        //    //    objBal.InsertActivityLog(objParam);
        //    //}
        //    //public static void InsertFunctionDataTemp(string UserCode = "", string TokenID = "")
        //    //{
        //    //    Account_BAL objBal = new Account_BAL();
        //    //    Account_Entity objParam = new Account_Entity();
        //    //    objParam.TokenID = TokenID;
        //    //    objParam.Message = "EmployeeCode: " + UserCode.ToString();
        //    //    objBal.InsertActivityLog(objParam);
        //    //}



        //    public static string DataTableToJSON(DataTable table)
        //    {
        //        var JSONString = new StringBuilder();
        //        if (table.Rows.Count > 0)
        //        {
        //            JSONString.Append("[");
        //            for (int i = 0; i < table.Rows.Count; i++)
        //            {
        //                JSONString.Append("{");
        //                for (int j = 0; j < table.Columns.Count; j++)
        //                {
        //                    if (j < table.Columns.Count - 1)
        //                    {
        //                        JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString() + "\",");
        //                    }
        //                    else if (j == table.Columns.Count - 1)
        //                    {
        //                        JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString() + "\"");
        //                    }
        //                }
        //                if (i == table.Rows.Count - 1)
        //                {
        //                    JSONString.Append("}");
        //                }
        //                else
        //                {
        //                    JSONString.Append("},");
        //                }
        //            }
        //            JSONString.Append("]");
        //        }
        //        return JSONString.ToString();
        //    }
        //}
    }
}
