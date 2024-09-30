using Moq;
using TranslationManagement.Api.Controllers;

namespace TranslationMnagementTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }


        /// <summary>
        /// Testing all positive statuses
        /// previousStatuses: All statuses except "Completed"
        /// newStatuses: All statuses except "New"
        /// forbiddenCombination: newStatus = "New", previousStatus = "Completed"
        /// Expects: Does not contain "invalid status" or "invalid status change"
        /// </summary>
        [Test]
        public void ValidateNewJobStatus_Positive()
        {
            // Arrange
            List<string> negativeStatuses = new() { JobStatuses.InvalidStatus, JobStatuses.InvalidStatusChange };
            var acceptablePreviousStatuses = JobStatuses.AllStatuses.Where(x => x != JobStatuses.Completed && !negativeStatuses.Contains(x));
            var acceptableNewStatuses = JobStatuses.AllStatuses.Where(x => x != JobStatuses.New && !negativeStatuses.Contains(x));
            Tuple<string, string> forbiddenCombination = new(JobStatuses.New, JobStatuses.Completed);

            // Act
            foreach (var previousStatus in acceptablePreviousStatuses)
            {
                foreach(var newStatus in acceptableNewStatuses)
                {
                    if (newStatus == JobStatuses.Completed && previousStatus == JobStatuses.New) // forbidden combination
                    { continue; } 

                    string status = JobStatuses.ValidateNewJobStatus(previousStatus, newStatus);
                    Assert.IsFalse(negativeStatuses.Contains(status));
                }
            }
        }

        /// <summary>
        /// Testing invalid JobStatuses
        /// previousStatus = New, Completed, InProgress
        /// newStatus = New, Completed, InProgress
        /// Expects: Contains either "invalid status" or "invalid status change"
        /// </summary>
        [Test]
        [TestCase(JobStatuses.Completed, JobStatuses.New)]
        [TestCase(JobStatuses.New, JobStatuses.Completed)]
        [TestCase(JobStatuses.Completed, JobStatuses.Inprogress)]
        [TestCase(JobStatuses.Inprogress, JobStatuses.New)]
        public void ValidateNewJobStatus_Negative1(string previousStatus, string newStatus)
        {
            // Arrange
            List<string> negativeStatuses = new() { JobStatuses.InvalidStatus, JobStatuses.InvalidStatusChange };

            // Act
            string status = JobStatuses.ValidateNewJobStatus(previousStatus, newStatus);

            // Assert
            Assert.IsTrue(negativeStatuses.Contains(status));
        }


        /// <summary>
        /// Testing invalid new job status
        /// newStatus = "wrongStatus", "New"
        /// previousStatus = "InProgress"
        /// Expects: Contains either "invalid status" or "invalid status change"
        /// </summary>
        [Test]
        [TestCase("wrongStatus")]
        [TestCase(JobStatuses.New)]
        public void ValidateNewJobStatus_Negative2(string newStatus)
        {
            // Arrange
            string previousStatus = JobStatuses.Inprogress;
            List<string> negativeStatuses = new() { JobStatuses.InvalidStatus, JobStatuses.InvalidStatusChange };

            // Act
            string status = JobStatuses.ValidateNewJobStatus(previousStatus, newStatus);

            // Assert
            Assert.IsTrue(negativeStatuses.Contains(status));
        }

        
        
    }
}