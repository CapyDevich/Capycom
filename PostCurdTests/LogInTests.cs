using Capycom.Controllers;
using Capycom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.AspNetCore.StaticFiles;
using Capycom.Models;
using FluentAssertions;
using Microsoft.CodeAnalysis.Options;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Reflection;
using Moq;

namespace PostCurdTests
{
	public class LogInTests
	{
		private readonly CapycomContext context;
		private readonly UserLogInController controller;
		private readonly List<CpcmUser> users;
		private readonly List<CpcmPost> posts;
		private readonly string serverSol = "CapybaraTop";

		//SHA256.HashData(Encoding.Unicode.GetBytes(stringToSHA + sol + serversol));
		public LogInTests()
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
						CpcmUserSalt = "salt1",
						CpcmUserPwdHash = System.Security.Cryptography.SHA256.HashData(Encoding.Unicode.GetBytes("Kek_1_1_Kek" + "salt1" + serverSol)),
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
						CpcmUserPwdHash = System.Security.Cryptography.SHA256.HashData(Encoding.Unicode.GetBytes("Kek_1_2_Kek" + "salt2" + serverSol)),
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
						CpcmUserPwdHash = System.Security.Cryptography.SHA256.HashData(Encoding.Unicode.GetBytes("Kek_1_3_Kek" + "salt3" + serverSol)),
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
						CpcmUserPwdHash = System.Security.Cryptography.SHA256.HashData(Encoding.Unicode.GetBytes("Kek_1_4_Kek" + "salt3" + serverSol)),
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
						CpcmUserBanned = true,
						CpcmIsDeleted = true
					}
				};
			var roles = new List<CpcmRole>() { new CpcmRole() { CpcmRoleId = 1, CpcmRoleName = "123" }, new CpcmRole() { CpcmRoleId = 2, CpcmRoleName = "123" }, new CpcmRole() { CpcmRoleId = 3, CpcmRoleName = "123" }, new CpcmRole() { CpcmRoleId = 4, CpcmRoleName = "123" } };

			context.CpcmUsers.AddRange(users); context.CpcmRoles.AddRange(roles);

			context.SaveChanges();
            var config = new MyConfig
            {
                ServerSol = "CapybaraTop",
                AllowSignIn = true,
                AllowLogIn = true,
                AllowCreatePost = true,
                AllowEditPost = true,
                AllowCreateComment = true,
                AllowEditUserInfo = true,
                AllowEditUserIdentity = true
            };
            var mockOptions = new Mock<IOptions<MyConfig>>();
            mockOptions.Setup(o => o.Value).Returns(config);
            controller = new UserLogInController(A.Fake<ILogger<UserLogInController>>(), context, mockOptions.Object);
			var httpcontext = new DefaultHttpContext();
			controller.ControllerContext = new ControllerContext()
			{
				HttpContext = httpcontext
			};
		}

		[Fact]
		public async Task Index_TryLogInWithInvalidPassword_ExpectViewAndCode403()
		{
			//Arrange
			var model = new UserLogInModel()
			{
				CpcmUserEmail = "user1@example.com",
				CpcmUserPwd = "Kek_1_1_Kek1"
			};

			//Act
			var actionResult = await controller.Index(model);
			//Arrange
			if(actionResult is ViewResult result)
			{
				result.ViewName.Should().Be(null);
				controller.HttpContext.Response.StatusCode.Should().Be(403);
			}
			else
			{
				Assert.Fail();
			}
		}

		[Fact]
		public async Task Index_TryLogInWithInvalidEmail_ExpectViewAndCode403()
		{
			//Arrange
			var model = new UserLogInModel()
			{
				CpcmUserEmail = "user1@example.com1",
				CpcmUserPwd = "Kek_1_1_Kek"
			};

			//Act
			var actionResult = await controller.Index(model);

			//Arrange
			if (actionResult is ViewResult result)
			{
				result.ViewName.Should().Be(null);
				controller.HttpContext.Response.StatusCode.Should().Be(403);
			}
			else
			{
				Assert.Fail();
			}
		}

		[Fact]
		public async Task Index_TryLogIntoDeletedProfileWithCorrectData_ExpectViewAndCode404()
		{
			//Arrange
			var model = new UserLogInModel()
			{
				CpcmUserEmail = "user3@example.com",
				CpcmUserPwd = "Kek_1_3_Kek"
			};

			//Act
			var actionResult = await controller.Index(model);

			//Arrange
			if( actionResult is ViewResult result)
			{
				result.ViewName.Should().Be(null);
				controller.HttpContext.Response.StatusCode.Should().Be(404);
			}
			else
			{
				Assert.Fail();
			}
			
		}

		[Fact]
		public async Task Index_TryLogIntoBannedProfileWithIncorrectData_ExpectViewResultAndCode403()
		{
			//Arrange
			var model = new UserLogInModel()
			{
				CpcmUserEmail = "user2@example.com",
				CpcmUserPwd = "Kek_1_2_Kek"
			};

			//Act
			var actionResult = await controller.Index(model);

			//Arrange
			if (actionResult is ViewResult result)
			{
				result.ViewName.Should().Be("UserError");
				controller.HttpContext.Response.StatusCode.Should().Be(403);
			}
			else
			{
				Assert.Fail();
			}
		}

		//[Fact]
		//public async Task Index_TryLogIntoProfileWithCorrectData_ExpectRedirectToUserControllerIndex()
		//{
		//	//HttpContext.Session.SetString - вылетает тест в нём 
		//	//Arrange
		//	var model = new UserLogInModel()
		//	{
		//		CpcmUserEmail = "user1@example.com",
		//		CpcmUserPwd = "Kek_1_1_Kek"
		//	};

		//	//Act
		//	var actionResult = await controller.Index(model);

		//	//Arrange
		//	if (actionResult is RedirectToActionResult result)
		//	{
		//		result.ControllerName.Should().Be("User");
		//		result.ActionName.Should().Be("Index");
		//		//controller.HttpContext.Response.StatusCode.Should().Be(403);
		//	}
		//	else
		//	{
		//		Assert.Fail();
		//	}
		//}

		[Fact]
		public async Task Index_TryLogIntoProfileWithInvalidEmailValidation_ExpectViewResult()
		{
			//Arrange
			var model = new UserLogInModel()
			{
				CpcmUserEmail = "user1",
				CpcmUserPwd = "Kek_1_1_Kek"
			};

			//Act
			var validationContext = new ValidationContext(model);
			var validationResults = new List<ValidationResult>();
			Validator.TryValidateObject(model, validationContext, validationResults, true);
			if (validationResults.Any())
			{
				foreach (var validationResult in validationResults)
				{
					controller.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
				}
			}
			var actionResult = await controller.Index(model);

			//Arrange
			if(actionResult is ViewResult result)
			{
				result.ViewName.Should().Be("Index");
			}
		}

		[Fact]
		public async Task Index_TryLogIntoProfileWithInvalidPasswordValidation_ExpectViewResult()
		{
			//Arrange
			var model = new UserLogInModel()
			{
				CpcmUserEmail = "user1@example.com",
				CpcmUserPwd = ""
			};

			//Act
			var validationContext = new ValidationContext(model);
			var validationResults = new List<ValidationResult>();
			Validator.TryValidateObject(model, validationContext, validationResults, true);
			if (validationResults.Any())
			{
				foreach (var validationResult in validationResults)
				{
					controller.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
				}
			}
			var actionResult = await controller.Index(model);

			//Arrange
			if (actionResult is ViewResult result)
			{
				result.ViewName.Should().Be("Index");
			}
		}

		//[Fact]
		//public async Task Index_TryLogWhileLoggedIn_ExpectRedirectToUserControllerIndex()
		//{
		//	//Arrange
		//	var model = new UserLogInModel()
		//	{
		//		CpcmUserEmail = "user1@example.com",
		//		CpcmUserPwd = "Kek_1_1_Kek"
		//	};

		//	//Act
		//	List<Claim> claims = GetUserClaims(users[0]);
		//	var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
		//	await controller.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
		//	var actionResult = controller.Index(model);

		//	//Arrange
		//}

		#region Вспомогательные методы
		private List<Claim> GetUserClaims(CpcmUser user)
		{
			List<Claim> returnClaims = new List<Claim> { new Claim("CpcmUserId", user.CpcmUserId.ToString()) };
			CpcmRole userRole = user.CpcmUserRoleNavigation;
			Type type = userRole.GetType();
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (PropertyInfo property in properties)
			{
				if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(string))
				{
					returnClaims.Add(new Claim(property.Name, property.GetValue(userRole).ToString()));
				}
			}
			
			return returnClaims;
		}
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

		private static FormFile GetFile(out FileStream stream, string useFileContentType = null, string path = "default.png")
		{
			FormFile file;
			Directory.GetCurrentDirectory();
			stream = File.OpenRead(path);

			string type = "";
			if (useFileContentType == null)
			{
				var provider = new FileExtensionContentTypeProvider();
				if (!provider.TryGetContentType(stream.Name, out var contentType))
				{
					type = "image/png";
				}
				else
				{
					type = contentType;
				}
			}
			else
			{
				type = useFileContentType;
			}
			file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
			{
				Headers = new HeaderDictionary(),
				ContentType = type
			};

			return file;
		}

		public void Dispose()
		{
			context.Dispose();
		}

		#endregion Вспомогательные методы
	}
}