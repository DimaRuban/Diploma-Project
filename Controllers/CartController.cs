using StoreTrainee.Models.Data;
using StoreTrainee.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace StoreTrainee.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            // Объявляем лист типа CartVM
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Проверяем, пуста ли корзина
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Ваша корзина пуста.";
                return View();
            }
            // Складываем сумму и записываем во ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            // Возвращаем лист в представление
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            // Объявляем модель CartVM
            CartVM model = new CartVM();

            // Объявляем переменную количества
            int qty = 0;

            // Объявляем переменную цены
            decimal price = 0m;

            // Проверяем сессию корзины
            if (Session["cart"] != null)
            {
                // Получаем общее количество товаров и цену
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                // Или устанавливаем количество и цену в 0
                model.Quantity = 0;
                model.Price = 0m;
            }

            // Возвращаем частичное представление с моделью
            return PartialView("_CartPartial", model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            // Объявляем лист, параметризированный типом CartVM
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Объявляем модель CartVM
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                // Получаем продукт
                ProductDTO product = db.Products.Find(id);

                // Проверяем, находится ли товар уже в корзине
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                // Если нет, то добавляем новый товар в корзину
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                // Если да, добавляем единицу товара
                else
                {
                    productInCart.Quantity++;
                }
            }
            // Получаем общее количество, цену и добавляем данные в модель
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            // Сохраняем состояние корзины в сессию
            Session["cart"] = cart;

            // Возвращаем частичное представление с моделью
            return PartialView("_AddToCartPartial", model);
        }

        // GET: /cart/IncrementProduct
        public JsonResult IncrementProduct(int productId)
        {
            // Объявляем лист cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Получаем модель CartVM из листа
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Добавляем количество
                model.Quantity++;

                // Сохраняем необходимые данные
                var result = new { qty = model.Quantity, price = model.Price };

                // Возвращаем JSON ответ с данными
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /cart/DecrementProduct
        public ActionResult DecrementProduct(int productId)
        {
            // Объявляем лист cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Получаем модель CartVM из листа
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Отнимаем количество
                if (model.Quantity > 1)
                    model.Quantity--;
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                // Сохраняем необходимые данные
                var result = new { qty = model.Quantity, price = model.Price };

                // Возвращаем JSON ответ с данными
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public void RemoveProduct(int productId)
        {
            // Объявляем лист cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Получаем модель CartVM из листа
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                cart.Remove(model);
            }
        }

        [HttpPost]
        public void PlaceOrder()
        {
            //Получаем список товаров в коризине
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //Получаем имя пользователя
            string userName = User.Identity.Name;

            //Объявялем переменную в ордер айди
            int orderId = 0;

            using (Db db = new Db())
            {
                //Объявляем модель в ордер дто
                OrderDTO orderDTO = new OrderDTO();

                //получаем айди пользователя
                var q = db.Users.FirstOrDefault(x => x.Username == userName);
                int userId = q.Id;

                //заполняем модель в ордер дто данными и сохраняем
                orderDTO.UserId = userId;
                //orderDTO.CreateAt = DateTime.Now;

                db.Orders.Add(orderDTO);
                db.SaveChanges();

                //получаем ордер айди
                orderId = orderDTO.OrderId;

                //объявляем модель ордер детаилсдто
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();
                
                //добавляем в модель данные
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);
                    db.SaveChanges();
                }
            }
           
            ////отправляем письмо о заказе на почту админа
            //var client = new SmtpClient("smtp.mailtrap.io", 2525)
            //{
            //    Credentials = new NetworkCredential("373133b4c83d85", "a4692c00a22f92"),
            //    EnableSsl = true
            //};
            //client.Send("from@example.com", "to@example.com", "Hello world", "testbody");

            //обнуляем сессию
            Session["cart"] = null;
        }
    }
}