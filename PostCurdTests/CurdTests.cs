using Capycom;
using Capycom.Controllers;
using Microsoft.EntityFrameworkCore;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Serilog.Events;
using Serilog;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
namespace PostCurdTests
{
	public class CurdUserPosrTests : IDisposable
	{
		private readonly CapycomContext context;
		private readonly UserController controller;

		public CurdUserPosrTests()
		{
			var options = new DbContextOptionsBuilder<CapycomContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			context = new CapycomContext(options);
			context.Database.EnsureDeleted();
			context.Database.EnsureCreated();

			//AddUsers
			var users = new List<CpcmUser>
				{
					new CpcmUser
					{
						CpcmUserId = new Guid("00000000-0000-0000-0000-000000000001"),
						CpcmUserEmail = "user1@example.com",
						CpcmUserTelNum = "123456789",
						CpcmUserPwdHash = new byte[] { 0x01, 0x02, 0x03 },
						CpcmUserSalt = "salt1",
						CpcmUserAbout = "About user 1",
						CpcmUserCity = new Guid("00000000-0000-0000-0000-000000000002"),
						CpcmUserSite = "user1.com",
						CpcmUserBooks = "Books user 1",
						CpcmUserFilms = "Films user 1",
						CpcmUserMusics = "Musics user 1",
						CpcmUserSchool = new Guid("00000000-0000-0000-0000-000000000003"),
						CpcmUserUniversity = new Guid("00000000-0000-0000-0000-000000000004"),
						CpcmUserImagePath = "user1.jpg",
						CpcmUserCoverPath = "user1_cover.jpg",
						CpcmUserNickName = "User1",
						CpcmUserFirstName = "First1",
						CpcmUserSecondName = "Second1",
						CpcmUserAdditionalName = "Additional1",
						CpcmUserRole = 1,
						CpcmUserBanned = false,
						CpcmIsDeleted = false
					},
					new CpcmUser
					{
						CpcmUserId = new Guid("00000000-0000-0000-0000-000000000005"),
						CpcmUserEmail = "user2@example.com",
						CpcmUserTelNum = "987654321",
						CpcmUserPwdHash = new byte[] { 0x04, 0x05, 0x06 },
						CpcmUserSalt = "salt2",
						CpcmUserAbout = "About user 2",
						CpcmUserCity = new Guid("00000000-0000-0000-0000-000000000006"),
						CpcmUserSite = "user2.com",
						CpcmUserBooks = "Books user 2",
						CpcmUserFilms = "Films user 2",
						CpcmUserMusics = "Musics user 2",
						CpcmUserSchool = new Guid("00000000-0000-0000-0000-000000000007"),
						CpcmUserUniversity = new Guid("00000000-0000-0000-0000-000000000008"),
						CpcmUserImagePath = "user2.jpg",
						CpcmUserCoverPath = "user2_cover.jpg",
						CpcmUserNickName = "User2",
						CpcmUserFirstName = "First2",
						CpcmUserSecondName = "Second2",
						CpcmUserAdditionalName = "Additional2",
						CpcmUserRole = 2,
						CpcmUserBanned = true,
						CpcmIsDeleted = false
					},
					new CpcmUser
					{
						CpcmUserId = new Guid("00000000-0000-0000-0000-000000000009"),
						CpcmUserEmail = "user3@example.com",
						CpcmUserTelNum = "555555555",
						CpcmUserPwdHash = new byte[] { 0x07, 0x08, 0x09 },
						CpcmUserSalt = "salt3",
						CpcmUserAbout = "About user 3",
						CpcmUserCity = new Guid("00000000-0000-0000-0000-000000000010"),
						CpcmUserSite = "user3.com",
						CpcmUserBooks = "Books user 3",
						CpcmUserFilms = "Films user 3",
						CpcmUserMusics = "Musics user 3",
						CpcmUserSchool = new Guid("00000000-0000-0000-0000-000000000011"),
						CpcmUserUniversity = new Guid("00000000-0000-0000-0000-000000000012"),
						CpcmUserImagePath = "user3.jpg",
						CpcmUserCoverPath = "user3_cover.jpg",
						CpcmUserNickName = "User3",
						CpcmUserFirstName = "First3",
						CpcmUserSecondName = "Second3",
						CpcmUserAdditionalName = "Additional3",
						CpcmUserRole = 3,
						CpcmUserBanned = false,
						CpcmIsDeleted = true
					}
				};

			context.CpcmUsers.AddRange(users);

			//AddPosts
			var lastGuid = new Guid("00000000-0000-0000-0000-000000000012");
			var usernum = 0;
			List<CpcmPost> posts = new List<CpcmPost>();
			for (int i = 0; i < 18; i++)
			{
				if (i % 6 == 0)
				{
					usernum++;
				}
				var post = new CpcmPost
				{
					CpcmPostId = NextGuid(lastGuid),
					CpcmUserId = users[usernum - 1].CpcmUserId,
					CpcmGroupId = null,
					CpcmPostText = $"Post {(i % 6) + 1} by User {usernum}",
					CpcmPostCreationDate = DateTime.UtcNow,
					CpcmPostPublishedDate = null,
					CpcmPostBanned = false,
					CpcmIsDeleted = false
				};
				posts.Add(post);
				lastGuid = post.CpcmPostId;
			}
			posts[0].CpcmIsDeleted = true;
			posts[1].CpcmPostBanned = true;
			posts[2].CpcmIsDeleted = true; posts[2].CpcmPostBanned=true;
			posts[3].CpcmPostPublishedDate = posts[3].CpcmPostCreationDate + TimeSpan.FromHours(1);

			posts[6].CpcmIsDeleted = true;
			posts[7].CpcmPostBanned = true;
			posts[8].CpcmIsDeleted = true; posts[8].CpcmPostBanned = true;
			posts[9].CpcmPostPublishedDate = posts[9].CpcmPostCreationDate + TimeSpan.FromHours(1);

			posts[12].CpcmIsDeleted = true;
			posts[13].CpcmPostBanned = true;
			posts[14].CpcmIsDeleted = true; posts[14].CpcmPostBanned = true;
			posts[15].CpcmPostPublishedDate = posts[15].CpcmPostCreationDate + TimeSpan.FromHours(1);
			context.CpcmPosts.AddRange(posts);
			context.SaveChanges();

			controller = new UserController(A.Fake<ILogger<UserController>>(), context, A.Fake<IOptions<MyConfig>>());
			var httpcontext = new DefaultHttpContext();
			controller.ControllerContext = new ControllerContext()
			{
				HttpContext = httpcontext
			};
			controller.User.AddIdentity(new ClaimsIdentity(new List<Claim>() { new Claim("CpcmUserId", users[0].CpcmUserId.ToString()) }, "Cookies"));
		}

		

		[Fact]
		public async Task CreatePostP_SendNullModel_ExpectViewWithMessageAnd400Code()
		{
			// Arrange

			

			var userPostModel = new Capycom.Models.UserPostModel();
			var validationContext = new ValidationContext(userPostModel);
			var validationResults = new List<ValidationResult>();
			Validator.TryValidateObject(userPostModel, validationContext, validationResults, true);
			if (validationResults.Any())
			{
				foreach (var validationResult in validationResults)
				{
					controller.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
				}
			}

			//Act

			var result = await controller.CreatePostP(userPostModel);


			//Asserts

			result.Should().BeAssignableTo<IActionResult>();
			//if (result is ObjectResult)
			//{
			//	httpcontext.Response.StatusCode.Should().Be(200);
			//}
			//if (result is StatusCodeResult statusCodeResult)
			//{
			//	statusCodeResult.StatusCode.Should().Be(200);
			//}
			if (result is ViewResult viewResult)
			{
				viewResult.ViewName.Should().Be("CreatePost");
				//viewResult.StatusCode.Should().Be(400);
				controller.HttpContext.Response.StatusCode.Should().Be(400);
				viewResult.ViewData["Message"].Should().NotBeNull();
			}
			//else if (result is PartialViewResult partialViewResult)
			//{
			//	//var model = partialViewResult.Model;
			//	//model.Should().NotBeNull();
			//	//model.ToString().Should().Contain("expected text");

			//	partialViewResult.ViewName.Should().Be("UserError");
			//	partialViewResult.ViewData["Message"].Should().NotBeNull();
			//}

		}

		[Fact]
		public async Task CreatePostP_SendPostWithTextNoFilesPublishedSet_ExpectViewAnd200Code()
		{
			//Arrange

			var userPostModel = new Capycom.Models.UserPostModel()
			{
				Files = null,
				Text = "Text",
				PostFatherId = null,
				Published = DateTime.UtcNow + new TimeSpan(1, 0, 0)
			};
			var validationContext = new ValidationContext(userPostModel);
			var validationResults = new List<ValidationResult>();
			Validator.TryValidateObject(userPostModel, validationContext, validationResults, true);
			if (validationResults.Any())
			{
				foreach (var validationResult in validationResults)
				{
					controller.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
				}
			}

			//Act

			var result = await controller.CreatePostP(userPostModel);

			//Assert
			if (result is ViewResult viewResult)
			{
				viewResult.ViewName.Should().Be("Index");
				//viewResult.StatusCode.Should().Be(400);
				controller.HttpContext.Response.StatusCode.Should().Be(200);
			}
		}

		#region ��������������� ������
		private static Guid NextGuid(Guid guid)
		{
			var guidStr = guid.ToString("N");
			var last5 = guidStr.Substring(guidStr.Length - 5);
			var number = int.Parse(last5, System.Globalization.NumberStyles.HexNumber);
			number++;
			var newLast5 = number.ToString("X").PadLeft(5, '0');
			var newGuidStr = guidStr.Substring(0, guidStr.Length - 5) + newLast5;
			return Guid.ParseExact(newGuidStr, "N");
		}

		public void Dispose()
		{
			context.Dispose();
		}
		#endregion
	}
}