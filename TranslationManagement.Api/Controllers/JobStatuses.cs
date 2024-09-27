using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TranslationManagement.Api.Controllers
{
    internal class JobStatuses
    {
        internal static readonly string New = "New";
        internal static readonly string Inprogress = "InProgress";
        internal static readonly string Completed = "Completed";
        internal static readonly string InvalidStatus = "invalid status";
        internal static readonly string InvalidStatusChange = "invalid status change";

        internal static HashSet<string> AllStatuses = new HashSet<string> 
        {
            JobStatuses.New,
            JobStatuses.Inprogress, 
            JobStatuses.Completed,
            JobStatuses.InvalidStatus,
            JobStatuses.InvalidStatusChange

        };



    }

 }
