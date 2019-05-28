using System;
using WorkFlowCore.AppCode;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Http;
using WorkFlowCore.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web;
using System.IO;
using System.Net.Http;
using System.Net;

namespace WorkFlowCore.Controllers
{
    [System.Web.Http.RoutePrefix("api/CACSearch")]
    public class CACSearchController : ApiController
    {
        private ApplicationDbContext _context;
        private static string CSU                   = "CUSTOMER SERVICE OFFICER";
        private static string REQUESTSTAGE          = "Initiate Request";
        private static string DEFAULT_SUBMIT_STATUS = "Submitted";
        private static string ENTRYPOINT            = "webAPI";
        private CustomerSearchEntryModel CustomerSearchEntryModel;

        public CACSearchController()
        {
            _context = new ApplicationDbContext("AppraisalDbConnectionString");
        }

        /// <summary>
        /// This endpoint queries customer search entry database to fetch search request records using workflowid
        /// </summary>
        /// <param name="workflowid">The workflowid of the request in the search table</param>
        /// <returns>
        /// responseCode    :   0 for success, non zero for exceptions
        /// responseMessage :   Returns null/empty string for success and has a value for exceptions
        /// record          :   Returns the records of the search request having the same workflowid in the database table
        /// </returns>

        [Route("GetSearchRequestEntry")]
        public IHttpActionResult GetSearchRequestEntry(string workflowid){

            int retCode = 0;
            string retVal = string.Empty;        
            string connectionString = ConfigurationManager.ConnectionStrings["AppraisalDbConnectionString"].ConnectionString;
            string returnMessage = "{{\"responseCode\":\"{0}\",\"responseMessage\":\"{1}\",\"result\":\"{2}\"}}";

            //Validate The Input Parameters
            if (workflowid.ToString().Equals(null))
            {
                return NotFound();
                //returnMessage = await Task.FromResult(string.Format(returnMessage, "01", "Invalid workflowid", null));
                // return JsonConvert.DeserializeObject(returnMessage.ToString());
            }


            var Result = LINQCalls.getSearchRequestEntry(workflowid);

            if (Result == null){
                return NotFound();
                
            }
            return Ok(Result);

            //returnMessage = await Task.FromResult(string.Format(returnMessage, retCode.ToString(), retVal, Result));
            //return JsonConvert.DeserializeObject(returnMessage.ToString());
        }
       

        /// <summary>
        /// This endpoint processes the user's entry from the WEbAPI for insertion into the Search database.
        /// Only CSU staff are allowed to call this API from EPMA for 'active corporate accounts'
        /// </summary>
        /// <param name="employee_number">The staff number of the initiator</param>
        /// <param name="acct_no">The account for which we are conducting a search enquiry</param>
        /// <param name="registration_no">The custoner's registration number as issued by CAC</param>
        /// <param name = "CAC_certificate">CAC document</param>
        /// <returns>
        /// JSOnObject defined as follows:
        /// responseCode    : 0 for success, non-zero for exceptions
        /// responseMessage : Returns null/empty string for success and has a value for exceptions
        /// workflowid      : Returns the DB generated workflowid for a success transaction
        /// </returns>
        //[System.Web.Mvc.Route("POSTInitiateSearchRequest/{staffId}/{acct_no}/{registration_no}/{imagefile}")]
        [Route("InitiateSearchRequest")]
        [HttpPost]
        public async Task<object> POSTInitiateSearchRequest([FromBody]SearchRequest request)
        {         

            int retCode             = 0;
            string retVal           = string.Empty;
            string workflowid       = string.Empty;
            byte[] rc_certificate_document_bytes = new byte[] { };
            string attachment1_filename         = "";
            string attachment1_contenttype      = "";
            string connectionString = ConfigurationManager.ConnectionStrings["AppraisalDbConnectionString"].ConnectionString;
            string returnMessage    = "{{\"responseCode\":\"{0}\",\"responseMessage\":\"{1}\",\"workflowid\":\"{2}\"}}";
                        
            //Validate The Input Parameters
            if (request.employee_id.ToString().Equals(null) || request.employee_id <= 1) {
                returnMessage = await Task.FromResult(string.Format(returnMessage, "01", "Invalid employee_number",null));
                return JsonConvert.DeserializeObject(returnMessage.ToString());
            }

            if (string.IsNullOrEmpty(request.acct_no)) {
                returnMessage = await Task.FromResult(string.Format(returnMessage, "02", "Invalid acct_no", null));
                return JsonConvert.DeserializeObject(returnMessage.ToString());
            }

            if (string.IsNullOrEmpty(request.registration_no)) {
                returnMessage = await Task.FromResult(string.Format(returnMessage, "03", "Invalid registration_no", null));
                return JsonConvert.DeserializeObject(returnMessage.ToString());
            }

            //Validate the employee_number ojbect
            //Employee must be a CSU staff
            //Employee must be active on the system

            //PHOENIX STATUS CHECK
            var phoenixValidator = await PhoenixValidator.getStaffPhoenixDetails(employee_id: request.employee_id);
            if (phoenixValidator.Item1 != 0 || phoenixValidator.Item2.Trim().ToLower() != "active")
            {
                returnMessage = await Task.FromResult(string.Format(returnMessage, "04", "Staff profile on Phoenix is not active/does not exist - " + phoenixValidator.Item2, null));
                return JsonConvert.DeserializeObject(returnMessage.ToString());
            }

            var staffProfile  = DataHandler.getStaffADProfile(phoenixValidator.Item3, 0);

            //PROFILE CHECK
            if (staffProfile == null)
            {
                returnMessage = await Task.FromResult(string.Format(returnMessage, "05", "Error validating staff profile from Xceed and AD", null));
                //return Content(returnMessage, "application/json");
                return JsonConvert.DeserializeObject(returnMessage.ToString());
            }

            //CSU CHECK
            if ( string.IsNullOrEmpty(staffProfile.jobtitle) || !staffProfile.jobtitle.Trim().ToUpper().Equals(CSU))
            {
                returnMessage = await Task.FromResult(string.Format(returnMessage, "06", "Staff is not a CSU personnel", null));
                return JsonConvert.DeserializeObject(returnMessage.ToString());
            }

            //Validate the account
            //Account must be a current account
            var acctValidator        = await PhoenixValidator.getAccountType(account_number: request.acct_no);
            if (acctValidator.Item1 != 0 || string.IsNullOrEmpty(acctValidator.Item2) || acctValidator.Item3.Trim().ToUpper() != "CA")
            {
                returnMessage = await Task.FromResult(string.Format(returnMessage, "07", "Account must be a valid corporate/current account - " + acctValidator.Item3, null));
                return JsonConvert.DeserializeObject(returnMessage.ToString());
            }


            //Validate the document
            //rc_certificate_document must not be empty or null
            //if (string.IsNullOrEmpty(request.rc_certificate_document))
            //{
            //    returnMessage = await Task.FromResult(string.Format(returnMessage, "08", "Invalid document attached", null));
            //    return JsonConvert.DeserializeObject(returnMessage.ToString());
            //}

            if (!string.IsNullOrEmpty(request.rc_certificate_document)){
                 rc_certificate_document_bytes = Convert.FromBase64String(request.rc_certificate_document);
                 attachment1_filename = acctValidator.Item2 + "_" + request.acct_no + "_" + request.registration_no;
                 attachment1_contenttype = "application/pdf";
            }
                
            //if ( rc_certificate_document_bytes == null && rc_certificate_document_bytes.Length <= 0 )
            //{
            //    returnMessage = await Task.FromResult(string.Format(returnMessage, "09", "Invalid document attached  - Unable to convert to file bytes", null));
            //    return JsonConvert.DeserializeObject(returnMessage.ToString());
            //}

         
            //We have come this far 
            //Looks like data is fine so let's perform the POST

            try {

                using (SqlConnection conn   = new SqlConnection(connectionString)) {
                    using (SqlCommand cmd = new SqlCommand("zsp_customersearch_process", conn)) {

                        cmd.CommandType = CommandType.StoredProcedure;

                        //cmd.Parameters.AddWithValue("@entry_key"        , "");
                        cmd.Parameters.AddWithValue("@workflowid"       , null);
                        cmd.Parameters.AddWithValue("@requeststageid"   , 0);
                        cmd.Parameters.AddWithValue("@requeststage"     , REQUESTSTAGE);
                        cmd.Parameters.AddWithValue("@status"           , DEFAULT_SUBMIT_STATUS);

                        cmd.Parameters.AddWithValue("@employee_number"  , staffProfile.employee_number);
                        cmd.Parameters.AddWithValue("@name"             , staffProfile.name);
                        cmd.Parameters.AddWithValue("@email"            , staffProfile.email);
                        cmd.Parameters.AddWithValue("@grade"            , staffProfile.grade);
                        cmd.Parameters.AddWithValue("@org_id"           , staffProfile.org_id);
                        cmd.Parameters.AddWithValue("@ranking"          , staffProfile.ranking);
                        cmd.Parameters.AddWithValue("@phone"            , staffProfile.mobile_phone);

                        cmd.Parameters.AddWithValue("@unitcode"         , staffProfile.branch_code);
                        cmd.Parameters.AddWithValue("@unitname"         , staffProfile.branch_name);
                        cmd.Parameters.AddWithValue("@deptname"         , staffProfile.SelectedDept);
                        cmd.Parameters.AddWithValue("@deptcode"         , staffProfile.department_id);
                        cmd.Parameters.AddWithValue("@groupcode"        , staffProfile.groupcode);
                        cmd.Parameters.AddWithValue("@groupname"        , staffProfile.groupname);
                        cmd.Parameters.AddWithValue("@supergroupcode"   , 1);
                        cmd.Parameters.AddWithValue("@supergroupname"   , "ZENITH BANK PLC");
                        cmd.Parameters.AddWithValue("@ad_org_id"        , 1);

                        cmd.Parameters.AddWithValue("@account_no"       , request.acct_no);
                        cmd.Parameters.AddWithValue("@account_name"     , acctValidator.Item2);
                            
                        cmd.Parameters.AddWithValue("@registration_no"  , request.registration_no);
                        cmd.Parameters.AddWithValue("@requeststatus"    , string.Empty);

                        cmd.Parameters.AddWithValue("@comments"         , string.Empty);
                        cmd.Parameters.AddWithValue("@entrypoint"       , ENTRYPOINT);

                        cmd.Parameters.AddWithValue("@attachment1_filename"     , attachment1_filename);
                        cmd.Parameters.AddWithValue("@attachment1_contenttype"  , attachment1_contenttype );  
                        cmd.Parameters.AddWithValue("@attachment1_bytes"        , rc_certificate_document_bytes);

                        cmd.Parameters.Add("@rErrorCode", SqlDbType.Int, 2).Direction       = ParameterDirection.Output;
                        cmd.Parameters.Add("@rErrorMsg", SqlDbType.VarChar, 255).Direction  = ParameterDirection.Output;

                        conn.Open();

                        var dr = cmd.ExecuteReader();

                        while (dr.Read())
                        {
                            workflowid = (dr["workflowid"].ToString());
                        }

                        dr.Close();

                        retCode = int.Parse(cmd.Parameters["@rErrorCode"].Value.ToString());

                        if (retCode != 0) {
                            retVal = cmd.Parameters["@rErrorMsg"].Value.ToString();
                        }

                        cmd.Dispose();
                    }
                    conn.Close();
                }
            } catch (SqlException ex) {
                retCode = ex.Number;
                retVal = ex.Message;
            } catch (Exception ex) {
                retCode = 08;
                retVal = ex.Message;
            }

            returnMessage   = await Task.FromResult(string.Format(returnMessage, retCode.ToString(), retVal, workflowid));
            return JsonConvert.DeserializeObject(returnMessage.ToString());
        }
    }
}
