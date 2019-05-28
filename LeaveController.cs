using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using WorkFlowCore.AppCode;
using WorkFlowCore.Models;
using System.Web.Http.Cors;
using System.Configuration;
using Newtonsoft.Json;
using System.Web.Security;

namespace WorkFlowCore.Controllers
{
    //ZENITH.LOCAL

    [EnableCors(origins: "http://sv001sptest09", headers: "*", methods: "*")]
    [System.Web.Http.RoutePrefix("api/GetLeaveDetails")]
    public class LeaveController : ApiController
    {
        private ApplicationDbContext _context;
        private ApplicationDbContext _contextExceed;
        public LeaveController()
        {
            _context = new ApplicationDbContext("XceedTestLeaveDB");
            _contextExceed = new ApplicationDbContext("ZenithConnectionString");
        }

        public bool IsHOP;
        public bool IsBranchHead;
        public bool IsZonalHead;
        public bool IsLeaveUnitHead;
        public bool IsLeaveDeptHead;

        LogWriter logwriter = new LogWriter();

        [System.Web.Http.Route("GetLeaveTypes/")]
        public IHttpActionResult getLeaveType()
        {
            var leaveTypes_ = new List<LeaveTypeModel>();

            XceedEntities xceedcnxn = new XceedEntities();
            leaveTypes_ = (from a in xceedcnxn.Vw_LeaveType
                           where a.org_id == 1
                           orderby a.LeaveType ascending
                           select new LeaveTypeModel
                           {
                               leaveTypeId = a.leave_id,
                               leaveType = a.LeaveType
                           }).ToList();
            return Ok(leaveTypes_);
        }

        [System.Web.Http.Route("GetLeaveDetails/{logonname}/{leaveTypeId}")]
        public IHttpActionResult getLeaveDetails(string logonname, int leaveTypeId)
        {
            logwriter.WriteTolog("In get leave details");
      
            string employee_number = getEmployeeNumber(@"AFRICA\" + logonname);
            var leave_details = new List<LeaveDetailsModel>();
            XceedEntities xceedcnxn = new XceedEntities();
            leave_details =  (from a in xceedcnxn.fn_ReturnLeavedaysentitled(employee_number, leaveTypeId)
                                 select new LeaveDetailsModel
                                 {
                                     daysEntitled = a.daysEntitled,
                                     daysAlreadyTaken = a.daysAlreadyTaken,
                                     prevLeaveBalance = a.prevLeaveBal,
                                     daysCarriedForward = a.daysCarriedForward,
                                    lastleaveDate = a.lastLeaveDate
                                 }).ToList();
            return Ok(leave_details);
        }

        [System.Web.Http.Route("GetLeaveReportingLine/{logonname}/{leaveTypeId}")]
        public IHttpActionResult getLeaveReportingLine(string leaveTypeId, string logonName)
        {
            logwriter.WriteTolog("In getLeaveReportingLine");
            EmployeeInfoModel model = getEmployeeInfo(@"AFRICA\" + logonName);
            string employee_number = model.employee_number;
            string employee_branch_code = model.employee_branch_code;
            using (var context = new XceedEntitiesTest())
            {
                logwriter.WriteTolog("In 2");
                var reporting_line_list = new List<Employee_Reporting_Line> ();
                var dt = new DataTable();
                var conn = context.Database.Connection;
                var connectionState = conn.State;
                try
                {
                    if (connectionState != ConnectionState.Open) conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "zsp_get_reporting_line";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("appid", ""));
                        cmd.Parameters.Add(new SqlParameter("workflowid", ""));
                        cmd.Parameters.Add(new SqlParameter("employee_number", employee_number));
                        cmd.Parameters.Add(new SqlParameter("branch_code", employee_branch_code));
                        cmd.Parameters.Add(new SqlParameter("name", ""));
                        cmd.Parameters.Add(new SqlParameter("employee_id", ""));
                        cmd.Parameters.Add(new SqlParameter("hop_unit_head_number", ""));
                        cmd.Parameters.Add(new SqlParameter("hop_unit_head_name", ""));
                        cmd.Parameters.Add(new SqlParameter("hop_unit_head_logon", ""));
                        cmd.Parameters.Add(new SqlParameter("hop_unit_head_email", ""));
                        cmd.Parameters.Add(new SqlParameter("bhead_dept_head_number", ""));
                        cmd.Parameters.Add(new SqlParameter("bhead_dept_head_name", ""));
                        cmd.Parameters.Add(new SqlParameter("bhead_dept_head_logon", ""));
                        cmd.Parameters.Add(new SqlParameter("bhead_dept_head_email", ""));
                        cmd.Parameters.Add(new SqlParameter("ghead_zonal_head_number", ""));
                        cmd.Parameters.Add(new SqlParameter("ghead_zonal_head_name", ""));
                        cmd.Parameters.Add(new SqlParameter("ghead_zonal_head_logon", ""));
                        cmd.Parameters.Add(new SqlParameter("ghead_zonal_head_email", ""));
                        cmd.Parameters.Add(new SqlParameter("ed_number", ""));
                        cmd.Parameters.Add(new SqlParameter("ed_name", ""));
                        cmd.Parameters.Add(new SqlParameter("ed_logon", ""));
                        cmd.Parameters.Add(new SqlParameter("ed_email", ""));
                        cmd.Parameters.Add(new SqlParameter("md_number", ""));
                        cmd.Parameters.Add(new SqlParameter("md_name", ""));
                        cmd.Parameters.Add(new SqlParameter("md_logon", ""));
                        cmd.Parameters.Add(new SqlParameter("md_email", ""));
                        cmd.Parameters.Add(new SqlParameter("email", ""));
                        cmd.Parameters.Add(new SqlParameter("logon_name", ""));
                        cmd.Parameters.Add(new SqlParameter("zsErrorCode", ""));
                        cmd.Parameters.Add(new SqlParameter("zsErrorMsg", ""));
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader); ;
                        }
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Employee_Reporting_Line reporting_line = new Employee_Reporting_Line();


                        if (employee_branch_code.Equals("001"))
                        {
                            reporting_line.hop_unit_head_number = dt.Rows[i]["hop_unit_head_number"].ToString();
                            reporting_line.hop_unit_head_name = dt.Rows[i]["unithead_name"].ToString();
                            reporting_line.hop_unit_head_logon = dt.Rows[i]["uh_logon_name"].ToString();
                            reporting_line.hop_unit_head_email = dt.Rows[i]["uh_email"].ToString();

                            reporting_line.bhead_dept_head_number = dt.Rows[i]["depthead_number"].ToString();
                            reporting_line.bhead_dept_head_name = dt.Rows[i]["depthead_name"].ToString();
                            reporting_line.bhead_dept_head_logon = dt.Rows[i]["dh_logon_name"].ToString();
                            reporting_line.bhead_dept_head_email = dt.Rows[i]["dh_email"].ToString();

                            reporting_line.ghead_zonal_head_number = dt.Rows[i]["grouphead_number"].ToString();
                            reporting_line.ghead_zonal_head_name = dt.Rows[i]["_group_name"].ToString();
                            reporting_line.ghead_zonal_head_logon = dt.Rows[i]["gh_logon_name"].ToString();
                            reporting_line.ghead_zonal_head_email = dt.Rows[i]["gh_email"].ToString();
                        }
                        else
                        {
                            reporting_line.hop_unit_head_number = dt.Rows[i]["hop_unit_head_number"].ToString();
                            reporting_line.hop_unit_head_name = dt.Rows[i]["hop_name"].ToString();
                            reporting_line.hop_unit_head_logon = dt.Rows[i]["ho_logon_name"].ToString();
                            reporting_line.hop_unit_head_email = dt.Rows[i]["ho_email"].ToString();

                            reporting_line.bhead_dept_head_number = dt.Rows[i]["branch_head_number"].ToString();
                            reporting_line.bhead_dept_head_name = dt.Rows[i]["branchhead_name"].ToString();
                            reporting_line.bhead_dept_head_logon = dt.Rows[i]["bh_logon_name"].ToString();
                            reporting_line.bhead_dept_head_email = dt.Rows[i]["bh_email"].ToString();

                            reporting_line.ghead_zonal_head_number = dt.Rows[i]["zonalhead_number"].ToString();
                            reporting_line.ghead_zonal_head_name = dt.Rows[i]["zonalhead_name"].ToString();
                            reporting_line.ghead_zonal_head_logon = dt.Rows[i]["zh_logon_name"].ToString();
                            reporting_line.ghead_zonal_head_email = dt.Rows[i]["zh_email"].ToString();
                        }

                        reporting_line.ed_number = dt.Rows[i]["ed_number"].ToString();
                        reporting_line.ed_name = dt.Rows[i]["ed_name"].ToString();
                        reporting_line.ed_logon = dt.Rows[i]["ed_logon_name"].ToString();
                        reporting_line.ed_email = dt.Rows[i]["ed_email"].ToString();

                        reporting_line.md_number = dt.Rows[i]["md_no"].ToString();
                        reporting_line.md_name = dt.Rows[i]["md_name"].ToString();
                        reporting_line.md_logon = dt.Rows[i]["md_logon_name"].ToString();
                        reporting_line.md_email = dt.Rows[i]["md_email"].ToString();

                        reporting_line_list.Add(reporting_line);
                    }

                    model.employee_reporting_line = reporting_line_list;
                    model.employee_months_in_bank = GetMonthDifference(model.employee_doe).ToString();
                    return Ok(model);
                }
                catch (Exception ex)
                {
                    logwriter.WriteTolog("Error in Leave inner exception: " + ex.InnerException);
                    logwriter.WriteTolog("Error in Leave message: " + ex.Message);
                    throw;
                }
                finally
                {
                    if (connectionState != ConnectionState.Closed) conn.Close();
                }
            }
        }

        [System.Web.Http.Route("GetLeaveAllowanceAmount/{logonname}")]
        public IHttpActionResult getLeaveAllowanceAmount(string logonname)
        {
            logwriter.WriteTolog("In get leave Allowance");

            string employee_number = getEmployeeNumber(@"AFRICA\" + logonname);
            string employee_grade = getEmployeeInfo(@"AFRICA\" + logonname).employee_grade;

            var leave_allowance = new List<LeaveAllowanceModel>();
            if (!string.IsNullOrEmpty(employee_number))
            {
                XceedEntities xceedcnxn = new XceedEntities();
                leave_allowance = (from a in xceedcnxn.vw_leaveallowance
                                   where a.description == employee_grade
                                   select new LeaveAllowanceModel
                                   {
                                       leaveAllowance = a.allow_amount,
                                       leaveEffPeriod = a.eff_period_from,
                                       staffGrade = a.description,
                                       financialYear = DateTime.Now.Year
                                   }).ToList();
                
                return Ok(leave_allowance);
            }
            else
            {
                return Ok(leave_allowance);
            }

        }

        [System.Web.Http.Route("PostLeaveAllowanceToAccount/{initiator_employee_number}/{amount}/" +
                               "transaction_ref/approver_employee_number")]
        public IHttpActionResult PostLeaveAllowanceToAccount(string initiator_employee_number,
                                                            string amount,
                                                            string transaction_ref,
                                                            string approver_employee_number)
        {
            logwriter.WriteTolog("In get leave Allowance");
            StaffADProfile initiatorADProfile = new StaffADProfile();
            initiatorADProfile.employee_number = initiator_employee_number;
            initiatorADProfile = LINQCalls.getXceedProfile(initiatorADProfile);
            string leave_gl_code = "01-" + initiatorADProfile.branch_code + "-1-53055";
            string connString = ConfigurationManager.ConnectionStrings["PhoenixConn"].ConnectionString;
            FTPostingProfille postingProfile = new FTPostingProfille();
            List<PostingEntries> plist = new List<PostingEntries>();


            postingProfile = new FTPostingProfille(initiatorADProfile.employee_number,
                                                  initiatorADProfile.branch_code,
                                                  amount, transaction_ref, approver_employee_number);

            //force debit outstanding amount
            plist = new List<PostingEntries>();
            plist.Add(new PostingEntries(leave_gl_code, amount, "DR", "LEAVE ALLOWANCE " + postingProfile.getLeaveFinancialYear() + " /" + initiatorADProfile.name + "/" + initiatorADProfile.account_no));
            plist.Add(new PostingEntries(initiatorADProfile.account_no, amount, "CR", "LEAVE ALLOWANCE " + postingProfile.getLeaveFinancialYear() + " /" + initiatorADProfile.name + "/" + initiatorADProfile.account_no));
            postingProfile.PostingEntries = plist;
            Tuple<int, string> postResult = postingProfile.insertTransaction();

            if (!postResult.Item1.Equals(0))
            {
                return Ok(postResult.Item1 + " : " + postResult.Item2);
            }
            else
            {
                return Ok("00: Post Success");
            }
        }


        [System.Web.Http.Route("SaveLeavetoExceed")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult SaveLeavetoExceed(SaveLeave saveleave)
        {
            string sqlQuery;
            SqlParameter[] sqlParams;

            List<SqlParameter> parameter = new List<SqlParameter>();
            List<SaveLeaveReturns> returns = new List<SaveLeaveReturns>();

            SaveLeaveReturns result;

            string connectionString = ConfigurationManager.ConnectionStrings["XceedTestLeaveDB"].ToString();


            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors.Select(z => z.Exception));

                return BadRequest();
            }




            try
            {
                

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("spSaveLeaveZenith", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@employee_number", saveleave.employee_number));
                    cmd.Parameters.Add(new SqlParameter("@daysAppliedFor", saveleave.daysAppliedFor));
                    cmd.Parameters.Add(new SqlParameter("@startDate", saveleave.startDate));
                    cmd.Parameters.Add(new SqlParameter("@EndDate", saveleave.EndDate));
                    cmd.Parameters.Add(new SqlParameter("@resumption_date", saveleave.resumption_date));
                    cmd.Parameters.Add(new SqlParameter("@reasonForLeave", saveleave.reasonForLeave));
                    cmd.Parameters.Add(new SqlParameter("@address", saveleave.address));
                    cmd.Parameters.Add(new SqlParameter("@phoneNo", saveleave.phoneNo));
                    cmd.Parameters.Add(new SqlParameter("@leave_id", saveleave.leave_id));
                    cmd.Parameters.Add(new SqlParameter("@addMoredays", saveleave.addMoredays));
                    cmd.Parameters.Add(new SqlParameter("@AddDaysPrevYr", saveleave.AddDaysPrev));
                    cmd.Parameters.Add(new SqlParameter("@leaveUnique_id", saveleave.leaveUnique_id));
                    cmd.Parameters.Add(new SqlParameter("@org_id", saveleave.org_id));
                    cmd.Parameters.Add(new SqlParameter("@backofficer_number", saveleave.backofficer_number));


                    var outputParam = new SqlParameter("@errorcode", SqlDbType.Int, 10);
                    outputParam.Direction = ParameterDirection.Output;
                    outputParam.Value = 0;
                    cmd.Parameters.Add(outputParam);

                    var outputParam2 = new SqlParameter("@message", SqlDbType.VarChar, 200);
                    outputParam2.Direction = ParameterDirection.Output;
                    outputParam2.Value = "";
                    cmd.Parameters.Add(outputParam2);

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        
                            var count1 = cmd.Parameters["@errorcode"].Value.ToString();

                         result = new SaveLeaveReturns
                        {
                            errorcode = int.Parse(cmd.Parameters["@errorcode"].Value.ToString()),
                            message = cmd.Parameters["@message"].Value.ToString()

                        };
                        
                    }

                }


            }
            catch (Exception ex)
            {

                throw;
            }


            return Created(new Uri(Request.RequestUri.ToString()), result);
        }


        [System.Web.Http.Route("EFSaveLeavetoExceed")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult EFSaveLeavetoExceed(SaveLeave saveleave)
        {
            string sqlQuery;
            SqlParameter[] sqlParams;

            List<SqlParameter> parameter = new List<SqlParameter>();
           

            SaveLeaveReturns result;

            


            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors.Select(z => z.Exception));

                return BadRequest();
            }




            try
            {
                //uncomment
                sqlQuery = "spSaveLeaveZenith @employee_number, @daysAppliedFor, @startDate, @EndDate, @resumption_date, @reasonForLeave, @address, " +
                           "@phoneNo, @leave_id, @addMoredays, @AddDaysPrevYr, @leaveUnique_id, @org_id, @backofficer_number, @errorcode, @message";


                parameter.Add(new SqlParameter("@employee_number", saveleave.employee_number));
                parameter.Add(new SqlParameter("@daysAppliedFor", saveleave.daysAppliedFor));
                parameter.Add(new SqlParameter("@startDate", saveleave.startDate));
                parameter.Add(new SqlParameter("@EndDate", saveleave.EndDate));
                parameter.Add(new SqlParameter("@resumption_date", saveleave.resumption_date));
                parameter.Add(new SqlParameter("@reasonForLeave", saveleave.reasonForLeave));
                parameter.Add(new SqlParameter("@address", saveleave.address));
                parameter.Add(new SqlParameter("@phoneNo", saveleave.phoneNo));
                parameter.Add(new SqlParameter("@leave_id", saveleave.leave_id));
                parameter.Add(new SqlParameter("@addMoredays", saveleave.addMoredays));
                parameter.Add(new SqlParameter("@AddDaysPrevYr", saveleave.AddDaysPrev));
                parameter.Add(new SqlParameter("@leaveUnique_id", saveleave.leaveUnique_id));
                parameter.Add(new SqlParameter("@org_id", saveleave.org_id));
                parameter.Add(new SqlParameter("@backofficer_number", saveleave.backofficer_number));

                var outputParam = new SqlParameter("@errorcode", SqlDbType.Int, 10);
                outputParam.Direction = ParameterDirection.Output;
                // outputParam.Value = 0;
                parameter.Add(outputParam);

                var outputParam2 = new SqlParameter("@message", SqlDbType.VarChar, 200);
                outputParam2.Direction = ParameterDirection.Output;
                //outputParam2.Value = "";
                parameter.Add(outputParam2);

                //end of uncomment 

                //     sqlParams = new SqlParameter[]
                //     {
                //         new SqlParameter{ParameterName = "@employee_number", Value = saveleave.employee_number, Direction = System.Data.ParameterDirection.Input},
                //         new SqlParameter{ParameterName = "@daysAppliedFor", Value = saveleave.daysAppliedFor, Direction = System.Data.ParameterDirection.Input},
                //         new SqlParameter{ParameterName = "@startDate", Value = saveleave.startDate, Direction = System.Data.ParameterDirection.Input},
                //         new SqlParameter{ParameterName = "@EndDate", Value = saveleave.EndDate, Direction = System.Data.ParameterDirection.Input},
                //         new SqlParameter{ParameterName = "@resumption_date", Value = saveleave.resumption_date, Direction = System.Data.ParameterDirection.Input},
                //         new SqlParameter{ParameterName = "@reasonForLeave", Value = saveleave.reasonForLeave, Direction = System.Data.ParameterDirection.Input},
                //         new SqlParameter{ParameterName = "@address", Value = saveleave.address, Direction = System.Data.ParameterDirection.Input},
                //         new SqlParameter{ParameterName = "@phoneNo", Value = saveleave.phoneNo, Direction = System.Data.ParameterDirection.Input},
                //         new SqlParameter{ParameterName = "@leave_id", Value = saveleave.leave_id, Direction = System.Data.ParameterDirection.Input},
                //         new SqlParameter{ParameterName = "@addMoredays", Value = saveleave.addMoredays, Direction = System.Data.ParameterDirection.Input},
                //         new SqlParameter{ParameterName = "@AddDaysPrevYr", Value = saveleave.AddDaysPrev, Direction = System.Data.ParameterDirection.Input},
                //         new SqlParameter{ParameterName = "@leaveUnique_id", Value = saveleave.leaveUnique_id, Direction = System.Data.ParameterDirection.Input},
                //         new SqlParameter{ParameterName = "@org_id", Value = saveleave.org_id, Direction = System.Data.ParameterDirection.Input},
                //         new SqlParameter{ParameterName = "@backofficer_number", Value = saveleave.backofficer_number, Direction = System.Data.ParameterDirection.Input},
                //         //new SqlParameter{ParameterName = "@errorcode", Value = saveleave.errorcode, Direction = System.Data.ParameterDirection.InputOutput},
                //         //new SqlParameter{ParameterName = "@message", Value = saveleave.message, Direction = System.Data.ParameterDirection.InputOutput},

                //};





                var rStructure = _context.Database.SqlQuery<SaveLeaveReturns>(sqlQuery, parameter.ToArray()).Single();
                
                //var rStructure = _context.Database.ExecuteSqlCommand(sqlQuery, parameter.ToArray());

                var errcode = (outputParam.SqlValue).ToString();
                var errMsg = (outputParam2.SqlValue).ToString();

                //Console.WriteLine(rStructure);




            }
            catch (Exception ex)
            {

                throw;
            }


            return Ok();
        }

        [System.Web.Http.Route("StaffInfo")]
        public IHttpActionResult GetStaffInfo([FromUri]PagingParameterModel pagingParameterModel)
        {
            //if (pagingParameterModel.pageNumber == null)
            //{
            //    pagingParameterModel.pageNumber = 1;
            //}


            var rStructure = _contextExceed.vw_employeeinfo.ToList();

           // var rStructure = _contextExceed.vw_employeeinfo.Where(e => e.jobtitle.Contains(job_title)).ToList();

            //No of rows count
            int count = rStructure.Count();

            //Parameter is passed from query string if not available sets to 1
            int CurrentPage = pagingParameterModel.pageNumber;

            int PageSize = pagingParameterModel.pageSize;

            //to display to user
            int TotalCount = count;

            //calculating totalpage by (No of records/ PageSize)
            int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

            //returns list of employees after applying paging
            var items = rStructure.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

            //if currentpage is greater than 1 it has previous page
            var previousPage = CurrentPage > 1 ? "Yes" : "No";

            var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

            var paginationMetaData = new
            {
                totalCount = TotalCount,
                pageSize = PageSize,
                currentPage = CurrentPage,
                totalPages = TotalPages,
                previousPage,
                nextPage,
            };

            HttpContext.Current.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetaData));








            if (rStructure == null)
            {
                return NotFound();
            }


            return Ok(items);
        }

        //[System.Web.Http.Route("Login/{logonname}/{password}")]
        //public IHttpActionResult Login(string logonName, string password)
        //{
        //    LoginModel model = new LoginModel();
        //    try
        //    {
        //        if (Membership.ValidateUser(logonName, password))
        //        {
        //            FormsAuthentication.SetAuthCookie(logonName, false);
        //            try
        //            {
        //                StaffADProfile staffADProfile = getStaffADProfile(logonName);
        //            }
        //            catch (Exception ex)
        //            {
        //                this.ModelState.AddModelError(string.Empty, "Your staff profile has not been properly setup. Please contact InfoTech");
        //            }
        //            model.responseCode = "00";
        //            model.responseDescription = "successful";
        //            return Ok(model);

        //        }
        //        model.responseCode = "20";
        //        model.responseDescription = "The user name or password provided is incorrect.";
        //    }
        //    catch (Exception ex)
        //    {
        //        model.responseCode = "40";
        //        model.responseDescription = ex.Message;
        //    }
        //    return Ok(model);
        //}



        public string getEmployeeNumber(string logonname)
        {
            XceedEntities xceedcnxn = new XceedEntities();
            var employee_number = (from u in xceedcnxn.vw_employeeinfo
                        where u.logon_name == logonname
                        select u.employee_number).FirstOrDefault();

            logwriter.WriteTolog("getEmployeeNumber");
            return employee_number.ToString();
        }

        public EmployeeInfoModel getEmployeeInfo(string logonname)
        {
            XceedEntities xceedcnxn = new XceedEntities();
            var employee_info = (from u in xceedcnxn.vw_employeeinfo
                                   where u.logon_name == logonname
                                   select new EmployeeInfoModel
                                   {
                                       employee_account_number = u.account_no,
                                       employee_number = u.employee_number,
                                       employee_branch_code = u.Branch_code,
                                       employee_branch = u.Branch,
                                       employee_doe = (DateTime) u.employment_date,
                                       employee_email = u.email,
                                       employee_grade = u.paygroup,
                                       employee_id = u.employee_id,
                                       employee_name = u.name
                                   }).ToList();

            logwriter.WriteTolog("getEmployeeInfo");
            return employee_info.FirstOrDefault();
        }

        public static int GetMonthDifference(DateTime startDate)
        {
            DateTime endDate = DateTime.Now;
            int monthsApart = 12 * (startDate.Year - endDate.Year) + startDate.Month - endDate.Month;
            return Math.Abs(monthsApart);
        }

        public static StaffADProfile getStaffADProfile(string user_name = "")
        {

            user_name = string.IsNullOrEmpty(user_name) ? System.Web.HttpContext.Current.User.Identity.Name : user_name.Trim();

            //Get the staff profile
            StaffADProfile staffADProfile = new StaffADProfile();
            staffADProfile.user_logon_name = user_name;
            //AD
            ActiveDirectoryQuery activeDirectoryQuery = new ActiveDirectoryQuery(staffADProfile);
            staffADProfile = activeDirectoryQuery.GetStaffProfile();
            if (staffADProfile == null)
            {
                return null;
            }

            return LINQCalls.getXceedProfile(staffADProfile);
        }
    }
}