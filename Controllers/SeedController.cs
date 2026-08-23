using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Models;

namespace MyMvcApp.Controllers;

/// <summary>
/// REST API nội bộ — dùng để seed/thêm dữ liệu qua Postman
/// Base URL: /api/seed
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SeedController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SeedController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ================================================================
    // DTO classes
    // ================================================================
    public record DoctorDto(
        string HoTen,
        string SoDienThoai,
        string Email,
        string MatKhau,
        int? MaChuyenKhoa,
        bool TrangThai = true
    );

    public record CashierDto(
        string HoTen,
        string SoDienThoai,
        string Email,
        string MatKhau,
        bool TrangThai = true
    );

    public record PatientDto(
        string HoTen,
        string NgaySinh,      // "yyyy-MM-dd"
        string GioiTinh,      // "Nam" | "Nữ"
        string SoDienThoai,
        string Email,
        string MatKhau,
        string? CCCD,
        string? DiaChi,
        string? DiUng
    );

    public record DrugDto(
        string TenThuoc,
        string DonViTinh,
        decimal DonGia,
        int SoLuongTon,
        string? HanSuDung     // "yyyy-MM-dd" hoặc null
    );

    public record ServiceDto(
        string TenDichVu,
        string MoTa,
        decimal DonGia
    );

    // ================================================================
    // GET — Kiểm tra API còn sống
    // ================================================================
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { status = "ok", time = DateTime.Now, message = "MediCore Seed API" });

    // ================================================================
    // GET — Danh sách nhanh
    // ================================================================
    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors()
    {
        var list = await _context.NguoiDungs
            .Include(n => n.ChuyenKhoa)
            .Where(n => n.Role == "BacSi")
            .Select(n => new {
                n.MaNguoiDung, n.HoTen, n.Email, n.SoDienThoai,
                ChuyenKhoa = n.ChuyenKhoa!.TenChuyenKhoa,
                n.TrangThai
            })
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("cashiers")]
    public async Task<IActionResult> GetCashiers()
    {
        var list = await _context.NguoiDungs
            .Where(n => n.Role == "ThuNgan")
            .Select(n => new { n.MaNguoiDung, n.HoTen, n.Email, n.SoDienThoai, n.TrangThai })
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("patients")]
    public async Task<IActionResult> GetPatients()
    {
        var list = await _context.BenhNhans
            .Select(b => new {
                b.MaBenhNhan, b.HoTen, b.NgaySinh, b.GioiTinh,
                b.SoDienThoai, b.Email, b.DiaChi, b.DiUng
            })
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("drugs")]
    public async Task<IActionResult> GetDrugs()
    {
        var list = await _context.Thuocs
            .Select(t => new {
                t.MaThuoc, t.TenThuoc, t.DonViTinh,
                t.DonGia, t.SoLuongTon, t.HanSuDung, t.TrangThai
            })
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("specialties")]
    public async Task<IActionResult> GetSpecialties()
    {
        var list = await _context.ChuyenKhoas
            .Select(c => new { c.MaChuyenKhoa, c.TenChuyenKhoa, c.MoTa })
            .ToListAsync();
        return Ok(list);
    }

    // ================================================================
    // POST — Thêm Bác sĩ
    // ================================================================
    /// <summary>
    /// POST /api/seed/doctors
    /// Body: { "hoTen": "BS. Abc", "soDienThoai": "09xxxxxxxx", "email": "abc@medicore.com",
    ///         "matKhau": "123456", "maChuyenKhoa": 1, "trangThai": true }
    /// </summary>
    [HttpPost("doctors")]
    public async Task<IActionResult> AddDoctor([FromBody] DoctorDto dto)
    {
        if (await _context.NguoiDungs.AnyAsync(n => n.Email == dto.Email))
            return Conflict(new { error = $"Email '{dto.Email}' đã tồn tại!" });

        if (await _context.NguoiDungs.AnyAsync(n => n.SoDienThoai == dto.SoDienThoai))
            return Conflict(new { error = $"SĐT '{dto.SoDienThoai}' đã tồn tại!" });

        var entity = new NguoiDung
        {
            HoTen       = dto.HoTen,
            SoDienThoai = dto.SoDienThoai,
            Email       = dto.Email,
            MatKhau     = dto.MatKhau,
            Role        = "BacSi",
            MaChuyenKhoa = dto.MaChuyenKhoa,
            TrangThai   = dto.TrangThai,
            NgayTao     = DateTime.Now
        };

        _context.NguoiDungs.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDoctors), new { id = entity.MaNguoiDung },
            new { message = "Thêm bác sĩ thành công!", maNguoiDung = entity.MaNguoiDung, hoTen = entity.HoTen });
    }

    // ================================================================
    // POST — Thêm Thu ngân
    // ================================================================
    /// <summary>
    /// POST /api/seed/cashiers
    /// Body: { "hoTen": "Thu Ngân X", "soDienThoai": "09xxxxxxxx", "email": "x@medicore.com", "matKhau": "123456" }
    /// </summary>
    [HttpPost("cashiers")]
    public async Task<IActionResult> AddCashier([FromBody] CashierDto dto)
    {
        if (await _context.NguoiDungs.AnyAsync(n => n.Email == dto.Email))
            return Conflict(new { error = $"Email '{dto.Email}' đã tồn tại!" });

        if (await _context.NguoiDungs.AnyAsync(n => n.SoDienThoai == dto.SoDienThoai))
            return Conflict(new { error = $"SĐT '{dto.SoDienThoai}' đã tồn tại!" });

        var entity = new NguoiDung
        {
            HoTen       = dto.HoTen,
            SoDienThoai = dto.SoDienThoai,
            Email       = dto.Email,
            MatKhau     = dto.MatKhau,
            Role        = "ThuNgan",
            TrangThai   = dto.TrangThai,
            NgayTao     = DateTime.Now
        };

        _context.NguoiDungs.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCashiers), new { id = entity.MaNguoiDung },
            new { message = "Thêm thu ngân thành công!", maNguoiDung = entity.MaNguoiDung, hoTen = entity.HoTen });
    }

    // ================================================================
    // POST — Thêm Bệnh nhân
    // ================================================================
    /// <summary>
    /// POST /api/seed/patients
    /// Body: { "hoTen": "Nguyễn Văn A", "ngaySinh": "1990-05-20", "gioiTinh": "Nam",
    ///         "soDienThoai": "098xxxxxxx", "email": "a@gmail.com", "matKhau": "123456",
    ///         "cccd": "0123456789", "diaChi": "Hà Nội", "diUng": null }
    /// </summary>
    [HttpPost("patients")]
    public async Task<IActionResult> AddPatient([FromBody] PatientDto dto)
    {
        if (await _context.BenhNhans.AnyAsync(b => b.SoDienThoai == dto.SoDienThoai))
            return Conflict(new { error = $"SĐT '{dto.SoDienThoai}' đã tồn tại!" });

        if (!string.IsNullOrEmpty(dto.Email) && await _context.BenhNhans.AnyAsync(b => b.Email == dto.Email))
            return Conflict(new { error = $"Email '{dto.Email}' đã tồn tại!" });

        if (!DateTime.TryParse(dto.NgaySinh, out var ngaySinh))
            return BadRequest(new { error = "Ngày sinh không hợp lệ. Dùng format 'yyyy-MM-dd'." });

        var entity = new BenhNhan
        {
            HoTen       = dto.HoTen,
            NgaySinh    = ngaySinh,
            GioiTinh    = dto.GioiTinh,
            SoDienThoai = dto.SoDienThoai,
            Email       = dto.Email,
            MatKhau     = dto.MatKhau,
            CCCD        = dto.CCCD,
            DiaChi      = dto.DiaChi,
            DiUng       = dto.DiUng,
            NgayTao     = DateTime.Now
        };

        _context.BenhNhans.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPatients), new { id = entity.MaBenhNhan },
            new { message = "Thêm bệnh nhân thành công!", maBenhNhan = entity.MaBenhNhan, hoTen = entity.HoTen });
    }

    // ================================================================
    // POST — Thêm Thuốc vào kho
    // ================================================================
    /// <summary>
    /// POST /api/seed/drugs
    /// Body: { "tenThuoc": "Amoxicillin 500mg", "donViTinh": "Viên", "donGia": 3000,
    ///         "soLuongTon": 200, "hanSuDung": "2027-12-31" }
    /// </summary>
    [HttpPost("drugs")]
    public async Task<IActionResult> AddDrug([FromBody] DrugDto dto)
    {
        DateTime? hanSuDung = null;
        if (!string.IsNullOrEmpty(dto.HanSuDung))
        {
            if (!DateTime.TryParse(dto.HanSuDung, out var parsedDate))
                return BadRequest(new { error = "Hạn sử dụng không hợp lệ. Dùng format 'yyyy-MM-dd'." });
            hanSuDung = parsedDate;
        }

        var entity = new Thuoc
        {
            TenThuoc    = dto.TenThuoc,
            DonViTinh   = dto.DonViTinh,
            DonGia      = dto.DonGia,
            SoLuongTon  = dto.SoLuongTon,
            HanSuDung   = hanSuDung,
            TrangThai   = dto.SoLuongTon > 0 ? "ConHang" : "HetHang"
        };

        _context.Thuocs.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDrugs), new { id = entity.MaThuoc },
            new { message = "Thêm thuốc vào kho thành công!", maThuoc = entity.MaThuoc, tenThuoc = entity.TenThuoc });
    }

    // ================================================================
    // POST — Batch: Thêm nhiều thuốc cùng lúc
    // ================================================================
    /// <summary>
    /// POST /api/seed/drugs/batch
    /// Body: [ { "tenThuoc": "...", ... }, { ... } ]
    /// </summary>
    [HttpPost("drugs/batch")]
    public async Task<IActionResult> AddDrugsBatch([FromBody] List<DrugDto> dtos)
    {
        if (dtos == null || !dtos.Any())
            return BadRequest(new { error = "Danh sách thuốc trống!" });

        var added = new List<object>();
        foreach (var dto in dtos)
        {
            DateTime? hanSuDung = null;
            if (!string.IsNullOrEmpty(dto.HanSuDung))
                DateTime.TryParse(dto.HanSuDung, out var pd);

            if (!string.IsNullOrEmpty(dto.HanSuDung) && DateTime.TryParse(dto.HanSuDung, out var parsedDate))
                hanSuDung = parsedDate;

            var entity = new Thuoc
            {
                TenThuoc   = dto.TenThuoc,
                DonViTinh  = dto.DonViTinh,
                DonGia     = dto.DonGia,
                SoLuongTon = dto.SoLuongTon,
                HanSuDung  = hanSuDung,
                TrangThai  = dto.SoLuongTon > 0 ? "ConHang" : "HetHang"
            };
            _context.Thuocs.Add(entity);
            added.Add(new { tenThuoc = dto.TenThuoc });
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = $"Đã thêm {added.Count} thuốc vào kho!", items = added });
    }

    // ================================================================
    // POST — Batch: Thêm nhiều bác sĩ cùng lúc
    // ================================================================
    [HttpPost("doctors/batch")]
    public async Task<IActionResult> AddDoctorsBatch([FromBody] List<DoctorDto> dtos)
    {
        if (dtos == null || !dtos.Any())
            return BadRequest(new { error = "Danh sách bác sĩ trống!" });

        var added = new List<object>();
        var errors = new List<object>();

        foreach (var dto in dtos)
        {
            if (await _context.NguoiDungs.AnyAsync(n => n.Email == dto.Email || n.SoDienThoai == dto.SoDienThoai))
            {
                errors.Add(new { dto.HoTen, reason = "Email hoặc SĐT đã tồn tại" });
                continue;
            }

            var entity = new NguoiDung
            {
                HoTen        = dto.HoTen,
                SoDienThoai  = dto.SoDienThoai,
                Email        = dto.Email,
                MatKhau      = dto.MatKhau,
                Role         = "BacSi",
                MaChuyenKhoa = dto.MaChuyenKhoa,
                TrangThai    = dto.TrangThai,
                NgayTao      = DateTime.Now
            };
            _context.NguoiDungs.Add(entity);
            added.Add(new { dto.HoTen, dto.Email });
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = $"Đã thêm {added.Count} bác sĩ.", added, errors });
    }

    // ================================================================
    // POST — Batch: Thêm nhiều bệnh nhân cùng lúc
    // ================================================================
    [HttpPost("patients/batch")]
    public async Task<IActionResult> AddPatientsBatch([FromBody] List<PatientDto> dtos)
    {
        if (dtos == null || !dtos.Any())
            return BadRequest(new { error = "Danh sách bệnh nhân trống!" });

        var added = new List<object>();
        foreach (var dto in dtos)
        {
            if (await _context.BenhNhans.AnyAsync(b => b.SoDienThoai == dto.SoDienThoai))
                continue;

            DateTime.TryParse(dto.NgaySinh, out var ngaySinh);
            var entity = new BenhNhan
            {
                HoTen       = dto.HoTen,
                NgaySinh    = ngaySinh,
                GioiTinh    = dto.GioiTinh,
                SoDienThoai = dto.SoDienThoai,
                Email       = dto.Email,
                MatKhau     = dto.MatKhau,
                CCCD        = dto.CCCD,
                DiaChi      = dto.DiaChi,
                DiUng       = dto.DiUng,
                NgayTao     = DateTime.Now
            };
            _context.BenhNhans.Add(entity);
            added.Add(new { dto.HoTen });
        }
        await _context.SaveChangesAsync();
        return Ok(new { message = $"Đã thêm {added.Count} bệnh nhân.", added });
    }

    // ================================================================
    // POST — Batch: Thêm nhiều dịch vụ cùng lúc
    // ================================================================
    [HttpPost("services/batch")]
    public async Task<IActionResult> AddServicesBatch([FromBody] List<ServiceDto> dtos)
    {
        if (dtos == null || !dtos.Any())
            return BadRequest(new { error = "Danh sách dịch vụ trống!" });

        var added = new List<object>();
        foreach (var dto in dtos)
        {
            var entity = new DichVu
            {
                TenDichVu = dto.TenDichVu,
                MoTa      = dto.MoTa,
                DonGia    = dto.DonGia
            };
            _context.DichVus.Add(entity);
            added.Add(new { dto.TenDichVu });
        }
        await _context.SaveChangesAsync();
        return Ok(new { message = $"Đã thêm {added.Count} dịch vụ.", added });
    }

    // ================================================================
    // POST — Batch: Thêm nhiều Lễ tân cùng lúc
    // ================================================================
    [HttpPost("receptionists/batch")]
    public async Task<IActionResult> AddReceptionistsBatch([FromBody] List<CashierDto> dtos)
    {
        var added = new List<object>();
        foreach (var dto in dtos)
        {
            if (await _context.NguoiDungs.AnyAsync(n => n.Email == dto.Email || n.SoDienThoai == dto.SoDienThoai)) continue;
            var entity = new NguoiDung
            {
                HoTen = dto.HoTen, SoDienThoai = dto.SoDienThoai, Email = dto.Email, MatKhau = dto.MatKhau,
                Role = "LeTan", TrangThai = dto.TrangThai, NgayTao = DateTime.Now
            };
            _context.NguoiDungs.Add(entity);
            added.Add(new { dto.HoTen });
        }
        await _context.SaveChangesAsync();
        return Ok(new { message = $"Đã thêm {added.Count} lễ tân.", added });
    }

    // ================================================================
    // DELETE — Xóa (cẩn thận!)
    // ================================================================
    [HttpDelete("doctors/{id}")]
    public async Task<IActionResult> DeleteDoctor(int id)
    {
        var entity = await _context.NguoiDungs.FirstOrDefaultAsync(n => n.MaNguoiDung == id && n.Role == "BacSi");
        if (entity == null) return NotFound(new { error = "Không tìm thấy bác sĩ!" });
        _context.NguoiDungs.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok(new { message = $"Đã xóa bác sĩ {entity.HoTen}." });
    }

    [HttpDelete("patients/{id}")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var entity = await _context.BenhNhans.FindAsync(id);
        if (entity == null) return NotFound(new { error = "Không tìm thấy bệnh nhân!" });
        _context.BenhNhans.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok(new { message = $"Đã xóa bệnh nhân {entity.HoTen}." });
    }

    [HttpDelete("drugs/{id}")]
    public async Task<IActionResult> DeleteDrug(int id)
    {
        var entity = await _context.Thuocs.Include(t => t.ChiTietDonThuocs).FirstOrDefaultAsync(t => t.MaThuoc == id);
        if (entity == null) return NotFound(new { error = "Không tìm thấy thuốc!" });
        if (entity.ChiTietDonThuocs.Any())
            return Conflict(new { error = "Không thể xóa thuốc đã có trong đơn thuốc!" });
        _context.Thuocs.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok(new { message = $"Đã xóa thuốc {entity.TenThuoc}." });
    }
}
