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
        public StatusCodeResult UpdateJobStatus(int jobId, int translatorId, string newStatus = "")
        {
            _logger.LogInformation("Job status update request received: " + newStatus + " for job " + jobId.ToString() + " by translator " + translatorId);


            var job = _context.TranslationJobs.Single(j => j.Id == jobId);
            job.Status = JobStatuses.ValidateNewJobStatus(job.Status, newStatus);

            _context.SaveChanges();
            return new StatusCodeResult(StatusCodes.Status201Created);
        }

        [HttpPut]
        public StatusCodeResult UpdateJobTranslator(int jobId, int translatorId)
        {
            _logger.LogInformation("Job translator update request received for job " + jobId.ToString() + " by translator " + translatorId);
            var job = _context.TranslationJobs.Single(j => j.Id == jobId);
            var translator = _context.Translators.Single(t => t.Id == translatorId);

            if (translator.Status != TranslatorStatuses.CertifiedStatus)
            {
                _logger.LogWarning($"Translator {translatorId} is not certified and can't work on job: {jobId}");
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed); 
            }

            job.TranslatorID = translatorId;
            _context.SaveChanges();
            return new StatusCodeResult(StatusCodes.Status201Created);
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