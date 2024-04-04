using Capycom.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Capycom.Models
{
    public class CommentAddModel
    {

        //[HiddenInput]
        //public Guid CpcmCommentId { get; set; }
        [Required]
        [HiddenInput]
        public Guid CpcmPostId { get; set; }

        
        
        public string? CpcmCommentText { get; set; }


        public Guid? CpcmUserId { get; set; }

        [HiddenInput]
        public Guid? CpcmCommentFather { get; set; }

        [MaxFileCount(2)]
        public IFormFileCollection? Files { get; set; }
    }
}
