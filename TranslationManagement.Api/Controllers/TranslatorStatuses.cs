using System;
using System.Collections.Generic;

namespace TranslationManagement.Api.Controllers
{
    public class TranslatorStatuses
    {
        public const string ApplicantStatus = "Applicant";
        public const string CertifiedStatus = "Certified";
        public const string DeletedStatus = "Deleted";

        internal static HashSet<string> AllStatuses = new HashSet<string>
        {
            ApplicantStatus,
            CertifiedStatus,
            DeletedStatus,

        };

        public static bool IsTranslatorStatusValid( string newStatus)
        {
            return TranslatorStatuses.AllStatuses.Contains(newStatus);
        }
    }
}
