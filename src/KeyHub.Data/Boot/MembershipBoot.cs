﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web.Security;
using KeyHub.Core.Errors;
using KeyHub.Core.Kernel;
using KeyHub.Model;
using WebMatrix.WebData;

namespace KeyHub.Data.Boot
{
    /// <summary>
    /// Holds the boot procedure for the Membership provider.
    /// This boot procedure must run after the RolesBoot class.
    /// </summary>
    public class MembershipBoot : IKernelEvent
    {
        private List<IError> issueList = new List<IError>();

        public KernelEventCompletedArguments Execute()
        {
            WebSecurity.InitializeDatabaseConnection("DataContext", "Users", "UserId", "MembershipUserIdentifier", autoCreateTables:true);

            // Create administator Role if not already present
            var currentRoles = Roles.GetAllRoles();
            if (!currentRoles.Contains(Role.SystemAdmin))
                Roles.CreateRole(Role.SystemAdmin);
            
            if (!currentRoles.Contains(Role.RegularUser))
                Roles.CreateRole(Role.RegularUser);

            // Create an administator of not already present        
            if (!Roles.GetUsersInRole(Role.SystemAdmin).Any())
            {
                // Create administrator user
                var adminMembershipIdentifier = Guid.NewGuid().ToString();
                WebSecurity.CreateUserAndAccount(adminMembershipIdentifier, "password", new { Email = "admin" });
                Roles.AddUserToRole(adminMembershipIdentifier, Role.SystemAdmin);
            }

            return new KernelEventCompletedArguments { AllowContinue = (!issueList.Any()), KernelEventSucceeded = (!issueList.Any()), Issues = issueList.ToArray() };
        }

        public string DisplayName
        {
            get { return "Membership boot"; }
        }

        public KernelEventsTypes EventType
        {
            get { return KernelEventsTypes.Startup; }
        }

        public int Priority
        {
            get { return 101; }
        }
    }
}