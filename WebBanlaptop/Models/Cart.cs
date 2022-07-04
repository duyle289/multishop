using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanlaptop.Models
{
    public class Cart
    {
        public int? masp { get; set; }
        public int? mamau { get; set; }
        public string tensp { get; set; }
        public string hinhanh { get; set; }
        public int soluong { get; set; }
        public Double dongia { get; set; }
        public Double tongtien
        {
            get { return soluong * dongia; }
        }
        public string tenmau { get; set; }
        public int? slton { get; set; } // sồ lượng tồn

        public Cart(int? Masp, int? Mamau, int sl)
        {
            QLBANLAPTOPEntities db = new QLBANLAPTOPEntities();
            masp = Masp;
            mamau = Mamau;
            SANPHAM product = db.SANPHAM.Single(n => n.MASP == masp);
            tensp = product.TENSP;
            hinhanh = product.HINHANH;
            dongia = double.Parse(product.DONGIA.ToString());
            soluong = sl;

            var shoesSize = db.MAUSAC.FirstOrDefault(p => p.MAMAU == mamau);
            tenmau = shoesSize.TENMAU;

            var maxAmount = db.CHITIETSP.FirstOrDefault(p => p.MASP == masp && p.MAMAU == mamau);
            slton = maxAmount.SOLUONGTON;
        }

    }
}