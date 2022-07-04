
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanlaptop.Models;
namespace WebBanlaptop.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public List<Cart> GetCart() // tạo list giỏ hàng hoặc lấy giỏ hàng
        {
            List<Cart> lstSPInCart = Session["Cart"] as List<Cart>;
            if (lstSPInCart == null)
            {
                lstSPInCart = new List<Cart>();
                Session["Cart"] = lstSPInCart;
            }
            return lstSPInCart;
        }
        private int tongSoLuong() // lấy tổng số lượng trong giỏ hàng
        {
            List<Cart> lstgh = Session["Cart"] as List<Cart>;
            if (lstgh == null)
            {
                return 0;
            }
            return lstgh.Sum(n => n.soluong);
        }

        private double tongTien() // lấy tổng tiền của giỏ hàng
        {
            double tongTien = 0;
            List<Cart> listProductInCart = Session["Cart"] as List<Cart>;
            if (listProductInCart != null)
            {
                tongTien = listProductInCart.Sum(n => n.tongtien);
            }
            return tongTien;
        }

        private void capNhatSL(CHITIETHD cthd) // cập nhật số lượng tồn của mỗi giày
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var shoes = db.CHITIETSP.Single(p => p.MASP == cthd.MASP && p.MAMAU == cthd.MAMAU);
            shoes.SOLUONGTON = shoes.SOLUONGTON - cthd.SOLUONG;
            db.SaveChanges();
        }

        public ActionResult Cart() // giỏ hàng
        {
            List<Cart> lstSPInCart = GetCart();
            if (lstSPInCart.Count == 0)
            {
                return RedirectToAction("enmtyCart", "Cart");
            }
            KHACHHANG kh = (KHACHHANG)Session["User"];
            ViewBag.tsl = tongSoLuong();
            ViewBag.tt = tongTien();
            if (kh != null)
            {
                ViewBag.tenkh = kh.TENKH;
            }
            
            return View(lstSPInCart);
        }
        public ActionResult enmtyCart() // giỏ hàng
        {
            
            return View();
        }
        public ActionResult _CartPartial() // partial giỏ hàng hiện số lượng
        {
            ViewBag.tsl = tongSoLuong();
            return PartialView();
        }
        [HttpPost]
        public ActionResult themGH(int? masp, string strURl) // thêm sản phẩm vào giỏ hàng
        {
            int? mamau = null;
            int sl = 1;
            List<Cart> listProductInCart = GetCart();
            if (Request.Form["maMau"] == null)
            {
                ViewBag.loimau = "Vui Lòng Chọn Size Giày";
                return Redirect(strURl);
            }
            mamau = Int32.Parse(Request.Form["maMau"].ToString());
            sl = Int32.Parse(Request.Form["soluong"].ToString());
            Cart product = listProductInCart.Find(n => n.masp == masp && n.mamau == mamau);
            if (product == null)
            {
                product = new Cart(masp, mamau,sl);
                listProductInCart.Add(product);
                return Redirect(strURl);
            }
            else
            {
                product.soluong++;
                return Redirect(strURl);
            }
        }
        [HttpPost]
        public ActionResult capNhatSLSP( int? masp , int? mamau, FormCollection f)
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            List<Cart> listProductInCart = GetCart();
            var sp = listProductInCart.SingleOrDefault(n => n.masp == masp && n.mamau == mamau);
            if(sp != null)
            {
                sp.soluong = int.Parse(f["soluong"].ToString());
            }

            return RedirectToAction("Cart");
        }
        public ActionResult xoa1SPInCart(int? masp, int? mamau) // xóa 1 món hàng ra khỏi giỏ hàng
        {
            List<Cart> listProductInCart = GetCart();
            Cart product = listProductInCart.SingleOrDefault(n => n.masp == masp && n.mamau == mamau);
            if (product != null)
            {
                listProductInCart.RemoveAll(n => n.masp == masp && n.mamau == mamau);
            }
            if (listProductInCart.Count == 0)
            {
                return RedirectToAction("enmtyCart", "Cart");
            }
            return RedirectToAction("Cart");
        }
        public ActionResult xoaGH()
        {
            List<Cart> listProductInCart = GetCart();
            listProductInCart.Clear();
            return RedirectToAction("enmtyCart", "Cart");
        }
        public static string RandomChar()
        {
            var chars = "0123456789";
            var stringChars = new char[4];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }

        [HttpPost]
        public ActionResult DatHang(string strURL, FormCollection f)
        {

            if (Session["User"] == null || Session["User"].ToString() == "")
                return RedirectToAction("Login", "User", new { @strURL = strURL }); // truyền url để lưu trang web quay về sau khi login
            if (Session["Cart"] == null)
                return RedirectToAction("enmtyCart", "Cart");
            string tenKh = f["cusName"].ToString();
            string soDT = f["cusPhone"].ToString();
            string diaChi = f["cusStreet"].ToString();
            string yeuCau = f["cusRequest"].ToString();
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            
            HOADON ddh = new HOADON();
            KHACHHANG kh = (KHACHHANG)Session["User"];
            List<Cart> gh = GetCart();
            var checkMHD = db.HOADON;
            ddh.MAHD = "DH-" + DateTime.Now.ToString("ddMMyy") + "-" + RandomChar();
            foreach(var item in checkMHD)
            {
                if (item.MAHD.Equals(ddh.MAHD))
                {
                    ddh.MAHD = "DH." + DateTime.Now.ToString("ddMMyy") + "." + RandomChar();
                }
            }
            ddh.NGAYLAP = DateTime.Now;
            ddh.NGAYGIAO = DateTime.Now.AddDays(10);
            ddh.TONGTIEN = (decimal)tongTien();
            ddh.TRANGTHAI = 0;
            ddh.MAKH = kh.MAKH;
            ddh.DIACHIGIAOHANG = diaChi;
            ddh.YEUCAUKHAC = yeuCau;
            ddh.TENKH = tenKh;
            ddh.SDTKH = soDT;
            db.HOADON.Add(ddh);
            db.SaveChanges();
            
            foreach (var item in gh)
            {
                CHITIETHD cthd = new CHITIETHD();
                cthd.MAHD = ddh.MAHD;
                cthd.MAMAU = item.mamau;
                cthd.MASP = item.masp;
                cthd.SOLUONG = item.soluong;
                cthd.DONGIA = (decimal?)item.dongia;
                capNhatSL(cthd);
                db.CHITIETHD.Add(cthd);
            }
            db.SaveChanges();

            Session["Cart"] = null;
            return RedirectToAction("XacNhanDatHang", "Cart",ddh);
        }
        public ActionResult XacNhanDatHang(HOADON ddh)
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            var dh = db.HOADON.FirstOrDefault(n => n.MAHD == ddh.MAHD);
            return View(dh);
        }
    }
}