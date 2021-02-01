using PagedList;
using StoreTrainee.Areas.Admin.Models.ViewModels.Shop;
using StoreTrainee.Models.Data;
using StoreTrainee.Models.ViewModels.Pages;
using StoreTrainee.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace StoreTrainee.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            //Объявляем модель типа List
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                //Инициализируем модель данными
                categoryVMList = db.Categories
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x))
                    .ToList();
            }
            //Возвращаем List в представление
            return View(categoryVMList);
        }
       
        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catname)
        {
            //Объявляем строковую переменную Id
            string id;
            using (Db db = new Db())
            {
                //Проверяем имя категории на уникальность
                if (db.Categories.Any(x=>x.Name==catname))
                {
                    return "titletaken";
                }
                //Инициалезируем модель DTO
                CategoryDTO dto = new CategoryDTO();
                //Добавляем данные в модель
                dto.Name = catname;
                dto.Slug = catname.Replace(" ", "-").ToLower();
                dto.Sorting = 100;
                //Сохранить
                db.Categories.Add(dto);
                db.SaveChanges();
                //Получаем для возврата в представление
                id = dto.Id.ToString();
            }
            //Возвращаем Id в представление
            return id;
        }
       
        // POST: Admin/Shop/RecoderCategories
        [HttpPost]
        public void RecoderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //Реализуем счетчик
                int count = 1;
                //инициализируем модель
                CategoryDTO dto;
                //Устанавливаем сортировку для каждой страницы
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }

        }

        // GET: Admin/Shop/DeleteCategory
        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //получаем категорию
                CategoryDTO dto = db.Categories.Find(id);
                //удаляем категорию
                db.Categories.Remove(dto);
                //сохраняем изменения в БД
                db.SaveChanges();
            }
            //Сообщение об успешном удалении
            TempData["SM"] = "Категория удалена!";
            //переадресация в Index
            return RedirectToAction("Categories");
        }

        // POST: Admin/Shop/RenameCategory
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                //Проверяем имя на уникальность
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";
                //Получаем данные из DTO
                CategoryDTO dto = db.Categories.Find(id);
                //Редактируем модель DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();
                //Сохраняем изминение
                db.SaveChanges();
            }
            //Вернуть что-то
            return "Im ok";
        }

        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //Объявить модель
            ProductVM model = new ProductVM();

            //Добавляем список категорий из БД
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
            }
            //Возвращаем модель в представление 
            return View(model);
        }

        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }
            //Проверяем продукт на уникальность
            using (Db db=new Db())
            {
                if (db.Products.Any(x=>x.Name==model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Данный продукт уже создан!");
                    return View(model);
                }
            }
            //Объявляем переменную ProductId
            int id;
            //Инициализируем и сохранчемм модель на основе ProductDTO
            using (Db db=new Db())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Color = model.Color;
                product.Memory = model.Memory;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }

            //Добавляем сообщение в TempData
            TempData["SM"] = "Продукт добавлен!";
            
            //работа с загрузкой изображения
            #region UploadImage
            // Создаем необходимые ссылки на дериктории
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\"+id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString()+"\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            //Проверяем наличие дириктории
            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            //Проверяем загружен ли файл
            if (file != null && file.ContentLength>0)
            {
                //Получаем расширение файла
                string ext = file.ContentType.ToLower();
                //Проверяем расширение

                if (ext!="image/jpg"&&
                    ext != "image/jpeg"&&
                    ext != "image/pjpg"&&
                    ext != "image/gif"&&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db=new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Изображение не было загружено - не верный формат файла!");
                        return View(model);
                    }
                }
            //Объявляем переменную с имененем изображения
            string imageName = file.FileName;

            //Сохраняем имя изображения в модель DTO
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.ImageName = imageName;

                db.SaveChanges();
            }
                //Назначаем пути к оригинальному и уменьшиному изображению
                var path = string.Format($"{pathString2}\\{imageName}");
                var path2 = string.Format($"{pathString3}\\{imageName}");

                //Сохраняем оригинальное изображение 
                file.SaveAs(path);

                //Создаем и сохраняем уменьшиную копию
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1,1);
                img.Save(path2);
            }

            #endregion

            //Переадрисовываем пользователя 
            return RedirectToAction("AddProduct");
        }

        // GET: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            //Объявляем ProductVM типа list
            List<ProductVM> listOfProductVM;

            //Устанавливаем номер страницы
            var pageNumber = page ?? 1;
            using (Db db = new Db())
            {
                //Инициализируем list и заполняем данными
                listOfProductVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();
               
                //Заполняем категории данными
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                
                //Устанавливаем выбранную категорию
                ViewBag.SelectedCat = catId.ToString();
            }
            
            //Устанавливаем постраничную навигацию
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 10);
            ViewBag.onePageOfProducts = onePageOfProducts;
           
            //Возвращаем представление с данными
            return View(listOfProductVM);
        }
       
        // GET: Admin/Shop/EditProducts
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //Объявить модель 
            ProductVM model;
            using (Db db = new Db())
            {
                //Получить продукт
                ProductDTO dto = db.Products.Find(id);
                
                //Проверяем доступен ли продукт
                if (dto==null)
                    return Content("Продукт не доступен!");

                //Инициализируем модель данными
                model = new ProductVM(dto);

                //создаем список категорий
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                //Получаем все изображения из галереи

                model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }
           
            //Возвращаем модель в представление 
            return View(model);
        }

        // POST: Admin/Shop/EditProducts
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Получить ID
            int id = model.Id;
            
            //Заполнить список категориями и изображениями
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            model.GalleryImages = Directory
                  .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                  .Select(fn => Path.GetFileName(fn));
            //Проверка модели на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //Проверка продукта на уникальность
            using (Db db = new Db())
            {
                if (db.Products.Where(x=>x.Id !=id).Any(x=>x.Name==model.Name))
                {
                    ModelState.AddModelError("", "Имя продукта занято");
                    return View(model);
                }
            }

            //Обновить продукт в БД
            using (Db db=new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Color = model.Color;
                dto.Memory = model.Memory;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }
            //Установить сообщение в ТемпДата
            TempData["SM"] = "Продукт обновлен";

            //загрузка изображения
            #region ImageUpload
            //Проверяем загрузку файла
            if (file!=null&&file.ContentLength>0)
            {
                //Получаем расширение файла
                string ext = file.ContentType.ToLower();

                //Проверяем расширение 
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "Изображение не было загружено - не верный формат файла!");
                        return View(model);
                    }
                }
                //Устанавливаем пути загрузки
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
              
                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //Удаляем существующие файлы и директории
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);
                foreach (var file2 in di1.GetFiles())
                {
                    file2.Delete();
                }

                foreach (var file3 in di2.GetFiles())
                {
                    file3.Delete();
                }
                //Сохраняем изображение
                string imageName = file.FileName;

                using (Db db=new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }
                //Сохраняем оригинал и превью версии
                var path = string.Format($"{pathString1}\\{imageName}");
                var path2 = string.Format($"{pathString2}\\{imageName}");

                //Сохраняем оригинальное изображение 
                file.SaveAs(path);

                //Создаем и сохраняем уменьшиную копию
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1,1);
                img.Save(path2);
            }
            #endregion

            //Переадресовать пользователя на представление 
            return RedirectToAction("EditProduct");
        }
       
        // POST: Admin/Shop/DeleteProducts/id
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            // Удаляем товар из базы данных
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                //db.Entry(dto).State = EntityState.Deleted;
                db.SaveChanges();
            }
           
            // Удаляем дериктории товара (изображения)
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);

            // Переадресовываем пользователя
             return RedirectToAction("Products");
        }

        // POST: Admin/Shop/SaveGalleryImages/id
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            // Перебираем все полученные файлы
            foreach (string fileName in Request.Files)
            {
                // Инициализируем файлы
                HttpPostedFileBase file = Request.Files[fileName];

                // Проверяем на NULL
                if (file != null && file.ContentLength > 0)
                {
                    // Назначаем пути к директориям
                    var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    // Назначаем пути изображений
                    var path = string.Format($"{pathString1}\\{file.FileName}");
                    var path2 = string.Format($"{pathString2}\\{file.FileName}");

                    // Сохраняем оригинальные изображения и уменьшенные копии
                    file.SaveAs(path);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200).Crop(1, 1);
                    img.Save(path2);
                }

            }
        }

        // POST: Admin/Shop/DeleteImage/id/imageName
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }

        public ActionResult Orders()
        {
            //Инициализируем модель OrdersForAdminVm
            List<OrdersForAdminVm> ordersForAdmin = new List<OrdersForAdminVm>();

            using (Db db = new Db())
            {
                //Инициализируем модель OrderVm
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                //Перебираем данные в OrderVM
                foreach (var order in orders)
                {

                    //Инициализируем словарь товаров
                    Dictionary<string, int> productAndQuantity = new Dictionary<string, int>();

                    //Объявляем переменную общей суммы
                    decimal total = 0m;

                    //Инициализируем лист OrderDetailDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //Получаем имя пользователя
                    UserDTO user = db.Users.FirstOrDefault(x => x.Id == order.UserId);
                    string userName = user.Username;
                    string adress = user.Adress;
                    string phone = user.Phone;

                    //Перебираем список товаров из OrderDetailDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        //Получаем товар
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                        //Получаем цену товара
                        decimal price = product.Price;

                        //Получаем название товара
                        string name = product.Name;

                        //Добавляем товар в словарь
                        productAndQuantity.Add(name, orderDetails.Quantity);

                        //Получаем общую стоимость товаров
                        total += orderDetails.Quantity * price;
                    }
                    //Добавляем данные в модель OrdersForAdminVm
                    ordersForAdmin.Add(new OrdersForAdminVm()
                    {
                        OrderNumber = order.OrderId,
                        UserName = userName,
                        Total=total,
                        ProductAndQuantity = productAndQuantity,
                        CreatedAt=order.CreateAt,
                        Phone = phone,
                        Adress=adress

                    });
                }
            }
            //Возвращаем представленеи с моделью OrdersForAdminVm
            return View(ordersForAdmin);
        }
    }
}