using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanlaptop.Models;

namespace WebBanlaptop.Controllers
{
    public class HomeController : Controller
    {

        QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
        private char[] charsToTrim;
        public ActionResult Index()
        {
            var lstSPNB = db.SANPHAM.Where(n => n.SPNOIBAT == true);
            ViewBag.lstSPNB = lstSPNB;
            var lstSPM = db.SANPHAM.Where(n => n.SANPHAMMOI == true);
            ViewBag.lstSPM = lstSPM;
            return View();
        }
        public ActionResult navbarPartial()
        {
            var lstSP = db.SANPHAM;
            return PartialView(lstSP);
        }
        public ActionResult showUser()
        {
            return PartialView();
        }
        #region Tìm kiếm sản phẩm
        [HttpPost]
        public ActionResult KQTimKiem(string TuKhoa)
        {
            string temp = (string)Session["tukhoa"];
            if (temp != null)
            {
                if (temp.Equals(TuKhoa))
                {
                    Session["tukhoa"] = temp;
                }
            }
            
            Session["tukhoa"] = TuKhoa;
            var listSP = db.SANPHAM.Where(n => n.TENSP.Contains(TuKhoa));
            int i = listSP.Count();
            if (i == 0)
            {
                return RedirectToAction("KQTKEmty","Home");
            }
            ViewBag.lstSP = listSP.OrderBy(n => n.TENSP).ToList();
            
            return View();
        }
        public ActionResult KQTKEmty()
        {
            return View();
        }
        #endregion
        #region Giới thiệu
        public ActionResult GioiThieu()
        {
            return View();
        }
        #endregion

        #region tin tức
        public ActionResult TinTuc()
        {
            var listTintuc = new PostTT().ListAllTinTuc();
            return View(listTintuc);
        }
        public ActionResult NoiDung(long id)
        {
            var tt = new PostTT().ViewDetail(id);
            return View(tt);
        }
        #endregion


        #region Liên hệ Góp ý
        public ActionResult Lienhe()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Gopy(FormCollection collection)
        {
            var txtName = collection["txtName"].Trim();
            var txtEmail = collection["txtEmail"].Trim();
            var txtNoidung = collection["txtNoidung"].Trim();
            if (String.IsNullOrEmpty(txtName) || String.IsNullOrEmpty(txtEmail) || String.IsNullOrEmpty(txtNoidung))
            {
                ViewBag.Error = "Vui lòng điền đủ thông tin";
                return View("Lienhe");
            }
            var name = txtName + ""; // nối tên
            name = name.Trim(charsToTrim);
            var comment = new GOPY();
            comment.TEN = name;
            comment.EMAIL = txtEmail;
            comment.NOIDUNG = txtNoidung;
            comment.TRANGTHAI = false;
            comment.NGAYGUI = DateTime.Now;
            db.GOPY.Add(comment);
            db.SaveChanges();
            ViewBag.Success = "Gửi thành công";


            return View("Lienhe");
        }
        #endregion
        #region Giới thiệu thành viên
        public ActionResult ThanhVien()
        {
            return View();
        }
        #endregion
    }
}