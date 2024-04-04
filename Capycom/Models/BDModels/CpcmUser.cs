using Capycom.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capycom;

public partial class CpcmUser
{
    public Guid CpcmUserId { get; set; }

    public string CpcmUserEmail { get; set; } = null!;

    public string CpcmUserTelNum { get; set; } = null!;

    public byte[] CpcmUserPwdHash { get; set; } = null!;

    public string CpcmUserSalt { get; set; } = null!;

    public string? CpcmUserAbout { get; set; }

    public Guid? CpcmUserCity { get; set; }

    public string? CpcmUserSite { get; set; }

    public string? CpcmUserBooks { get; set; }

    public string? CpcmUserFilms { get; set; }

    public string? CpcmUserMusics { get; set; }

    public Guid? CpcmUserSchool { get; set; }

    public Guid? CpcmUserUniversity { get; set; }

    public string? CpcmUserImagePath { get; set; }

    public string? CpcmUserCoverPath { get; set; }

    public string? CpcmUserNickName { get; set; }

    public string CpcmUserFirstName { get; set; } = null!;

    public string CpcmUserSecondName { get; set; } = null!;

    public string? CpcmUserAdditionalName { get; set; }

    public int CpcmUserRole { get; set; }

    public bool CpcmUserBanned { get; set; }

    public bool CpcmIsDeleted { get; set; }

    public virtual ICollection<CpcmComment> CpcmComments { get; set; } = new List<CpcmComment>();

    public virtual ICollection<CpcmGroupfollower> CpcmGroupfollowers { get; set; } = new List<CpcmGroupfollower>();

    public virtual CpcmCity? CpcmUserCityNavigation { get; set; }
	[ValidateNever]
	public virtual CpcmRole CpcmUserRoleNavigation { get; set; } = null!;

    public virtual CpcmSchool? CpcmUserSchoolNavigation { get; set; }

    public virtual CpcmUniversity? CpcmUserUniversityNavigation { get; set; }

    public virtual ICollection<CpcmUserfollower> CpcmUserfollowerCpcmFollowers { get; set; } = new List<CpcmUserfollower>();

    public virtual ICollection<CpcmUserfollower> CpcmUserfollowerCpcmUsers { get; set; } = new List<CpcmUserfollower>();

    public virtual ICollection<CpcmUserfriend> CpcmUserfriendCmcpFriends { get; set; } = new List<CpcmUserfriend>();

    public virtual ICollection<CpcmUserfriend> CpcmUserfriendCmcpUsers { get; set; } = new List<CpcmUserfriend>();

    public virtual ICollection<CpcmPost> CpcmPosts { get; set; } = new List<CpcmPost>();

    public virtual ICollection<CpcmPost> CpcmPostsNavigation { get; set; } = new List<CpcmPost>();



	[NotMapped]
	public FriendStatusEnum IsFriend { get; set; }
	[NotMapped]
	public bool IsFollowing { get; set; }
}
