using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WorkFlowCore;
using WorkFlowCore.Models;

namespace WorkFlowCore.AppCode
{
    public class DataHandler
    {
        public const string APP_ID = "LEAVE";
        public static StaffADProfile getStaffADProfile(string staff_id, int chk = 0)
        {
            //Get the staff profile
            StaffADProfile staffADProfile = new StaffADProfile();
            staffADProfile.employee_number = staff_id;
            //AD
            //ActiveDirectoryQuery activeDirectoryQuery = new ActiveDirectoryQuery(staffADProfile, staff_number);
            // = activeDirectoryQuery.GetStaffProfileByNumber();
            //if (staffADProfile == null)
            //{
            //    return null;
            //}

            return LINQCalls.getXceedProfile(staffADProfile);
        }

        internal static Tuple<int, string, int> getXceedServer(StaffADProfile staffADProfile)
        {
            return LINQCalls.getXceedConnector(staffADProfile);
        }

        public static int getFinancialYear()
        {
            System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/");
            System.Configuration.KeyValueConfigurationElement FinancialYear = rootWebConfig.AppSettings.Settings["FinancialYear"];
            return int.Parse(FinancialYear.Value.ToString());
        }

        internal static zib_workflow_xceed_definitions getXceedDefinition(StaffADProfile staffADProfile)
        {
            AppraisalDbEntities conn = new AppraisalDbEntities();
            var entry = (from d in conn.zib_workflow_xceed_definitions
                         where (d.ad_org_id.Equals(staffADProfile.org_id))
                         select d
                        ).First();
            return entry;
        }
    }
}