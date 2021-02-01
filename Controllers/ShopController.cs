using StoreTrainee.Models.Data;
using StoreTrainee.Models.ViewModels.Pages;
using StoreTrainee.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StoreTrainee.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            // Объявляем модель типа List<> CategoryVM
            List<CategoryVM> categoryVMList;

            // Инициализируем модель данными
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x))
                    .ToList();
            }

            // Возвращаем частичное представление с моделью
            return PartialView("_CategoryMenuPartial", categoryVMList);
        }

        public ActionResult Category(string name)
        {
            //Обьявляем список типа лист
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                //Получаем айди категории
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();

                int catId = categoryDTO.Id;

                //инициализируем список данными
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                //получаем список категорий
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();

                //делаем проверку на налл
                if (productCat==null)
                {
                    var catName = db.Categories.Where(x => x.Slug == name).Select(x => x.Name).FirstOrDefault();
                    ViewBag.CategoryName = catName;
                }
                else
                {
                    ViewBag.CategoryName = productCat.CategoryName;
                }
            }
                //возвращаем предстовление
                return View(productVMList);
        }
       
        // GET: Shop/product-details/name
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            // Объявляем модели DTO и VM
            ProductDTO dto;
            ProductVM model;

            // Инициализируем ID продукта
            int id = 0;

            using (Db db = new Db())
            { 
                // Проверяем, доступен ли продукт
                if (!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                // Инициализируем модель DTO данными
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                // Получаем ID
                id = dto.Id;

                // Инициализируем модель VM данными
                model = new ProductVM(dto);
            }
            // Получаем изображения из галереи
            model.GalleryImages = Directory
                .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                .Select(fn => Path.GetFileName(fn));

            // Возвращаем модель в представление
            return View("ProductDetails", model);
        }
    }
}