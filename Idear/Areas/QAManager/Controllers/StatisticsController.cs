﻿using Idear.Areas.QAManager.ViewModels;
using Idear.Areas.Staff.ViewModels;
using Idear.Data;
using Idear.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Idear.Areas.QAManager.Controllers
{
    [Area("QAManager")]
    [Authorize(Roles = "QA Manager")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatisticsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var noCommentIdeas = await _context.Ideas
                .Include(i => i.Comments)
                .Include(i => i.User)
                .Where(i => i.Comments!.Count == 0)
                .OrderByDescending(i => i.DateTime)
                .Take(5)
                .ToListAsync();
            var anonymousIdeas = await _context.Ideas
                .Include(i => i.User)
                .Where(i => i.IsAnonymous)
                .OrderByDescending(i => i.DateTime)
                .Take(5)
                .ToListAsync();
            var anonymousComments = await _context.Comments
                .Include(c => c.Idea)
                .Include(c => c.User)
                .Where(c => c.IsAnonymous)
                .OrderByDescending(c => c.Datetime)
                .Take(5)
                .ToListAsync();
            var reports = await _context.Reports
                .Include(r => r.ReportedIdea)
                .Include(r => r.ReportedComment)
                .Include(r => r.Reporter)
                .OrderByDescending(r => r.DateTime)
                .Take(5)
                .AsSplitQuery()
                .ToListAsync();

            var reportVM = new ExceptionReportsVM
            {
                NoCommentIdeas = noCommentIdeas,
                AnonymousIdeas = anonymousIdeas,
                AnonymousComments = anonymousComments,
                Reports = reports
            };

            return View(reportVM);
        }

        public async Task<Dictionary<string, object>> GetDepartIdeaCount()
        {
            var labels = await _context.Departments
                .Select(d => d.Name)
                .ToListAsync();
            var ideaCounts = await _context.Departments
                .Include(d => d.Users!)
                    .ThenInclude(u => u.Ideas)
                .Select(d => d.Users.SelectMany(u => u.Ideas!).Count())
                .ToListAsync();

            Dictionary<string, object> data = new();
            data["labels"] = labels;
            data["ideaCounts"] = ideaCounts;

            return data;
        }

        public async Task<Dictionary<string, object>> GetDepartUserCount()
        {
            var labels = await _context.Departments
                .Select(d => d.Name)
                .ToListAsync();
            var userCounts = await _context.Departments
                .Include(d => d.Users!)
                    .ThenInclude(u => u.Ideas)
                .Select(d => d.Users.Where(u => u.Ideas!.Count > 0).Count())
                .ToListAsync();

            Dictionary<string, object> data = new();
            data["labels"] = labels;
            data["userCounts"] = userCounts;

            return data;
        }

        public async Task<IActionResult> ListIdea(string filter, int? page)
        {
            IQueryable<Idea> ideaQuery = null;
            switch (filter)
            {
                case "anonymous":
                    ideaQuery = _context.Ideas
                        .Where(i => i.IsAnonymous)
                        .OrderByDescending(i => i.DateTime);
                    break;
                case "noComment":
                    ideaQuery = _context.Ideas
                        .Where(i => i.Comments!.Count == 0)
                        .OrderByDescending(i => i.DateTime);
                    break;
                default:
                    ideaQuery = _context.Ideas
                        .OrderByDescending(i => i.DateTime);
                    break;
            }
            var ideas = await ideaQuery
                .Include(i => i.User)
                .Include(i => i.Views)
                .Include(i => i.Comments)
                .Include(i => i.Reacts)
                .AsSplitQuery()
                .ToListAsync();
            return View(PaginatedList<Idea>.Create(ideas, page ?? 1));
        }

        public async Task<IActionResult> ListComment(int? page)
        {
            var anonymousComments = await _context.Comments
                .Include(c => c.Idea)
                .Include(c => c.User)
                .Where(c => c.IsAnonymous)
                .OrderByDescending(c => c.Datetime)
                .ToListAsync();
            return View(PaginatedList<Comment>.Create(anonymousComments, page ?? 1));
        }

        public async Task<IActionResult> ListReport(string filter, int? page)
        {
            IQueryable<Report> reportQuery = null;
            switch (filter)
            {
                case "pending":
                    reportQuery = _context.Reports
                        .Where(r => r.Status == ReportStatus.Pending);
                    break;
                case "omitted":
                    reportQuery = _context.Reports
                        .Where(r => r.Status == ReportStatus.Omitted);
                    break;
                case "resolved":
                    reportQuery = _context.Reports
                        .Where(r => r.Status == ReportStatus.Resolved);
                    break;
                default:
                    reportQuery = _context.Reports;
                    break;
            }
            var reports = await reportQuery
                .Include(r => r.ReportedIdea)
                    .ThenInclude(i=>i!.User)
                .Include(r => r.ReportedComment)
                    .ThenInclude(c => c!.Idea)
				.Include(r => r.ReportedComment)
					.ThenInclude(c => c!.User)
				.Include(r => r.Reporter)
                .OrderByDescending(r => r.DateTime)
                .AsSplitQuery()
                .ToListAsync();
            return View(PaginatedList<Report>.Create(reports, page ?? 1));
        }
    }
}
