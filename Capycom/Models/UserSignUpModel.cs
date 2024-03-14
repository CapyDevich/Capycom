using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Capycom.Models
{
    public class UserSignUpModel
    {
        public string CpcmUserEmail { get; set; } = null!;

        public string CpcmUserTelNum { get; set; } = null!;

        public string CpcmUserPwd { get; set; } = null!;

        public string CpcmUserPwdConfirm { get; set; } = null!;

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

        [BindNever]
        public int CpcmUserRole { get; set; }

    }
}
