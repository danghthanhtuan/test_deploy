CREATE DATABASE MANAGEMENT;

USE MANAGEMENT;

-- T?O B?NG COMPANY
CREATE TABLE COMPANY (
    ID INT PRIMARY KEY IDENTITY NOT NULL,
    CUSTOMERID VARCHAR(15) NOT NULL UNIQUE,      -- MÃ KH (S? ???C T?O T? ??NG)
    COMPANYNAME NVARCHAR(150) NOT NULL,			-- TÊN KH
    TAXCODE VARCHAR(30) NOT NULL UNIQUE,		-- MÃ S? THU?
    COMPANYACCOUNT VARCHAR(40) NOT NULL UNIQUE, -- EMAIL
    ACCOUNTISSUEDDATE DATETIME,				-- NGÀY C?P TK
    CPHONENUMBER VARCHAR(10) NOT NULL UNIQUE, -- S?T	
    CADDRESS NVARCHAR(150) NOT NULL,     -- ??A CH?
    CUSTOMERTYPE BIT NOT NULL,            -- PHÂN LO?I KH (0: BÌNH TH??NG, 1: VIP)
	--SERVICETYPE NVARCHAR(50) NOT NULL,	 -- LO?I D?CH V?
	--CONTRACTNUMBER VARCHAR(10) NOT NULL  -- S? H?P ??NG
	OPERATINGSTATUS BIT NOT NULL,            -- TR?NG THÁI (0: KHÔNG HO?T ??NG, 1: HO?T ??NG)
);

-- T?O B?NG ACCOUNT
CREATE TABLE ACCOUNT (
    CUSTOMERID VARCHAR(15) PRIMARY KEY NOT NULL, -- MÃ KH (KHÓA CHÍNH & KHÓA NGO?I)
    ROOTACCOUNT VARCHAR(40) NOT NULL UNIQUE, -- EMAIL
    ROOTNAME NVARCHAR(40) NOT NULL,          -- H? TÊN
    RPHONENUMBER VARCHAR(10) NOT NULL UNIQUE, -- S?T
    DATEOFBIRTH DATETIME NOT NULL,           -- NGÀY SINH
    GENDER BIT NOT NULL,                     -- GI?I TÍNH (0: NAM, 1: N?)
    CONSTRAINT FK_ACCOUNT_COMPANY FOREIGN KEY (CUSTOMERID) REFERENCES COMPANY(CUSTOMERID) ON DELETE CASCADE
);

--T?O B?NG LOGIN CLIENT
CREATE TABLE LOGINCLIENT(
	CUSTOMERID VARCHAR(15) NOT NULL PRIMARY KEY,
	FOREIGN KEY (CUSTOMERID) REFERENCES COMPANY(CUSTOMERID) ON DELETE CASCADE,
	USERNAME VARCHAR(40) NOT NULL,
	PASSWORDCLIENT VARCHAR(100) NOT NULL
);

--T?O B?NG RESETPASS
CREATE TABLE RESETPASSWORD(
	ID INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	CUSTOMERID VARCHAR(15) NOT NULL,			--MÃ KH
	FOREIGN KEY (CUSTOMERID) REFERENCES COMPANY(CUSTOMERID),
	USERNAME VARCHAR(40) NOT NULL,
	PASSWORDCLIENT VARCHAR(100) NOT NULL
);

--Lo?i h? tr?
CREATE TABLE SUPPORT_TYPE (
    ID INT PRIMARY KEY IDENTITY(1,1) NOT NULL, 
    SUPPORT_CODE VARCHAR(10) NOT NULL UNIQUE, 
    SUPPORT_NAME NVARCHAR(40) NOT NULL UNIQUE
);

--Nhóm d?ch v? 
CREATE TABLE SERVICE_GROUP (
    ID INT PRIMARY KEY IDENTITY(1,1) NOT NULL, 
    SERVICE_GROUPID VARCHAR(10) NOT NULL UNIQUE, 
    GROUP_NAME NVARCHAR(50) NOT NULL
);

--Lo?i d?ch v?
CREATE TABLE SERVICE_TYPE (
    ID INT PRIMARY KEY IDENTITY(1,1) NOT NULL,  
    SERVICE_GROUPID VARCHAR(10) NOT NULL,
    SERVICE_TYPEName NVARCHAR(255) NOT NULL UNIQUE,
    FOREIGN KEY (SERVICE_GROUPID) REFERENCES SERVICE_GROUP(SERVICE_GROUPID)
);

--T?O B?NG H?P ??NG
CREATE TABLE CONTRACTS (
    ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY, -- Chỉ dùng ID làm khóa chính
    CONTRACTNUMBER VARCHAR(10) NOT NULL unique,
    STARTDATE DATETIME NOT NULL,
    ENDDATE DATETIME NOT NULL,
	SERVICE_TYPEID INT NOT NULL,
	CUSTOMERID VARCHAR(15) NOT NULL,
	ORIGINAL VARCHAR(10) NULL,
    FOREIGN KEY (SERVICE_TYPEID) REFERENCES SERVICE_TYPE(ID),
    FOREIGN KEY (CUSTOMERID) REFERENCES COMPANY(CUSTOMERID)
);


--T?O B?NG NHÂN VIÊN
CREATE TABLE STAFF (
	ID INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	STAFFID VARCHAR(10) NOT NULL UNIQUE,
	STAFFNAME NVARCHAR(40) NOT NULL,
	STAFFPHONE VARCHAR(10) NOT NULL UNIQUE,
	DEPARTMENT NVARCHAR(50) NOT NULL --B? PH?N
);

--T?O B?NG LOGIN NHÂN VIÊN
CREATE TABLE LOGINADMIN(
	STAFFID VARCHAR(10) NOT NULL PRIMARY KEY,				
	FOREIGN KEY (STAFFID) REFERENCES STAFF(STAFFID) ON DELETE CASCADE,
	USERNAMEAD VARCHAR(40) NOT NULL,
	PASSWORDAD VARCHAR(100) NOT NULL
);

--T?O B?NG YÊU C?U
CREATE TABLE REQUIREMENTS
(
	ID INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	REQUIREMENTSID VARCHAR(10) NOT NULL UNIQUE, --MÃ YÊU C?U
	REQUIREMENTSSTATUS NVARCHAR(40) NOT NULL,--TR?NG THÁI YÊU C?U
	DATEOFREQUEST DATETIME,					--NGÀY T?O
	DESCRIPTIONOFREQUEST NVARCHAR(MAX) NOT NULL   ,    --MÔ  TA YÊU C?U 
	CUSTOMERID VARCHAR(15) NOT NULL,			--MÃ KH
	SUPPORT_CODE VARCHAR(10) NOT NULL UNIQUE, 
	FOREIGN KEY (CUSTOMERID) REFERENCES COMPANY(CUSTOMERID),
	FOREIGN KEY (SUPPORT_CODE) REFERENCES SUPPORT_TYPE(SUPPORT_CODE)
);

--T?O B?NG L?CH S? C?P NH?T YÊU C?U
CREATE TABLE HISTORYREQ
(
	ID INT PRIMARY KEY IDENTITY(1,1) NOT NULL, 
	REQUIREMENTSID VARCHAR(10) NOT NULL, 
	DESCRIPTIONOFREQUEST NVARCHAR(MAX) NOT NULL   ,    --MÔ  TA YÊU C?U 
	DATEOFUPDATE DATETIME,					--NGÀY T?O
	BEFORSTATUS NVARCHAR(40) NOT NULL,--TR?NG THÁI YÊU C?U TR??C 
	APTERSTATUS NVARCHAR(40) NOT NULL,--TR?NG THÁI YÊU C?U SAU
	STAFFID VARCHAR(10) NOT NULL,
	FOREIGN KEY (STAFFID) REFERENCES STAFF(STAFFID),
	FOREIGN KEY (REQUIREMENTSID) REFERENCES REQUIREMENTS(REQUIREMENTSID)
);

--tạo bảng phân công yêu cầu cho nhân viên bộ phận nào 
CREATE TABLE Assign(
	ID INT PRIMARY KEY IDENTITY(1,1) NOT NULL, 
	REQUIREMENTSID VARCHAR(10) NOT NULL, 
	DEPARTMENT NVARCHAR(50) NOT NULL,--B? PH?N lấy theo loại hỗ trợ
	STAFFID VARCHAR(10),   --lúc sau nhân viên bộ phận đó có thể nhận
	FOREIGN KEY (STAFFID) REFERENCES STAFF(STAFFID),
	FOREIGN KEY (REQUIREMENTSID) REFERENCES REQUIREMENTS(REQUIREMENTSID)
);

--T?O B?NG ?ÁNH GIÁ
CREATE TABLE REVIEW (
    ID INT PRIMARY KEY IDENTITY(1,1) NOT NULL, 
    REQUIREMENTSID VARCHAR(10) NOT NULL,  -- Mã yêu c?u
    --CUSTOMERID VARCHAR(15) NOT NULL,  -- Mã khách hàng
    COMMENT NVARCHAR(MAX) NOT NULL,  -- Mô t? ?ánh giá t?ng th?
    DATEOFUPDATE DATETIME DEFAULT GETDATE(),  -- Ngày ?ánh giá
	--STAFFID VARCHAR(10) NOT NULL,
	--FOREIGN KEY (STAFFID) REFERENCES STAFF(STAFFID),
    --FOREIGN KEY (CUSTOMERID) REFERENCES COMPANY(CUSTOMERID),
    FOREIGN KEY (REQUIREMENTSID) REFERENCES REQUIREMENTS(REQUIREMENTSID)
);

CREATE TABLE REVIEW_CRITERIA (
    ID INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    CRITERIA_NAME NVARCHAR(100) NOT NULL UNIQUE  -- Tên tiêu chí (VD: "Thái ??", "Th?i gian x? lý")
);

CREATE TABLE REVIEW_DETAIL (
    ID INT PRIMARY KEY IDENTITY(1,1) NOT NULL, 
    REVIEW_ID INT NOT NULL,  -- Liên k?t v?i b?ng REVIEW
    CRITERIA_ID INT NOT NULL,  -- Liên k?t v?i b?ng REVIEW_CRITERIA
    STAR INT CHECK (STAR BETWEEN 1 AND 5) NOT NULL,  -- ?i?m (1-5 sao)
    FOREIGN KEY (REVIEW_ID) REFERENCES REVIEW(ID),
    FOREIGN KEY (CRITERIA_ID) REFERENCES REVIEW_CRITERIA(ID)
);

--Tạo bảng quản lý thanh toán
CREATE TABLE PAYMENT (
    ID INT PRIMARY KEY IDENTITY(1,1) NOT NULL, 
    CONTRACTNUMBER VARCHAR(10) NOT NULL,  -- Mã hợp đồng
    AMOUNT DECIMAL(18,2) NOT NULL,        -- Số tiền thanh toán
    PAYMENT_DATE DATETIME,                -- Ngày thanh toán
    PAYMENT_METHOD NVARCHAR(50),          -- Phương thức thanh toán 
    PAYMENTSTATUS BIT NOT NULL,           -- 0: chưa thanh toán, 1: đã thanh toán
    TRANSACTION_CODE VARCHAR(50) NULL, -- Mã giao dịch
    FOREIGN KEY (CONTRACTNUMBER) REFERENCES CONTRACTS(CONTRACTNUMBER)
    
);



--nếu hợp đồng >=6 tháng giảm 5%
--nếu hợp đồng >=12 tháng giảm 10%
--nếu TELCO 1.000.000/ tháng 
--nếu CLOUD_SW 1.500.000/ tháng
--nếu SUPPORT 1.800.000/ tháng
--nếu CONTRACT 1.200.000/ tháng
--VIP tăng 30%

INSERT INTO SUPPORT_TYPE ( SUPPORT_CODE, SUPPORT_NAME) VALUES 
('SP0001',N'Hỗ trợ Cước phí'),
('SP0002',N'Cập nhật hợp đồng, dịch vụ'),
('SP0003',N'Hỗ trợ Kỹ thuật'),
('SP0004',N'Bảo hành thiết bị');	

-- Thêm nhóm dịch vụ
INSERT INTO SERVICE_GROUP (SERVICE_GROUPID, GROUP_NAME) VALUES
('TELCO', N'Viễn thông & Truyền thông'),
('CLOUD_SW', N'Điện toán đám mây & Phần mềm'),
('SUPPORT', N'Hỗ trợ & An toàn thông tin'),
('CONTRACT', N'Hợp đồng & Đối tác');

-- Thêm loại dịch vụ
INSERT INTO SERVICE_TYPE (SERVICE_GROUPID, SERVICE_TYPEName) VALUES 
('TELCO', N'Đầu số thoại'),
('TELCO', N'Kênh truyền'),
('TELCO', N'Tổng đài'),
('TELCO', N'Hội nghị truyền hình'),
('TELCO', N'Tin nhắn'),
('TELCO', N'Dịch vụ truyền hình'),
('CLOUD_SW', N'Cloud partner'),
('CLOUD_SW', N'Điện toán đám mây'),
('CLOUD_SW', N'Dịch vụ điện tử'),
('CLOUD_SW', N'Dịch vụ phần mềm (SaaS)'),
('SUPPORT', N'An toàn thông tin'),
('SUPPORT', N'Giám sát'),
('SUPPORT', N'Trung tâm dữ liệu'),
('SUPPORT', N'Thiết bị'),
('SUPPORT', N'Hỗ trợ CNTT'),
('CONTRACT', N'Hợp đồng tích hợp/dự án'),
('CONTRACT', N'Hợp đồng hợp tác'),
('CONTRACT', N'Đối tác');

INSERT INTO Company ( customerID, companyName, taxCode, companyAccount, accountIssuedDate, cPhoneNumber, cAddress, customerType,OPERATINGSTATUS) VALUES
('IT03030001', N'TNHH Nam Á', '012345432', 'rireland0@answers.com', '2024-08-25', '0147258369', N'Đường Lê Lợi, quận 10', 0,0),
('IT03030002', N'Công ty Cổ phần ABC', '012355432', 'contact@abc.com', '2024-09-10', '0147258370', N'123 Đường Trần Hưng Đạo, quận 1', 1,1),
('IT03030003', N'Công ty TNHH XYZ', '012365432', 'info@xyz.com', '2024-10-05', '0147258371', N'456 Đường Nguyễn Huệ, quận 3', 0,0),
('IT03030004', N'Công ty TNHH DEF', '012375432', 'support@def.com', '2024-11-15', '0147258372', N'789 Đường Lý Thường Kiệt, quận 5', 1,1),
('IT03030005', N'Công ty Cổ phần GHI', '012385432', 'sales@ghi.com', '2024-12-20', '0147258373', N'321 Đường Phạm Ngũ Lão, quận 7', 0,0),
('IT03030006', N'Công ty TNHH JKL', '012395432', 'contact@jkl.com', '2025-01-10', '0147258374', N'654 Đường Hai Bà Trưng, quận 2', 1,1),
('IT03030007', N'Công ty Cổ phần MNO', '012405432', 'info@mno.com', '2025-02-05', '0147258375', N'987 Đường Lê Văn Sỹ, quận 4', 0,0),
('IT03030008', N'Công ty TNHH PQR', '012415432', 'support@pqr.com', '2025-03-15', '0147258376', N'159 Đường Nguyễn Thị Minh Khai, quận 6', 1,1),
('IT03030009', N'Công ty Cổ phần STU', '012425432', 'sales@stu.com', '2025-04-20', '0147258377', N'753 Đường Điện Biên Phủ, quận 8', 0,0),
('IT03030010', N'Công ty TNHH VWX', '012435432', 'contact@vwx.com', '2025-05-25', '0147258378', N'852 Đường Phan Đình Phùng, quận 9', 1,1),
('IT03030011', N'Công ty Cổ phần YZA', '012445432', 'info@yza.com', '2025-06-30', '0147258379', N'951 Đường Hoàng Văn Thụ, quận 11', 0,0),
('IT03030012', N'Công ty TNHH BCD', '012455432', 'support@bcd.com', '2025-07-05', '0147258380', N'159 Đường Lý Chính Thắng, quận 12', 1,1),
('IT03030013', N'Công ty Cổ phần EFG', '012465432', 'sales@efg.com', '2025-08-10', '0147258381', N'357 Đường Nam Kỳ Khởi Nghĩa, quận Bình Thạnh', 0,0),
('IT03030014', N'Công ty TNHH HIJ', '012475432', 'contact@hij.com', '2025-09-15', '0147258382', N'753 Đường Võ Thị Sáu, quận Phú Nhuận', 1,1),
('IT03030015', N'Công ty Cổ phần KLM', '012485432', 'info@klm.com', '2025-10-20', '0147258383', N'456 Đường Nguyễn Văn Trỗi, quận Tân Bình', 0,0);

INSERT INTO Account (customerID, rootAccount, rootName, rPhoneNumber, dateOfBirth, gender) VALUES
('IT03030001', 'nam.a@domain.com', N'Nguyễn Văn Huy', '0912345671',  '1985-05-15', 0),
('IT03030002', 'abc@domain.com', N'Trần Quốc Khang', '0912345672',  '1990-08-22', 0),
('IT03030003', 'xyz@domain.com', N'Lê Hoàng Bảo', '0912345673',  '1987-12-05', 0),
('IT03030004', 'def@domain.com', N'Phạm Minh Anh', '0912345674',  '1992-03-18', 1),
('IT03030005', 'ghi@domain.com', N'Hoàng Gia Hân', '0912345675',  '1989-07-30', 1),
('IT03030006', 'jkl@domain.com', N'Vũ Tuấn Khoa', '0912345676',  '1991-11-11', 0),
('IT03030007', 'mno@domain.com', N'Đặng Thế Phát', '0912345677',  '1986-02-25', 0),
('IT03030008', 'pqr@domain.com', N'Bùi Đình Đạt', '0912345678',  '1993-09-09', 0),
('IT03030009', 'stu@domain.com', N'Ngô Khánh Lan', '0912345679',  '1988-06-21', 1),
('IT03030010', 'vwx@domain.com', N'Đỗ Văn Long', '0912345680',  '1994-04-14', 0),
('IT03030011', 'yz@domain.com', N'Phan Minh Nam', '0912345681',  '1990-10-10', 0),
('IT03030012', 'abc2@domain.com', N'Nguyễn Ngọc Bảo Nhi', '0912345682', '1985-01-20', 1),
('IT03030013', 'def2@domain.com', N'Trần Văn Quân', '0912345683',  '1992-05-25', 0),
('IT03030014', 'ghi2@domain.com', N'Lê Thành Kim Yến', '0912345684', '1987-08-08', 1),
('IT03030015', 'jkl2@domain.com', N'Phạm Hữu Thịnh', '0912342685',  '1991-12-12', 0);

INSERT INTO REQUIREMENTS (requirementsID, requirementsStatus, dateOfRequest, descriptionOfRequest, customerID) VALUES
('RS0001', N'Yêu cầu hỗ trợ','2025-01-01',N'Gọi điện thoại trước khi đến','IT03030001'),
('RS0002', N'Yêu cầu hỗ trợ','2025-01-01',N'Gọi điện thoại trước khi đến','IT03030002'),
('RS0003', N'Yêu cầu hỗ trợ','2025-01-05',N'Gọi điện thoại trước khi đến','IT03030003'),
('RS0004', N'Yêu cầu hỗ trợ','2025-01-07',N'Gọi điện thoại trước khi đến','IT03030004'),
('RS0005', N'Yêu cầu hỗ trợ','2025-01-07',N'Gọi điện thoại trước khi đến','IT03030005'),
('RS0006', N'Yêu cầu hỗ trợ','2025-01-09', N'Gọi điện thoại trước khi đến','IT03030006'),
('RS0007', N'Yêu cầu hỗ trợ','2025-01-11',N'Gọi điện thoại trước khi đến','IT03030007'),
('RS0008', N'Yêu cầu hỗ trợ','2025-01-012',N'Gọi điện thoại trước khi đến','IT03030008'),
('RS0009', N'Yêu cầu hỗ trợ','2025-01-15',N'Gọi điện thoại trước khi đến','IT03030009'),
('RS0010', N'Yêu cầu hỗ trợ','2025-02-01',N'Gọi điện thoại trước khi đến','IT03030010');

INSERT INTO CONTRACTS (CONTRACTNUMBER,STARTDATE,ENDDATE,SERVICE_TYPEName,CUSTOMERID) VALUES
('SV0001','2025-01-01','2025-09-01',N'Đầu số thoại','IT03030001'),
('SV0002','2025-01-01','2025-09-01',N'An toàn thông tin','IT03030002'),
('SV0003','2025-01-02','2025-09-02',N'Cloud partner','IT03030003'),
('SV0004','2025-01-02','2025-10-02',N'Dịch vụ điện tử','IT03030004'),
('SV0005','2025-01-04','2025-10-04',N'Dịch vụ phần mềm (SaaS)','IT03030005'),
('SV0006','2025-01-05','2025-11-05',N'Điện toán đám mây','IT03030006'),
('SV0007','2025-01-07','2025-12-07',N'Giám sát','IT03030007'),
('SV0008','2025-01-09','2025-12-09',N'Trung tâm dữ liệu','IT03030008'),
('SV0009','2025-01-15','2026-01-15',N'Thiết bị','IT03030009'),
('SV0010','2025-01-20','2026-01-20',N'Hội nghị truyền hình','IT03030010'),
('SV0011','2025-02-01','2026-02-01',N'Kênh truyền','IT03030011'),
('SV0012','2025-02-01','2026-02-01',N'Tin nhắn','IT03030012'),
('SV0013','2025-02-05','2026-02-05',N'Tổng đài','IT03030013'),
('SV0014','2025-02-08','2026-02-08',N'Hỗ trợ CNTT','IT03030014'),
('SV0015','2025-02-09','2026-02-09',N'Hợp đồng tích hợp/dự án','IT03030015');

select * from company 
select * from Account 
select * from contracts
select * from Payment

select * from contracts
select * from LOGINclient
select * from RESETPASSWORD
select HISTORYREQ.DESCRIPTIONOFREQUEST, DATEOFREQUEST, BEFORSTATUS, APTERSTATUS, HISTORYREQ.STAFFID 
select *
from REQUIREMENTS join HISTORYREQ on REQUIREMENTS.REQUIREMENTSID = HISTORYREQ.REQUIREMENTSID 
where   REQUIREMENTS.REQUIREMENTSID = 'RS0004'
select * from REQUIREMENTS
select * from HISTORYREQ
select * from staff
select * from LOGINadmin
select * from SUPPORT_TYPE
select * from Assign
select * from contracts

select * from Payment

--thiếu insert payment ở tạo tài khoản. 
update contracts set STARTDATE ='2025-03-14' where contractnumber = 'SV0002'
update contracts set enddate ='2025-04-14' where contractnumber = 'SV0002'