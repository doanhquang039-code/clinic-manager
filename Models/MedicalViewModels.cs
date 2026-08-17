using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MyMvcApp.Models;

public class LichKhamViewModel
{
    public LichKham LichKham { get; set; } = new LichKham();
    public IEnumerable<SelectListItem>? BenhNhanList { get; set; }
    public IEnumerable<SelectListItem>? BacSiList { get; set; }
}

public class PhieuKhamViewModel
{
    public PhieuKham PhieuKham { get; set; } = new PhieuKham();
    public IEnumerable<SelectListItem>? LichKhamList { get; set; }
    public IEnumerable<SelectListItem>? BenhNhanList { get; set; }
    public IEnumerable<SelectListItem>? BacSiList { get; set; }
}
