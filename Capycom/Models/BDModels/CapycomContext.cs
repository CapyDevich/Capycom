﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Serilog;
namespace Capycom;

public partial class CapycomContext : DbContext
{
	public CapycomContext()
	{
	}

	public CapycomContext(DbContextOptions<CapycomContext> options)
		: base(options)
	{
	}

	public virtual DbSet<CpcmCity> CpcmCities { get; set; }

	public virtual DbSet<CpcmComment> CpcmComments { get; set; }

	public virtual DbSet<CpcmGroup> CpcmGroups { get; set; }

	public virtual DbSet<CpcmGroupRole> CpcmGroupRoles { get; set; }

	public virtual DbSet<CpcmGroupfollower> CpcmGroupfollowers { get; set; }

	public virtual DbSet<CpcmGroupsubject> CpcmGroupsubjects { get; set; }

	public virtual DbSet<CpcmImage> CpcmImages { get; set; }

	public virtual DbSet<CpcmLogEvent> CpcmLogEvents { get; set; }

	public virtual DbSet<CpcmPost> CpcmPosts { get; set; }

	public virtual DbSet<CpcmPostrepost> CpcmPostreposts { get; set; }

	public virtual DbSet<CpcmRole> CpcmRoles { get; set; }

	public virtual DbSet<CpcmSchool> CpcmSchools { get; set; }

	public virtual DbSet<CpcmUniversity> CpcmUniversities { get; set; }

	public virtual DbSet<CpcmUser> CpcmUsers { get; set; }

	public virtual DbSet<CpcmUserfollower> CpcmUserfollowers { get; set; }

	public virtual DbSet<CpcmUserfriend> CpcmUserfriends { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		//optionsBuilder.UseLazyLoadingProxies();
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<CpcmCity>(entity =>
		{
			entity.ToTable("CPCM_CITIES");

			entity.Property(e => e.CpcmCityId)
				.ValueGeneratedNever()
				.HasColumnName("CPCM_CityID");
			entity.Property(e => e.CpcmCityName)
				.HasMaxLength(64)
				.HasColumnName("CPCM_CityName");
		});

		modelBuilder.Entity<CpcmComment>(entity =>
		{
			entity.ToTable("CPCM_COMMENTS", tb => tb.HasTrigger("DeleteCommentImageTirgger"));

			entity.Property(e => e.CpcmCommentId)
				.ValueGeneratedNever()
				.HasColumnName("CPCM_CommentID");
			entity.Property(e => e.CpcmCommentBanned).HasColumnName("CPCM_CommentBanned");
			entity.Property(e => e.CpcmCommentCreationDate)
				.HasColumnType("datetime")
				.HasColumnName("CPCM_CommentCreationDate");
			entity.Property(e => e.CpcmCommentFather).HasColumnName("CPCM_CommentFather");
			entity.Property(e => e.CpcmCommentText).HasColumnName("CPCM_CommentText");
			entity.Property(e => e.CpcmIsDeleted).HasColumnName("CPCM_IsDeleted");
			entity.Property(e => e.CpcmPostId).HasColumnName("CPCM_PostID");
			entity.Property(e => e.CpcmUserId).HasColumnName("CPCM_UserID");

			entity.HasOne(d => d.CpcmCommentFatherNavigation).WithMany(p => p.InverseCpcmCommentFatherNavigation)
				.HasForeignKey(d => d.CpcmCommentFather)
				.HasConstraintName("FK_CPCM_COMMENTS_CPCM_COMMENTS");

			entity.HasOne(d => d.CpcmPost).WithMany(p => p.CpcmComments)
				.HasForeignKey(d => d.CpcmPostId)
				.HasConstraintName("FK_CPCM_COMMENTS_CPCM_POSTS");

			entity.HasOne(d => d.CpcmUser).WithMany(p => p.CpcmComments)
				.HasForeignKey(d => d.CpcmUserId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK_CPCM_COMMENTS_CPCM_USERS");
		});

		modelBuilder.Entity<CpcmGroup>(entity =>
		{
			entity.ToTable("CPCM_GROUPS");

			entity.HasIndex(e => e.CpcmGroupNickName, "IX_CPCM_GROUPS").IsUnique();

			entity.Property(e => e.CpcmGroupId)
				.ValueGeneratedNever()
				.HasColumnName("CPCM_GroupID");
			entity.Property(e => e.CpcmGroupAbout).HasColumnName("CPCM_GroupAbout");
			entity.Property(e => e.CpcmGroupBanned).HasColumnName("CPCM_GroupBanned");
			entity.Property(e => e.CpcmGroupCity).HasColumnName("CPCM_GroupCity");
			entity.Property(e => e.CpcmGroupCovet).HasColumnName("CPCM_GroupCovet");
			entity.Property(e => e.CpcmGroupImage).HasColumnName("CPCM_GroupImage");
			entity.Property(e => e.CpcmGroupName).HasColumnName("CPCM_GroupName");
			entity.Property(e => e.CpcmGroupNickName)
				.HasMaxLength(128)
				.HasColumnName("CPCM_GroupNickName");
			entity.Property(e => e.CpcmGroupSubject).HasColumnName("CPCM_GroupSubject");
			entity.Property(e => e.CpcmGroupTelNum).HasColumnName("CPCM_GroupTelNum");
			entity.Property(e => e.CpcmIsDeleted).HasColumnName("CPCM_IsDeleted");

			entity.HasOne(d => d.CpcmGroupCityNavigation).WithMany(p => p.CpcmGroups)
				.HasForeignKey(d => d.CpcmGroupCity)
				.HasConstraintName("FK_CPCM_GROUPS_CPCM_CITIES");

			entity.HasOne(d => d.CpcmGroupSubjectNavigation).WithMany(p => p.CpcmGroups)
				.HasForeignKey(d => d.CpcmGroupSubject)
				.HasConstraintName("FK_CPCM_GROUPS_CPCM_GROUPSUBJECTS");
		});

		modelBuilder.Entity<CpcmGroupRole>(entity =>
		{
			entity.HasKey(e => e.CpcmRoleId);

			entity.ToTable("CPCM_GroupRoles");

			entity.Property(e => e.CpcmRoleId)
				.ValueGeneratedNever()
				.HasColumnName("CPCM_RoleID");
			entity.Property(e => e.CpcmCanDelPost).HasColumnName("CPCM_CanDelPost");
			entity.Property(e => e.CpcmCanEditGroup).HasColumnName("CPCM_CanEditGroup");
			entity.Property(e => e.CpcmCanEditPost).HasColumnName("CPCm_CanEditPost");
			entity.Property(e => e.CpcmCanMakePost).HasColumnName("CPCM_CanMakePost");
			entity.Property(e => e.CpcmRoleName)
				.HasMaxLength(64)
				.HasColumnName("CPCM_RoleName");
		});

		modelBuilder.Entity<CpcmGroupfollower>(entity =>
		{
			entity.HasKey(e => new { e.CpcmGroupId, e.CpcmUserId });

			entity.ToTable("CPCM_GROUPFOLLOWERS");

			entity.Property(e => e.CpcmGroupId).HasColumnName("CPCM_GroupID");
			entity.Property(e => e.CpcmUserId).HasColumnName("CPCM_UserID");
			entity.Property(e => e.CpcmUserRole).HasColumnName("CPCM_UserRole");

			entity.HasOne(d => d.CpcmGroup).WithMany(p => p.CpcmGroupfollowers)
				.HasForeignKey(d => d.CpcmGroupId)
				.HasConstraintName("FK_CPCM_GROUPFOLLOWERS_CPCM_GROUPS");

			entity.HasOne(d => d.CpcmUser).WithMany(p => p.CpcmGroupfollowers)
				.HasForeignKey(d => d.CpcmUserId)
				.HasConstraintName("FK_CPCM_GROUPFOLLOWERS_CPCM_USERS");

			entity.HasOne(d => d.CpcmUserRoleNavigation).WithMany(p => p.CpcmGroupfollowers)
				.HasForeignKey(d => d.CpcmUserRole)
				.HasConstraintName("FK_CPCM_GROUPFOLLOWERS_CPCM_GroupRoles");
		});

		modelBuilder.Entity<CpcmGroupsubject>(entity =>
		{
			entity.HasKey(e => e.CpcmSubjectId);

			entity.ToTable("CPCM_GROUPSUBJECTS");

			entity.Property(e => e.CpcmSubjectId)
				.ValueGeneratedNever()
				.HasColumnName("CPCM_SubjectID");
			entity.Property(e => e.CpcmSubjectName)
				.HasMaxLength(64)
				.HasColumnName("CPCM_SubjectName");
		});

		modelBuilder.Entity<CpcmImage>(entity =>
		{
			entity.ToTable("CPCM_IMAGES");

			entity.Property(e => e.CpcmImageId)
				.ValueGeneratedNever()
				.HasColumnName("CPCM_ImageID");
			entity.Property(e => e.CpcmCommentId).HasColumnName("CPCM_CommentID");
			entity.Property(e => e.CpcmImageOrder).HasColumnName("CPCM_ImageOrder");
			entity.Property(e => e.CpcmImagePath).HasColumnName("CPCM_ImagePath");
			entity.Property(e => e.CpcmPostId).HasColumnName("CPCM_PostID");

			entity.HasOne(d => d.CpcmComment).WithMany(p => p.CpcmImages)
				.HasForeignKey(d => d.CpcmCommentId)
				.HasConstraintName("FK_CPCM_IMAGES_CPCM_COMMENTS");

			entity.HasOne(d => d.CpcmPost).WithMany(p => p.CpcmImages)
				.HasForeignKey(d => d.CpcmPostId)
				.HasConstraintName("FK_CPCM_IMAGES_CPCM_POSTS");
		});

		modelBuilder.Entity<CpcmLogEvent>(entity =>
		{
			entity.ToTable("CPCM_LogEvents");

			entity.Property(e => e.TimeStamp).HasColumnType("datetime");
		});

		modelBuilder.Entity<CpcmPost>(entity =>
		{
			entity.ToTable("CPCM_POSTS", tb => tb.HasTrigger("DeletePostImageTirgger"));

			entity.Property(e => e.CpcmPostId)
				.ValueGeneratedNever()
				.HasColumnName("CPCM_PostID");
			entity.Property(e => e.CpcmGroupId).HasColumnName("CPCM_GroupID");
			entity.Property(e => e.CpcmIsDeleted).HasColumnName("CPCM_IsDeleted");
			entity.Property(e => e.CpcmPostBanned).HasColumnName("CPCM_PostBanned");
			entity.Property(e => e.CpcmPostCreationDate)
				.HasColumnType("datetime")
				.HasColumnName("CPCM_PostCreationDate");
			entity.Property(e => e.CpcmPostFather).HasColumnName("CPCM_PostFather");
			entity.Property(e => e.CpcmPostPublishedDate)
				.HasColumnType("datetime")
				.HasColumnName("CPCM_PostPublishedDate");
			entity.Property(e => e.CpcmPostText).HasColumnName("CPCM_PostText");
			entity.Property(e => e.CpcmUserId).HasColumnName("CPCM_UserID");

			entity.HasOne(d => d.CpcmPostFatherNavigation).WithMany(p => p.InverseCpcmPostFatherNavigation)
				.HasForeignKey(d => d.CpcmPostFather)
				.HasConstraintName("FK_CPCM_POSTS_CPCM_POSTS");

			entity.HasMany(d => d.CpcmUsers).WithMany(p => p.CpcmPosts)
				.UsingEntity<Dictionary<string, object>>(
					"CpcmPostlike",
					r => r.HasOne<CpcmUser>().WithMany()
						.HasForeignKey("CpcmUserId")
						.HasConstraintName("FK_CPCM_POSTLIKES_CPCM_USERS"),
					l => l.HasOne<CpcmPost>().WithMany()
						.HasForeignKey("CpcmPostId")
						.HasConstraintName("FK_CPCM_POSTLIKES_CPCM_POSTS"),
					j =>
					{
						j.HasKey("CpcmPostId", "CpcmUserId");
						j.ToTable("CPCM_POSTLIKES");
						j.IndexerProperty<Guid>("CpcmPostId").HasColumnName("CPCM_PostID");
						j.IndexerProperty<Guid>("CpcmUserId").HasColumnName("CPCM_UserID");
					});
		});

		modelBuilder.Entity<CpcmPostrepost>(entity =>
		{
			entity.HasKey(e => new { e.CpcmPostId, e.CpcmUserId });

			entity.ToTable("CPCM_POSTREPOSTS");

			entity.Property(e => e.CpcmPostId).HasColumnName("CPCM_PostID");
			entity.Property(e => e.CpcmUserId).HasColumnName("CPCM_UserID");
			entity.Property(e => e.CpcmIsDeleted).HasColumnName("CPCM_IsDeleted");

			entity.HasOne(d => d.CpcmPost).WithMany(p => p.CpcmPostreposts)
				.HasForeignKey(d => d.CpcmPostId)
				.HasConstraintName("FK_CPCM_POSTREPOSTS_CPCM_POSTS");

			entity.HasOne(d => d.CpcmUser).WithMany(p => p.CpcmPostreposts)
				.HasForeignKey(d => d.CpcmUserId)
				.HasConstraintName("FK_CPCM_POSTREPOSTS_CPCM_USERS");
		});

		modelBuilder.Entity<CpcmRole>(entity =>
		{
			entity.ToTable("CPCM_ROLES");

			entity.Property(e => e.CpcmRoleId)
				.ValueGeneratedNever()
				.HasColumnName("CPCM_RoleID");
			entity.Property(e => e.CpcmCanBanGroupsPost).HasColumnName("CPCM_CanBanGroupsPost");
			entity.Property(e => e.CpcmCanBanUsers).HasColumnName("CPCM_CanBanUsers");
			entity.Property(e => e.CpcmCanBanUsersComment).HasColumnName("CPCM_CanBanUsersComment");
			entity.Property(e => e.CpcmCanBanUsersPost).HasColumnName("CPCM_CanBanUsersPost");
			entity.Property(e => e.CpcmCanEditGroupRoles).HasColumnName("CPCM_CanEditGroupRoles");
			entity.Property(e => e.CpcmCanEditGroups).HasColumnName("CPCM_CanEditGroups");
			entity.Property(e => e.CpcmCanEditGroupsPost).HasColumnName("CPCM_CanEditGroupsPost");
			entity.Property(e => e.CpcmCanEditRoles).HasColumnName("CPCM_CanEditRoles");
			entity.Property(e => e.CpcmCanEditUsers).HasColumnName("CPCM_CanEditUsers");
			entity.Property(e => e.CpcmCanEditUsersPost).HasColumnName("CPCM_CanEditUsersPost");
			entity.Property(e => e.CpcmCanBanGroups).HasColumnName("CPCM_CanBanGroups");
			entity.Property(e => e.CpcmRoleName)
				.HasMaxLength(64)
				.HasColumnName("CPCM_RoleName");
		});

		modelBuilder.Entity<CpcmSchool>(entity =>
		{
			entity.HasKey(e => e.CpcmSchooldId);

			entity.ToTable("CPCM_SCHOOLS");

			entity.Property(e => e.CpcmSchooldId)
				.ValueGeneratedNever()
				.HasColumnName("CPCM_SchooldID");
			entity.Property(e => e.CpcmSchoolName)
				.HasMaxLength(64)
				.HasColumnName("CPCM_SchoolName");
		});

		modelBuilder.Entity<CpcmUniversity>(entity =>
		{
			entity.ToTable("CPCM_UNIVERSITIES");

			entity.Property(e => e.CpcmUniversityId)
				.ValueGeneratedNever()
				.HasColumnName("CPCM_UniversityID");
			entity.Property(e => e.CpcmUniversityName)
				.HasMaxLength(64)
				.HasColumnName("CPCM_UniversityName");
		});

		modelBuilder.Entity<CpcmUser>(entity =>
		{
			entity.ToTable("CPCM_USERS", tb => tb.HasTrigger("DeleteUserTirgger"));

			entity.HasIndex(e => e.CpcmUserEmail, "IX_CPCM_USERS").IsUnique();

			entity.HasIndex(e => e.CpcmUserTelNum, "IX_CPCM_USERS_1").IsUnique();

			entity.HasIndex(e => e.CpcmUserId, "IX_CPCM_USERS_2").IsUnique();

			entity.HasIndex(e => e.CpcmUserNickName, "IX_CPCM_USERS_3").IsUnique();

			entity.Property(e => e.CpcmUserId)
				.ValueGeneratedNever()
				.HasColumnName("CPCM_UserId");
			entity.Property(e => e.CpcmIsDeleted).HasColumnName("CPCM_IsDeleted");
			entity.Property(e => e.CpcmUserAbout).HasColumnName("CPCM_UserAbout");
			entity.Property(e => e.CpcmUserAdditionalName).HasColumnName("CPCM_UserAdditionalName");
			entity.Property(e => e.CpcmUserBanned).HasColumnName("CPCM_UserBanned");
			entity.Property(e => e.CpcmUserBooks).HasColumnName("CPCM_UserBooks");
			entity.Property(e => e.CpcmUserCity).HasColumnName("CPCM_UserCity");
			entity.Property(e => e.CpcmUserCoverPath).HasColumnName("CPCM_UserCoverPath");
			entity.Property(e => e.CpcmUserEmail)
				.HasMaxLength(64)
				.HasColumnName("CPCM_UserEmail");
			entity.Property(e => e.CpcmUserFilms).HasColumnName("CPCM_UserFilms");
			entity.Property(e => e.CpcmUserFirstName).HasColumnName("CPCM_UserFirstName");
			entity.Property(e => e.CpcmUserImagePath).HasColumnName("CPCM_UserImagePath");
			entity.Property(e => e.CpcmUserMusics).HasColumnName("CPCM_UserMusics");
			entity.Property(e => e.CpcmUserNickName)
				.HasMaxLength(128)
				.HasColumnName("CPCM_UserNickName");
			entity.Property(e => e.CpcmUserPwdHash)
				.HasMaxLength(32)
				.HasColumnName("CPCM_UserPwdHash");
			entity.Property(e => e.CpcmUserRole).HasColumnName("CPCM_UserRole");
			entity.Property(e => e.CpcmUserSalt)
				.HasMaxLength(16)
				.HasColumnName("CPCM_UserSalt");
			entity.Property(e => e.CpcmUserSchool).HasColumnName("CPCM_UserSchool");
			entity.Property(e => e.CpcmUserSecondName).HasColumnName("CPCM_UserSecondName");
			entity.Property(e => e.CpcmUserSite).HasColumnName("CPCM_UserSite");
			entity.Property(e => e.CpcmUserTelNum)
				.HasMaxLength(64)
				.HasColumnName("CPCM_UserTelNum");
			entity.Property(e => e.CpcmUserUniversity).HasColumnName("CPCM_UserUniversity");

			entity.HasOne(d => d.CpcmUserCityNavigation).WithMany(p => p.CpcmUsers)
				.HasForeignKey(d => d.CpcmUserCity)
				.OnDelete(DeleteBehavior.SetNull)
				.HasConstraintName("FK_CPCM_USERS_CPCM_CITIES");

			entity.HasOne(d => d.CpcmUserRoleNavigation).WithMany(p => p.CpcmUsers)
				.HasForeignKey(d => d.CpcmUserRole)
				.HasConstraintName("FK_CPCM_USERS_CPCM_ROLES");

			entity.HasOne(d => d.CpcmUserSchoolNavigation).WithMany(p => p.CpcmUsers)
				.HasForeignKey(d => d.CpcmUserSchool)
				.OnDelete(DeleteBehavior.SetNull)
				.HasConstraintName("FK_CPCM_USERS_CPCM_SCHOOLS");

			entity.HasOne(d => d.CpcmUserUniversityNavigation).WithMany(p => p.CpcmUsers)
				.HasForeignKey(d => d.CpcmUserUniversity)
				.OnDelete(DeleteBehavior.SetNull)
				.HasConstraintName("FK_CPCM_USERS_CPCM_UNIVERSITIES");
		});

		modelBuilder.Entity<CpcmUserfollower>(entity =>
		{
			entity.HasKey(e => e.CpcmFollowersId);

			entity.ToTable("CPCM_USERFOLLOWERS");

			entity.Property(e => e.CpcmFollowersId)
				.ValueGeneratedNever()
				.HasColumnName("CPCM_FollowersID");
			entity.Property(e => e.CpcmFollowerId).HasColumnName("CPCM_FollowerID");
			entity.Property(e => e.CpcmUserId).HasColumnName("CPCM_UserID");

			entity.HasOne(d => d.CpcmFollower).WithMany(p => p.CpcmUserfollowerCpcmFollowers)
				.HasForeignKey(d => d.CpcmFollowerId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK_CPCM_USERFOLLOWERS_CPCM_USERS1");

			entity.HasOne(d => d.CpcmUser).WithMany(p => p.CpcmUserfollowerCpcmUsers)
				.HasForeignKey(d => d.CpcmUserId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK_CPCM_USERFOLLOWERS_CPCM_USERS");
		});

		modelBuilder.Entity<CpcmUserfriend>(entity =>
		{
			entity.HasKey(e => new { e.CmcpUserId, e.CmcpFriendId });

			entity.ToTable("CPCM_USERFRIENDS");

			entity.Property(e => e.CmcpUserId).HasColumnName("CMCP_UserID");
			entity.Property(e => e.CmcpFriendId).HasColumnName("CMCP_FriendID");
			entity.Property(e => e.CpcmFriendRequestStatus).HasColumnName("CPCM_FriendRequestStatus");

			entity.HasOne(d => d.CmcpFriend).WithMany(p => p.CpcmUserfriendCmcpFriends)
				.HasForeignKey(d => d.CmcpFriendId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK_CPCM_USERFRIENDS_CPCM_USERS1");

			entity.HasOne(d => d.CmcpUser).WithMany(p => p.CpcmUserfriendCmcpUsers)
				.HasForeignKey(d => d.CmcpUserId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK_CPCM_USERFRIENDS_CPCM_USERS");
		});

		OnModelCreatingPartial(modelBuilder);
	}

	partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		Log.Information("Вызов SaveChangesAsync начат");
		var entries = ChangeTracker.Entries()
				.Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);



		foreach (var entry in entries)
		{
			var currentValues = entry.CurrentValues;
			var values = new Dictionary<string, object>();

			foreach (var property in entry.Metadata.GetProperties())
			{
				var propertyName = property.Name;
				var currentValue = currentValues[propertyName];
				values[propertyName] = currentValue;
			}

			if (entry.State == EntityState.Added)
			{
				// Если сущность новая, логируем все текущие значения
				Log.Information("Создана новая сущнсоть {EntityName} со значениями: {Values}",
					entry.Entity.GetType().Name,
					values);
			}
			else if (entry.State == EntityState.Modified)
			{
				// Если сущность была изменена, логируем только измененные значения
				var originalValues = entry.OriginalValues.Clone();
				var changedValues = new Dictionary<string, object>();

				foreach (var property in entry.Metadata.GetProperties())
				{
					var propertyName = property.Name;
					var originalValue = originalValues[propertyName];
					var currentValue = currentValues[propertyName];

					if (!object.Equals(originalValue, currentValue))
					{
						changedValues[propertyName] = new { Original = originalValue, Current = currentValue };
					}
				}

				Log.Information("Сущность {EntityName} изменила значения: {Values}",
					entry.Entity.GetType().Name,
					changedValues);
			}
		}
		var result = await base.SaveChangesAsync(cancellationToken);
		Log.Information("Вызов SaveChangesAsync прошел");
		return result;
	}
}
