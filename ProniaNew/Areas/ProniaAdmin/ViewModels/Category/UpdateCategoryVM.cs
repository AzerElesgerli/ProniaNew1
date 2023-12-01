﻿using System.ComponentModel.DataAnnotations;

namespace ProniaNew.Areas.ProniaAdmin.ViewModels.Category
{
	public class UpdateCategoryVM
	{
		[Required(ErrorMessage = "Name is required")]
		[MaxLength(25, ErrorMessage = "Name's max length is 25")]
		public string Name { get; set; }
	}
}
