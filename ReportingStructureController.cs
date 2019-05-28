using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WorkFlowCore.Models;

namespace WorkFlowCore.Controllers
{
    [RoutePrefix("api/ReportingStructure")]
    public class ReportingStructureController : ApiController
    {
        private ApplicationDbContext _context;

        public ReportingStructureController()
        {
            _context = new ApplicationDbContext("AppraisalDbConnectionString");
        }

        [Route("")]
        //api/ReportingStructure
        public IHttpActionResult GetReportingStructure()
        {
            var rStructure = _context.zib_leave_approvers.ToList();

            return Ok(rStructure);
        }

        //api/ReportingStructure/
        [Route("ByAdOrgID/{ad_org_id:int}")]
        public IHttpActionResult GetReportingStructureByAD_Org_ID(int ad_org_id)
        {

            var rStructure = _context.zib_leave_approvers.Where(c => c.ad_org_id == ad_org_id).ToList();
           
            if (rStructure == null)
            {
                return NotFound();
            }

            return Ok(rStructure);
        }

     

        //api/ReportingStructure/1
        [Route("ByOrgID/{org_id}")]
        public  IHttpActionResult GetReportingStructureByOrgID(int org_id)
        {
            var rStructure = _context.zib_leave_approvers.Where(c => c.org_id == org_id).ToList();

            if (rStructure == null)
            {
                return NotFound();
            }

            return Ok(rStructure);
        }

        //api/ReportingStructure/1/1/054
        [Route("ByZoneID/{ad_org_id}/{org_id}/{zone_id}")]
        
        public IHttpActionResult GetReportingStructureByZone(int ad_org_id, int org_id, string zone_id)
        {
            var rStructure = _context.zib_leave_approvers.Where(c => c.ad_org_id == ad_org_id && c.org_id == org_id && c.groupcode == zone_id).ToList();

            if (rStructure == null)
            {
                return NotFound();
                
            }


            return Ok(rStructure);
        }



        //api/ReportingStructure/1/1/054
        [Route("ByBranchCode/{ad_org_id}/{org_id}/{branch_code}")]
        
        public IHttpActionResult GetReportingStructureByBranch(int ad_org_id, int org_id, string branch_code)
        {
            var rStructure = _context.zib_leave_approvers.Where(c => c.ad_org_id == ad_org_id && c.org_id == org_id && c.deptcode == branch_code).ToList();

            if (rStructure == null)
            {
                return NotFound();
            }


            return Ok(rStructure);
        }
        

        [Route("ByStaffID/{staff_id}/{branch_code}")]
        public IHttpActionResult GetReportingStructureByStaffID(string staff_id, string branch_code)
        {
            //var empty = new object();
            var name = new System.Data.Entity.Core.Objects.ObjectParameter("name", typeof(string));
            var employee_id = new System.Data.Entity.Core.Objects.ObjectParameter("employee_id", typeof(string));
            var hop_unit_head_number = new System.Data.Entity.Core.Objects.ObjectParameter("hop_unit_head_number", typeof(string));
            var hop_unit_head_name = new System.Data.Entity.Core.Objects.ObjectParameter("hop_unit_head_name", typeof(string));
            var hop_unit_head_logon = new System.Data.Entity.Core.Objects.ObjectParameter("hop_unit_head_logon", typeof(string));
            var hop_unit_head_email = new System.Data.Entity.Core.Objects.ObjectParameter("hop_unit_head_email", typeof(string));
            var bhead_dept_head_number = new System.Data.Entity.Core.Objects.ObjectParameter("bhead_dept_head_number", typeof(string));
            var bhead_dept_head_name = new System.Data.Entity.Core.Objects.ObjectParameter("bhead_dept_head_name", typeof(string));
            var bhead_dept_head_logon = new System.Data.Entity.Core.Objects.ObjectParameter("bhead_dept_head_logon", typeof(string));
            var bhead_dept_head_email = new System.Data.Entity.Core.Objects.ObjectParameter("bhead_dept_head_email", typeof(string));
            var ghead_zonal_head_number = new System.Data.Entity.Core.Objects.ObjectParameter("ghead_zonal_head_number", typeof(string));
            var ghead_zonal_head_name = new System.Data.Entity.Core.Objects.ObjectParameter("ghead_zonal_head_name", typeof(string));
            var ghead_zonal_head_logon = new System.Data.Entity.Core.Objects.ObjectParameter("ghead_zonal_head_logon", typeof(string));
            var ghead_zonal_head_email = new System.Data.Entity.Core.Objects.ObjectParameter("ghead_zonal_head_email", typeof(string));
            var ed_number = new System.Data.Entity.Core.Objects.ObjectParameter("ed_number", typeof(string));
            var ed_name = new System.Data.Entity.Core.Objects.ObjectParameter("ed_name", typeof(string));
            var ed_logon = new System.Data.Entity.Core.Objects.ObjectParameter("ed_logon", typeof(string));
            var ed_email = new System.Data.Entity.Core.Objects.ObjectParameter("ed_email", typeof(string));
            var md_number = new System.Data.Entity.Core.Objects.ObjectParameter("md_number", typeof(string));
            var md_name = new System.Data.Entity.Core.Objects.ObjectParameter("md_name", typeof(string));
            var md_logon = new System.Data.Entity.Core.Objects.ObjectParameter("md_logon", typeof(string));
            var md_email = new System.Data.Entity.Core.Objects.ObjectParameter("md_email", typeof(string));
            var email = new System.Data.Entity.Core.Objects.ObjectParameter("email", typeof(string));
            var logon_name = new System.Data.Entity.Core.Objects.ObjectParameter("logon_name", typeof(string));
            var ZsErrorCode = new System.Data.Entity.Core.Objects.ObjectParameter("ZsErrorCode", typeof(string));
            var ZsErrorMsg = new System.Data.Entity.Core.Objects.ObjectParameter("ZsErrorMsg", typeof(string));

            XceedEntities ent = new XceedEntities();
            List<string> rStructure = new List<string>();
            //rStructure = ent.zsp_get_reporting_line("", "",  staff_id, branch_code, name, employee_id, hop_unit_head_number, hop_unit_head_name,
            //                                                  hop_unit_head_logon, hop_unit_head_email, bhead_dept_head_number,
            //                                                  bhead_dept_head_name, bhead_dept_head_logon, bhead_dept_head_email,
            //                                                  ghead_zonal_head_number, ghead_zonal_head_name, ghead_zonal_head_logon,
            //                                                  ghead_zonal_head_email, ed_number, ed_name, ed_logon, ed_email,
            //                                                  md_number, md_name, md_logon, md_email, email, logon_name,
            //                                                  ZsErrorCode, ZsErrorMsg).ToList();

            if (rStructure == null)
            {
                return NotFound();
            }
            return Ok(rStructure);
        }
    }
}
