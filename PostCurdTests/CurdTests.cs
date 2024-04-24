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
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Microsoft.AspNetCore.StaticFiles;

namespace PostCurdTests
{
	public class CurdUserPosrTests : IDisposable
	{
		private readonly CapycomContext context;
		private readonly UserController controller;
		private readonly List<CpcmUser> users;
		private readonly List<CpcmPost> posts;

		public CurdUserPosrTests()
		{
			var options = new DbContextOptionsBuilder<CapycomContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			context = new CapycomContext(options);
			context.Database.EnsureDeleted();
			context.Database.EnsureCreated();

			//AddUsers
			users = new List<CpcmUser>
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
					},
					new CpcmUser
					{
						CpcmUserId = new Guid("00000000-0000-0000-0000-000010000009"),
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
						CpcmIsDeleted = false
					}
				};

			context.CpcmUsers.AddRange(users);

			//AddPosts
			var lastGuid = new Guid("00000000-0000-0000-0000-000000000012");
			var usernum = 0;
			posts = new List<CpcmPost>();
			for (int i = 0; i < 24; i++)
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
					CpcmPostCreationDate = DateTime.UtcNow - new TimeSpan(1, 0, 0),
					CpcmPostPublishedDate = DateTime.UtcNow - new TimeSpan(1, 0, 0),
					CpcmPostBanned = false,
					CpcmIsDeleted = false
				};
				posts.Add(post);
				lastGuid = post.CpcmPostId;
			}
			posts[0].CpcmIsDeleted = true;
			posts[1].CpcmPostBanned = true;
			posts[2].CpcmIsDeleted = true; posts[2].CpcmPostBanned = true;
			posts[3].CpcmPostPublishedDate = posts[3].CpcmPostCreationDate + TimeSpan.FromHours(2);

			posts[6].CpcmIsDeleted = true;
			posts[7].CpcmPostBanned = true;
			posts[8].CpcmIsDeleted = true; posts[8].CpcmPostBanned = true;
			posts[9].CpcmPostPublishedDate = posts[9].CpcmPostCreationDate + TimeSpan.FromHours(2);

			posts[12].CpcmIsDeleted = true;
			posts[13].CpcmPostBanned = true;
			posts[14].CpcmIsDeleted = true; posts[14].CpcmPostBanned = true;
			posts[15].CpcmPostPublishedDate = posts[15].CpcmPostCreationDate + TimeSpan.FromHours(2);
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

			//Act
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
				//controller.ModelState.IsValid.Should().BeFalse();
				viewResult.ViewData["Message"].Should().NotBeNull();
			}
			else
			{
				Assert.Fail();
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
		public async Task CreatePostP_SendPostWithTextNoFilesPublishedSet_ExpectViewAnd300Code()
		{
			//Arrange

			var userPostModel = new Capycom.Models.UserPostModel()
			{
				Files = null,
				Text = "Text",
				PostFatherId = null,
				Published = DateTime.UtcNow + new TimeSpan(1, 0, 0)
			};

			//Act
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
			var result = await controller.CreatePostP(userPostModel);

			//Assert
			if (result is RedirectToActionResult viewResult)
			{
				viewResult.ActionName.Should().Be("Index");
				//viewResult.StatusCode.Should().Be(400);
				//controller.HttpContext.Response.StatusCode.Should().Be(200);
			}
			else
			{
				Assert.Fail();
			}
		}

		[Fact]
		public async Task CreatePostP_SendPostWithTextNoFilesSelfRepost_ExpectStatusCode417()
		{
			//Arrange

			var userPostModel = new Capycom.Models.UserPostModel()
			{
				Files = null,
				Text = "Text",
				PostFatherId = posts[1].CpcmPostId,
			};

			//Act
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
			var result = await controller.CreatePostP(userPostModel);

			//Assert
			if (result is StatusCodeResult statusCodeResult)
			{
				statusCodeResult.StatusCode.Should().Be(417);
			}
			else
			{
				Assert.Fail();
			}
		}

		[Fact]
		public async Task CreatePostP_SendPostWithTextNoFilesReposAuthorBanned_ExpectStatusCode403()
		{
			//Arrange

			var userPostModel = new Capycom.Models.UserPostModel()
			{
				Files = null,
				Text = "Text",
				PostFatherId = posts[10].CpcmPostId,
			};

			//Act
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
			var result = await controller.CreatePostP(userPostModel);

			//Assert
			if (result is StatusCodeResult statusCodeResult)
			{
				statusCodeResult.StatusCode.Should().Be(403);
			}
			else
			{
				Assert.Fail();
			}
		}

		[Fact]
		public async Task CreatePostP_SendPostWithTextNoFilesRepost_ExpectStatusCode200()
		{
			//Arrange

			var userPostModel = new Capycom.Models.UserPostModel()
			{
				Files = null,
				Text = "Text",
				PostFatherId = posts[20].CpcmPostId,
			};

			//Act
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
			var result = await controller.CreatePostP(userPostModel);
			//Func<Task> act = async () => await controller.CreatePostP(userPostModel);

			//Assert
			//await act.Should().ThrowAsync<InvalidOperationException>();
			if (result is ObjectResult objectResult)
			{
				objectResult.StatusCode.Should().Be(200);
			}
			else
			{
				Assert.Fail();
			}
		}

		[Fact]
		public async Task CreatePostP_SendPostWithFileAndText_RedirectToIndex()
		{
			// Arrange

			//var fileMock = new Mock<IFormFile>();
			//var content = "Hello World from a Fake File";
			//var fileName = "test.pdf";
			//var ms = new MemoryStream();
			//var writer = new StreamWriter(ms);
			//writer.Write(content);
			//writer.Flush();
			//ms.Position = 0;
			//fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
			//fileMock.Setup(_ => _.FileName).Returns(fileName);
			//fileMock.Setup(_ => _.Length).Returns(ms.Length);

			//var file = fileMock.Object;

			FormFile file;
			Directory.GetCurrentDirectory();
			using (var stream = File.OpenRead("default.png"))
			{
				var provider = new FileExtensionContentTypeProvider();
				if (!provider.TryGetContentType(stream.Name, out var contentType))
				{
					contentType = "image/png";
				}
				file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
				{
					Headers = new HeaderDictionary(),
					ContentType = contentType
				};

				var userPostModel = new Capycom.Models.UserPostModel()
				{
					Files = new FormFileCollection() { file },
					Text = "asd",
					Published = DateTime.UtcNow + new TimeSpan(1, 0, 0)
				};

				// Act
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
				var result = await controller.CreatePostP(userPostModel);

				// Assert

				if (result is RedirectToActionResult viewResult)
				{
					viewResult.ActionName.Should().Be("Index");
					//viewResult.StatusCode.Should().Be(400);
					//controller.HttpContext.Response.StatusCode.Should().Be(200);
				}
				else
				{
					Assert.Fail();
				}
			}
		}

		[Fact]
		public async Task CreatePostP_SendPostWith5FilesAndText_ExpectRejectValidation()
		{
			// Arrange

			//var fileMock = new Mock<IFormFile>();
			//var content = "Hello World from a Fake File";
			//var fileName = "test.pdf";
			//var ms = new MemoryStream();
			//var writer = new StreamWriter(ms);
			//writer.Write(content);
			//writer.Flush();
			//ms.Position = 0;
			//fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
			//fileMock.Setup(_ => _.FileName).Returns(fileName);
			//fileMock.Setup(_ => _.Length).Returns(ms.Length);

			//var file = fileMock.Object;

			FormFile file;
			Directory.GetCurrentDirectory();
			using (var stream = File.OpenRead("default.png"))
			{
				var provider = new FileExtensionContentTypeProvider();
				if (!provider.TryGetContentType(stream.Name, out var contentType))
				{
					contentType = "image/png";
				}
				file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
				{
					Headers = new HeaderDictionary(),
					ContentType = contentType
				};

				var userPostModel = new Capycom.Models.UserPostModel()
				{
					Files = new FormFileCollection() { file, file, file, file, file },
					Text = "asd",
					Published = DateTime.UtcNow + new TimeSpan(1, 0, 0)
				};

				// Act
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
				var result = await controller.CreatePostP(userPostModel);

				// Assert

				if (result is ViewResult viewResult)
				{
					viewResult.ViewName.Should().Be("CreatePost");
					//viewResult.StatusCode.Should().Be(400);
					controller.HttpContext.Response.StatusCode.Should().Be(200);
					controller.ModelState.IsValid.Should().BeFalse();
				}
				else
				{
					Assert.Fail();
				}
			}
		}

		[Fact]
		public async Task CreatePostP_SendPostWithInvalidFileTypeAndText_ExpectRejectValidation()
		{
			// Arrange

			//var fileMock = new Mock<IFormFile>();
			//var content = "Hello World from a Fake File";
			//var fileName = "test.pdf";
			//var ms = new MemoryStream();
			//var writer = new StreamWriter(ms);
			//writer.Write(content);
			//writer.Flush();
			//ms.Position = 0;
			//fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
			//fileMock.Setup(_ => _.FileName).Returns(fileName);
			//fileMock.Setup(_ => _.Length).Returns(ms.Length);

			//var file = fileMock.Object;

			FormFile file;
			Directory.GetCurrentDirectory();
			using (var stream = File.OpenRead("default.png"))
			{
				var provider = new FileExtensionContentTypeProvider();
				if (!provider.TryGetContentType(stream.Name, out var contentType))
				{
					contentType = "image/bmp";
				}
				file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
				{
					Headers = new HeaderDictionary(),
					ContentType = contentType
				};

				var userPostModel = new Capycom.Models.UserPostModel()
				{
					Files = new FormFileCollection() { file, file, file, file, file },
					Text = "asd",
					Published = DateTime.UtcNow + new TimeSpan(1, 0, 0)
				};

				// Act
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
				var result = await controller.CreatePostP(userPostModel);

				// Assert

				if (result is ViewResult viewResult)
				{
					viewResult.ViewName.Should().Be("CreatePost");
					//viewResult.StatusCode.Should().Be(400);
					controller.HttpContext.Response.StatusCode.Should().Be(200);
					controller.ModelState.IsValid.Should().BeFalse();
				}
				else
				{
					Assert.Fail();
				}
			}
		}

		[Fact]
		public async Task DelPostP_TryDeleteDeletedPost_ExpectStatusCode404()
		{
			//Arrange
			var post = posts[0];

			//Act
			var result = await controller.DeletePost(post.CpcmPostId);

			//Assert
			if (result is StatusCodeResult statusCodeResult)
			{
				statusCodeResult.StatusCode.Should().Be(404);
			}
			else
			{
				Assert.Fail();
			}
		}

		//[Fact]
		//public async Task DelPostP_TryDeletePostWithOutRole_ExpectStatusCode403()
		//{
		//	//Arrange
		//	var post = posts[1];

		//	//Act
		//	var result = await controller.DeletePost(post.CpcmPostId);

		//	//Assert
		//	if (result is StatusCodeResult statusCodeResult)
		//	{
		//		statusCodeResult.StatusCode.Should().Be(403);
		//	}
		//	else
		//	{
		//		Assert.Fail();
		//	}
		//}

		//[Fact]
		//public async Task DelPostP_TryDeletePostWithRole_ExpectStatucCode500()
		//{
		//	//Arrange
		//	var post = posts[0];

		//	//Act
		//	var result = await controller.DeletePost(post.CpcmPostId);

		//	//Assert
		//	if (result is StatusCodeResult statusCodeResult)
		//	{
		//		statusCodeResult.StatusCode.Should().Be(403);
		//	}
		//	else
		//	{
		//		Assert.Fail();
		//	}
		//}

		[Fact]
		public async Task BanUnbanPost_TryBanPostWithOutRole_ExpectStatusCode403()
		{
			//Arrange
			var post = posts[1];

			//Act
			var result = await controller.BanUnbanPost(post.CpcmPostId);
			//Assert
			if (result is StatusCodeResult statusCodeResult)
			{
				statusCodeResult.StatusCode.Should().Be(403);
			}
			else
			{
				Assert.Fail();
			}
		}

		[Fact]
		public async Task BanUnbanPost_TryBanPostWithRole_ExpectStatusCode403()
		{
			//Arrange
			var post = posts[1];
			controller.User.AddIdentity(new ClaimsIdentity(new List<Claim>() { new Claim("CpcmCanDelUsersPosts", "True") }, "Cookies"));

			//Act
			var result = await controller.BanUnbanPost(post.CpcmPostId);
			//Assert
			if (result is ObjectResult objectResult)
			{
				objectResult.StatusCode.Should().Be(200);
			}
			else
			{
				Assert.Fail();
			}
		}

		[Fact]
		public async Task BanUnbanPost_TryBanUnavailablePostWithRole_ExpectStatusCode404()
		{
			//Arrange
			controller.User.AddIdentity(new ClaimsIdentity(new List<Claim>() { new Claim("CpcmCanDelUsersPosts", "True") }, "Cookies"));

			//Act
			var result = await controller.BanUnbanPost(new Guid("00010010-0010-0000-0000-000000000012"));
			//Assert
			if (result is StatusCodeResult statusCodeResult)
			{
				statusCodeResult.StatusCode.Should().Be(404);
			}
			else
			{
				Assert.Fail();
			}
		}

		[Fact]
		public async Task BanUnbanPost_TryBanDeletedPostWithRole_ExpectStatusCode404()
		{
			//Arrange
			var post = posts[0];
			controller.User.AddIdentity(new ClaimsIdentity(new List<Claim>() { new Claim("CpcmCanDelUsersPosts", "True") }, "Cookies"));

			//Act
			var result = await controller.BanUnbanPost(post.CpcmPostId);
			//Assert
			if (result is StatusCodeResult statusCodeResult)
			{
				statusCodeResult.StatusCode.Should().Be(404);
			}
			else
			{
				Assert.Fail();
			}
		}

		[Fact]
		public async Task EditPostP_TryEditUnavailablePost_ExpectStatusCode404()
		{
			//Arrange

			var postmodel = new Capycom.Models.UserPostEditModel()
			{
				Id = new Guid("00010010-0010-0000-0000-000000000012"),
				Text = "Text",
				PostFatherId = null,
				NewPublishDate = DateTime.UtcNow + new TimeSpan(1, 0, 0)
			};

			//Act
			var result = await controller.EditPostP(postmodel);
			//Assert
			if (result is StatusCodeResult codeResult)
			{
				codeResult.StatusCode.Should().Be(404);
			}
			else
			{
				Assert.Fail();
			}
		}

		[Fact]
		public async Task EditPostP_TryEditDeletedPost_ExpectStatusCode404()
		{
			//Arrange

			var postmodel = new Capycom.Models.UserPostEditModel()
			{
				Id = posts[0].CpcmPostId,
				Text = "Text",
				PostFatherId = null,
				NewPublishDate = DateTime.UtcNow + new TimeSpan(1, 0, 0)
			};

			//Act
			var result = await controller.EditPostP(postmodel);
			//Assert
			if (result is StatusCodeResult codeResult)
			{
				codeResult.StatusCode.Should().Be(404);
			}
			else
			{
				Assert.Fail();
			}
		}

		[Fact]
		public async Task EditPostP_TryEditPostWithOutRoles_ExpectStatusCode404()
		{
			//Arrange

			var postmodel = new Capycom.Models.UserPostEditModel()
			{
				Id = posts[23].CpcmPostId,
				Text = "Text",
				PostFatherId = null,
				NewPublishDate = DateTime.UtcNow + new TimeSpan(1, 0, 0)
			};

			//Act
			var result = await controller.EditPostP(postmodel);
			//Assert
			if (result is StatusCodeResult codeResult)
			{
				codeResult.StatusCode.Should().Be(403);
			}
			else
			{
				Assert.Fail();
			}
		}

		[Fact]
		public async Task EditPostP_TryEditPostWithRolesNotMyPost_ExpectRedirectToIndex()
		{
			//Arrange

			var postmodel = new Capycom.Models.UserPostEditModel()
			{
				Id = posts[23].CpcmPostId,
				Text = "Text",
				PostFatherId = null,
				NewPublishDate = DateTime.UtcNow + new TimeSpan(1, 0, 0)
			};
			controller.User.AddIdentity(new ClaimsIdentity(new List<Claim>() { new Claim("CpcmCanDelUsersPosts", "True") }, "Cookies"));

			//Act
			var result = await controller.EditPostP(postmodel);
			//Assert
			if (result is RedirectToActionResult redirectResult)
			{
				redirectResult.ActionName.Should().Be("Index");
				//controller.HttpContext.Response.StatusCode.Should.Be(300)
			}
			else
			{
				Assert.Fail();
			}
		}

		[Fact]
		public async Task EditPostP_TryEditMyPost_ExpectRedirectToIndex()
		{
			//Arrange

			var postmodel = new Capycom.Models.UserPostEditModel()
			{
				Id = posts[4].CpcmPostId,
				Text = "Text",
				PostFatherId = null,
				NewPublishDate = DateTime.UtcNow + new TimeSpan(1, 0, 0)
			};
			//controller.User.AddIdentity(new ClaimsIdentity(new List<Claim>() { new Claim("CpcmCanDelUsersPosts", "True") }, "Cookies"));

			//Act
			var result = await controller.EditPostP(postmodel);
			//Assert
			if (result is RedirectToActionResult redirectResult)
			{
				redirectResult.ActionName.Should().Be("Index");
				//controller.HttpContext.Response.StatusCode.Should.Be(300)
			}
			else
			{
				Assert.Fail();
			}
		}	

		public async Task EditPostP_TryAddFileToPost_ExpectRetirectTiTindex()
		{
			//Arrange
			var file = GetFile();
			var postmodel = new Capycom.Models.UserPostEditModel()
			{
				Id = posts[4].CpcmPostId,
				Text = "Text",
				PostFatherId = null,
				NewPublishDate = DateTime.UtcNow + new TimeSpan(1, 0, 0)
			};
			//controller.User.AddIdentity(new ClaimsIdentity(new List<Claim>() { new Claim("CpcmCanDelUsersPosts", "True") }, "Cookies"));

			//Act
			var result = await controller.EditPostP(postmodel);
			//Assert
			if (result is RedirectToActionResult redirectResult)
			{
				redirectResult.ActionName.Should().Be("Index");
				//controller.HttpContext.Response.StatusCode.Should.Be(300)
			}
			else
			{
				Assert.Fail();
			}
		}

		#region Вспомогательные методы

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

		private static FormFile GetFile(string path = "default.png")
		{
			FormFile file;
			Directory.GetCurrentDirectory();
			using (var stream = File.OpenRead(path))
			{
				var provider = new FileExtensionContentTypeProvider();
				if (!provider.TryGetContentType(stream.Name, out var contentType))
				{
					contentType = "image/png";
				}
				file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
				{
					Headers = new HeaderDictionary(),
					ContentType = contentType
				};

			}
			return file;
		}

		public void Dispose()
		{
			context.Dispose();
		}

		#endregion Вспомогательные методы

		//controller.User.AddIdentity(new ClaimsIdentity(new List<Claim>() { new Claim("CpcmUserId", users[0].CpcmUserId.ToString()) }, "Cookies")); CpcmCanDelUsersPosts
	}
}