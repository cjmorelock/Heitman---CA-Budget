using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model {
    public class ActiveDirectoryHelper {
        public static string LDAP_PATH = ConfigurationManager.AppSettings["LDAP_PATH"] ?? "LDAP://DC=heitman,DC=com";

        public CAUser FindUserByUserName(string userName) {
            userName = userName.Contains('\\') ? userName.Substring(userName.LastIndexOf('\\') + 1) : userName; // remove domain name part, if found

            using (DirectoryEntry searchRoot = new DirectoryEntry(LDAP_PATH)) {
                using (DirectorySearcher searcher = new DirectorySearcher(searchRoot)) {
                    searcher.Filter = $@"(&(objectClass=user)(sAMAccountName={userName}))";
                    searcher.PropertiesToLoad.Clear();
                    searcher.PropertiesToLoad.AddRange(new string[] {
                        "objectGUID",
                        "sAMAccountName",
                        "distinguishedName",
                        "displayName",
                        "mail"});

                    var user = searcher.FindOne();

                    return user == null
                        ? null
                        : new CAUser (){
                            DisplayName = (string)user.Properties["displayName"][0]
                          , Username = (string)user.Properties["sAMAccountName"][0]
                          , Email = (string)user.Properties["mail"][0]
                        };
                }
            }

        }

        #region AD_Helper_Methods_Copied_From_Elsewhere_Not_Used_Here
        public IEnumerable<CAUser> GetMembersOfGroup(string group) {
            List<CAUser> members = new List<CAUser>();
            using (DirectoryEntry searchRoot = new DirectoryEntry(LDAP_PATH)) {
                string groupDn = group;
                using (DirectorySearcher searcher = new DirectorySearcher(searchRoot)) {
                    searcher.Filter = $"(&(objectClass=group)(name={group}))";
                    searcher.PropertiesToLoad.Add("distinguishedName");
                    var groupSearchResult = searcher.FindOne();
                    if (groupSearchResult == null) return members;
                    groupDn = (string)groupSearchResult.Properties["distinguishedName"][0];
                }

                IEnumerable<SearchResult> users = GetUsersRecursively(searchRoot, groupDn);
                foreach (var user in users) {
                    members.Add(new CAUser() {
                        DisplayName = (string)user.Properties["displayName"][0]
                      , Username = (string)user.Properties["sAMAccountName"][0]
                      , Email = (string)user.Properties["mail"][0]
                    });
                }
            }

            return members;
        }

        private IEnumerable<SearchResult> GetUsersRecursively(DirectoryEntry searchRoot, string groupDn) {
            HashSet<string> searchedGroups = new HashSet<string>();
            HashSet<string> searchedUsers = new HashSet<string>();
            return GetUsersRecursively(searchRoot, groupDn, searchedGroups, searchedUsers);
        }

        private IEnumerable<SearchResult> GetUsersRecursively(
            DirectoryEntry searchRoot,
            string groupDn,
            HashSet<string> searchedGroups,
            HashSet<string> searchedUsers) {

            // Depth-first, post-order traversal
            var subGroups = GetMembers(searchRoot, groupDn, "group");
            foreach (var subGroup in subGroups) {
                string subGroupName = ((string)subGroup.Properties["sAMAccountName"][0]).ToUpperInvariant();
                if (searchedGroups.Contains(subGroupName)) {
                    continue;
                }
                searchedGroups.Add(subGroupName);
                string subGroupDn = ((string)subGroup.Properties["distinguishedName"][0]);
                foreach (var user in GetUsersRecursively(searchRoot, subGroupDn, searchedGroups, searchedUsers)) {
                    yield return user;
                }
            }
            var users = GetMembers(searchRoot, groupDn, "user");
            foreach (var user in users) {
                string userName = ((string)user.Properties["sAMAccountName"][0]).ToUpperInvariant();
                if (searchedUsers.Contains(userName)) {
                    continue;
                }
                searchedUsers.Add(userName);
                yield return user;
            }
        }
        
        private IEnumerable<SearchResult> GetMembers(DirectoryEntry searchRoot, string groupDn, string objectClass) {
            using (DirectorySearcher searcher = new DirectorySearcher(searchRoot)) {
                searcher.Filter = "(&(objectClass=" + objectClass + ")(memberOf=" + groupDn + "))";
                searcher.PropertiesToLoad.Clear();
                searcher.PropertiesToLoad.AddRange(new string[] {
                "objectGUID",
                "sAMAccountName",
                "distinguishedName",
                "displayName",
                "mail"});
                searcher.Sort = new SortOption("sAMAccountName", SortDirection.Ascending);
                searcher.PageSize = 1000;
                searcher.SizeLimit = 0;
                foreach (SearchResult result in searcher.FindAll()) {
                    yield return result;
                }
            }
        }
        #endregion
    }
}
