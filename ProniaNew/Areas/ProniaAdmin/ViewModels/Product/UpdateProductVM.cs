﻿using ProniaNew.Models;

namespace ProniaNew.Areas.ProniaAdmin.ViewModels
{
    public class UpdateProductVM
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string SKU { get; set; }
        public int CategoryId { get; set; }
        public List<int> TagIds { get; set; }

        
        public List<ProductTag> Tags { get; set; }
    }
}
