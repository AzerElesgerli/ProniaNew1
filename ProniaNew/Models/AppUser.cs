using Microsoft.AspNetCore.Identity;

namespace ProniaNew.Models
{
    public class AppUser: IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Gender { get; set; }
		public List<BasketItem> BasketItems { get; set; }
		public List<Order> Orders { get; set; }

	}
}
