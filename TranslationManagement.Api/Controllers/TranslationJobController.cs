using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using External.ThirdParty.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TranslationManagement.Api.Controlers;
using TranslationManagement.Api.Models;
using System.Reflection;


namespace TranslationManagement.Api.Controllers
{
    [ApiController]
    [Route("api/jobs/[action]")]
    public class TranslationJobController : ControllerBase
    {

        private AppDbContext _context;
        private readonly ILogger<TranslatorManagementController> _logger;


        /// <summary>
        /// Validates if the stasus change is valid.
        /// </summary>
        /// <param name="previousStatus">Job status that is going to be changed by a new status.</param>
        /// <param name="newStatus">Job status that we would like to change to.</param>
        /// <returns>newStatus if it was valid. <cref>JobStatuses.InvalidStatusChange</cref> and <cref>JobStatuses.InvalidStatus</cref> 
        ///          are also considered as valid states.</returns>
        private string ValidateNewJobStatus(string previousStatus, string newStatus)
        {
            if (false == JobStatuses.AllStatuses.Contains(newStatus))
            { return JobStatuses.InvalidStatus; }

            if (IsJobStatusChangeValid(previousStatus, newStatus))
            { return JobStatuses.InvalidStatusChange; }

            return newStatus;
        }

        private bool IsJobStatusChangeValid(string previousStatus, string newStatus)
        {
            if ((previousStatus == JobStatuses.New && newStatus == JobStatuses.Completed) ||
                 previousStatus == JobStatuses.Completed ||
                 newStatus == JobStatuses.New)
            {
                return false;
            }
            return true;
        }

        public TranslationJobController(IServiceScopeFactory scopeFactory, ILogger<TranslatorManagementController> logger)
        {
            _context = scopeFactory.CreateScope().ServiceProvider.GetService<AppDbContext>();
            _logger = logger;
        }

        [HttpGet]
        public ITranslationJob[] GetJobs()
        {
            return _context.TranslationJobs.ToArray();
        }

        const double PricePerCharacter = 0.01;
        private void SetPrice(ITranslationJob job)
        {
            job.Price = job.OriginalContent.Length * PricePerCharacter;
        }

        [HttpPost]
        public bool CreateJob(TranslationJob job)
        {
            job.Status = JobStatuses.New;
            SetPrice(job);
            _context.TranslationJobs.Add(job);
            bool success = _context.SaveChanges() > 0;
            if (success)
            {
                var notificationSvc = new UnreliableNotificationService();
                while (!notificationSvc.SendNotification("Job created: " + job.Id).Result)
                {
                }

                _logger.LogInformation("New job notification sent");
            }

            return success;
        }

        [HttpPost]
        public bool CreateJobWithFile(IFormFile file, string customer)
        {
            var reader = new StreamReader(file.OpenReadStream());
            string content;

            if (file.FileName.EndsWith(".txt"))
            {
                content = reader.ReadToEnd();
            }
            else if (file.FileName.EndsWith(".xml"))
            {
                var xdoc = XDocument.Parse(reader.ReadToEnd());
                content = xdoc.Root.Element("Content").Value;
                customer = xdoc.Root.Element("Customer").Value.Trim();
            }
            else
            {
                throw new NotSupportedException("unsupported file");
            }

            var newJob = new TranslationJob()
            {
                OriginalContent = content,
                TranslatedContent = "",
                CustomerName = customer,
            };

            SetPrice(newJob);

            return CreateJob(newJob);
        }

        [HttpPut]
        public string UpdateJobStatus(int jobId, int translatorId, string newStatus = "")
        {
            _logger.LogInformation("Job status update request received: " + newStatus + " for job " + jobId.ToString() + " by translator " + translatorId);


            var job = _context.TranslationJobs.Single(j => j.Id == jobId);
            job.Status = ValidateNewJobStatus(job.Status, newStatus);

            _context.SaveChanges();
            return "updated";
        }

        [HttpPut]
        public string UpdateJobTranslator(int jobId, int translatorId)
        {
            _logger.LogInformation("Job translator update request received for job " + jobId.ToString() + " by translator " + translatorId);
            var job = _context.TranslationJobs.Single(j => j.Id == jobId);

           job.TranslatorID = translatorId;
            _context.SaveChanges();
            return "updated";
        }

        [HttpDelete]
        public bool DeleteJob(int id)
        {

            var job = _context.TranslationJobs.Find(id);
            if (job != null)
            {
                try
                {
                    if (_context.TranslationJobs.Remove(job).State == EntityState.Deleted)
                    {
                        _logger.LogInformation($"job {id} has been removed");
                        return _context.SaveChanges() > 0;
                    }
                    else
                    {
                        _logger.LogError($"job removal of {id} has encountered an error!");
                        return false;
                    }
                }
                catch (NullReferenceException)
                { // only example on multiple exceptions that can be thrown
                    // Caused by logger, do nothing;
                    return _context.SaveChanges() > 0;
                }
                catch (Exception exception)
                {
                    _logger.LogError($"Unknown exception when deleting job with id: {id} exception: {exception.ToString()} callStack: {exception.StackTrace}");
                    return false;
                }

            }

            return false;
        }


    }
}