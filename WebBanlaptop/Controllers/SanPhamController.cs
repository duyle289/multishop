using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebBanlaptop.Models;

namespace WebBanlaptop.Controllers
{
    public class SanPhamController : Controller
    {
        QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
        // GET: SanPham
        public ActionResult danhMucSP()
        {
            var lstLSP = db.SANPHAM;
            return PartialView(lstLSP);
        }
        public ActionResult showSPNoiBatPartial()
        {
            
            return PartialView();
        }
        [HttpPost]
        public ActionResult showSPPartial()
        {
            return PartialView();
        }
        public ActionResult showmauSP()
        {
            var nsx = db.NHASANXUAT.ToList();
            return PartialView(nsx);
        }
        public ActionResult showSP(bool type)
        {
            if (type)
            {
                var lstSPNB = db.SANPHAM.Where(n => n.SPNOIBAT == true);
                ViewBag.lstSP = lstSPNB;
                return View();
            }
            else
            {
                var lstSPM = db.SANPHAM.Where(n => n.SANPHAMMOI == true);
                ViewBag.lstSP = lstSPM;
                return View();
            }
            
            
        }
        public ActionResult showAllSP()
        {
            var lstSP = db.SANPHAM.Where(n=>n.LOAISANPHAM.TRANGTHAI==true&&n.NHASANXUAT.TRANGTHAI==true);
            if (lstSP.Count() == 0)
            {
                return HttpNotFound();
            }
            ViewBag.lstSP = lstSP;
            return View();
        }
        public ActionResult showSPTheoNSX(int? MaLSP, int? MaNXS)
        {
            if (MaLSP == null || MaNXS == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var lstSP = db.SANPHAM.Where(n => n.MALSP == MaLSP && n.MANSX == MaNXS);
            if (lstSP.Count() == 0)
            {
                return HttpNotFound();
            }
            ViewBag.lstSP = lstSP;
            return View();
        }
        public ActionResult showSPTheoLoai(int? MaLSP)
        {
            if (MaLSP == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var lstSP = db.SANPHAM.Where(n => n.MALSP == MaLSP);
            if (lstSP.Count() == 0)
            {
                return HttpNotFound();
            }
            ViewBag.lstSP = lstSP;
            return View();
        }

        public JsonResult SapXep(string type,List<SANPHAM> model)
        {
            if (type.Equals("LowHigh"))
            {
                var lstSP = model.OrderBy(n => n.DONGIA);
                ViewBag.lstSP = lstSP;
                //return View(lstSP);
            }
            else if (type.Equals("HighLow"))
            {
                var lstSP = model.OrderByDescending(n => n.DONGIA);
                ViewBag.lstSP = lstSP;
                //return View(lstSP);
            }
            return Json(type);
        }
        public ActionResult showSPTheoNSX1(int? MaLSP)
        {
            if (MaLSP == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var lstSP = db.SANPHAM.Where(n => n.MANSX == MaLSP);
            if (lstSP.Count() == 0)
            {
                return HttpNotFound();
            }
            ViewBag.lstSP = lstSP;
            return View("showSPTheoNSX", lstSP);
        }
        public ActionResult chiTietSP(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SANPHAM sp = db.SANPHAM.SingleOrDefault(n => n.MASP == id);
            
            var SPCungLoai = db.SANPHAM.Where(n=>n.MALSP == sp.MALSP).OrderByDescending(n=>n.DONGIA).Take(10);
            ViewBag.SPCungLoai = SPCungLoai;
            var NSX = db.NHASANXUAT.FirstOrDefault(p => p.MANSX == sp.MANSX);
            var maMau = db.CHITIETSP.Where(p => p.MASP == id).Select(p => p.MAMAU).ToList();
            var soLuongTon = db.CHITIETSP.Where(p => p.MASP == id).Select(p => p.SOLUONGTON).ToList();
            var tenMau = db.MAUSAC.Select(p => p.TENMAU).ToList();
            var demSanPham = soLuongTon.Sum(p => p.Value); // đếm list số lượng tồn của các size
            if (NSX != null)
            {
                sp.maNSX = NSX.MANSX;
                sp.tenNSX = NSX.TENNSX;
                sp.maMau = maMau;
                sp.soluongton = soLuongTon;
                sp.mauSP = tenMau;
            }
            if (demSanPham == 0) sp.tinhTrangSanPham = false;
            else sp.tinhTrangSanPham = true;
            if (sp == null)
            {
                return HttpNotFound();
            }
            return View(sp);
        }
        [HttpPost]
        public ActionResult Review(int? id,FormCollection f, string strUrl,string num)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KHACHHANG kh = (KHACHHANG)Session["User"];
            if (Session["User"] == null || Session["User"].ToString() == "")
                return RedirectToAction("Login", "User", new { @strURL = strUrl }); // truyền url để lưu trang web quay về sau khi login
            
            BINHLUAN bl = new BINHLUAN();
            bl.MASP = id;
            bl.MAKH = kh.MAKH;
            bl.NGAYGUI = DateTime.Now;
            bl.NOIDUNG = f["noidung"].ToString();
            bl.TENKH = kh.TENKH;
            bl.TRANGTHAI = true;
            db.BINHLUAN.Add(bl);
            db.SaveChanges();
            return Redirect(strUrl);
        }
        public ActionResult showReview(int? id, string strUrl)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var bl = db.BINHLUAN.Where(n => n.MASP == id).ToList();

            return PartialView(bl);
        }

    }
}