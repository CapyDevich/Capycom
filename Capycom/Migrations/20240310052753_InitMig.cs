using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capycom.Migrations
{
    /// <inheritdoc />
    public partial class InitMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "CPCM_CITIES",
            //    columns: table => new
            //    {
            //        CPCM_CityID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_CityName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CPCM_CITIES", x => x.CPCM_CityID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CPCM_GroupRoles",
            //    columns: table => new
            //    {
            //        CPCM_RoleID = table.Column<int>(type: "int", nullable: false),
            //        CPCM_RoleName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
            //        CPCM_CanEditGroup = table.Column<bool>(type: "bit", nullable: false),
            //        CPCM_CanMakePost = table.Column<bool>(type: "bit", nullable: false),
            //        CPCM_CanDelPost = table.Column<bool>(type: "bit", nullable: false),
            //        CPCm_CanEditPost = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CPCM_GroupRoles", x => x.CPCM_RoleID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CPCM_GROUPSUBJECTS",
            //    columns: table => new
            //    {
            //        CPCM_SubjectID = table.Column<int>(type: "int", nullable: false),
            //        CPCM_SubjectName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CPCM_GROUPSUBJECTS", x => x.CPCM_SubjectID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CPCM_POSTS",
            //    columns: table => new
            //    {
            //        CPCM_PostID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        CPCM_GroupID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        CPCM_PostText = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CPCM_PostFather = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        CPCM_PostCreationDate = table.Column<DateTime>(type: "datetime", nullable: false),
            //        CPCM_PostPublishedDate = table.Column<DateTime>(type: "datetime", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CPCM_POSTS", x => x.CPCM_PostID);
            //        table.ForeignKey(
            //            name: "FK_CPCM_POSTS_CPCM_POSTS",
            //            column: x => x.CPCM_PostFather,
            //            principalTable: "CPCM_POSTS",
            //            principalColumn: "CPCM_PostID");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CPCM_ROLES",
            //    columns: table => new
            //    {
            //        CPCM_RoleID = table.Column<int>(type: "int", nullable: false),
            //        CPCM_RoleName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
            //        CPCM_CanEditUsers = table.Column<bool>(type: "bit", nullable: false),
            //        CPCM_CanEditGroups = table.Column<bool>(type: "bit", nullable: false),
            //        CPCM_CanEditRoles = table.Column<bool>(type: "bit", nullable: false),
            //        CPCM_CanDelUsersPosts = table.Column<bool>(type: "bit", nullable: false),
            //        CPCM_CanDelUsersComments = table.Column<bool>(type: "bit", nullable: false),
            //        CPCM_CanDelGroupsPosts = table.Column<bool>(type: "bit", nullable: false),
            //        CPCM_CanAddPost = table.Column<bool>(type: "bit", nullable: false),
            //        CPCM_CanAddGroups = table.Column<bool>(type: "bit", nullable: false),
            //        CPCM_CanAddComments = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CPCM_ROLES", x => x.CPCM_RoleID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CPCM_SCHOOLS",
            //    columns: table => new
            //    {
            //        CPCM_SchooldID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_SchoolName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CPCM_SCHOOLS", x => x.CPCM_SchooldID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CPCM_UNIVERSITIES",
            //    columns: table => new
            //    {
            //        CPCM_UniversityID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_UniversityName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CPCM_UNIVERSITIES", x => x.CPCM_UniversityID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CPCM_GROUPS",
            //    columns: table => new
            //    {
            //        CPCM_GroupID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        CPCM_GroupAbout = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CPCM_GroupImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CPCM_GroupCovet = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CPCM_GroupSubject = table.Column<int>(type: "int", nullable: false),
            //        CPCM_GroupCity = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        CPCM_GroupTelNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CPCM_GroupNickName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CPCM_GROUPS", x => x.CPCM_GroupID);
            //        table.ForeignKey(
            //            name: "FK_CPCM_GROUPS_CPCM_GROUPSUBJECTS",
            //            column: x => x.CPCM_GroupSubject,
            //            principalTable: "CPCM_GROUPSUBJECTS",
            //            principalColumn: "CPCM_SubjectID",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CPCM_COMMENTS",
            //    columns: table => new
            //    {
            //        CPCM_CommentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_PostID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_CommentText = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CPCM_UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_CommentFather = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        CPCM_CommentCreationDate = table.Column<DateTime>(type: "datetime", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CPCM_COMMENTS", x => x.CPCM_CommentID);
            //        table.ForeignKey(
            //            name: "FK_CPCM_COMMENTS_CPCM_COMMENTS",
            //            column: x => x.CPCM_CommentFather,
            //            principalTable: "CPCM_COMMENTS",
            //            principalColumn: "CPCM_CommentID");
            //        table.ForeignKey(
            //            name: "FK_CPCM_COMMENTS_CPCM_POSTS",
            //            column: x => x.CPCM_PostID,
            //            principalTable: "CPCM_POSTS",
            //            principalColumn: "CPCM_PostID",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CPCM_USERS",
            //    columns: table => new
            //    {
            //        CPCM_UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_UserEmail = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
            //        CPCM_UserTelNum = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
            //        CPCM_UserPwdHash = table.Column<byte[]>(type: "varbinary(32)", maxLength: 32, nullable: false),
            //        CPCM_UserSalt = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
            //        CPCM_UserAbout = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CPCM_UserCity = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        CPCM_UserSite = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CPCM_UserBooks = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CPCM_UserFilms = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CPCM_UserMusics = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CPCM_UserSchool = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        CPCM_UserUniversity = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        CPCM_UserImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CPCM_UserCoverPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CPCM_UserNickName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
            //        CPCM_UserFirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        CPCM_UserSecondName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        CPCM_UserAdditionalName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CPCM_UserRole = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CPCM_USERS", x => x.CPCM_UserId);
            //        table.ForeignKey(
            //            name: "FK_CPCM_USERS_CPCM_CITIES",
            //            column: x => x.CPCM_UserCity,
            //            principalTable: "CPCM_CITIES",
            //            principalColumn: "CPCM_CityID",
            //            onDelete: ReferentialAction.SetNull);
            //        table.ForeignKey(
            //            name: "FK_CPCM_USERS_CPCM_ROLES",
            //            column: x => x.CPCM_UserRole,
            //            principalTable: "CPCM_ROLES",
            //            principalColumn: "CPCM_RoleID",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_CPCM_USERS_CPCM_SCHOOLS",
            //            column: x => x.CPCM_UserSchool,
            //            principalTable: "CPCM_SCHOOLS",
            //            principalColumn: "CPCM_SchooldID",
            //            onDelete: ReferentialAction.SetNull);
            //        table.ForeignKey(
            //            name: "FK_CPCM_USERS_CPCM_UNIVERSITIES",
            //            column: x => x.CPCM_UserUniversity,
            //            principalTable: "CPCM_UNIVERSITIES",
            //            principalColumn: "CPCM_UniversityID",
            //            onDelete: ReferentialAction.SetNull);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CPCM_IMAGES",
            //    columns: table => new
            //    {
            //        CPCM_ImageID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_CommentID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        CPCM_PostID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        CPCM_ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        CPCM_ImageOrder = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CPCM_IMAGES", x => x.CPCM_ImageID);
            //        table.ForeignKey(
            //            name: "FK_CPCM_IMAGES_CPCM_COMMENTS",
            //            column: x => x.CPCM_CommentID,
            //            principalTable: "CPCM_COMMENTS",
            //            principalColumn: "CPCM_CommentID");
            //        table.ForeignKey(
            //            name: "FK_CPCM_IMAGES_CPCM_POSTS",
            //            column: x => x.CPCM_PostID,
            //            principalTable: "CPCM_POSTS",
            //            principalColumn: "CPCM_PostID");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CPCM_GROUPFOLLOWERS",
            //    columns: table => new
            //    {
            //        CPCM_GroupID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_UserRole = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CPCM_GROUPFOLLOWERS", x => new { x.CPCM_GroupID, x.CPCM_UserID });
            //        table.ForeignKey(
            //            name: "FK_CPCM_GROUPFOLLOWERS_CPCM_GROUPS",
            //            column: x => x.CPCM_GroupID,
            //            principalTable: "CPCM_GROUPS",
            //            principalColumn: "CPCM_GroupID",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_CPCM_GROUPFOLLOWERS_CPCM_GroupRoles",
            //            column: x => x.CPCM_UserRole,
            //            principalTable: "CPCM_GroupRoles",
            //            principalColumn: "CPCM_RoleID",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_CPCM_GROUPFOLLOWERS_CPCM_USERS",
            //            column: x => x.CPCM_UserID,
            //            principalTable: "CPCM_USERS",
            //            principalColumn: "CPCM_UserId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CPCM_POSTLIKES",
            //    columns: table => new
            //    {
            //        CPCM_PostID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CPCM_POSTLIKES", x => new { x.CPCM_PostID, x.CPCM_UserID });
            //        table.ForeignKey(
            //            name: "FK_CPCM_POSTLIKES_CPCM_POSTS",
            //            column: x => x.CPCM_PostID,
            //            principalTable: "CPCM_POSTS",
            //            principalColumn: "CPCM_PostID",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_CPCM_POSTLIKES_CPCM_USERS",
            //            column: x => x.CPCM_UserID,
            //            principalTable: "CPCM_USERS",
            //            principalColumn: "CPCM_UserId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CPCM_POSTREPOSTS",
            //    columns: table => new
            //    {
            //        CPCM_PostID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CPCM_POSTREPOSTS", x => new { x.CPCM_PostID, x.CPCM_UserID });
            //        table.ForeignKey(
            //            name: "FK_CPCM_POSTREPOSTS_CPCM_POSTS",
            //            column: x => x.CPCM_PostID,
            //            principalTable: "CPCM_POSTS",
            //            principalColumn: "CPCM_PostID",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_CPCM_POSTREPOSTS_CPCM_USERS",
            //            column: x => x.CPCM_UserID,
            //            principalTable: "CPCM_USERS",
            //            principalColumn: "CPCM_UserId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CPCM_USERFOLLOWERS",
            //    columns: table => new
            //    {
            //        CPCM_FollowersID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_FollowerID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CPCM_USERFOLLOWERS", x => x.CPCM_FollowersID);
            //        table.ForeignKey(
            //            name: "FK_CPCM_USERFOLLOWERS_CPCM_USERS",
            //            column: x => x.CPCM_UserID,
            //            principalTable: "CPCM_USERS",
            //            principalColumn: "CPCM_UserId");
            //        table.ForeignKey(
            //            name: "FK_CPCM_USERFOLLOWERS_CPCM_USERS1",
            //            column: x => x.CPCM_FollowerID,
            //            principalTable: "CPCM_USERS",
            //            principalColumn: "CPCM_UserId");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CPCM_USERFRIENDS",
            //    columns: table => new
            //    {
            //        CMCP_UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CMCP_FriendID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CPCM_FriendRequestStatus = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CPCM_USERFRIENDS", x => new { x.CMCP_UserID, x.CMCP_FriendID });
            //        table.ForeignKey(
            //            name: "FK_CPCM_USERFRIENDS_CPCM_USERS",
            //            column: x => x.CMCP_UserID,
            //            principalTable: "CPCM_USERS",
            //            principalColumn: "CPCM_UserId");
            //        table.ForeignKey(
            //            name: "FK_CPCM_USERFRIENDS_CPCM_USERS1",
            //            column: x => x.CMCP_FriendID,
            //            principalTable: "CPCM_USERS",
            //            principalColumn: "CPCM_UserId");
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_COMMENTS_CPCM_CommentFather",
            //    table: "CPCM_COMMENTS",
            //    column: "CPCM_CommentFather");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_COMMENTS_CPCM_PostID",
            //    table: "CPCM_COMMENTS",
            //    column: "CPCM_PostID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_GROUPFOLLOWERS_CPCM_UserID",
            //    table: "CPCM_GROUPFOLLOWERS",
            //    column: "CPCM_UserID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_GROUPFOLLOWERS_CPCM_UserRole",
            //    table: "CPCM_GROUPFOLLOWERS",
            //    column: "CPCM_UserRole");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_GROUPS",
            //    table: "CPCM_GROUPS",
            //    column: "CPCM_GroupNickName",
            //    unique: true,
            //    filter: "[CPCM_GroupNickName] IS NOT NULL");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_GROUPS_CPCM_GroupSubject",
            //    table: "CPCM_GROUPS",
            //    column: "CPCM_GroupSubject");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_IMAGES_CPCM_CommentID",
            //    table: "CPCM_IMAGES",
            //    column: "CPCM_CommentID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_IMAGES_CPCM_PostID",
            //    table: "CPCM_IMAGES",
            //    column: "CPCM_PostID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_POSTLIKES_CPCM_UserID",
            //    table: "CPCM_POSTLIKES",
            //    column: "CPCM_UserID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_POSTREPOSTS_CPCM_UserID",
            //    table: "CPCM_POSTREPOSTS",
            //    column: "CPCM_UserID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_POSTS_CPCM_PostFather",
            //    table: "CPCM_POSTS",
            //    column: "CPCM_PostFather");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_USERFOLLOWERS_CPCM_FollowerID",
            //    table: "CPCM_USERFOLLOWERS",
            //    column: "CPCM_FollowerID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_USERFOLLOWERS_CPCM_UserID",
            //    table: "CPCM_USERFOLLOWERS",
            //    column: "CPCM_UserID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_USERFRIENDS_CMCP_FriendID",
            //    table: "CPCM_USERFRIENDS",
            //    column: "CMCP_FriendID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_USERS",
            //    table: "CPCM_USERS",
            //    column: "CPCM_UserEmail",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_USERS_1",
            //    table: "CPCM_USERS",
            //    column: "CPCM_UserTelNum",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_USERS_2",
            //    table: "CPCM_USERS",
            //    column: "CPCM_UserId",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_USERS_3",
            //    table: "CPCM_USERS",
            //    column: "CPCM_UserNickName",
            //    unique: true,
            //    filter: "[CPCM_UserNickName] IS NOT NULL");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_USERS_CPCM_UserCity",
            //    table: "CPCM_USERS",
            //    column: "CPCM_UserCity");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_USERS_CPCM_UserRole",
            //    table: "CPCM_USERS",
            //    column: "CPCM_UserRole");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_USERS_CPCM_UserSchool",
            //    table: "CPCM_USERS",
            //    column: "CPCM_UserSchool");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CPCM_USERS_CPCM_UserUniversity",
            //    table: "CPCM_USERS",
            //    column: "CPCM_UserUniversity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "CPCM_GROUPFOLLOWERS");

            //migrationBuilder.DropTable(
            //    name: "CPCM_IMAGES");

            //migrationBuilder.DropTable(
            //    name: "CPCM_POSTLIKES");

            //migrationBuilder.DropTable(
            //    name: "CPCM_POSTREPOSTS");

            //migrationBuilder.DropTable(
            //    name: "CPCM_USERFOLLOWERS");

            //migrationBuilder.DropTable(
            //    name: "CPCM_USERFRIENDS");

            //migrationBuilder.DropTable(
            //    name: "CPCM_GROUPS");

            //migrationBuilder.DropTable(
            //    name: "CPCM_GroupRoles");

            //migrationBuilder.DropTable(
            //    name: "CPCM_COMMENTS");

            //migrationBuilder.DropTable(
            //    name: "CPCM_USERS");

            //migrationBuilder.DropTable(
            //    name: "CPCM_GROUPSUBJECTS");

            //migrationBuilder.DropTable(
            //    name: "CPCM_POSTS");

            //migrationBuilder.DropTable(
            //    name: "CPCM_CITIES");

            //migrationBuilder.DropTable(
            //    name: "CPCM_ROLES");

            //migrationBuilder.DropTable(
            //    name: "CPCM_SCHOOLS");

            //migrationBuilder.DropTable(
            //    name: "CPCM_UNIVERSITIES");
        }
    }
}
