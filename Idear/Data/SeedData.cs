﻿using Idear.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Idear.Data
{
	public class SeedData : IDisposable
	{
		private readonly ApplicationDbContext _context;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly UserManager<ApplicationUser> _userManager;
		public SeedData(IServiceProvider serviceProvider)
		{
			_context = serviceProvider.GetRequiredService<ApplicationDbContext>();
			_roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
			_userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
		}

		public void Initialize()
		{
			if (_context.ApplicationUsers.Any())
			{
				return;
			}

			string[] roles = { "Staff", "Admin", "QA Manager", "QA Coordinator" };
			CreateRoles(roles);

			string[] departments = { "QA", "IT", "HR", "Finance" };
			CreateDepartments(departments);

			string[] categories = { "General", "Improvement", "Change/Update" };
			CreateCategories(categories);

			CreateTopics();

			CreateUsers();

			CreateIdeas();

			CreateComments();

			CreateViews();

			CreateReactes();
		}

		public void Dispose()
		{
			_context.Dispose();
			_roleManager.Dispose();
			_userManager.Dispose();
			GC.SuppressFinalize(this);
		}

		private void CreateRoles(string[] roleList)
		{
			foreach (string role in roleList)
			{
				if (!_context.Roles.Any(r => r.Name == role))
				{
					var createdResult = _roleManager.CreateAsync(new IdentityRole(role)).Result;
					if (!createdResult.Succeeded)
					{
						throw new Exception(string.Join(";", createdResult.Errors));
					}
				}
			}
		}

		private void CreateUsers()
		{
			CreateUser("Loc123@gmail.com", "Le Huynh Loc", "Loc123@gmail.com", "Staff", "QA");
			CreateUser("N1@gmail.com", "Nhan Vien 1", "N1@gmail.com", "Staff", "IT");
			CreateUser("N2@gmail.com", "Nhan Vien 2", "N2@gmail.com", "Staff", "HR");
			CreateUser("N3@gmail.com", "Nhan Vien 3", "N3@gmail.com", "Staff", "Finance");
			CreateUser("Khoa234@gmail.com", "Le Dong Khoa", "Khoa234@gmail.com", "Admin", "QA");
			CreateUser("Huy345@gmail.com", "Nguyen Phuoc Huy", "Huy345@gmail.com", "QA Manager",
				"QA");
			CreateUser("Tri456@gmail.com", "Tat Khai Tri", "Tri456@gmail.com", "QA Coordinator",
				"IT");
			CreateUser("Admin1@gmail.com", "Admin", "Admin1@gmail.com", "Admin",
				"HR");
		}

		private void CreateUser(string email, string fullName, string password, string role,
			string department)
		{
			var user = new ApplicationUser
			{
				Email = email,
				UserName = email,
				FullName = fullName,
				Department = _context.Departments.First(d => d.Name == department)
			};

			if (!_context.Users.Any(u => u.UserName == user.UserName))
			{
				var createdResult = _userManager.CreateAsync(user, password).Result;
				if (!createdResult.Succeeded)
				{
					throw new Exception(string.Join(";", createdResult.Errors));
				}
			}

			var assignedResult = _userManager.AddToRoleAsync(user, role).Result;
			if (!assignedResult.Succeeded)
			{
				throw new Exception(string.Join(";", assignedResult.Errors));
			}
		}

		private void CreateTopics()
		{
			_context.Topics.AddRange(
				new Topic
				{
					Id = Guid.NewGuid().ToString(),
					Name = "Workshop ideas for IT students",
					ClosureDate = new DateTime(2023, 1, 1, 0, 0, 0, kind: DateTimeKind.Utc),
					FinalClosureDate = new DateTime(2023, 2, 1, 0, 0, 0, kind: DateTimeKind.Utc)
				},
				new Topic
				{
					Id = Guid.NewGuid().ToString(),
					Name = "Event ideas for welcoming new students",
					ClosureDate = new DateTime(2023, 1, 1, 0, 0, 0, kind: DateTimeKind.Utc),
					FinalClosureDate = new DateTime(2023, 12, 31, 0, 0, 0, kind: DateTimeKind.Utc)
				},
				new Topic
				{
					Id = Guid.NewGuid().ToString(),
					Name = "Improve work evironment for students and staffs",
					ClosureDate = new DateTime(2023, 6, 1, 0, 0, 0, kind: DateTimeKind.Utc),
					FinalClosureDate = new DateTime(2023, 12, 31, 0, 0, 0, kind: DateTimeKind.Utc)
				}
			);
			_context.SaveChanges();
		}

		private void CreateDepartments(string[] departments)
		{
			foreach (string department in departments)
			{
				_context.Departments.Add(
					new()
					{
						Id = Guid.NewGuid().ToString(),
						Name = department
					}
				);
			}
			_context.SaveChanges();
		}

		private void CreateCategories(string[] categories)
		{
			foreach (string category in categories)
			{
				_context.Categories.Add(
					new()
					{
						Id = Guid.NewGuid().ToString(),
						Name = category
					}
				);
			}
			_context.SaveChanges();
		}

		private void CreateIdeas()
		{
			_context.Ideas.AddRange(
				new Idea
				{
					Id = Guid.NewGuid().ToString(),
					Text = "Invite BlockChain experts to share with students",
					DateTime = new DateTime(2023, 1, 10, 3, 30, 0, DateTimeKind.Utc),
					User = _context.ApplicationUsers.First(u => u.UserName == "N1@gmail.com"),
					IsAnonymous = true,
					Topic = _context.Topics.First(t => t.Name == "Workshop ideas for IT students"),
					Category = _context.Categories.First(c => c.Name == "General")
				},
				new Idea
				{
					Id = Guid.NewGuid().ToString(),
					Text = "Decorate the university's mainhall with balloon",
					DateTime = new DateTime(2023, 1, 10, 3, 30, 0, DateTimeKind.Utc),
					User = _context.ApplicationUsers.First(u => u.UserName == "N2@gmail.com"),
					Topic = _context.Topics.First(t => t.Name == "Event ideas for welcoming new students"),
					Category = _context.Categories.First(c => c.Name == "Change/Update")
				},
				new Idea
				{
					Id = Guid.NewGuid().ToString(),
					Text = "Add kitchen tools at the pastry for students and staffs to use",
					DateTime = new DateTime(2023, 2, 20, 13, 30, 0, DateTimeKind.Utc),
					User = _context.ApplicationUsers.First(u => u.UserName == "N2@gmail.com"),
					Topic = _context.Topics.First(t => t.Name == "Improve work evironment for students and staffs"),
					Category = _context.Categories.First(c => c.Name == "Change/Update")
				},
				new Idea
				{
					Id = Guid.NewGuid().ToString(),
					Text = "Improve the air conditional for student room",
					DateTime = new DateTime(2023, 2, 24, 13, 30, 0, DateTimeKind.Utc),
					User = _context.ApplicationUsers.First(u => u.UserName == "Loc123@gmail.com"),
					IsAnonymous = true,
					Topic = _context.Topics.First(t => t.Name == "Improve work evironment for students and staffs"),
					Category = _context.Categories.First(c => c.Name == "Improvement")
				}
			);
			_context.SaveChanges();
		}

		private void CreateComments()
		{
			_context.Comments.AddRange(
				new Comment
				{
					Id = Guid.NewGuid().ToString(),
					Text = "100% agree.",
					Datetime = DateTime.UtcNow,
					Idea = _context.Ideas.First(i => i.Text == "Improve the air conditional for student room"),
					IsAnonymous = true,
					User = _context.ApplicationUsers.First(u => u.UserName == "Tri456@gmail.com")
				},
				new Comment
				{
					Id = Guid.NewGuid().ToString(),
					Text = "They won't fix it, it's been an issue for months.",
					Datetime = DateTime.UtcNow,
					Idea = _context.Ideas.First(i => i.Text == "Improve the air conditional for student room"),
					User = _context.ApplicationUsers.First(u => u.UserName == "N3@gmail.com")
				},
				new Comment
				{
					Id = Guid.NewGuid().ToString(),
					Text = "It's not necessary. Just a waste of money!",
					Datetime = DateTime.UtcNow,
					Idea = _context.Ideas.First(i => i.Text == "Add kitchen tools at the pastry for students and staffs to use"),
					User = _context.ApplicationUsers.First(u => u.UserName == "Khoa234@gmail.com")
				}
			);
			_context.SaveChanges();
		}

		private void CreateViews()
		{
			_context.Views.AddRange(
				new View
				{
					Id = Guid.NewGuid().ToString(),
					VisitTime = 3,
					Idea = _context.Ideas.First(i => i.Text == "Improve the air conditional for student room"),
					User = _context.ApplicationUsers.First(u => u.UserName == "N3@gmail.com"),
					ViewDateTime = DateTime.UtcNow
				},
				new View
				{
					Id = Guid.NewGuid().ToString(),
					VisitTime = 1,
					Idea = _context.Ideas.First(i => i.Text == "Improve the air conditional for student room"),
					User = _context.ApplicationUsers.First(u => u.UserName == "Tri456@gmail.com"),
					ViewDateTime = DateTime.UtcNow
				},
				new View
				{
					Id = Guid.NewGuid().ToString(),
					VisitTime = 2,
					Idea = _context.Ideas.First(i => i.Text == "Decorate the university's mainhall with balloon"),
					User = _context.ApplicationUsers.First(u => u.UserName == "Tri456@gmail.com"),
					ViewDateTime = DateTime.UtcNow
				},
				new View
				{
					Id = Guid.NewGuid().ToString(),
					VisitTime = 2,
					Idea = _context.Ideas.First(i => i.Text == "Invite BlockChain experts to share with students"),
					User = _context.ApplicationUsers.First(u => u.UserName == "Tri456@gmail.com"),
					ViewDateTime = DateTime.UtcNow
				}
			);
			_context.SaveChanges();
		}

		private void CreateReactes()
		{
			_context.Reactes.AddRange(
				new React
				{
					Id = Guid.NewGuid().ToString(),
					ReactFlag = 1,
					Idea = _context.Ideas.First(i => i.Text == "Improve the air conditional for student room"),
					User = _context.ApplicationUsers.First(u => u.UserName == "Tri456@gmail.com")
				},
				new React
				{
					Id = Guid.NewGuid().ToString(),
					ReactFlag = -1,
					Idea = _context.Ideas.First(i => i.Text == "Improve the air conditional for student room"),
					User = _context.ApplicationUsers.First(u => u.UserName == "N3@gmail.com")
				},
				new React
				{
					Id = Guid.NewGuid().ToString(),
					ReactFlag = 1,
					Idea = _context.Ideas.First(i => i.Text == "Invite BlockChain experts to share with students"),
					User = _context.ApplicationUsers.First(u => u.UserName == "N3@gmail.com")
				},
				new React
				{
					Id = Guid.NewGuid().ToString(),
					ReactFlag = 1,
					Idea = _context.Ideas.First(i => i.Text == "Invite BlockChain experts to share with students"),
					User = _context.ApplicationUsers.First(u => u.UserName == "Khoa234@gmail.com")
				}
			);
			_context.SaveChanges();
		}
	}
}
