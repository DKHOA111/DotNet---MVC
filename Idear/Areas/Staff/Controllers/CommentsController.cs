﻿using Idear.Data;
using Idear.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Idear.Services;
using System.Globalization;

namespace Idear.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISendMailService _sendMailService;

        public CommentsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ISendMailService sendMailService)
        {
            _context = context;
            _userManager = userManager;
            _sendMailService = sendMailService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string cmtText, bool isAnonymous, string ideaId)
        {
			// check if the idea is null
			var idea = await _context.Ideas
				.Include(i => i.Topic)
				.FirstOrDefaultAsync(i => i.Id == ideaId);
			if (idea == null)
			{
				return BadRequest("The idea id for this comment is invalid.");
			}

			// check if the comment text is null or the topic final closure date is passed
			var topicFinalClosureDate = idea.Topic!.FinalClosureDate;
            if (string.IsNullOrEmpty(cmtText) || topicFinalClosureDate < DateTime.UtcNow)
            {
                return BadRequest("The request is invalid.");
            }

            var cmt = new Comment()
            {
                Id = Guid.NewGuid().ToString(),
                Text = cmtText,
                IsAnonymous = isAnonymous,
                Idea = idea,
                User = await _userManager.GetUserAsync(User),
                Datetime = DateTime.UtcNow
            };

            _context.Comments.Add(cmt);
            await _context.SaveChangesAsync();

            var currentIdea = cmt.Idea!;
            await _context.Entry(currentIdea).Reference(i => i.User).LoadAsync();
            var emailReceiver = currentIdea.User!;

            // send email using MailKit
            var url = Url.Action("Details", "Ideas", new { id = ideaId }, Request.Scheme);

            if (isAnonymous)
            {
                MailContent content = new MailContent
                {
                    To = emailReceiver.Email,
                    Subject = "Someone commented on your idea!",
                    Body = $"<p>An anonymous user has added a <a href=\"{url}#cmt-{cmt.Id}\">comment</a> on your \"<b>{cmt.Idea!.Text}</b>\" idea</p>",
                };
                _ = _sendMailService.SendMail(content);
            } else
            {
                MailContent content = new MailContent
                {
                    To = emailReceiver.Email,
                    Subject = "Someone commented on your idea!",
                    Body = $"<p>{cmt.User.FullName} has added a <a href=\"{url}#cmt-{cmt.Id}\">comment</a> on your \"<b>{cmt.Idea!.Text}</b>\" idea</p>",
                };
                _ = _sendMailService.SendMail(content);
            }

            return Json(new { id = cmt.Id, user = cmt.User.FullName, dateTime = cmt.Datetime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture) });
        }

		public async Task<IActionResult> Details(string id)
		{
			if (id == null || _context.Comments == null)
			{
				return NotFound();
			}

			var comment = await _context.Comments
				.FindAsync(id);
			if (comment == null)
			{
				return NotFound();
			}

			return Json(comment);
		}

		[HttpPut]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, [Bind("Text,IsAnonymous")] Comment comment)
		{
			var cmt = _context.Comments
				.Include(c => c.User)
				.FirstOrDefault(c => c.Id == id);

			if (cmt == null)
			{
				return BadRequest("No comment of that id found!");
			}

			// check if it's the current user's comment
			var currentUser = await _userManager.GetUserAsync(User);
			if (cmt.User != currentUser)
			{
				return BadRequest("You cannot edit other people's comment!");
			}

			if (ModelState.IsValid)
			{
				cmt.Text = comment.Text;
				cmt.IsAnonymous = comment.IsAnonymous;
				cmt.Datetime = DateTime.UtcNow;
				await _context.SaveChangesAsync();
				return Ok();
			}
			return BadRequest();
		}

		[HttpDelete]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(string id)
		{
			if (_context.Comments == null)
			{
				return Problem("Entity set 'ApplicationDbContext.Comments' is null.");
			}

			var comment = await _context.Comments
				.Include(c => c.User)
				.FirstOrDefaultAsync(c => c.Id == id);

			if (comment == null)
			{
				return BadRequest("Cannot delete comment as it is null!");
			}

			// check if it's the current user's comment
			var currentUser = await _userManager.GetUserAsync(User);
			if (comment.User != currentUser)
			{
				return BadRequest("You cannot delete other people's comment!");
			}

			_context.Comments.Remove(comment);
			await _context.SaveChangesAsync();
			return Ok();
		}
	}
}
