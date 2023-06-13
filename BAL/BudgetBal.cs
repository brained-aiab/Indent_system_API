using DAL;
using Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL
{
    public class BudgetBal
    {
        BudgetDal objDal = new BudgetDal();
        public DataSet GetYear()
        {
            return new BudgetDal().GetYear();
        }

        public DataSet GetLocationMaster1()
        {
            return new BudgetDal().GetLocationMaster1();
        }

        public DataSet BranchDetils()
        {
            return new BudgetDal().BranchDetils();
        }

        public DataSet GetBusinesslevel(BudgetEntity objEntity)
        {
            return new BudgetDal().GetBusinesslevel(objEntity);
        }

        public DataSet GetLocation(BudgetEntity objEntity)
        {
            return new BudgetDal().GetLocation(objEntity);
        }

        public DataSet UserLogin(string action, BudgetEntity objEntity)
        {

            DataSet result = new DataSet();
            result = objDal.UserLogin(action, objEntity);
            return result;
        }

        public DataSet InsertBudget(BudgetEntity objEntity)
        {
            return new BudgetDal().InsertBudget(objEntity);
        }

        public DataSet UpdateBudget(BudgetEntity objEntity)
        {
            return new BudgetDal().UpdateBudget(objEntity);
        }

        public DataSet GetBudgetDetails(BudgetEntity objEntity)
        {
            return new BudgetDal().GetBudgetDetails(objEntity);
        }

        public DataSet InsertZone(BudgetEntity objEntity)
        {
            return new BudgetDal().InsertZone(objEntity);
        }

        public DataSet GetBudgetStaggering(BudgetEntity objEntity)
        {
            return new BudgetDal().GetBudgetStaggering(objEntity);
        }

        public DataSet addBudgetStaggering(BudgetEntity objEntity)
        {
            return new BudgetDal().addBudgetStaggering(objEntity);
        }

        public DataSet ChangeDefaultBudgetYear(BudgetEntity objEntity)
        {
            return new BudgetDal().ChangeDefaultBudgetYear(objEntity);
        }

        public DataSet UpadateStaggering(BudgetEntity objEntity)
        {
            return new BudgetDal().UpadateStaggering(objEntity);
        }

        public DataSet DeleteStaggering(BudgetEntity objEntity)
        {
            return new BudgetDal().DeleteStaggering(objEntity);
        }

        public DataSet GetRemarks(BudgetEntity objEntity)
        {
            return new BudgetDal().GetRemarks(objEntity);
        }

        public DataSet GetBudgetAttachment(BudgetEntity objEntity)
        {
            return new BudgetDal().GetBudgetAttachment(objEntity);
        }

        public DataSet GetGradeDetails(BudgetEntity objEntity)
        {
            return new BudgetDal().GetGradeDetails(objEntity);
        }

        public DataSet GetRAId(BudgetEntity objEntity)
        {
            return new BudgetDal().GetRAId(objEntity);
        }

        public DataSet InsertIndent(BudgetEntity objEntity)
        {
            return new BudgetDal().InsertIndent(objEntity);
        }

        public DataSet GetIndentDetails(BudgetEntity objEntity)
        {
            return new BudgetDal().GetIndentDetails(objEntity);
        }

        public DataSet GetBudgetMappingDetails(BudgetEntity objEntity)
        {
            return new BudgetDal().GetBudgetMappingDetails(objEntity);
        }

        public DataSet GetIndentTypeMaster(BudgetEntity objEntity)
        {
            return new BudgetDal().GetIndentTypeMaster(objEntity);
        }

        public DataTable ImportBusinesslevelMapping(DataTable csvFileData, BudgetEntity objEntity)
        {
            return new BudgetDal().ImportBusinesslevelMapping(csvFileData, objEntity);
        }

        public DataTable ImportLocationlevelMapping(DataTable csvFileData, BudgetEntity objEntity)
        {
            return new BudgetDal().ImportLocationlevelMapping(csvFileData, objEntity);
        }

        public DataTable ExportCsvFile(BudgetEntity objEntity)
        {
            return new BudgetDal().ExportCsvFile(objEntity);
        }

        public DataTable ImportResponseCsvFile(DataTable csvFileData)
        {
            return new BudgetDal().ImportResponseCsvFile(csvFileData);
        }

        public DataSet GetLocationMaster(BudgetEntity objEntity)
        {
            return new BudgetDal().GetLocationMaster(objEntity);
        }

        public DataSet GetBusinessMaster(BudgetEntity objEntity)
        {
            return new BudgetDal().GetBusinessMaster(objEntity);
        }

        public DataSet UpadateIndent(BudgetEntity objEntity)
        {
            return new BudgetDal().UpadateIndent(objEntity);
        }

        public DataSet InsertLocationMapping(BudgetEntity objEntity)
        {
            return new BudgetDal().InsertLocationMapping(objEntity);
        }

        public DataSet UpdateLocationMapping(BudgetEntity objEntity)
        {
            return new BudgetDal().UpdateLocationMapping(objEntity);
        }

        public DataSet GetLocationMapping(BudgetEntity objEntity)
        {
            return new BudgetDal().GetLocationMapping(objEntity);
        }

        public DataSet InsertZoneMasterMappingData(BudgetEntity objEntity)
        {
            return new BudgetDal().InsertZoneMasterMappingData(objEntity);
        }

        public DataSet InsertManualIndent(BudgetEntity objEntity)
        {
            return new BudgetDal().InsertManualIndent(objEntity);
        }

        public DataSet GetRole(BudgetEntity objEntity)
        {
            return new BudgetDal().GetRole(objEntity);
        }

        public DataSet GetMaingroupSubgropDepartment(BudgetEntity objEntity)
        {
            return new BudgetDal().GetMaingroupSubgropDepartment(objEntity);
        }

        public DataSet InsertBusinessMappingData(BudgetEntity objEntity)
        {
            return new BudgetDal().InsertBusinessMappingData(objEntity);
        }

        public DataSet UpdateJobCodeInBusinessMapping(BudgetEntity objEntity)
        {
            return new BudgetDal().UpdateJobCodeInBusinessMapping(objEntity);
        }

        public DataTable IndentReport(BudgetEntity objEntity)
        {
            return new BudgetDal().IndentReport(objEntity);
        }

        public DataTable BudgetingReport(BudgetEntity objEntity)
        {
            return new BudgetDal().BudgetingReport(objEntity);
        }

        public DataTable GradeReport(BudgetEntity objEntity)
        {
            return new BudgetDal().GradeReport(objEntity);
        }

        public DataTable LocationReport(BudgetEntity objEntity)
        {
            return new BudgetDal().LocationReport(objEntity);
        }

        public DataTable JobcodeReport(BudgetEntity objEntity)
        {
            return new BudgetDal().JobcodeReport(objEntity);
        }

        public DataTable BusinessLevelReport(BudgetEntity objEntity)
        {
            return new BudgetDal().BusinessLevelReport(objEntity);
        }

        public DataTable ErrorLogFilter(BudgetEntity objEntity)
        {
            return new BudgetDal().ErrorLogFilter(objEntity);
        }

        public DataTable SenderEmail(BudgetEntity objEntity)
        {
            return new BudgetDal().SenderEmail(objEntity);
        }

        public DataSet IndentGetRemarks(BudgetEntity objEntity)
        {
            return new BudgetDal().IndentGetRemarks(objEntity);
        }

        public DataSet GetMZRMapping(BudgetEntity objEntity)
        {
            return new BudgetDal().GetMZRMapping(objEntity);
        }

        public DataSet InsertMZRZoneMaster(BudgetEntity objEntity)
        {
            return new BudgetDal().InsertMZRZoneMaster(objEntity);
        }

        public DataSet UpdateZoneMaster(BudgetEntity objEntity)
        {
            return new BudgetDal().UpdateZoneMaster(objEntity);
        }

        public DataSet SearchBusinesslevel(BudgetEntity objEntity)
        {
            return new BudgetDal().SearchBusinesslevel(objEntity);
        }

        public DataSet SearchLocationlevel(BudgetEntity objEntity)
        {
            return new BudgetDal().SearchLocationlevel(objEntity);
        }

        public DataSet SearchIndent(BudgetEntity objEntity)
        {
            return new BudgetDal().SearchIndent(objEntity);
        }

        public DataSet SearchBudgetDetails(BudgetEntity objEntity)
        {
            return new BudgetDal().SearchBudgetDetails(objEntity);
        }

        public DataSet GetUserDetails(BudgetEntity objEntity)
        {
            return new BudgetDal().GetUserDetails(objEntity);
        }

        public DataSet GetCurrentFY(BudgetEntity objEntity)
        {
            return new BudgetDal().GetCurrentFY(objEntity);
        }

        public DataSet InsertIndividualMZRMapping(BudgetEntity objEntity)
        {
            return new BudgetDal().InsertIndividualMZRMapping(objEntity);
        }

        public DataTable ImportBudget(DataTable csvFileData, BudgetEntity objEntity)
        {
            return new BudgetDal().ImportBudget(csvFileData, objEntity);
        }

        public DataSet InsertBookmarkBudget(BudgetEntity objEntity)
        {
            return new BudgetDal().InsertBookmarkBudget(objEntity);
        }

        public DataSet GetBookmarkBudget(BudgetEntity objEntity)
        {
            return new BudgetDal().GetBookmarkBudget(objEntity);
        }

        public DataSet DeleteBookmarkBudget(BudgetEntity objEntity)
        {
            return new BudgetDal().DeleteBookmarkBudget(objEntity);
        }

        public DataSet Logout(BudgetEntity objEntity)
        {
            return new BudgetDal().Logout(objEntity);
        }

        public DataSet GetTokenByUserId(BudgetEntity objEntity)
        {
            return new BudgetDal().GetTokenByUserId(objEntity);
        }

      



    }

}
