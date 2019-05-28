using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Web;
using WorkFlowCore.Models;


namespace WorkFlowCore.AppCode
{
    public class ActiveDirectoryQuery : IDisposable
    {
        public DirectorySearcher dirSearch = null;

        private StaffADProfile staffADProfile;
        private LogWriter logWriter;
        private string StaffNumber;

        public ActiveDirectoryQuery(StaffADProfile staffADProfile)
        {
            this.staffADProfile = staffADProfile;
            this.logWriter = new LogWriter();
        }

        public ActiveDirectoryQuery(string _staffNumber)
        {
            this.StaffNumber = _staffNumber;
            this.logWriter = new LogWriter();
        }
        public ActiveDirectoryQuery(StaffADProfile staffADProfile, string empno)
        {
            this.staffADProfile = staffADProfile;
            this.StaffNumber = staffADProfile.employee_number;
            this.logWriter = new LogWriter();
        }

        private string GetSystemDomain()
        {
            try
            {
                return Domain.GetComputerDomain().ToString().ToLower();
            }
            catch (Exception e)
            {
                e.Message.ToString();
                return string.Empty;
            }
        }

        internal string GetUserName()
        {

            logWriter.WriteTolog(string.Format("Entered GetUserName"));
            string username = null;

            try
            {
                System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/");
                System.Configuration.KeyValueConfigurationElement sADUser = null;
                System.Configuration.KeyValueConfigurationElement sADPassword = null;
                System.Configuration.KeyValueConfigurationElement sDomain = null;

                if (rootWebConfig.AppSettings.Settings.Count > 0)
                {
                    sADUser = rootWebConfig.AppSettings.Settings["sADUser"];
                    sADPassword = rootWebConfig.AppSettings.Settings["sADPassword"];
                    sDomain = rootWebConfig.AppSettings.Settings["sDomain"];
                    if (sADUser == null)
                    {
                        username = null;
                        logWriter.WriteTolog(string.Format("No ad admin profile application string"));
                    }
                    else
                    {
                        username = SearchUserName(sADUser.Value.ToString(), sADPassword.Value.ToString(), sDomain.Value.ToString());
                    }
                }
                else
                {
                    username = null;
                }
            }
            catch (Exception ex)
            {
                username = null;
                logWriter.WriteTolog(string.Format(" GetStaffProfile : Exception / {0} / {1}", staffADProfile.employee_number, ex.Message));
            }
            return username;
        }
        private string SearchUserName(string username, string password, string domain)
        {

            SearchResult rs = null;
            string uname = null;
            try
            {

                Debug.WriteLine(username);
                Debug.WriteLine(password);
                Debug.WriteLine(domain);

                rs = SearchUserByStaffNumber(GetDirectorySearcher(username, password, domain));

                if (rs != null)
                {

                    DirectoryEntry de = rs.GetDirectoryEntry();

                    uname = object.ReferenceEquals(de.Properties["sAMAccountName"].Value as string, null) ? String.Empty : de.Properties["sAMAccountName"].Value.ToString();
                    logWriter.WriteTolog(string.Format("SearchUserName : User found!!! / {0}", uname));
                }
                else
                {
                    uname = null;
                    logWriter.WriteTolog(string.Format("SearchUserName : User not found!!! / {0}", uname));
                }

            }
            catch (Exception ex)
            {
                logWriter.WriteTolog(string.Format("SearchUserName : Exception!!! / {0}", ex.Message));
                uname = null;
            }
            finally
            {
                Dispose();
                rs = null;
            }
            return uname;
        }

        internal StaffADProfile GetStaffProfile()
        {

            logWriter.WriteTolog(string.Format("Entered GetStaffProfile"));

            try
            {

                System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/");
                System.Configuration.KeyValueConfigurationElement sADUser = null;
                System.Configuration.KeyValueConfigurationElement sADPassword = null;
                System.Configuration.KeyValueConfigurationElement sDomain = null;

                if (rootWebConfig.AppSettings.Settings.Count > 0)
                {
                    sADUser = rootWebConfig.AppSettings.Settings["sADUser"];
                    sADPassword = rootWebConfig.AppSettings.Settings["sADPassword"];
                    sDomain = rootWebConfig.AppSettings.Settings["sDomain"];
                    if (sADUser == null)
                    {
                        staffADProfile = null;
                        logWriter.WriteTolog(string.Format("No ad admin profile application string"));
                    }
                    else
                    {
                        staffADProfile = GetStaffInformation(sADUser.Value.ToString(), sADPassword.Value.ToString(), sDomain.Value.ToString());
                    }
                }
                else
                {
                    staffADProfile = null;
                }
            }
            catch (Exception ex)
            {
                staffADProfile = null;
                logWriter.WriteTolog(string.Format(" GetStaffProfile : Exception / {0} / {1}", staffADProfile.employee_number, ex.Message));
            }

            return staffADProfile;
        }

        private StaffADProfile GetStaffInformation(string username, string password, string domain)
        {

            SearchResult rs = null;
            try
            {

                Debug.WriteLine(username);
                Debug.WriteLine(password);
                Debug.WriteLine(domain);

                rs = SearchUserByUserName(GetDirectorySearcher(username, password, domain));

                if (rs != null)
                {

                    DirectoryEntry de = rs.GetDirectoryEntry();

                    staffADProfile.employee_number = object.ReferenceEquals(de.Properties["description"].Value as string, null) ? string.Empty : de.Properties["description"].Value.ToString();
                    staffADProfile.branch_name = object.ReferenceEquals(de.Properties["physicalDeliveryOfficeName"].Value as string, null)
                                                                                            ? string.Empty : de.Properties["physicalDeliveryOfficeName"].Value.ToString();
                    staffADProfile.branch_code = object.ReferenceEquals(de.Parent.Properties["description"].Value as string, null) ? string.Empty : de.Parent.Properties["description"].Value.ToString();


                    staffADProfile.branch_address = object.ReferenceEquals(de.Properties["streetAddress"].Value as string, null) ? string.Empty : de.Properties["streetAddress"].Value.ToString();
                    staffADProfile.mobile_phone = object.ReferenceEquals(de.Properties["mobile"].Value as string, null) ? string.Empty : de.Properties["mobile"].Value.ToString();
                    staffADProfile.gsm = object.ReferenceEquals(de.Properties["telephoneNumber"].Value as string, null) ? string.Empty : de.Properties["telephoneNumber"].Value.ToString();
                    staffADProfile.jobtitle = object.ReferenceEquals(de.Properties["title"].Value as string, null) ? string.Empty : de.Properties["title"].Value.ToString();
                    staffADProfile.office_ext = object.ReferenceEquals(de.Properties["pager"].Value as string, null) ? string.Empty : de.Properties["pager"].Value.ToString();
                    staffADProfile.SelectedDept = object.ReferenceEquals(de.Properties["department"].Value as string, null) ? string.Empty : de.Properties["department"].Value.ToString();
                    staffADProfile.user_logon_name = object.ReferenceEquals(de.Properties["sAMAccountName"].Value as string, null) ? string.Empty : de.Properties["sAMAccountName"].Value.ToString();
                    staffADProfile.email = object.ReferenceEquals(de.Properties["mail"].Value as string, null) ? string.Empty : de.Properties["mail"].Value.ToString();
                    staffADProfile.name = object.ReferenceEquals(de.Properties["displayName"].Value as string, null) ? string.Empty : de.Properties["displayName"].Value.ToString();

                    staffADProfile.membership = getMemberships(de);

                    Tuple<int, string> organization = getOrganization(de);
                    staffADProfile.org_id = organization.Item1;
                    staffADProfile.org_name = organization.Item2;

                    Tuple<int, string, int> connector = DataHandler.getXceedServer(staffADProfile);
                    staffADProfile.org_id = connector.Item1;
                    staffADProfile.xceed_server = connector.Item2;
                    staffADProfile.ad_org_id = connector.Item3;
                    
                    staffADProfile.xceed_definition = DataHandler.getXceedDefinition(staffADProfile);

                    staffADProfile.branch_code = de.Parent.Properties["description"].Value as string;

                    //string deUserContainer = de.Parent.Properties["description"].Value as string;

                    logWriter.WriteTolog(string.Format("GetStaffInformation : User found!!! / {0}", staffADProfile.user_logon_name));
                }
                else
                {
                    staffADProfile = null;
                    logWriter.WriteTolog(string.Format("GetStaffInformation : User not found!!! / {0}", staffADProfile.user_logon_name));
                }

            }
            catch (Exception ex)
            {
                logWriter.WriteTolog(string.Format("GetStaffInformation : Exception!!! / {0}", ex.Message));
                staffADProfile = null;
            }
            finally
            {
                Dispose();
                rs = null;
            }

            return staffADProfile;
        }

        internal StaffADProfile GetStaffProfileByNumber()
        {

            logWriter.WriteTolog(string.Format("Entered GetStaffProfile"));

            try
            {

                System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/");
                System.Configuration.KeyValueConfigurationElement sADUser = null;
                System.Configuration.KeyValueConfigurationElement sADPassword = null;
                System.Configuration.KeyValueConfigurationElement sDomain = null;

                if (rootWebConfig.AppSettings.Settings.Count > 0)
                {
                    sADUser = rootWebConfig.AppSettings.Settings["sADUser"];
                    sADPassword = rootWebConfig.AppSettings.Settings["sADPassword"];
                    sDomain = rootWebConfig.AppSettings.Settings["sDomain"];
                    if (sADUser == null)
                    {
                        staffADProfile = null;
                        logWriter.WriteTolog(string.Format("No ad admin profile application string"));
                    }
                    else
                    {
                        staffADProfile = GetStaffInformationByStaffNumber(sADUser.Value.ToString(), sADPassword.Value.ToString(), sDomain.Value.ToString());
                    }
                }
                else
                {
                    staffADProfile = null;
                }
            }
            catch (Exception ex)
            {
                staffADProfile = null;
                logWriter.WriteTolog(string.Format(" GetStaffProfile : Exception / {0} / {1}", staffADProfile.employee_number, ex.Message));
            }

            return staffADProfile;
        }

        private StaffADProfile GetStaffInformationByStaffNumber(string username, string password, string domain)
        {

            SearchResult rs = null;
            try
            {

                Debug.WriteLine(username);
                Debug.WriteLine(password);
                Debug.WriteLine(domain);

                rs = SearchUserByStaffNumber(GetDirectorySearcher(username, password, domain));

                if (rs != null)
                {

                    DirectoryEntry de = rs.GetDirectoryEntry();

                    staffADProfile.employee_number = object.ReferenceEquals(de.Properties["description"].Value as string, null) ? string.Empty : de.Properties["description"].Value.ToString();
                    staffADProfile.branch_name = object.ReferenceEquals(de.Properties["physicalDeliveryOfficeName"].Value as string, null)
                                                                                            ? string.Empty : de.Properties["physicalDeliveryOfficeName"].Value.ToString();
                    staffADProfile.branch_code = object.ReferenceEquals(de.Parent.Properties["description"].Value as string, null) ? string.Empty : de.Parent.Properties["description"].Value.ToString();


                    staffADProfile.branch_address = object.ReferenceEquals(de.Properties["streetAddress"].Value as string, null) ? string.Empty : de.Properties["streetAddress"].Value.ToString();
                    staffADProfile.mobile_phone = object.ReferenceEquals(de.Properties["mobile"].Value as string, null) ? string.Empty : de.Properties["mobile"].Value.ToString();
                    staffADProfile.gsm = object.ReferenceEquals(de.Properties["telephoneNumber"].Value as string, null) ? string.Empty : de.Properties["telephoneNumber"].Value.ToString();
                    staffADProfile.jobtitle = object.ReferenceEquals(de.Properties["title"].Value as string, null) ? string.Empty : de.Properties["title"].Value.ToString();
                    staffADProfile.office_ext = object.ReferenceEquals(de.Properties["pager"].Value as string, null) ? string.Empty : de.Properties["pager"].Value.ToString();
                    staffADProfile.SelectedDept = object.ReferenceEquals(de.Properties["department"].Value as string, null) ? string.Empty : de.Properties["department"].Value.ToString();
                    staffADProfile.user_logon_name = object.ReferenceEquals(de.Properties["sAMAccountName"].Value as string, null) ? string.Empty : de.Properties["sAMAccountName"].Value.ToString();
                    staffADProfile.email = object.ReferenceEquals(de.Properties["mail"].Value as string, null) ? string.Empty : de.Properties["mail"].Value.ToString();
                    staffADProfile.name = object.ReferenceEquals(de.Properties["displayName"].Value as string, null) ? string.Empty : de.Properties["displayName"].Value.ToString();

                    staffADProfile.membership = getMemberships(de);

                    Tuple<int, string> organization = getOrganization(de);
                    staffADProfile.org_id = organization.Item1;
                    staffADProfile.org_name = organization.Item2;

                    Tuple<int, string, int> connector = DataHandler.getXceedServer(staffADProfile);
                    staffADProfile.org_id = connector.Item1;
                    staffADProfile.xceed_server = connector.Item2;
                    staffADProfile.ad_org_id = connector.Item3;

                    //staffADProfile.roles = LINQCalls.getUserRoles(staffADProfile.employee_number);
                    staffADProfile.xceed_definition = DataHandler.getXceedDefinition(staffADProfile);

                    logWriter.WriteTolog(string.Format("GetStaffInformation : User found!!! / {0}", staffADProfile.user_logon_name));
                }
                else
                {
                    staffADProfile = null;
                    logWriter.WriteTolog(string.Format("GetStaffInformation : User not found!!! / {0}", staffADProfile.user_logon_name));
                }

            }
            catch (Exception ex)
            {
                logWriter.WriteTolog(string.Format("GetStaffInformation : Exception!!! / {0}", ex.Message));
                staffADProfile = null;
            }
            finally
            {
                Dispose();
                rs = null;
            }

            return staffADProfile;
        }


        private Tuple<int, string> getOrganization(DirectoryEntry de)
        {

            //List<OUObject> ouObjects    = new List<OUObject>();
            bool rootOUFound = false;
            Tuple<int, string> result = Tuple.Create(1, "Zenithbank Nigeria");
            DirectoryEntry ou = de.Parent;

            do
            {
                string pname = ou.Parent.Name.ToString().ToUpper();
                if (pname.Contains("DC="))
                {
                    rootOUFound = true;
                    result = Tuple.Create(int.Parse(ou.Properties["description"].Value.ToString()), ou.Properties["name"].Value.ToString());
                }
                else
                {
                    ou = ou.Parent;
                }

            } while (!rootOUFound);

            return result;
        }

        private List<string> getMemberships(DirectoryEntry de)
        {

            string dn;
            int equalsIndex, commaIndex;
            int propertyCount = de.Properties["memberOf"].Count;

            List<string> memberships = new List<string>();

            for (int propertyCounter = 0; propertyCounter < propertyCount; propertyCounter++)
            {

                dn = String.IsNullOrEmpty(de.Properties["memberOf"][propertyCounter].ToString()) ? String.Empty : (string)de.Properties["memberOf"][propertyCounter].ToString();

                equalsIndex = dn.IndexOf("=", 1);
                commaIndex = dn.IndexOf(",", 1);

                Debug.WriteLine(dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1));

                if (-1 == equalsIndex || dn.Equals(String.Empty))
                {
                    return null;
                }
                else
                {
                    memberships.Add(dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1));



                }
            }

            return memberships;
        }

        private SearchResult SearchUserByUserName(DirectorySearcher ds)
        {

            SearchResult userObject = null;


            try
            {

                ds.Filter = "(&((&(objectCategory=Person)(objectClass=User)))(samaccountname=" + staffADProfile.user_logon_name + "))";

                ds.SearchScope = SearchScope.Subtree;
                ds.ServerTimeLimit = TimeSpan.FromSeconds(90);

                userObject = ds.FindOne();

                if (userObject != null)
                {
                    return userObject;
                }
                else
                {
                    logWriter.WriteTolog(string.Format("SearchUserByUserName : None Found {0} ", staffADProfile.user_logon_name));
                }
            }
            catch (Exception ex)
            {
                logWriter.WriteTolog(string.Format("SearchUserByUserName : Exception!!! {0} / {1}", ex.Message, staffADProfile.user_logon_name));
            }
            finally
            {
                Dispose();
            }

            return userObject;
        }

        private SearchResult SearchUserByEmail(DirectorySearcher ds, string email)
        {

            SearchResult userObject = null;
            try
            {
                ds.Filter = "(&((&(objectCategory=Person)(objectClass=User)))(mail=" + email + "))";

                ds.SearchScope = SearchScope.Subtree;
                ds.ServerTimeLimit = TimeSpan.FromSeconds(90);

                userObject = ds.FindOne();

                if (userObject != null)
                {
                    return userObject;
                }
                else
                {
                    logWriter.WriteTolog(string.Format("SearchUserByEmail : None Found {0} ", staffADProfile.user_logon_name));
                }
            }
            catch (Exception ex)
            {
                logWriter.WriteTolog(string.Format("SearchUserByEmail : Exception!!! {0} / {1}", ex.Message, staffADProfile.employee_number));
            }
            finally
            {
                Dispose();
            }

            return userObject;
        }

        private SearchResult SearchUserByStaffNumber(DirectorySearcher ds)
        {

            SearchResult userObject = null;

            try
            {
                ds.Filter = "(&((&(objectCategory=Person)(objectClass=User)))( description=" + StaffNumber + "))";

                ds.SearchScope = SearchScope.Subtree;
                ds.ServerTimeLimit = TimeSpan.FromSeconds(90);

                userObject = ds.FindOne();

                if (userObject != null)
                {
                    logWriter.WriteTolog(string.Format("SearchUserByStaffNumber : One Found {0} ", StaffNumber));
                }
                else
                {
                    logWriter.WriteTolog(string.Format("SearchUserByStaffNumber : None Found {0} ", StaffNumber));
                }
            }
            catch (Exception ex)
            {
                logWriter.WriteTolog(string.Format("SearchUserByStaffNumber : Exception!!! {0} / {1}", ex.Message, StaffNumber));
            }

            return userObject;

        }

        public DirectorySearcher GetDirectorySearcher(string username, string password, string domain)
        {

            //username = "africa\\admWorkflow";
            //password = "p@ssw0rd";
            //domain   = "africa.int.zenithbank.com";

            if (dirSearch == null)
            {

                try
                {
                    dirSearch = new DirectorySearcher(new DirectoryEntry("LDAP://" + domain.Trim(), username.Trim(), password.Trim()));
                }
                catch (DirectoryServicesCOMException ex)
                {
                    logWriter.WriteTolog(string.Format("Connection Creditial is Wrong!!!, please Check." + ex.Message));
                }
                return dirSearch;

            }
            else
            {
                return dirSearch;
            }
        }
        public void Dispose()
        {
            // If this function is being called the user wants to release the
            // resources. lets call the Dispose which will do this for us.
            Dispose(true);

            // Now since we have done the cleanup already there is nothing left
            // for the Finalizer to do. So lets tell the GC not to call it later.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                //someone want the deterministic release of all resources
                //Let us release all the managed resources
                ReleaseManagedResources();
            }
            else
            {
                // Do nothing, no one asked a dispose, the object went out of
                // scope and finalized is called so lets next round of GC 
                // release these resources
            }

            // Release the unmanaged resource in any case as they will not be 
            // released by GC
            ReleaseUnmangedResources();
        }

        ~ActiveDirectoryQuery()
        {
            // The object went out of scope and finalized is called
            // Lets call dispose in to release unmanaged resources 
            // the managed resources will anyways be released when GC 
            // runs the next time.
            Dispose(false);
        }

        void ReleaseManagedResources()
        {
            Console.WriteLine("Releasing Managed Resources");
            if (dirSearch != null)
            {
                dirSearch.Dispose();
            }
        }
        void ReleaseUnmangedResources()
        {
            Console.WriteLine("Releasing Unmanaged Resources");
        }
    }

    public class OUObject
    {
        public OUObject(int org_id, string org_name)
        {
            this.org_id = org_id;
            this.org_name = org_name;
        }

        public int org_id { get; set; }
        public string org_name { get; set; }
    }
}