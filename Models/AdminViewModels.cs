using System.ComponentModel.DataAnnotations;
using MyMvcApp.Models;

namespace MyMvcApp.Models;

public class AdminDashboardViewModel
{
    public int TotalAccounts { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalStaff { get; set; }
    public int TotalPatients { get; set; }
    public int ActiveAccounts { get; set; }
    public int LockedAccounts { get; set; }
    
    // Fake percentage growth data for UI representation
    public string TotalAccountsGrowth => "+8.4%";
    public string DoctorsGrowth => "+2";
    public string StaffGrowth => "+5";
    public string PatientsGrowth => "+12.1%";
    public string ActiveAccountsGrowth => "+3.2%";
    public string LockedAccountsGrowth => "-2";
}

public class AccountItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty; // e.g. @dung.001
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Admin, Doctor, Staff, Patient
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Type { get; set; } = string.Empty; // "NguoiDung" or "BenhNhan"
    public string? AvatarUrl { get; set; }
}

public class DoctorListViewModel
{
    public int TotalDoctors { get; set; }
    public List<KhoaSummary> Specialities { get; set; } = new List<KhoaSummary>();
    public List<DoctorItemViewModel> Doctors { get; set; } = new List<DoctorItemViewModel>();
}

public class KhoaSummary
{
    public string KhoaName { get; set; } = string.Empty;
    public int Count { get; set; }
    public string ColorClass { get; set; } = string.Empty;
}

public class DoctorItemViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty; // e.g. BS, ThS.BS, BS.CKII
    public string Speciality { get; set; } = string.Empty;
    public string ClinicRoom { get; set; } = string.Empty; // Placeholder if needed
    public string WorkSchedule { get; set; } = string.Empty; // Placeholder if needed
    public bool IsActive { get; set; }
}
