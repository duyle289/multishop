using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebBanlaptop.Models;
namespace WebBanlaptop.Controllers
{
    public class UserController : Controller
    {
        QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
        private static string urlAfterLogin; // lưu lại link đang ở trước khi nhấn đăng nhập
        private static string urlAfterForgot;
        private static string urlAfterSingup;
        #region MD5
        public static string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }
        #endregion
        #region sendmail
        public static void sendmail(string address, string subject, string message)
        {
            if (new EmailAddressAttribute().IsValid(address)) // check có đúng mail khách hàng
            {
                string email = "multishoplaptop@gmail.com";
                var senderEmail = new MailAddress(email, "MultiShop(tin nhắn tự động)");
                var receiverEmail = new MailAddress(address, "Receiver");
                var password = "xtpitmzjlbqpmizq";
                var sub = subject;
                var body = message;
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail.Address, password)
                };
                using (var mess = new MailMessage(senderEmail, receiverEmail)
                {
                    Subject = sub,
                    Body = body
                })
                    smtp.Send(mess);

            }
        }
        #endregion

        #region đăng ký
        [HttpGet]
        public ActionResult DangKy(string strURL)
        {
            urlAfterSingup = strURL;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(FormCollection f, KHACHHANG kh)
        {
            var tenkh = f["TENKH"].ToString();
            var DIACHI = f["DIACHI"].ToString();
            var USERNAME = f["USERNAME"].ToString();
            var PASSWORD = f["PASSWORD"].ToString();
            var NHAPLAIPASSWORD = f["NHAPLAIPASSWORD"].ToString();
            var EMAIL = f["EMAIL"].ToString();
            var SDT = f["SDT"].ToString();
            var CCCD = f["CCCD"].ToString();
            var userexist = db.KHACHHANG.Any(x => x.USERNAME == kh.USERNAME);
            var emailexist = db.KHACHHANG.Any(x => x.EMAIL == kh.EMAIL);
            var sdtexist = db.KHACHHANG.Any(x => x.SDT == kh.SDT);
            var cccdexist = db.KHACHHANG.Any(x => x.CCCD == kh.CCCD);
            if (ModelState.IsValid)
            {
                if(String.IsNullOrEmpty(tenkh) || String.IsNullOrEmpty(DIACHI) || String.IsNullOrEmpty(USERNAME) ||
                    String.IsNullOrEmpty(PASSWORD) || String.IsNullOrEmpty(NHAPLAIPASSWORD) || String.IsNullOrEmpty(EMAIL) ||
                    String.IsNullOrEmpty(SDT) || String.IsNullOrEmpty(CCCD))
                {
                    ViewBag.thongbao = "Điền đầy đủ thông tin";
                    return View();
                }
                if (userexist)
                {
                    ModelState.AddModelError("USERNAME", "Tên tài khoản đã tồn tại");
                    return View(kh);
                }
                if (emailexist)
                {
                    ModelState.AddModelError("EMAIL", "Email đã được đăng ký mời nhập emmil khác");
                    return View(kh);
                }
                if (sdtexist)
                {
                    ModelState.AddModelError("SDT", "SĐT đã được đăng ký mời nhập SĐT khác");
                    return View(kh);
                }
                if (cccdexist)
                {
                    ModelState.AddModelError("CCCD", "CCCD đã được đăng ký mời nhập CCCD khác");
                    return View(kh);
                }
                if (kh.SDT.Length != 10)
                {
                    ModelState.AddModelError("SDT", "Phải nhập đủ 10 số");
                }
                if (kh.CCCD.Length != 9 && kh.CCCD.Length != 12)
                {
                    ModelState.AddModelError("CCCD", "CCCD hoặc là 9 số hoặc là 12 số");
                }
                if(kh.TENKH.Length > 35)
                {
                    ModelState.AddModelError("TENKH","Tên khách hàng không được quá 35 ký tự");
                }    
                else
                {
                    string mk = f["PASSWORD"].ToString();
                    string xnmk = f["NHAPLAIPASSWORD"].ToString();
                    if (mk == xnmk)
                    {
                        string subject = "Chào mừng bạn đã đăng ký tài khoản thành công";
                        string message = "Chào mừng" + kh.TENKH + "đến với MutiShop, chúc bạn một ngày vui vẻ";
                        sendmail(kh.EMAIL, subject, message);
                        string pass = MD5Hash(mk);
                        kh.PASSWORD = pass;
                        kh.TRANGTHAI = true;
                        db.KHACHHANG.Add(kh);
                        db.SaveChanges();
                        return RedirectToAction("Login","User", urlAfterSingup);
                    }
                    else
                    {
                        ViewBag.thongbao = "mật khẩu không trung khớp !!!!!!";
                        return View();
                    }
                }
            }
            return View();
        }
        #endregion

        #region quên mật khẩu

        public ActionResult XacNhanGoiMK(string urlAfterForgot)
        {
            ViewBag.url = urlAfterForgot;
            return View();
        }
        [HttpGet]
        public ActionResult QuenMK(string strUrl)
        {
            urlAfterForgot = strUrl;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuenMK(FormCollection f)
        {
            var email = f["EMAIL"].ToString();
            KHACHHANG kh = db.KHACHHANG.FirstOrDefault(n => n.EMAIL == email);
            var checkEmail = db.KHACHHANG.Any(n => n.EMAIL == email);
            if (ModelState.IsValid)
            {
                if (!checkEmail)
                {
                    ModelState.AddModelError("EMAIL", "Email không tồn tại");
                    return View();
                }
                else
                {
                    string pass = RandomChar();
                    kh.PASSWORD = MD5Hash(pass);
                    try
                    {
                        db.SaveChanges();
                        string subject = "Mật khẩu đăng nhập mới";
                        string message = "Đây là mail gởi từ website MultiShop \n Mật khẩu đăng nhập mới của bạn là: " + pass + "\n Sau khi đăng nhập thành công bạn nên thay đổi mật khẩu để tiện cho lần đăng nhập kế tiếp <3";
                        sendmail(email, subject, message);
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                    {
                        Exception raise = dbEx;
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                string message = string.Format("{0}:{1}",
                                    validationErrors.Entry.Entity.ToString(),
                                    validationError.ErrorMessage);
                                // raise a new exception nesting  
                                // the current instance as InnerException  
                                raise = new InvalidOperationException(message, raise);
                            }
                        }
                        throw raise;
                    }

                    return RedirectToAction("XacNhanGoiMK","User", urlAfterForgot);
                }
            }
            return View();
        }
        public static string RandomChar()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }
        #endregion

        #region lịch sử đặt hàng
        private bool checkLogin() // check đã có người dung đăng nhập chưa nếu chưa thì return false
        {
            KHACHHANG admin = Session["User"] as KHACHHANG;
            if (admin == null)
            {
                Session["User"] = null;
                return false;
            }
            return true;
        }
        public ActionResult OdersHistory()
        {
            if (!checkLogin()) return RedirectToAction("Login");
            KHACHHANG kh = (KHACHHANG)Session["User"];
            List<HOADON> hd = db.HOADON.Where(n => n.MAKH == kh.MAKH).OrderByDescending(n=>n.NGAYLAP).ToList();
            ViewBag.lsthd = hd;
            return View();
        }
        public ActionResult OrderHistoryDetails(string id)
        {
            List<CHITIETHD> checkCTHD = db.CHITIETHD.Where(n => n.MAHD == id).ToList();
            foreach(var item in checkCTHD)
            {
                var tsp = db.SANPHAM.FirstOrDefault(n => n.MASP == item.MASP);
                if (tsp != null)
                {
                    item.tenSP = tsp.TENSP;
                }
            }
            foreach (var item in checkCTHD)
            {
                var tsp = db.MAUSAC.FirstOrDefault(n => n.MAMAU == item.MAMAU);
                if (tsp != null)
                {
                    item.tenMau = tsp.TENMAU;
                }
            }
            ViewBag.lstcthd = checkCTHD;
            ViewBag.mahd = id;
            return View();
        }
        #endregion

        #region đăng nhập
        [HttpGet]
        public ActionResult Login(string strURL)
        {
            // kiểm tra đường dẫn có null không, tránh tình trạng paste trực tiếp đường link vào url sẽ không lưu được trả về trang chủ
            if (!String.IsNullOrEmpty(strURL))
            {
                urlAfterLogin = strURL;
            }
            else
            {
                RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var username = collection["username"];
            var password = collection["password"];
            var user = db.KHACHHANG.SingleOrDefault(p => p.USERNAME == username);
            if (user == null)
            {
                ViewData["Error"] = "invalid username";
                return this.Login(urlAfterLogin);
            }
            else if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
            {
                ViewData["Error"] = "Vui lòng điền đầy đủ nội dung";
                return this.Login(urlAfterLogin);
            }
            else if (!user.TRANGTHAI) 
            {
                ViewData["Error"] = "Tài khoản này đã bị tạm khóa. Vui lòng liên hệ với chúng tôi để được kích hoạt lại!!!";
                return this.Login(urlAfterLogin);
            }
            else if (!String.Equals(MD5Hash(password), user.PASSWORD))
            {
                ViewData["Error"] = "invalid password";
                return this.Login(urlAfterLogin);
            }
            else
            {
                Session["User"] = user;
                if (urlAfterLogin != null)
                {
                    return Redirect(urlAfterLogin);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
                
            }
        }
        #endregion


        #region đăng xuất
        public ActionResult LogOut() // đăng xuất
        {
            Session["User"] = null;
            urlAfterLogin = null;
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region cá nhân
        [HttpGet]
        public ActionResult MyProfile() // chuyển đến trang hồ sơ
        {
            if (Session["User"] == null) RedirectToAction("Login");
            KHACHHANG kh = (KHACHHANG)Session["User"];

            if (kh.NGAYSINH != null)
            {
                DateTime date = (DateTime)kh.NGAYSINH;
            }
            return View(kh);
        }

        [HttpPost]
        public ActionResult MyProfile(string strURL, FormCollection collection)
        {

            if (Session["User"] == null) RedirectToAction("Login");
            KHACHHANG kh = (KHACHHANG)Session["User"];
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var user = db.KHACHHANG.SingleOrDefault(p => p.MAKH == kh.MAKH);
            var name = collection["NAME"];
            var email = collection["EMAIL"];
            var diachi = collection["DIACHI"];
            var sdt = collection["SDT"];
            var day = collection["NGAYSINH"];

            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(email) || // kiểm tra null
                String.IsNullOrEmpty(diachi) || String.IsNullOrEmpty(sdt) ||
                String.IsNullOrEmpty(day))
            {
                ViewData["Error"] = "Vui lòng điền đủ thông tin";
                return this.MyProfile();
            }

            //=================kiểm tra ngày hợp lệ=========================
            string date = day;
            var a = String.Format("{0:MM/dd/yyyy}", date);
            DateTime birthday;
            if (!DateTime.TryParse(a, out birthday))
            {
                ViewData["Error"] = "Ngày sinh không hợp lệ";
                return this.MyProfile();
            }
            //=============================================================
            kh.TENKH = name;
            kh.EMAIL = email;
            kh.DIACHI = diachi;
            kh.SDT = sdt;
            kh.NGAYSINH = birthday;
            db.KHACHHANG.AddOrUpdate(kh);
            db.SaveChanges();
            Session["User"] = user;
            ViewData["Success"] = "Cập nhật thành công";
            return this.MyProfile();
        }
        #endregion


        #region đổi mật khẩu
        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (Session["User"] == null) RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(FormCollection collection)
        {
            if (Session["User"] == null) RedirectToAction("Login");
            KHACHHANG kh = (KHACHHANG)Session["User"];
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var user = db.KHACHHANG.SingleOrDefault(p => p.MAKH == kh.MAKH);
            var oldPassword = collection["oldPassword"];
            var newPassword = collection["newPassword"];
            var confirmNewPassword = collection["confirmNewPassword"];

            if (String.IsNullOrEmpty(oldPassword) || String.IsNullOrEmpty(newPassword) || // trống textbox
                String.IsNullOrEmpty(confirmNewPassword))
            {
                ViewData["Error"] = "Vui lòng điền đủ thông tin";
                return this.ChangePassword();
            }
            else if (!String.Equals(newPassword, confirmNewPassword)) // 2 ô mật khẩu mới không khớp
            {
                ViewData["Error"] = "Mật khẩu mới không khớp";
                return this.ChangePassword();
            }
            else if (!String.Equals(MD5Hash(oldPassword), user.PASSWORD)) // kiểm tra mật khẩu cũ
            {
                ViewData["Error"] = "Sai mật khẩu cũ";
                return this.ChangePassword();
            }
            else // ==============thay đổi mật khẩu===================
            {
                newPassword = MD5Hash(newPassword);
                user.PASSWORD = newPassword;
                db.SaveChanges();
                Session["User"] = user;
                ViewData["Success"] = "Đổi mật khẩu thành công";
                return this.ChangePassword();
            }
        }
        #endregion


    }
}