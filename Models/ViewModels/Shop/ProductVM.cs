using StoreTrainee.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StoreTrainee.Models.ViewModels.Shop
{
    public class ProductVM
    {
        public ProductVM()
        {
        }

        public ProductVM(ProductDTO row)
        {
            Id = row.Id;
            Name = row.Name;
            Slug = row.Slug;
            Description = row.Description;
            Color = row.Color;
            Memory = row.Memory;
            Price = row.Price;
            CategoryName = row.CategoryName;
            CategoryId = row.CategoryId;
            ImageName = row.ImageName;
        }

        public int Id { get; set; }
        [Required]
        [DisplayName("Название")]
        public string Name { get; set; }
        public string Slug { get; set; }
        [Required]
        [DisplayName("Описание")]
        public string Description { get; set; }
        [DisplayName("Цвет")]
        public string Color { get; set; }
        [DisplayName("Память")]
        public string Memory { get; set; }
        [DisplayName("Стоимость")]
        public decimal Price { get; set; }
        public string CategoryName { get; set; }
        [Required]
        [DisplayName("Категория")]
        public int CategoryId { get; set; }
        [DisplayName("Изображение")]
        public string ImageName { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<string> GalleryImages { get; set; }
    }
}