using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using WebBanlaptop.Models;
using PagedList;
using PagedList.Mvc;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.IO;

namespace WebBanlaptop.Controllers
{
    public class AdminController : Controller
    {

        #region thống kê

        public static float percent()
        {
            float percent;
            int chuaxong = 0,tong=0;
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var hd = db.HOADON;
            foreach(var item in hd)
            {
                if(item.TRANGTHAI == 2)
                {
                    chuaxong++;
                }
            }
            tong = hd.Count();
            return percent = (float)chuaxong / tong;
        }
        public static double monthly()
        {
            double sum=0;
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var hd = db.HOADON;
            foreach (var item in hd)
            {
                if (item.NGAYLAP.Value.Month.ToString() == DateTime.Now.Month.ToString()&&item.TRANGTHAI==2)
                {
                    sum += (double)item.TONGTIEN;
                }
            }
            return sum;
        }
        public static double annual()
        {
            double sum = 0;
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var hd = db.HOADON;
            foreach (var item in hd)
            {
                if (item.NGAYLAP.Value.Year.ToString() == DateTime.Now.Year.ToString() && item.TRANGTHAI == 2)
                {
                    sum += (double)item.TONGTIEN;
                }
            }
            return sum;
        }
        public static int pendingrequests()
        {
            int moi = 0;
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var hd = db.HOADON;
            foreach (var item in hd)
            {
                if (item.TRANGTHAI == 0)
                {
                    moi++;
                }
            }
            return moi;
        }
        #endregion

        #region sendmail
        public static void sendmail(string address, string subject, string message)
        {
            //if (new EmailAddressAttribute().IsValid(address)) // check có đúng mail khách hàng
            //{
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

            //}
        }
        #endregion
        public ActionResult index()
        {
            if (!checkLogin()) return RedirectToAction("Login"); // session null thì bắt đăng nhập
            ViewBag.percent = percent();
            ViewBag.monthly = monthly();
            ViewBag.annual = annual();
            ViewBag.pendingrequests = pendingrequests();
            return View();
        }
        private bool checkLogin() // check đã có người dung đăng nhập chưa nếu chưa thì return false
        {
            NHANVIEN admin = Session["Admin"] as NHANVIEN;
            if (admin == null)
            {
                Session["Admin"] = null;
                return false;
            }
            return true;
        }
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

        #region đăng nhập, đăng xuất
        [HttpGet]
        public ActionResult Login()
        {
            if (checkLogin()) return RedirectToAction("index"); // nếu có lưu session đăng nhập thì vào luôn web
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection f)
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var username = f["username"];
            var password = f["password"];
            var admin = db.NHANVIEN.SingleOrDefault(p => p.USERNAME == username);
            if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
            {
                ViewData["Error"] = "Vui Lòng Điền Đầy Đủ Nội Dung";
                return this.Login();
            }
            else if (admin == null)
            {
                ViewData["Error"] = "Sai Tài Khoản";
                return this.Login();
            }
            else if (!String.Equals(MD5Hash(password), admin.PASSWORD))
            {
                ViewData["Error"] = "Sai Mật Khẩu";
                return this.Login();
            }
            else if (!admin.TRANGTHAI)
            {
                ViewData["Error"] = "Tài khoản Admin này đang bị khóa. Vui lòng liên hệ chủ shop.";
                return this.Login();
            }
            else
            {
                Session["Admin"] = admin;
                return RedirectToAction("index", "Admin");
            }
        }
        public ActionResult LogOut()
        {
            Session["Admin"] = null;
            return RedirectToAction("Login");
        }
        #endregion

        #region phần admins
        public ActionResult ListAdmin()
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            if (!checkLogin()) return RedirectToAction("Login");
            List<NHANVIEN> listAdmin = db.NHANVIEN.ToList();
            ViewBag.lstnv = listAdmin;
            return View();
        }
        [HttpGet]
        public ActionResult addAdmin()
        {
            if (!checkLogin()) return RedirectToAction("Login");
            return View();
        }
        [HttpPost]
        public ActionResult addAdmin(NHANVIEN nv)
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var checkNV = db.NHANVIEN.FirstOrDefault(n => n.USERNAME == nv.USERNAME);
            if (checkNV != null)
            {
                ViewData["Error"] = "Tài khoản admin đã tồn tại";
                return this.addAdmin();
            }
            else
            {
                try
                {
                    nv.PASSWORD = MD5Hash(nv.PASSWORD);
                    nv.TRANGTHAI = true;
                    nv.BIGBOSS = false;
                    db.NHANVIEN.Add(nv);
                    db.SaveChanges();
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

            }
            return RedirectToAction("ListAdmin", "Admin");
        }
        public JsonResult LockOrUnlockAdmin(string id)
        {
            bool lockOrUnlock = true; // khóa hay mở khóa tài khoản (true: khóa, false: mở khóa)
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var admin = db.NHANVIEN.Single(p => p.USERNAME.Equals(id));
            bool Status = false;
            string err = string.Empty;
            if (admin != null)
            {
                if ((bool)admin.BIGBOSS) // check chủ sỡ hữu
                    return Json(new
                    {
                        status = false,
                        exit = "Không thể khóa chủ sở hữu"
                    });
                if (admin.TRANGTHAI) admin.TRANGTHAI = false; // nếu đang hoạt động thì khóa
                else
                {
                    admin.TRANGTHAI = true;
                    lockOrUnlock = false; // mở khóa tài khoản
                }

                try
                {
                    db.SaveChanges();
                    Status = true;
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                }
            }

            return Json(new
            {
                status = Status,
                errorMessage = err,
                lockStatus = lockOrUnlock
            });

        }
        [HttpGet]
        public ActionResult changePWAdmin(int id, bool type)
        {
            if (!checkLogin()) return RedirectToAction("Login");
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var checkNV = db.NHANVIEN.FirstOrDefault(n => n.MANV == id);
            if (type)
            {
                ViewBag.type = true;
                return View(checkNV);
            }
            else
            {
                ViewBag.type = false;
                return View(checkNV);
            }
        }
        [HttpPost]
        public ActionResult changePWAdmin(NHANVIEN nv, bool type, FormCollection f, int id)
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var checkNV = db.NHANVIEN.FirstOrDefault(n => n.MANV == id);
            if (type)
            {
                try
                {
                    checkNV.PASSWORD = MD5Hash(nv.PASSWORD);
                    db.NHANVIEN.AddOrUpdate(checkNV);
                    db.SaveChanges();
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
            }
            else
            {
                var xnmk = f["XNPASSWORD"].ToString();
                if (!nv.PASSWORD.Equals(xnmk))
                {
                    ViewData["Error"] = "Tài khoản admin đã tồn tại";
                    return this.changePWAdmin(nv.MANV, type);
                }
                else
                {
                    try
                    {
                        checkNV.PASSWORD = MD5Hash(nv.PASSWORD);
                        db.NHANVIEN.AddOrUpdate(checkNV);
                        db.SaveChanges();
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
                }

            }

            return RedirectToAction("ListAdmin", "Admin");
        }
        #endregion

        #region phần users
        public ActionResult ListUser()
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            if (!checkLogin()) return RedirectToAction("Login");
            List<KHACHHANG> listUser = db.KHACHHANG.ToList();
            ViewBag.lstkh = listUser;
            return View();
        }
        public JsonResult LockOrUnlockUser(string id)
        {
            bool lockOrUnlock = true; // khóa hay mở khóa tài khoản (true: khóa, false: mở khóa)
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var user = db.KHACHHANG.Single(p => p.USERNAME.Equals(id));
            bool Status = false;
            string err = string.Empty;
            if (user != null)
            {
                if (user.TRANGTHAI) user.TRANGTHAI = false; // nếu đang hoạt động thì khóa
                else
                {
                    user.TRANGTHAI = true;
                    lockOrUnlock = false; // mở khóa tài khoản
                }

                try
                {
                    db.SaveChanges();
                    Status = true;
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                }
            }

            return Json(new
            {
                status = Status,
                errorMessage = err,
                lockStatus = lockOrUnlock
            });

        }
        #endregion

        #region Quản lý đơn đặt hàng
        public ActionResult ListDonDatHang()
        {
            if (!checkLogin()) return RedirectToAction("Login");
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();

            List<HOADON> listDonDatHang = db.HOADON.OrderByDescending(n => n.NGAYLAP).ToList();

            ViewBag.lstDDH = listDonDatHang;
            return View();
        }
        public ActionResult ChiTietDonDatHang(string id, int type)
        {
            if (!checkLogin()) return RedirectToAction("Login");
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            List<CHITIETHD> listChiTietHoaDon = db.CHITIETHD.ToList();
            var hd = db.HOADON.FirstOrDefault(n => n.MAHD == id);
            var cthd = db.CHITIETHD.Where(n => n.MAHD == hd.MAHD);
            foreach (var item in cthd)
            {
                var mausac = db.MAUSAC.FirstOrDefault(p => p.MAMAU == item.MAMAU);
                if (mausac != null)
                {
                    item.tenMau = mausac.TENMAU;
                }
            }
            foreach (var item in cthd)
            {
                var tensp = db.SANPHAM.FirstOrDefault(p => p.MASP == item.MASP);
                if (tensp != null)
                {
                    item.tenSP = tensp.TENSP;
                }
            }
            ViewBag.lstCTHD = cthd;
            ViewBag.id = id;
            ViewBag.type = type;
            return View();
        }
        [HttpPost]
        public ActionResult XacNhanDH(string id, bool type)
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var checkDH = db.HOADON.FirstOrDefault(n => n.MAHD == id);
            if (checkDH != null)
            {
                if (type)
                {
                    checkDH.TRANGTHAI = 1;
                    db.HOADON.AddOrUpdate(checkDH);
                    db.SaveChanges();
                    var checkKH = db.KHACHHANG.FirstOrDefault(n => n.MAKH == checkDH.MAKH);
                    string subject = "Thông báo đơn hàng";
                    string message = "Đây là mail gởi từ website MultiShop \n Đơn hàng của bạn đã được xác nhận và đang trong quá trình giao hàng.\n Thời gian giao hàng dự kiến: " + DateTime.Now.AddDays(10).ToString("dd/MM/yyyy") + "\n Cảm ơn bạn đã tin tưởng và ủng hộ shop <3";
                    sendmail(checkKH.EMAIL, subject, message);
                }
                else
                {
                    checkDH.TRANGTHAI = 2;
                    db.HOADON.AddOrUpdate(checkDH);
                    db.SaveChanges();
                }

            }
            return RedirectToAction("ListDonDatHang", "Admin");
        }


        #endregion


        #region quản lý bình luận
        public ActionResult ListBinhLuan()
        {
            if (!checkLogin()) return RedirectToAction("Login");
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            List<BINHLUAN> listBinhLuan = db.BINHLUAN.ToList();
            foreach (var item in listBinhLuan)
            {
                var sp = db.SANPHAM.FirstOrDefault(p => p.MASP == item.MASP);
                if (sp != null)
                {
                    item.tenSP = sp.TENSP;
                }
            }
            ViewBag.lstBL = listBinhLuan;
            return View();
        }


        public JsonResult LockOrUnlockBL(int? id)
        {
            bool lockOrUnlock = true; // khóa hay mở khóa tài khoản (true: khóa, false: mở khóa)
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var bl = db.BINHLUAN.FirstOrDefault(p => p.MABL == id);
            bool Status = false;
            string err = string.Empty;
            if (bl != null)
            {

                if (bl.TRANGTHAI) bl.TRANGTHAI = false; // nếu đang hoạt động thì khóa
                else
                {
                    bl.TRANGTHAI = true;
                    lockOrUnlock = false; // mở khóa tài khoản
                }

                try
                {
                    db.SaveChanges();
                    Status = true;
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                }
            }

            return Json(new
            {
                status = Status,
                errorMessage = err,
                lockStatus = lockOrUnlock
            });



        }


        #endregion

        #region Quản lý tin tức
        //quản lý tin tức
        public ActionResult ListTinTuc()
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            if (!checkLogin()) return RedirectToAction("Login");
            List<TINTUC> listTinTuc = db.TINTUC.ToList();
            ViewBag.lstTT = listTinTuc;
            return View();
        }
        [HttpGet]
        public ActionResult ThemTinTuc()
        {
            if (!checkLogin()) return RedirectToAction("Login");
            return View();
        }
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ThemTinTuc(TINTUC tt, HttpPostedFileBase HINHTT)
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            //kiem tra hinh anh đa tôn tại trong csdl hay chưa
            if (HINHTT.ContentLength > 0)
            {
                //lấy hinh ảnh tư bên ngoài vào
                var filename = Path.GetFileName(HINHTT.FileName);
                //sau đo chuyển hinh ảnh đó vào thư mục trong solution
                var path = Path.Combine(Server.MapPath("~/Content/HinhTinTuc"), filename);
                //kiểm tra nếu hình anh đã có rồi thi xuất ra thông báo và ngược lại nêu chưa co thi thêm vào
                if (System.IO.File.Exists(path))
                {
                    ViewBag.upload = "Hình ảnh đã tồn tại rồi!!";
                    return View();
                }
                else
                {
                    HINHTT.SaveAs(path);
                    tt.HINHTT = filename;

                }

            }
            try
            {
                db.TINTUC.Add(tt);
                db.SaveChanges();
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

            return RedirectToAction("ListTinTuc", "Admin");
        }

        public JsonResult LockOrUnlockTT(int? id)
        {
            bool lockOrUnlock = true; // khóa hay mở khóa tài khoản (true: khóa, false: mở khóa)
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var tt = db.TINTUC.SingleOrDefault(p => p.MATT == id);
            bool Status = false;
            string err = string.Empty;
            if (tt != null)
            {

                if (tt.TRANGTHAI) tt.TRANGTHAI = false; // nếu đang hoạt động thì khóa
                else
                {
                    tt.TRANGTHAI = true;
                    lockOrUnlock = false; // mở khóa tài khoản
                }

                try
                {
                    db.SaveChanges();
                    Status = true;
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                }
            }

            return Json(new
            {
                status = Status,
                errorMessage = err,
                lockStatus = lockOrUnlock
            });



        }

        [HttpGet]
        public ActionResult SuaTT(int? id)
        {
            if (!checkLogin()) return RedirectToAction("Login");
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var tt = db.TINTUC.SingleOrDefault(n => n.MATT == id);
            ViewBag.tt = tt;
            return View();
        }
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult SuaTT(TINTUC tt, HttpPostedFileBase HINHTT, int? id)

        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var itemTT = db.TINTUC.FirstOrDefault(n => n.MATT == id);
            if (itemTT != null)
            {
                if (HINHTT.ContentLength > 0)
                {
                    //lấy hinh ảnh tư bên ngoài vào
                    var filename = Path.GetFileName(HINHTT.FileName);
                    //sau đo chuyển hinh ảnh đó vào thư mục trong solution
                    var path = Path.Combine(Server.MapPath("~/Content/HinhTinTuc"), filename);
                    //kiểm tra nếu hình anh đã có rồi thi xuất ra thông báo và ngược lại nêu chưa co thi thêm vào
                    tt.MATT = itemTT.MATT;
                    HINHTT.SaveAs(path);
                    tt.HINHTT = filename;
                    db.TINTUC.AddOrUpdate(tt);
                    db.SaveChanges();
                }

            }

            return RedirectToAction("ListTinTuc");
        }

        #endregion

        #region Quản lý sản phẩm


        public ActionResult ListSanPham()
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            if (!checkLogin()) return RedirectToAction("Login");
            List<SANPHAM> listSanPham = db.SANPHAM.ToList();
            foreach (var item in listSanPham)
            {
                var NSX = db.NHASANXUAT.FirstOrDefault(p => p.MANSX == item.MANSX);
                if (NSX != null)
                {
                    item.tenNSX = NSX.TENNSX;
                }
            }
            foreach (var item in listSanPham)
            {
                var NSX = db.LOAISANPHAM.FirstOrDefault(p => p.MALSP == item.MALSP);
                if (NSX != null)
                {
                    item.tenLSP = NSX.TENLOAISP;
                }
            }


            ViewBag.lstSP = listSanPham;
            return View();
        }
        public ActionResult ChiTietSanPham(int? id)
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            if (!checkLogin()) return RedirectToAction("Login");
            var sp = db.SANPHAM.FirstOrDefault(n => n.MASP == id);
            var ctsp = db.CHITIETSP.Where(n => n.MASP == sp.MASP);
            foreach (var item in ctsp)
            {
                var mausac = db.MAUSAC.FirstOrDefault(p => p.MAMAU == item.MAMAU);
                if (mausac != null)
                {
                    item.tenMau = mausac.TENMAU;
                }

            }
            foreach (var item in ctsp)
            {
                var tensp = db.SANPHAM.FirstOrDefault(p => p.MASP == item.MASP);
                if (tensp != null)
                {
                    item.tenSP = tensp.TENSP;
                }

            }


            ViewBag.lstCTSP = ctsp;
            return View();
        }
        //them moi san pham
        [HttpGet]
        public ActionResult ThemMoiSP()
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            ViewBag.MANSX = new SelectList(db.NHASANXUAT.OrderBy(n => n.MANSX), "MANSX", "TENNSX");
            ViewBag.MALSP = new SelectList(db.LOAISANPHAM.OrderBy(n => n.MALSP), "MALSP", "TENLOAISP");
            ViewBag.MAMAU = new SelectList(db.MAUSAC.OrderBy(n => n.MAMAU), "MAMAU", "TENMAU");
            return View();
        }
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ThemMoiSP(SANPHAM sp, HttpPostedFileBase HINHANH, MAUSAC ms, FormCollection f)
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            ViewBag.MANSX = new SelectList(db.NHASANXUAT.OrderBy(n => n.MANSX), "MANSX", "TENNSX");
            ViewBag.MALSP = new SelectList(db.LOAISANPHAM.OrderBy(n => n.MALSP), "MALSP", "TENLOAISP");
            ViewBag.MAMAU = new SelectList(db.MAUSAC.OrderBy(n => n.MAMAU), "MAMAU", "TENMAU");
            //kiem tra hinh anh đa tôn tại trong csdl hay chưa
            if (HINHANH == null)
            {
                ViewBag.upload = "Thêm hình vào đmm";
                return View();
            }
            if (HINHANH.ContentLength > 0)
            {
                //lấy hinh ảnh tư bên ngoài vào
                var filename = Path.GetFileName(HINHANH.FileName);
                //sau đo chuyển hinh ảnh đó vào thư mục trong solution
                var path = Path.Combine(Server.MapPath("~/Content/HinhAnhSP"), filename);
                //kiểm tra nếu hình anh đã có rồi thi xuất ra thông báo và ngược lại nêu chưa co thi thêm vào
                if (System.IO.File.Exists(path))
                {
                    ViewBag.upload = "Hình ảnh đã tồn tại rồi!!";
                    return View();
                }
                else
                {
                    HINHANH.SaveAs(path);
                    sp.HINHANH = filename;

                }

            }

            db.SANPHAM.Add(sp);
            db.SaveChanges();

            CHITIETSP ctsp = new CHITIETSP();
            ctsp.MASP = sp.MASP;
            ctsp.MAMAU = ms.MAMAU;
            ctsp.SOLUONGTON = int.Parse(f["SOLUONGTON"].ToString());
            db.CHITIETSP.Add(ctsp);
            db.SaveChanges();
            return RedirectToAction("ListSanPham");
        }
        public JsonResult LockOrUnlockSP(int? id)
        {
            bool lockOrUnlock = true; // khóa hay mở khóa tài khoản (true: khóa, false: mở khóa)
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var sp = db.SANPHAM.Single(p => p.MASP == id);
            bool Status = false;
            string err = string.Empty;
            if (sp != null)
            {

                if (sp.TRANGTHAI) sp.TRANGTHAI = false; // nếu đang hoạt động thì khóa
                else
                {
                    sp.TRANGTHAI = true;
                    lockOrUnlock = false; // mở khóa tài khoản
                }

                try
                {
                    db.SaveChanges();
                    Status = true;
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                }
            }

            return Json(new
            {
                status = Status,
                errorMessage = err,
                lockStatus = lockOrUnlock
            });



        }

        // sua san pham

        [HttpGet]

        public ActionResult SuaSP(int? id)
        {
            if (!checkLogin()) return RedirectToAction("Login");
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var sp = db.SANPHAM.FirstOrDefault(n => n.MASP == id);
            ViewBag.MANSX = new SelectList(db.NHASANXUAT.OrderBy(n => n.MANSX), "MANSX", "TENNSX", sp.MANSX);
            ViewBag.MALSP = new SelectList(db.LOAISANPHAM.OrderBy(n => n.MALSP), "MALSP", "TENLOAISP", sp.MALSP);
            ViewBag.sp = sp;
            return View();

        }
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult SuaSP(SANPHAM sp, HttpPostedFileBase HINHANH, int? id)
        {

            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            ViewBag.MANSX = new SelectList(db.NHASANXUAT.OrderBy(n => n.MANSX), "MANSX", "TENNSX", sp.MANSX);
            ViewBag.MALSP = new SelectList(db.LOAISANPHAM.OrderBy(n => n.MALSP), "MALSP", "TENLOAISP", sp.MALSP);
            var currentSP = db.SANPHAM.FirstOrDefault(n => n.MASP == id);
            if (currentSP != null)
            {
                if (HINHANH.ContentLength > 0)
                {
                    //lấy hinh ảnh tư bên ngoài vào
                    var filename = Path.GetFileName(HINHANH.FileName);
                    //sau đo chuyển hinh ảnh đó vào thư mục trong solution
                    var path = Path.Combine(Server.MapPath("~/Content/HinhAnhSP"), filename);
                    //kiểm tra nếu hình anh đã có rồi thi xuất ra thông báo và ngược lại nêu chưa co thi thêm vào
                    try 
                    {
                    sp.MASP = currentSP.MASP;
                    HINHANH.SaveAs(path);
                    sp.HINHANH = filename;
                    db.SANPHAM.AddOrUpdate(sp);
                    db.SaveChanges();
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

                }

            }

            return RedirectToAction("ListSanPham");
        }

        [HttpGet]
        public ActionResult SuaCTSP(int? id)
        {
            if (!checkLogin()) return RedirectToAction("Login");
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();

            //var sp = db.SANPHAM.FirstOrDefault(n => n.MASP == );
            var ctsp = db.CHITIETSP.FirstOrDefault(n => n.MASP == id);
            
                var mausac = db.MAUSAC.FirstOrDefault(p => p.MAMAU == ctsp.MAMAU);
                if (mausac != null)
                {
                    ctsp.tenMau = mausac.TENMAU;
                }
                var tensp = db.SANPHAM.FirstOrDefault(p => p.MASP == ctsp.MASP);
                if (tensp != null)
                {
                ctsp.tenSP = tensp.TENSP;   
                }
            ViewBag.ctsp = ctsp;
            return View();
        }
        [HttpPost]
        public ActionResult SuaCTSP(int? id,FormCollection f)
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var itemCtsp = db.CHITIETSP.FirstOrDefault(n => n.MASP == id);
            if(itemCtsp!= null)
            {
                itemCtsp.SOLUONGTON = int.Parse(f["SOLUONGTON"].ToString());
                db.CHITIETSP.AddOrUpdate(itemCtsp);
                db.SaveChanges();
            }
            return RedirectToAction("ChiTietSanPham", "Admin", new { id=itemCtsp.MASP });
        }
        #endregion



        #region quản lý loai sản phẩm
        [HttpGet]
        public ActionResult listloaiSP()
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            if (!checkLogin()) return RedirectToAction("Login");
            List<LOAISANPHAM> listloaiSP = db.LOAISANPHAM.ToList();
            ViewBag.lstlsp = listloaiSP;
            return View();
        }

        [HttpGet]
        public ActionResult addLsp()
        {
            if (!checkLogin()) return RedirectToAction("Login");
            return View();
        }
        [HttpPost]
        public ActionResult addLsp(HttpPostedFileBase ICON, LOAISANPHAM lsp)
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            if (ICON.ContentLength > 0)
            {
                var filename = Path.GetFileName(ICON.FileName);
                //lấy hình ảnh bỏ vào trong thư mục
                var path = Path.Combine(Server.MapPath("~/Content/HinhAnhSP"), filename);
                //kiểm tra tồn tại
                if (System.IO.File.Exists(path))
                {
                    ViewBag.upload = "Hình ảnh đã tồn tại";
                }
                else
                {
                    ICON.SaveAs(path);
                    lsp.ICON = filename;
                    lsp.TRANGTHAI = true;
                    db.LOAISANPHAM.Add(lsp);
                    db.SaveChanges();
                }

            }

            return RedirectToAction("listloaisp", "Admin");
        }

        public JsonResult LockOrUnlocklsp(int? id)
        {
            bool lockOrUnlock = true; // khóa hay mở khóa tài khoản (true: khóa, false: mở khóa)
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var lsp = db.LOAISANPHAM.FirstOrDefault(n => n.MALSP == id);
            bool Status = false;
            string err = string.Empty;
            if (lsp != null)
            {
                if (lsp.TRANGTHAI) lsp.TRANGTHAI = false; // nếu đang hoạt động thì khóa
                else
                {
                    lsp.TRANGTHAI = true;
                    lockOrUnlock = false; // mở khóa tài khoản
                }

                try
                {
                    db.SaveChanges();
                    Status = true;
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                }
            }

            return Json(new
            {
                status = Status,
                errorMessage = err,
                lockStatus = lockOrUnlock
            });

        }
        [HttpGet]
        public ActionResult SuaLSP(int? id)
        {
            if (!checkLogin()) return RedirectToAction("Login");
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var lsp = db.LOAISANPHAM.FirstOrDefault(n => n.MALSP == id);
            ViewBag.lsp = lsp;
            return View();
        }
        [HttpPost]
        public ActionResult SuaLSP(LOAISANPHAM lsp, HttpPostedFileBase ICON, int? id)
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var currentLSP = db.LOAISANPHAM.FirstOrDefault(n => n.MALSP == id);
            if (currentLSP != null)
            {
                if (ICON.ContentLength > 0)
                {
                    //lấy hinh ảnh tư bên ngoài vào
                    var filename = Path.GetFileName(ICON.FileName);
                    //sau đo chuyển hinh ảnh đó vào thư mục trong solution
                    var path = Path.Combine(Server.MapPath("~/Content/HinhAnhSP"), filename);
                    //kiểm tra nếu hình anh đã có rồi thi xuất ra thông báo và ngược lại nêu chưa co thi thêm vào
                    lsp.MALSP = currentLSP.MALSP;
                    ICON.SaveAs(path);
                    lsp.ICON = filename;
                    db.LOAISANPHAM.AddOrUpdate(lsp);
                    db.SaveChanges();
                }

            }

            return RedirectToAction("listloaiSP");
        }

        #endregion



        #region quản lý nhà sản xuất
        public ActionResult ListNSX()
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            if (!checkLogin()) return RedirectToAction("Login");
            List<NHASANXUAT> listNSX = db.NHASANXUAT.ToList();
            ViewBag.lstNSX = listNSX;
            return View();
        }


        [HttpGet]
        public ActionResult ThemNSX()
        {
            if (!checkLogin()) return RedirectToAction("Login");
            return View();
        }
        [HttpPost]
        public ActionResult ThemNSX(HttpPostedFileBase Icon, NHASANXUAT nsx)
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            if (Icon.ContentLength > 0)
            {
                var filename = Path.GetFileName(Icon.FileName);

                //lấy hình ảnh bỏ vào trong thư mục
                var path = Path.Combine(Server.MapPath("~/Content/HinhAnhSP"), filename);
                //kiểm tra tồn tại
                if (System.IO.File.Exists(path))
                {
                    ViewBag.upload = "Hình ảnh đã tồn tại";
                }
                else
                {
                    Icon.SaveAs(path);
                    nsx.Icon = filename;
                    nsx.TRANGTHAI = true;
                    db.NHASANXUAT.Add(nsx);
                    db.SaveChanges();
                }

            }

            return RedirectToAction("ListNSX");
        }

        public JsonResult LockOrUnlockNSX(int? id)
        {
            bool lockOrUnlock = true; // khóa hay mở khóa tài khoản (true: khóa, false: mở khóa)
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var nsx = db.NHASANXUAT.FirstOrDefault(n => n.MANSX == id);
            bool Status = false;
            string err = string.Empty;
            if (nsx != null)
            {
                if (nsx.TRANGTHAI) nsx.TRANGTHAI = false; // nếu đang hoạt động thì khóa
                else
                {
                    nsx.TRANGTHAI = true;
                    lockOrUnlock = false; // mở khóa tài khoản
                }

                try
                {
                    db.SaveChanges();
                    Status = true;
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                }
            }

            return Json(new
            {
                status = Status,
                errorMessage = err,
                lockStatus = lockOrUnlock
            });

        }

        [HttpGet]
        public ActionResult SuaNSX(int? id)
        {
            if (!checkLogin()) return RedirectToAction("Login");
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var nsx = db.NHASANXUAT.FirstOrDefault(n => n.MANSX == id);
            ViewBag.nsx = nsx;
            return View();
        }
        [HttpPost]
        public ActionResult SuaNSX(HttpPostedFileBase Icon, NHASANXUAT nsx, int? id)
        {
            if (!checkLogin()) return RedirectToAction("Login");
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();

            var currentNSX = db.NHASANXUAT.FirstOrDefault(n => n.MANSX == id);
            if (currentNSX != null)
            {
                if (Icon.ContentLength > 0)
                {
                    //lấy hinh ảnh tư bên ngoài vào
                    var filename = Path.GetFileName(Icon.FileName);
                    //sau đo chuyển hinh ảnh đó vào thư mục trong solution
                    var path = Path.Combine(Server.MapPath("~/Content/HinhAnhSP"), filename);
                    //kiểm tra nếu hình anh đã có rồi thi xuất ra thông báo và ngược lại nêu chưa co thi thêm vào
                    nsx.MANSX = currentNSX.MANSX;
                    Icon.SaveAs(path);
                    nsx.Icon = filename;
                    db.NHASANXUAT.AddOrUpdate(nsx);
                    db.SaveChanges();
                }

            }

            return RedirectToAction("ListNSX");

        }
        #endregion
        #region phần quản lý góp ý
        public ActionResult ListGopy()
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            if (!checkLogin()) return RedirectToAction("Login");
            List<GOPY> listgopy = db.GOPY.ToList();
            ViewBag.lstgy = listgopy;
            return View();
        }
       
        [HttpGet]
        public ActionResult phanhoiGopy(int? id,bool type)
        {
            if (!checkLogin()) return RedirectToAction("Login");
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var gy = db.GOPY.FirstOrDefault(n => n.MAGOPY == id);
            var checkNV = db.NHANVIEN.FirstOrDefault(n => n.MANV == gy.MANV);
            if (checkNV != null)
            {
                ViewBag.tennv = checkNV.TENNV;
            }
            
            ViewBag.gy = gy;
            ViewBag.type = type;
            return View();
        }
        [HttpPost]
        public ActionResult phanhoiGopy(int? id,FormCollection f)
        {
            if (!checkLogin()) return RedirectToAction("Login");
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            NHANVIEN nv = (NHANVIEN)Session["Admin"];
            var gy = db.GOPY.FirstOrDefault(n => n.MAGOPY == id);
            string noidung = f["NOIDUNGPHANHOI"].ToString();
            if (gy != null)
            {
                
                gy.NGAYGUIPH = DateTime.Now;
                gy.NOIDUNGPHANHOI = noidung;
                gy.TRANGTHAI = true;
                gy.MANV = nv.MANV;
                string subject = "Phản hồi góp ý từ Multi Shop";
                string message = "Tên khách hàng: "+gy.TEN
                            +    "\nNgày gửi: "+gy.NGAYGUI.Value.ToString("dd/MM/yyyy")
                            +    "\nNội dung: "+gy.NOIDUNG
                            +    "\nNội dung phản hồi: \n"+ noidung
                            +    "\n From Multi Shop with love <3";
                sendmail(gy.EMAIL, subject, message);
                db.GOPY.AddOrUpdate(gy);
                db.SaveChanges();
                
                
            }
            return RedirectToAction("ListGopy");
        }



        #endregion
    }


}
