using AutoMapper;
using Newtonsoft.Json;
using Sybase.Data.AseClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WorkFlowCore.DTOs;
using WorkFlowCore.Models;


namespace WorkFlowCore.Controllers
{
    [RoutePrefix("api/GetStaff")]
    public class StaffController : ApiController
    {
        private ApplicationDbContext _context;
        private ApplicationDbContext _contextExceed;


        LogWriter logwriter = new LogWriter();
        

        public StaffController()
        {
            _context = new ApplicationDbContext("AppraisalDbConnectionString");
            _contextExceed = new ApplicationDbContext("ZenithConnectionString");
            // _contextPhoenix = new ApplicationDbContext("PhoenixConn");
        }

        [Route("StaffInfo")]
        public IHttpActionResult GetStaffInfo()
        {
            var rStructure = _contextExceed.vw_employeeinfo.ToList();


            if (rStructure == null)
            {
                return NotFound();
            }

            return Ok(rStructure);
        }
        //public IHttpActionResult GetStaffInfo([FromUri]PagingParameterModel pagingParameterModel)
        //{
        //    var rStructure = _contextExceed.vw_employeeinfo.ToList();


        //    //No of rows count
        //    int count = rStructure.Count();

        //    //Parameter is passed from query string if not available sets to 1
        //    int CurrentPage = pagingParameterModel.pageNumber;

        //    int PageSize = pagingParameterModel.pageSize;

        //    //to display to user
        //    int TotalCount = count;

        //    //calculating totalpage by (No of records/ PageSize)
        //    int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

        //    //returns list of employees after applying paging
        //    var items = rStructure.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

        //    //if currentpage is greater than 1 it has previous page
        //    var previousPage = CurrentPage > 1 ? "Yes" : "No";

        //    var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

        //    var paginationMetaData = new
        //    {
        //        totalCount = TotalCount,
        //        pageSize = PageSize,
        //        currentPage = CurrentPage,
        //        totalPages = TotalPages,
        //        previousPage,
        //        nextPage,
        //    };

        //    HttpContext.Current.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetaData));



        //    if (rStructure == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(items);
        //}

        [Route("StaffInfo/employeeID")]
        public IHttpActionResult GetStaffInfo(string employee_id)
        {

            var rStructure = _contextExceed.vw_employeeinfo.Where(e => e.employee_number == employee_id).ToList();

            if (rStructure == null)
            {
                return NotFound();
            }

            return Ok(rStructure);
        }

        [Route("StaffInfoByName/{*employee_name}")]
        public IHttpActionResult GetStaffInfoByName(string employee_name)
        {

            var rStructure = _contextExceed.vw_employeeinfo.Where(e => e.name.Contains(employee_name)).ToList();

            if (rStructure == null)
            {
                return NotFound();
            }

            return Ok(rStructure);
        }

        [Route("StaffInfo/{*job_title}")]
        public IHttpActionResult GetStaffInfoByTitle(string job_title)
        {

            var rStructure = _contextExceed.vw_employeeinfo.Where(e => e.jobtitle.Contains(job_title)).ToList();

            

            if (rStructure == null)
            {
                return NotFound();
            }


            return Ok(rStructure);
        }

        //public IHttpActionResult GetStaffInfoByTitle(string job_title, [FromUri]PagingParameterModel pagingParameterModel)
        //{

        //    var rStructure = _contextExceed.vw_employeeinfo.Where(e => e.jobtitle.Contains(job_title)).ToList();

        //    //No of rows count
        //    int count = rStructure.Count();

        //    //Parameter is passed from query string if not available sets to 1
        //    int CurrentPage = pagingParameterModel.pageNumber;

        //    int PageSize = pagingParameterModel.pageSize;

        //    //to display to user
        //    int TotalCount = count;

        //    //calculating totalpage by (No of records/ PageSize)
        //    int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

        //    //returns list of employees after applying paging
        //    var items = rStructure.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

        //    //if currentpage is greater than 1 it has previous page
        //    var previousPage = CurrentPage > 1 ? "Yes" : "No";

        //    var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

        //    var paginationMetaData = new
        //    {
        //        totalCount = TotalCount,
        //        pageSize = PageSize,
        //        currentPage = CurrentPage,
        //        totalPages = TotalPages,
        //        previousPage,
        //        nextPage,
        //    };

        //    HttpContext.Current.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetaData));








        //    if (rStructure == null)
        //    {
        //        return NotFound();
        //    }


        //    return Ok(items);
        //}







        [Route("StaffRequests/{employeeID}")]
        public IHttpActionResult GetStaffRequests()
        {

            var rStructure = _context.zib_workflow_master.ToList()
                .Select(Mapper.Map<zib_workflow_master, WorkflowMasterDto>);

            if (rStructure == null)
            {
                return NotFound();
            }

            return Ok(rStructure);
        }


        //api/GetStaff/StaffPhoenixDetails/20170096
        [Route("StaffPhoenixDetails/{employeeID}")]
        public IHttpActionResult GetStaffPhoenixDetails(string employeeID)
        {
            List<EmployeeInformation> employees = new List<EmployeeInformation>();

            string connectionString = ConfigurationManager.ConnectionStrings["sybaseconnection"].ToString();

            string queryString = "select b.user_id as user_id," +
                                               "b.status as status," +
                                               "convert(varchar(4), a.branch_no) as branch_no," +
                                               "a.employee_id as employee_id," +
                                               "c.name_1 as name_1," +
                                               "a.empl_class_code as empl_class_code," +
                                               "convert(varchar,a.last_logon_dt) as last_logon_dt," +
                                               "a.state as state," +
                                               "a.user_name as user_name," +
                                               "a.name as name," +
                                               "b.acct_no as acct_no," +
                                               "b.email_address as email_address " +
                                               "FROM phoenix..ad_gb_rsm a ," +
                                               "zenbase..zib_applications_users b ," +
                                               "phoenix..ad_gb_branch c " +
                                               " WHERE b.user_id = a.user_name " +
                                               " AND A.branch_no = c.branch_no " +
                                               " AND b.staff_id = '" +
                                               employeeID +
                                               "'";

            try
            {
                logwriter.WriteTolog("In try");
                using (AseConnection connection = new AseConnection(connectionString))
                {
                    AseCommand command = new AseCommand(queryString, connection);

                    connection.Open();

                    AseDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var employeeinfo = new EmployeeInformation()
                        {
                            user_id = reader["user_id"].ToString(),
                            status = reader["status"].ToString().Trim(),
                            branch_no = reader["branch_no"].ToString(),
                            employee_id = reader["employee_id"].ToString(),
                            name_1 = reader["name_1"].ToString(),
                            empl_class_code = reader["empl_class_code"].ToString(),
                            last_logon_dt = reader["last_logon_dt"].ToString(),
                            state = reader["state"].ToString(),
                            user_name = reader["user_name"].ToString(),
                            name = reader["name"].ToString(),
                            acct_no = reader["acct_no"].ToString(),
                            email_address = reader["email_address"].ToString()


                        };

                        employees.Add(employeeinfo);

                    }

                    reader.Close();

                }
            }
            catch (Exception ex)
            {


                logwriter.WriteTolog("Error in inner exception: " + ex.InnerException);
                logwriter.WriteTolog("Error in message: " + ex.Message);
            }


            return Ok(employees);



        }

        //api/GetStaff/StaffExceedDetails/20170096
        [Route("StaffPromotionExceedDetails/{employeeID}")]
        public IHttpActionResult GetStaffPromotionExceedDetails(string employeeID)
        {

            var promotionHist = _contextExceed.vw_promotion_hist.Where(e => e.employee_number == employeeID).ToList()
                                .Select(Mapper.Map<PromotionHistory, PromotionHistoryDto>);



            if (promotionHist == null)
            {
                return NotFound();
            }

            var returnJson = new XceedDetails
            {
                PromotionHistory = promotionHist,
                old_grade = _contextExceed.vw_promotion_hist
                            .Where(e => e.employee_number == employeeID)
                            .Select(e => e.old_grade+"/"+e.effective_date).ToList()
                
            };

            



            return Ok(returnJson);
        }

        //api/GetStaff/StaffADDetails/20170096

        [Route("StaffADDetails/{employeeID}")]
        public IHttpActionResult GetStaffADDetails(string employeeID)
        {
            return Ok();
        }


        [Route("GetAccountDetails/{accountNumber}")]
        public IHttpActionResult GetAccountDetails(string accountNumber)
        {
            List<AccountDetailsModel> accountDetails = new List<AccountDetailsModel>();
            string connectionString = ConfigurationManager.ConnectionStrings["sybaseconnection"].ToString();

            string query = "select c.title_1 as acct_name, " +
                            "b.gsm_no as Mobile_No, " +
                            "c.class_code as class_code, " +
                            "a.home_address as home_address " +
                            "from zenbase..zib_ecustomer b ," +
                            "zenbase..zib_kyc_corporate_signatories a, " +
                            "phoenix..dp_acct c " +
                            "where a.acct_no = b.acct_no " +
                            "and a.acct_no = c.acct_no " +
                            "and a.acct_no = '" +
                            accountNumber + "'";


            try
            {
                logwriter.WriteTolog("In try");
                using (AseConnection connection = new AseConnection(connectionString))
                {
                    AseCommand command = new AseCommand(query, connection);

                    connection.Open();

                    AseDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var accountDt = new AccountDetailsModel()
                        {

                            
                            mobile_no = reader["Mobile_No"].ToString().Trim(),
                            home_address = reader["home_address"].ToString().Trim(),
                            class_code = reader["class_code"].ToString().Trim(),
                            acct_name = reader["acct_name"].ToString().Trim()


                        };

                        accountDetails.Add(accountDt);

                    }

                    reader.Close();

                }
            }
            catch (Exception ex)
            {


                logwriter.WriteTolog("Error in inner exception: " + ex.InnerException);
                logwriter.WriteTolog("Error in message: " + ex.Message);
            }



            return Ok(accountDetails);
        }

        public IHttpActionResult GetLeaveFlow(string employeeID, int deptCode, int org_id, int ad_org_id)
        {


            return Ok();
        }

        //[Route("StaffExceedDetails/{employeeID}")]
        //public IHttpActionResult GetStaffExceedDetails(string employeeID)
        //{

        //    var promotionHist = _contextExceed.vw_promotion_hist.Where(e => e.employee_number == employeeID).ToList()
        //                        .Select(Mapper.Map<PromotionHistory, PromotionHistoryDto>);



        //    if (promotionHist == null)
        //    {
        //        return NotFound();
        //    }

        //    var returnJson = new XceedDetails
        //    {
        //        PromotionHistory = promotionHist,
        //        old_grade = _contextExceed.vw_promotion_hist
        //                    .Where(e => e.employee_number == employeeID)
        //                    .Select(e => e.old_grade + "/" + e.effective_date).ToList()

        //    };





        //    return Ok(returnJson);
        //}

        [Route("GetStaffDetails/{branchcode}")]
        public IHttpActionResult GetStaffInfoByBranch(string branchcode)
        {
            var rStructure = _contextExceed.vw_employeeinfo.Where(e => e.branch_code.Equals(branchcode)).ToList();

            if (rStructure == null)
            {
                return NotFound();
            }

            return Ok(rStructure);

            
        }
        


    }
}
