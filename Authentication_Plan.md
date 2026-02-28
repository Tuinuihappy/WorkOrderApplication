# Authentication Implementation Plan

## 📌 ภาพรวม
แผนการดำเนินการเพื่อเพิ่มระบบ Authentication (JWT) และ Authorization ให้กับ WorkOrderApplication API

---

## 📝 Tasks & Action Items

### Phase 1: เตรียมความพร้อมของฐานข้อมูล (Database & Entity)
- [x] **1.1 ปรับปรุง Entity `User.cs`**
  - เพิ่ม Property: `PasswordHash` (string) สำหรับเก็บรหัสผ่านที่เข้ารหัสแล้ว
  - เพิ่ม Property: `Role` (string) สำหรับกำหนดสิทธิ์ เช่น `Admin`, `User`, `Manager` (มีค่าเริ่มต้นเป็น `User`)
- [x] **1.2 อัปเดต DTO และ Validator**
  - อัปเดต `UserUpsertDto` ให้รองรับการตั้งรหัสผ่านตอนสร้าง User
  - เพิ่ม Validator สำหรับรหัสผ่าน (ความยาว, ตัวอักษร)
  - สร้าง `LoginRequestDto` `{ EmployeeId, Password }`
  - สร้าง `LoginResponseDto` `{ Token, UserInfo }`
- [x] **1.3 สร้าง Migration และ Update Database**
  - รันคำสั่ง `dotnet ef migrations add AddAuthFieldsToUser`
  - รันคำสั่ง `dotnet ef database update`

### Phase 2: ติดตั้งและตั้งค่าระบบความปลอดภัย (Security Setup)
- [x] **2.1 ติดตั้ง NuGet Packages ที่จำเป็น**
    - `Microsoft.AspNetCore.Authentication.JwtBearer`
    - `BCrypt.Net-Next` (สำหรับการ Hash รหัสผ่าน)
- [x] **2.2 เพิ่มการตั้งค่า JWT ใน `appsettings.json`**
  - กำหนด `Key` (Secret Key)
  - กำหนด `Issuer` และ `Audience`
  - กำหนด `ExpiresInMinutes`
- [x] **2.3 สร้างคลาส `JwtOptions` (Configuration Class)**
  - สร้างคลาสในโฟลเดอร์ `Configurations` เพื่อ map ค่าจาก `appsettings.json`

### Phase 3: การพัฒนาระบบให้บริการ (Services)
- [x] **3.1 สร้าง AuthService / TokenService**
  - ฟังก์ชัน `GenerateJwtToken(User user)`: สร้าง JWT Token ที่มี Claims (UserId, EmployeeId, Role)
  - ฟังก์ชัน `HashPassword(string password)`: เข้ารหัสผ่าน
  - ฟังก์ชัน `VerifyPassword(string password, string hash)`: ตรวจสอบรหัสผ่านตอน Login

### Phase 4: สร้างช่องทางการเชื่อมต่อ (Endpoints)
- [x] **4.1 สร้าง `AuthEndpoints.cs`**
  - `POST /api/auth/login`: รับข้อมูลการล็อกอิน, ตรวจสอบรหัสผ่าน, และคืนค่า JWT Token
  - `POST /api/auth/register` (หรือปรับปรุง `/api/users` เดิม): สำรับการสร้าง User ใหม่พร้อมเข้ารหัสผ่าน
- [x] **4.2 Map Endpoints ใน `Program.cs`**

### Phase 5: เปิดใช้งานและปกป้อง API (Middleware & Protection)
- [x] **5.1 กำหนดค่า Authentication/Authorization ใน `Program.cs`**
  - เพิ่ม `builder.Services.AddAuthentication().AddJwtBearer(...)`
  - เพิ่ม `builder.Services.AddAuthorization(...)`
  - เพิ่ม `app.UseAuthentication()` และ `app.UseAuthorization()` ก่อนถึงการเรียกใช้ Minimal API MapGroup
- [x] **5.2 ปรับ Endpoint ของเดิมให้มีความปลอดภัย**
  - เพิ่ม `.RequireAuthorization()` ใน API Group ต่างๆ (เช่น WorkOrders, Materials)
  - เพิ่ม Swagger Configuration ให้รองรับการใส่ Bearer Token (เพื่อให้ทดสอบผ่าน Swagger UI ได้)

### Phase 6: ทดสอบระบบ (Testing)
- [ ] **6.1 ทดสอบลงทะเบียน User ใหม่และเข้ารหัสผ่านเข้าฐานข้อมูล**
- [ ] **6.2 ทดสอบล็อกอินและรับ Token**
- [ ] **6.3 ทดสอบเรียกใช้ Protected Endpoint โดยมีการใช้ Bearer Token**
- [ ] **6.4 ทดสอบเรียกใช้ Protected Endpoint โดยไม่มี Token (ต้องได้ 401 Unauthorized)**

---

## 🚀 ขั้นตอนถัดไป
หากคุณพร้อม สามารถสั่งให้ผมเริ่มทำ **Phase 1** ได้เลยครับ โดยผมจะเริ่มจากการแก้ไขไฟล์ `Entities/User.cs` ให้คุณทันที!
