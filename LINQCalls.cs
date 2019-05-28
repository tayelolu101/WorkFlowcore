using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using WorkFlowCore;
using WorkFlowCore.Models;

namespace WorkFlowCore.AppCode
{
    public class LINQCalls
    {
        private static ApplicationDbContext _context;

        public LINQCalls()
        {
            _context = new ApplicationDbContext("XceedTestDB");
        }

        internal static Tuple<int, string, int> getXceedConnector(StaffADProfile staffADProfile)
        {
            AppraisalDbEntities conn = new AppraisalDbEntities();
            var entry = (from d in conn.zib_workflow_xceed_definitions
                         where (d.ad_org_id.Equals(staffADProfile.org_id))
                         select new
                         {
                             conn_name = d.conn_name,
                             org_id = d.org_id,
                             ad_org_id = d.ad_org_id
                         }).First();
            string connString = System.Configuration.ConfigurationManager.ConnectionStrings[entry.conn_name].ConnectionString;
            return Tuple.Create(entry.org_id, connString, entry.ad_org_id);
        }

        static public StaffADProfile getXceedProfile(StaffADProfile staffADProfile)
        {

            XceedEntities xceed = new XceedEntities();

            
            try
            {
                
                var profile = (from distinct in xceed.vw_employeeinfo
                                      from zone in xceed.vw_manpowergroupings
                               where (distinct.employee_number == staffADProfile.employee_number)
                                      && distinct.Branch_code.Equals(zone.branch_code)
                                      && distinct.org_id.Equals(zone.org_id)
                                      
                               select

                               new StaffADProfile
                               {
                                   branch_name = distinct.Branch,
                                   branch_code = distinct.Branch_code,
                                   employee_number = distinct.employee_number,
                                   name = distinct.name.Replace(",", ""),
                                   doe = distinct.employment_date/*.Value.ToString("dddd MMMM d, yyyy", CultureInfo.CreateSpecificCulture("en-US"))*/,
                                   dob = distinct.date_of_birth.Value/*.ToString("dddd MMMM d, yyyy", CultureInfo.CreateSpecificCulture("en-US"))*/,
                                   lastpromotiondate = distinct.last_promo_date,
                                   grade = distinct.grade_code,
                                   grade_id = distinct.grade_id,
                                   email = distinct.email,
                                   SelectedDept = distinct.dept,
                                   department_id = distinct.department_id,
                                   unit = distinct.unit,
                                   jobtitle = distinct.jobtitle,
                                   confirm = distinct.emp_confirm,
                                   gender = (int)distinct.gender,
                                   org_id = (int)distinct.org_id,
                                   ranking = (int)distinct.ranking,
                                   account_no = distinct.account_no,
                                   mobile_phone = distinct.mobile_phone,
                                   category = distinct.Category ?? "OPERATIONS",
                                   groupcode = zone.zone_id.ToString(),
                                   groupname = zone.zonal_name,
                                   //financial_year  = distinct.ye
                                   imagelink = "url(http://xceedservermain/EmployeePassport/" + distinct.employee_number + ".jpg)"

                               }).Distinct().First();

                staffADProfile.branch_name          = profile.branch_name;
                staffADProfile.branch_code          = profile.branch_code;
                staffADProfile.employee_number      = profile.employee_number;
                staffADProfile.name                 = profile.name;
                staffADProfile.doe                  = profile.doe;
                staffADProfile.dob                  = profile.dob;
                staffADProfile.lastpromotiondate    = profile.lastpromotiondate;
                staffADProfile.grade                = profile.grade;
                staffADProfile.grade_id             = profile.grade_id;
                staffADProfile.email                = profile.email;
                staffADProfile.SelectedDept         = profile.SelectedDept;
                staffADProfile.department_id        = profile.department_id;
                staffADProfile.unit                 = profile.unit;
                staffADProfile.jobtitle             = profile.jobtitle;
                staffADProfile.confirm              = profile.confirm;
                staffADProfile.gender               = profile.gender;
                staffADProfile.org_id               = profile.org_id;
                staffADProfile.ranking              = profile.ranking;
                staffADProfile.account_no           = profile.account_no;
                staffADProfile.mobile_phone         = profile.mobile_phone;
                staffADProfile.category             = profile.category;
                staffADProfile.imagelink            = profile.imagelink;
                staffADProfile.groupcode            = profile.groupcode;
                staffADProfile.groupname            = profile.groupname;
                staffADProfile.supergroupcode       = 1;
                staffADProfile.supergroupname       = "ZENITH BANK PLC";

                staffADProfile.financial_year = DataHandler.getFinancialYear();

            }
            catch (Exception ex)
            {
                LogWriter logWriter = new LogWriter();
                return null;
            }

            return staffADProfile;
        }

        static public List<CustomerSearchEntryModel> getSearchRequestEntry(string workflowid)
        {
            try
            {
                AppraisalDbEntities workflowcnxn = new AppraisalDbEntities();
                var entries = (from w in workflowcnxn.zib_customersearch_entries
                               where w.workflowid.Equals(workflowid)
                               select new CustomerSearchEntryModel
                               {

                                   StaffName = w.name,
                                   AccountNumber = w.account_no,
                                   AccountName = w.account_name,
                                   RegistrationNumber = w.registration_no,
                                   StaffNumber = w.employee_number,
                                   RequestStageId = w.requeststageid,
                                   RequestStage = w.requeststage,
                                   DateSubmitted = w.createdate,
                                   LastEditDate = w.lasteditdate,
                                   RequestStatus = w.status,
                                   EmailAddress = w.email,
                                   EmployeeNumber = w.employee_number,
                                   StaffGrade = w.grade,
                                   org_id = w.org_id,
                                   Ranking = w.ranking,
                                   Phone = w.phone,
                                   ad_org_id = w.ad_org_id,
                                   supergroupcode = w.supergroupcode,
                                   supergroupname = w.supergroupname,
                                   GroupName = w.groupname,
                                   GroupCode = w.groupcode,
                                   DeptCode = w.deptcode,
                                   DeptName = w.deptname,
                                   UnitName = w.unitname,
                                   UnitCode = w.unitcode,
                                   FeeAmount = w.feeAmount,
                                   Comments = w.comments,
                                   EntryPoint = w.entrypoint,


                               }).ToList();
                return entries;
            }
            catch(Exception ex){
                return null;
            }
            return null;
        }

        internal static bool IsBranchHead(StaffADProfile staffADProfile)
        {
            XceedEntities ctx = new XceedEntities();
            var result = new List<LinqResponseModel>();
            switch (staffADProfile.branch_code)
            {
                case "001":
                    result = (from w in ctx.vw_headoffice_workflow
                              where w.depthead_number == staffADProfile.employee_number
                              select new LinqResponseModel
                              {
                                  value = w.depthead_name
                              }).ToList();
                    break;
                case "013":
                    result = (from w in ctx.vw_abuja_workflow
                              where w.branch_head_number == staffADProfile.employee_number
                              select new LinqResponseModel
                              {
                                  value = w.branchhead_name
                              }).ToList();
                    break;
                default:
                    result = (from w in ctx.vw_branches_workflow
                              where w.branch_head_number == staffADProfile.employee_number
                              select new LinqResponseModel
                              {
                                  value = w.branchhead_name
                              }).ToList();
                    break;
            }

            if (result.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static bool IsHOP(StaffADProfile staffADProfile)
        {
            XceedEntities ctx = new XceedEntities();
            var result = new List<LinqResponseModel>();
            switch(staffADProfile.branch_code)
            {
                case "001":
                    result = (from w in ctx.vw_headoffice_workflow
                              where w.unithead_number == staffADProfile.employee_number
                              select new LinqResponseModel {
                              value = w.unithead_name
                                }).ToList();
                    break;
                case "013":
                    result = (from w in ctx.vw_abuja_workflow
                              where w.hop_number == staffADProfile.employee_number
                              select new LinqResponseModel
                              {
                                  value = w.hop_name
                              }).ToList();
                    break;
                default:
                    result = (from w in ctx.vw_branches_workflow
                              where w.hop_number == staffADProfile.employee_number
                              select new LinqResponseModel
                              {
                                  value = w.hop_name
                              }).ToList();
                    break;
            }
            
            if (result.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}