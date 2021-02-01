using StoreTrainee.Models.Data;
using StoreTrainee.Models.ViewModels.Account;
using StoreTrainee.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace StoreTrainee.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        // GET: account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            // Проверяем модель на валидность
            if (!ModelState.IsValid)
                return View("CreateAccount", model);

            // Проверяем соответствие пароля
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Пароль не совпадает!");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                // Проверяем имя на уникальность
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", $"Логин {model.Username} уже создан.");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                // Создаём экземпляр класса UserDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAdress = model.EmailAdress,
                    Username = model.Username,
                    Password = model.Password,
                    Phone = model.Phone,
                    Adress=model.Adress
                };

                // Добавляем данные в модель
                db.Users.Add(userDTO);

                // Сохраняем данные
                db.SaveChanges();

                // Добавляем роль пользователю
                int id = userDTO.Id;

                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }

            // Записать сообщение в TempData
            TempData["SM"] = "Регистрация прошла успешно.";

            // Переадресовываем пользователя
            return RedirectToAction("Login");
        }


        // GET: Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            // подтвердить, что пользователь не авторизован
            string userName = User.Identity.Name;

            //if (!string.IsNullOrEmpty(userName))
            //    return RedirectToAction("user-profile");

            // Возвращаем представление
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            // Проверяем модель на валидность
            if (!ModelState.IsValid)
                return View(model);

            // Проверяем пользователя на валидность
            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                    isValid = true;

                if (!isValid)
                {
                    ModelState.AddModelError("", "Логин или пароль введены неверно.");
                    return View(model);
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
                }
            }
        }

        // GET: /account/logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {
            // Получаем имя пользователя
            string userName = User.Identity.Name;

            // Объявляем модель
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                // Получаем пользователя
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);


                // Заполняем модель данными из контекста (DTO)
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }
            // Возвращаем частичное представление с моделью
            return PartialView(model);
        }

        // GET: /account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            // Получаем имя пользователя
            string userName = User.Identity.Name;

            // Объявляем модель
            UserProfileVM model;

            using (Db db = new Db())
            {
                // Получаем пользователя
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                // Инициализируем модель данными
                model = new UserProfileVM(dto);
            }
            // Возвращаем модель в представление
            return View("UserProfile", model);
        }

        // POST: /account/user-profile
        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            bool userNameIsChanged = false;

            // Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            // Проверяем пароль (если пользователь его меняет)
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Пароли не совпадают.");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                // Получаем имя пользователя
                string userName = User.Identity.Name;

                // Проверяем, сменилось ли имя пользователя
                if (userName != model.Username)
                {
                    userName = model.Username;
                    userNameIsChanged = true;
                }

                // Проверяем имя на уникальность
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == userName))
                {
                    ModelState.AddModelError("", $"Логин {model.Username} уже создан.");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                // Изменяем модель контекста данных
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAdress = model.EmailAdress;
                dto.Username = model.Username;
                dto.Phone = model.Phone;
                dto.Adress = model.Adress;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                // Сохраняем изменения
                db.SaveChanges();
            }

            // Устанавливаем сообщение в TempData
            TempData["SM"] = "Профиль изменен!";

            if (!userNameIsChanged)
                // Возвращаем представление с моделью
                return View("UserProfile", model);
            else
                return RedirectToAction("Logout");
        }

        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            //Инициализируем модель OrdersForUserVM
            List<OrdersForUserVM> ordersForUserVM = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {
                //Прочаем Id пользователя
                UserDTO user = db.Users.FirstOrDefault(x => x.Username == User.Identity.Name);
                int userId = user.Id;

                //Инициализируем модель OrderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

                //Перебираем список товаров из Orders
                foreach (var order in orders)
                {
                    //Инициализируем словарь товара
                    Dictionary<string, int> productAndQuantity = new Dictionary<string, int>();


                    //Объявляем переменную общей суммы
                    decimal total = 0m;

                    //Инициализируем лист OrderDetailDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

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
                    //Добавляяем полученные данные в модель OrdersForUserVM
                    ordersForUserVM.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,   
                        Total = total,
                        ProductAndQuantity = productAndQuantity,
                        CreatedAt = order.CreateAt
                    });
                }
            }
            //Возвращаем представляение с моделью OrdersForUserVM
            return View(ordersForUserVM);
        }
    }
}