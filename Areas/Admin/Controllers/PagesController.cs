using StoreTrainee.Models.Data;
using StoreTrainee.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StoreTrainee.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //Объявляет список для представления PageVM
            List<PageVM> pageList;
            //Инициализируем список в Db
            using(Db db=new Db())
            {
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }
            //Возвращаем список в представление 
            return View(pageList);
        }
        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {

            return View();
        }
        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //проверка модели на валидность
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {
                //объявляем переменную для краткого описания(slug)
                string slug;
                //инициализация класса PageDTO
                PagesDTO dto = new PagesDTO();
                //присваеваем заголовок модели
                dto.Title = model.Title.ToUpper();
                //проверяем есть ли краткое описание, если нет, присваиваем его
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                //убеждаемся, что заголовок описания уникальный 
                if (db.Pages.Any(x=>x.Title==model.Title))
                {
                    ModelState.AddModelError("", "That title alredy exist.");
                    return View(model);
                }
                else if (db.Pages.Any(x=>x.Slug==model.Slug))
                {
                    ModelState.AddModelError("", "That title alredy exist.");
                    return View(model);
                }
                //присваеваем оставшиеся значения модели 
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;
                //сохранияем модель в БД
                db.Pages.Add(dto);
                db.SaveChanges();
            }
            //передаем сообщение через TempData
            TempData["SM"] = "You have added a  new pages!";
            //переадресовываем пользователя на метод Index
            return RedirectToAction("Index");
        }
        // GEt: Admin/Pages/EditPage
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //объявим модель pagevm
            PageVM model;
            using (Db db=new Db())
            {
                //получаем страницу
                PagesDTO dto = db.Pages.Find(id);
                //проверяем, доступна ли страница
                if (dto==null)
                {
                    return Content("The page does not exist");
                }
                //инициализируем модель данных
                model = new PageVM(dto);
            }         
            //возвращаем представление
            return View(model);
        }
        // POST: Admin/Pages/EditPage
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //Проверка модели на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }
           
            using (Db db = new Db())
            {
                //получаем ID страницы
                int id = model.Id;

                //Объявляем переменную краткого заголовка
                string slug = "home";
                //получаем страницу по Id
                PagesDTO dto = db.Pages.Find(id);
                //Присваеваем название из полученой модели в DTO
                dto.Title = model.Title;
                //Проверяем краткий заголовок и присваеваем его если это необходимо
                if (model.Slug!="home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }
                //Проверяем Slug and Title на уникальность
                if (db.Pages.Where(x=>x.Id!=id).Any(x=>x.Title==model.Title))
                {
                    ModelState.AddModelError("", "That title alredy exist");
                    return View(model);
                }
                else if (db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That slug alredy exist");
                    return View(model);
                }

                //записать остальное значение в класс DTO
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                //Сохраняем изминение в БД
                db.SaveChanges();
            }
            //Устанавливаем сообщение в TenData
            TempData["SM"] = "You have edited the page";
            //переадресация пользователя
            return RedirectToAction("EditPage");
        }
        // GEt: Admin/Pages/Details
        [HttpGet]
        public ActionResult PageDetails(int id)
        {
            //объявляем модель PageVm
            PageVM model;
            using (Db db=new Db())
            {
                //получить страницу
                PagesDTO dto = db.Pages.Find(id);
                //поддтверждаем, что страница доступна
                if (dto==null)
                {
                    return Content("The page does not exist");
                }
                //присваеваем модели информацию из БД
                model = new PageVM(dto);
            }
           
            //вовращаем модель в представление
            return View(model);
        }
        // GET: Admin/Pages/Delete
        [HttpGet]
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                //получаем страницу
                PagesDTO dto = db.Pages.Find(id);
                //удаляем страницу
                db.Pages.Remove(dto);
                //сохраняем изменения в БД
                db.SaveChanges();
            }
            //Сообщение об успешном удалении
            TempData["SM"] = "You have deleted pages";
            //переадресация в Index
            return RedirectToAction("Index");
        }
        //Создаем метод сортировки
        // POST: Admin/Pages/RecoderPages
        [HttpPost]
        public void RecoderPages(int[] id)
        {
            using (Db db = new Db())
            {
                //Реализуем счетчик
                int count = 1;
                //инициализируем модель
                PagesDTO dto;
                //Устанавливаем сортировку для каждой страницы
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }
            
        }
        // GET: Admin/Pages/Editsidebar
        [HttpGet]
        public ActionResult Editsidebar()
        {
            //объявляем модель
            SidebarVM model;

            using (Db db = new Db())
            {
                // получаем данные из DTO
                SidebarDTO dto = db.Sidebars.Find(1);

                //заполняем модель данными
                model = new SidebarVM(dto);
            }
           
            //вернуть представление с моделью
            return View(model);
        }
        // POST: Admin/Pages/Editsidebar
        [HttpPost]
        public ActionResult Editsidebar(SidebarVM model)
        {
           
            using (Db db = new Db())
            {
                //Получуть данныее из DTO
                SidebarDTO dto = db.Sidebars.Find(1);
                //присваеваем данные в тело(в свойсвто body)
                dto.Body = model.Body;
                //сохранить в бд
                db.SaveChanges();
            }
            //присваеваем значение в tempdata
            TempData["SM"] = "You have edit the sidebar!";
          //переадресация пользователя
            return RedirectToAction("Editsidebar");
        }
    }
}