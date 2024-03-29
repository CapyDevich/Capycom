namespace Capycom.Models
{
	public class GroupFilterModel
	{
		public Guid? UserId { get; set; }
		public string? NickName { get; set; }
		public string? Name { get; set; }
		public int? SubjectID { get; set; }
		public Guid? CityId { get; set; }
				
		public Guid? lastId { get; set; }

	}
}
