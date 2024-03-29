namespace Capycom.Models
{
	public class UserFilterModel
	{
		//Guid id, Guid? cityId, Guid? schoolId, Guid? universityId, string? firstName, string? secondName, string? additionalName

		public Guid? UserId { get; set; }
		public Guid? CityId { get; set; }
		public Guid? SchoolId { get; set; }
		public Guid? UniversityId { get; set; }
		public string? NickName { get; set; }
		public string? FirstName { get; set; }
		public string? SecondName { get; set; }
		public string? AdditionalName { get; set; }
		public Guid? lastId { get; set; }

	}
}
