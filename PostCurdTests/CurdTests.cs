using Capycom;
using Microsoft.EntityFrameworkCore;
namespace PostCurdTests
{
	public class CurdTests
	{
		private readonly CapycomContext context;

		public CurdTests()
		{
			var options = new DbContextOptionsBuilder<CapycomContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			context = new CapycomContext(options);
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

			//AddPosts User1
			var lastGuid = new Guid("00000000-0000-0000-0000-000000000012");
			var usernum = 0;
			for (int i = 0; i < 18; i++)
			{
				if (i % 6 == 0)
				{
					usernum++;
				}
				var post = new CpcmPost
				{
					CpcmPostId = NextGuid(lastGuid),
					CpcmUserId = users[usernum-1].CpcmUserId,
					CpcmGroupId = null,
					CpcmPostText = $"Post {(i % 6) + 1} by User {usernum}",
					CpcmPostCreationDate = DateTime.UtcNow,
					CpcmPostPublishedDate = null,
					CpcmPostBanned = i%12==0?true:false,
					CpcmIsDeleted = i%12==0?false:true
				};
				context.CpcmPosts.Add(post);
				lastGuid = post.CpcmPostId;
			}

			context.SaveChanges();
			var a = context.CpcmPosts.ToList();
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

		[Fact]
		public async Task Test1()
		{
			Assert.True(true);
		}
	}
}