using System.ComponentModel.DataAnnotations;

namespace ProniaNew.ViewModels
{
	public class LoginVM
	{
		[Required]
		[MinLength(3)]
		public string UsernameorEmail { get; set; }
		[Required]
		[MinLength(6)]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		public bool isRemembered { get; set; }
	}
}
