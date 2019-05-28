using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using WorkFlowCore.Models;

namespace WorkFlowCore.Controllers
{
    [System.Web.Http.RoutePrefix("api/GetDepartment")]
    public class DepartmentController : ApiController
    {

        [System.Web.Http.Route("ByBranchCode/{branch_code}")]
        public IHttpActionResult GetDepartmentsByBranchCode(string branch_code)
        {
            var depts_ = new List<DepartmentModel>();
            if (branch_code.Equals("001"))
            {
                AppraisalDbEntities appcnxn = new AppraisalDbEntities();
                depts_ = (from a in appcnxn.zib_appraisal_dept_structure
                          where a.groupcode.Equals("001")
                          orderby a.deptname ascending
                          select new DepartmentModel
                          {
                              dept_code = a.deptcode,
                              dept_name = a.deptname,
                              unit_code = a.unitcode,
                              unit_name = a.unitname
                          }).ToList();
                return Ok(depts_);
            }
            depts_.Add(new DepartmentModel { Id = 0, dept_code = "OPNS", dept_name = "OPERATIONS", unit_code = "OPNS", unit_name = "OPERATIONS" });
            depts_.Add(new DepartmentModel { Id = 1, dept_code = "MKTG", dept_name = "MARKETING", unit_code = "OPNS", unit_name = "OPERATIONS" });
            return Ok(depts_);
        }


    }
}