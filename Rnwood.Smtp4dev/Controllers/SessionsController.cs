﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rnwood.Smtp4dev.DbModel;
using Rnwood.Smtp4dev.Hubs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO;
using MimeKit;
using HtmlAgilityPack;

namespace Rnwood.Smtp4dev.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [UseEtagFilterAttribute]
    public class SessionsController : Controller
    {
        public SessionsController(Smtp4devDbContext dbContext, SessionsHub sessionsHub)
        {
            this.dbContext = dbContext;
            this.sessionsHub = sessionsHub;
        }


        private Smtp4devDbContext dbContext;
        private SessionsHub sessionsHub;

        [HttpGet]
        public IEnumerable<ApiModel.SessionSummary> GetSummaries()
        {
            return dbContext.Sessions.Where(s => s.EndDate.HasValue)
                .Select(m => new ApiModel.SessionSummary(m));
        }

        [HttpGet("{id}")]
        public ApiModel.Session GetSession(Guid id)
        {
            Session result = dbContext.Sessions.FirstOrDefault(m => m.Id == id);
            return new ApiModel.Session(result);
        }

        [HttpGet("{id}/log")]
        public string GetSessionLog(Guid id)
        {
            Session result = dbContext.Sessions.FirstOrDefault(m => m.Id == id);
            return result.Log;
        }


        [HttpDelete("{id}")]
        public async Task Delete(Guid id)
        {
            dbContext.Sessions.RemoveRange(dbContext.Sessions.Where(s => s.Id == id ));

            dbContext.SaveChanges();

            await sessionsHub.OnSessionsChanged();

        }

        [HttpDelete("*")]
        public async Task DeleteAll()
        {
            dbContext.Sessions.RemoveRange(dbContext.Sessions.Where(s => s.EndDate.HasValue));
         
            dbContext.SaveChanges();

            await sessionsHub.OnSessionsChanged();

        }


    }
}
