-- Kịch bản thêm dữ liệu mẫu cho hệ thống Phòng Khám (MediCore)
-- Chạy script này trong SQL Server Management Studio (hoặc Azure Data Studio) sau khi đã chạy Entity Framework Migrations (Update-Database)

USE [db65136]; -- Thay bằng tên Database thực tế của bạn nếu khác
GO

-- 1. Thêm dữ liệu Chuyên khoa
SET IDENTITY_INSERT ChuyenKhoas ON;
INSERT INTO ChuyenKhoas (MaChuyenKhoa, TenChuyenKhoa, MoTa) VALUES 
(1, N'Khoa Nội', N'Chuyên khám và điều trị các bệnh nội khoa'),
(2, N'Khoa Ngoại', N'Chuyên phẫu thuật và điều trị các bệnh ngoại khoa'),
(3, N'Nhi khoa', N'Chuyên khám và điều trị bệnh cho trẻ em'),
(4, N'Sản phụ khoa', N'Chuyên khám và điều trị các bệnh phụ khoa, thai sản'),
(5, N'Nha khoa', N'Chuyên khám và điều trị các bệnh răng hàm mặt'),
(6, N'Da liễu', N'Chuyên khám và điều trị các bệnh về da');
SET IDENTITY_INSERT ChuyenKhoas OFF;
GO

-- 2. Thêm dữ liệu Người dùng (Admin, Manager, Staff, Doctor)
-- Mật khẩu mặc định cho tất cả là: 123456
SET IDENTITY_INSERT NguoiDungs ON;
INSERT INTO NguoiDungs (MaNguoiDung, HoTen, SoDienThoai, Email, MatKhau, Role, MaChuyenKhoa, TrangThai, NgayTao) VALUES 
(1, N'Quản trị viên Hệ thống', '0999999999', 'admin@medicore.com', '123456', 'Admin', NULL, 1, GETDATE()),
(2, N'Giám đốc Điều hành', '0988888888', 'manager@medicore.com', '123456', 'Manager', NULL, 1, GETDATE()),
(3, N'Lễ Tân Số 1', '0977777777', 'letan1@medicore.com', '123456', 'LeTan', NULL, 1, GETDATE()),
(4, N'Lễ Tân Số 2', '0966666666', 'letan2@medicore.com', '123456', 'LeTan', NULL, 1, GETDATE()),
(5, N'BS. Nguyễn Minh Khoa', '0911111111', 'khoanm@medicore.com', '123456', 'BacSi', 1, 1, GETDATE()),
(6, N'ThS.BS. Trần Hữu Lan', '0922222222', 'lanth@medicore.com', '123456', 'BacSi', 2, 1, GETDATE()),
(7, N'BS.CKII. Lê Quốc Minh', '0933333333', 'minhlq@medicore.com', '123456', 'BacSi', 3, 1, GETDATE()),
(8, N'BS. Phạm Hoàng Phúc', '0944444444', 'phucph@medicore.com', '123456', 'BacSi', 4, 1, GETDATE()),
(9, N'BS. Võ Thị Tuyết (Nghỉ việc)', '0955555555', 'tuyetvt@medicore.com', '123456', 'BacSi', 5, 0, GETDATE());
SET IDENTITY_INSERT NguoiDungs OFF;
GO

-- 3. Thêm dữ liệu Bệnh nhân
SET IDENTITY_INSERT BenhNhans ON;
INSERT INTO BenhNhans (MaBenhNhan, HoTen, NgaySinh, GioiTinh, SoDienThoai, Email, MatKhau, CCCD, DiaChi, DiUng, NgayTao) VALUES 
(1, N'Bệnh Nhân VIP', '1990-01-01', N'Nam', '0811111111', 'bn.vip@gmail.com', '123456', '079090001111', N'Hà Nội', NULL, GETDATE()),
(2, N'Bệnh Nhân Tiêu Chuẩn', '1995-05-15', N'Nữ', '0822222222', 'bn.normal@gmail.com', '123456', '079095002222', N'TP.HCM', N'Hải sản', GETDATE());
SET IDENTITY_INSERT BenhNhans OFF;
GO
