﻿using System;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TranslationManagement.Api.Controllers;
using TranslationManagement.Api.Models;


namespace TranslationManagement.Api.Controlers
{
    [ApiController]
    [Route("api/TranslatorsManagement/[action]")]
    public class TranslatorManagementController : ControllerBase
    {
        private readonly ILogger<TranslatorManagementController> _logger;
        private AppDbContext _context;

        public TranslatorManagementController(IServiceScopeFactory scopeFactory, ILogger<TranslatorManagementController> logger)
        {
            _context = scopeFactory.CreateScope().ServiceProvider.GetService<AppDbContext>();
            _logger = logger;
        }

        [HttpGet]
        public ITranslatorModel[] GetTranslators()
        {
            return _context.Translators.ToArray();
        }

        [HttpGet("{name}")]
        public ContentResult GetTranslatorsByName(string name)
        {
            var content = _context.Translators.Where(t => t.Name == name).ToArray();
            var serializedContent = JsonSerializer.Serialize(content);
            ContentResult contentResult = new ContentResult() { Content = serializedContent, StatusCode = StatusCodes.Status201Created};
            return contentResult;
        }

        [HttpPost]
        public StatusCodeResult AddTranslator(TranslatorModel translator)
        {
            _context.Translators.Add(translator);
            return _context.SaveChanges() > 0 ? new StatusCodeResult(StatusCodes.Status201Created) :  new StatusCodeResult(StatusCodes.Status400BadRequest);
        }

        [HttpPut]
        public string UpdateTranslatorStatus(int translatorId, string newStatus = "")
        {
            _logger.LogInformation("User status update request: " + newStatus + " for user " + translatorId.ToString());
            if (false == TranslatorStatuses.IsTranslatorStatusValid(newStatus)) // I use false, because ! is very easily overlooked
            {
                throw new ArgumentException("unknown status");
            }

            var job = _context.Translators.Single(j => j.Id == translatorId);
            job.Status = newStatus;
            _context.SaveChanges();

            return "updated";
        }

        [HttpDelete]
        public bool DeleteTranslator(int translatorId)
        {

            var translator = _context.Translators.Find(translatorId);
            if (translator != null)
            {
                try
                {
                    if (_context.Translators.Remove(translator).State == EntityState.Deleted)
                    {
                        _logger.LogInformation($"translator {translatorId} has been removed");
                        return _context.SaveChanges() > 0;
                    }
                    else
                    {
                        _logger.LogError($"translator removal of {translatorId} has encountered an error!");
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
                    _logger.LogError($"Unknown exception when deleting translator with translatorId: {translatorId} exception: {exception.ToString()} callStack: {exception.StackTrace}");
                    return false;
                }

            }

            return false;
        }
    }
}