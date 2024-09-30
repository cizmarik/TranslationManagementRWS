using System.Collections.Generic;

namespace TranslationManagement.Api.Controllers
{
    public static class JobStatuses
    {
        public const string New = "New";
        public const string Inprogress = "InProgress";
        public const string Completed = "Completed";
        public const string InvalidStatus = "invalid status";
        public const string InvalidStatusChange = "invalid status change";

        public static HashSet<string> AllStatuses = new HashSet<string> 
        {
            JobStatuses.New,
            JobStatuses.Inprogress, 
            JobStatuses.Completed,
            JobStatuses.InvalidStatus,
            JobStatuses.InvalidStatusChange

        };

        private static bool IsJobStatusChangeValid(string previousStatus, string newStatus)
        {
            if (previousStatus == newStatus)
            { return true; }

            if ((previousStatus == JobStatuses.New && newStatus == JobStatuses.Completed) ||
                 previousStatus == JobStatuses.Completed ||
                 newStatus == JobStatuses.New)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates if the stasus change is valid.
        /// </summary>
        /// <param name="previousStatus">Job status that is going to be changed by a new status.</param>
        /// <param name="newStatus">Job status that we would like to change to.</param>
        /// <returns>newStatus if it was valid. <cref>JobStatuses.InvalidStatusChange</cref> and <cref>JobStatuses.InvalidStatus</cref> 
        ///          are also considered as valid states.</returns>
        public static string ValidateNewJobStatus(string previousStatus, string newStatus)
        {
            if (false == JobStatuses.AllStatuses.Contains(newStatus))
            { return JobStatuses.InvalidStatus; }

            if (false == IsJobStatusChangeValid(previousStatus, newStatus))
            { return JobStatuses.InvalidStatusChange; }

            return newStatus;
        }
    }

 }
