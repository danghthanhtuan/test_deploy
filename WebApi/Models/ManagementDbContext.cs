using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Models;

public partial class ManagementDbContext : DbContext
{
    public ManagementDbContext()
    {
    }

    public ManagementDbContext(DbContextOptions<ManagementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Assign> Assigns { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<ContractFile> ContractFiles { get; set; }

    public virtual DbSet<ContractStatusHistory> ContractStatusHistories { get; set; }

    public virtual DbSet<Endow> Endows { get; set; }

    public virtual DbSet<Historyreq> Historyreqs { get; set; }

    public virtual DbSet<Loginadmin> Loginadmins { get; set; }

    public virtual DbSet<Loginclient> Loginclients { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Passwordresettoken> Passwordresettokens { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    public virtual DbSet<Regulation> Regulations { get; set; }

    public virtual DbSet<Requirement> Requirements { get; set; }

    public virtual DbSet<Resetpassword> Resetpasswords { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<ReviewCriterion> ReviewCriteria { get; set; }

    public virtual DbSet<ReviewDetail> ReviewDetails { get; set; }

    public virtual DbSet<ServiceGroup> ServiceGroups { get; set; }

    public virtual DbSet<ServiceType> ServiceTypes { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<SupportType> SupportTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=MSI;Initial Catalog=MANAGEMENT;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Customerid).HasName("PK__ACCOUNT__61DBD788F6AD021A");

            entity.ToTable("ACCOUNT");

            entity.HasIndex(e => e.Rphonenumber, "UQ__ACCOUNT__D5AD45F366241E46").IsUnique();

            entity.HasIndex(e => e.Rootaccount, "UQ__ACCOUNT__D6A078E445B5064A").IsUnique();

            entity.Property(e => e.Customerid)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("CUSTOMERID");
            entity.Property(e => e.Dateofbirth)
                .HasColumnType("datetime")
                .HasColumnName("DATEOFBIRTH");
            entity.Property(e => e.Gender).HasColumnName("GENDER");
            entity.Property(e => e.Rootaccount)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ROOTACCOUNT");
            entity.Property(e => e.Rootname)
                .HasMaxLength(40)
                .HasColumnName("ROOTNAME");
            entity.Property(e => e.Rphonenumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("RPHONENUMBER");

            entity.HasOne(d => d.Customer).WithOne(p => p.Account)
                .HasPrincipalKey<Company>(p => p.Customerid)
                .HasForeignKey<Account>(d => d.Customerid)
                .HasConstraintName("FK_ACCOUNT_COMPANY");
        });

        modelBuilder.Entity<Assign>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ASSIGN__3214EC2777143908");

            entity.ToTable("ASSIGN");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Department)
                .HasMaxLength(50)
                .HasColumnName("DEPARTMENT");
            entity.Property(e => e.Requirementsid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("REQUIREMENTSID");
            entity.Property(e => e.Staffid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("STAFFID");

            entity.HasOne(d => d.Requirements).WithMany(p => p.Assigns)
                .HasPrincipalKey(p => p.Requirementsid)
                .HasForeignKey(d => d.Requirementsid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ASSIGN__REQUIREM__6EF57B66");

            entity.HasOne(d => d.Staff).WithMany(p => p.Assigns)
                .HasPrincipalKey(p => p.Staffid)
                .HasForeignKey(d => d.Staffid)
                .HasConstraintName("FK__ASSIGN__STAFFID__6E01572D");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__COMPANY__3214EC27B9AABCDB");

            entity.ToTable("COMPANY");

            entity.HasIndex(e => e.Customerid, "UQ__COMPANY__61DBD78934FC324B").IsUnique();

            entity.HasIndex(e => e.Taxcode, "UQ__COMPANY__85178AA4B43F53BC").IsUnique();

            entity.HasIndex(e => e.Cphonenumber, "UQ__COMPANY__87EF622336CE995A").IsUnique();

            entity.HasIndex(e => e.Companyaccount, "UQ__COMPANY__A1F84A4E5AA08EDA").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Accountissueddate)
                .HasColumnType("datetime")
                .HasColumnName("ACCOUNTISSUEDDATE");
            entity.Property(e => e.Caddress)
                .HasMaxLength(150)
                .HasColumnName("CADDRESS");
            entity.Property(e => e.Companyaccount)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("COMPANYACCOUNT");
            entity.Property(e => e.Companyname)
                .HasMaxLength(150)
                .HasColumnName("COMPANYNAME");
            entity.Property(e => e.Cphonenumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CPHONENUMBER");
            entity.Property(e => e.Customerid)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("CUSTOMERID");
            entity.Property(e => e.IsActive).HasColumnName("IS_ACTIVE");
            entity.Property(e => e.Taxcode)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("TAXCODE");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("CONTACT");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Message).HasMaxLength(2000);
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Subject).HasMaxLength(500);
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CONTRACT__3214EC27D1FF53F0");

            entity.ToTable("CONTRACTS");

            entity.HasIndex(e => e.Contractnumber, "UQ__CONTRACT__29BF5AB058B9BA1F").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Constatus).HasColumnName("CONSTATUS");
            entity.Property(e => e.Contractnumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CONTRACTNUMBER");
            entity.Property(e => e.Customerid)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("CUSTOMERID");
            entity.Property(e => e.Customertype).HasColumnName("CUSTOMERTYPE");
            entity.Property(e => e.Enddate)
                .HasColumnType("datetime")
                .HasColumnName("ENDDATE");
            entity.Property(e => e.IsActive).HasColumnName("IS_ACTIVE");
            entity.Property(e => e.Original)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("ORIGINAL");
            entity.Property(e => e.ServiceTypeid).HasColumnName("SERVICE_TYPEID");
            entity.Property(e => e.Startdate)
                .HasColumnType("datetime")
                .HasColumnName("STARTDATE");

            entity.HasOne(d => d.ServiceType).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.ServiceTypeid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_CONTRACTS_SERVICE_TYPEID");
        });

        modelBuilder.Entity<ContractFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CONTRACT__3214EC27EF3C473A");

            entity.ToTable("CONTRACT_FILES");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ConfileName)
                .HasMaxLength(255)
                .HasColumnName("CONFILE_NAME");
            entity.Property(e => e.Contractnumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CONTRACTNUMBER");
            entity.Property(e => e.FilePath)
                .HasMaxLength(500)
                .HasColumnName("FILE_PATH");
            entity.Property(e => e.FileStatus).HasColumnName("FILE_STATUS");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("UPLOADED_AT");

            entity.HasOne(d => d.ContractnumberNavigation).WithMany(p => p.ContractFiles)
                .HasPrincipalKey(p => p.Contractnumber)
                .HasForeignKey(d => d.Contractnumber)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CONTRACT___FILE___571DF1D5");
        });

        modelBuilder.Entity<ContractStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CONTRACT__3214EC27E66FA9E0");

            entity.ToTable("CONTRACT_STATUS_HISTORY");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("CHANGED_AT");
            entity.Property(e => e.ChangedBy)
                .HasMaxLength(100)
                .HasColumnName("CHANGED_BY");
            entity.Property(e => e.Contractnumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CONTRACTNUMBER");
            entity.Property(e => e.NewStatus).HasColumnName("NEW_STATUS");
            entity.Property(e => e.OldStatus).HasColumnName("OLD_STATUS");

            entity.HasOne(d => d.ContractnumberNavigation).WithMany(p => p.ContractStatusHistories)
                .HasPrincipalKey(p => p.Contractnumber)
                .HasForeignKey(d => d.Contractnumber)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CONTRACT___CONTR__5AEE82B9");
        });

        modelBuilder.Entity<Endow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ENDOW__3214EC27624AB80A");

            entity.ToTable("ENDOW");

            entity.HasIndex(e => e.Endowid, "UQ__ENDOW__00ACFA5E24D2A299").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Descriptionendow)
                .HasMaxLength(255)
                .HasColumnName("DESCRIPTIONENDOW");
            entity.Property(e => e.Discount).HasColumnName("DISCOUNT");
            entity.Property(e => e.Duration).HasColumnName("DURATION");
            entity.Property(e => e.Enddate)
                .HasColumnType("datetime")
                .HasColumnName("ENDDATE");
            entity.Property(e => e.Endowid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("ENDOWID");
            entity.Property(e => e.ServiceGroupid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("SERVICE_GROUPID");
            entity.Property(e => e.Startdate)
                .HasColumnType("datetime")
                .HasColumnName("STARTDATE");

            entity.HasOne(d => d.ServiceGroup).WithMany(p => p.Endows)
                .HasPrincipalKey(p => p.ServiceGroupid)
                .HasForeignKey(d => d.ServiceGroupid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ENDOW__SERVICE_G__08B54D69");
        });

        modelBuilder.Entity<Historyreq>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HISTORYR__3214EC2754632FDA");

            entity.ToTable("HISTORYREQ");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Apterstatus)
                .HasMaxLength(40)
                .HasColumnName("APTERSTATUS");
            entity.Property(e => e.Beforstatus)
                .HasMaxLength(40)
                .HasColumnName("BEFORSTATUS");
            entity.Property(e => e.Dateofupdate)
                .HasColumnType("datetime")
                .HasColumnName("DATEOFUPDATE");
            entity.Property(e => e.Descriptionofrequest).HasColumnName("DESCRIPTIONOFREQUEST");
            entity.Property(e => e.Requirementsid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("REQUIREMENTSID");
            entity.Property(e => e.Staffid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("STAFFID");

            entity.HasOne(d => d.Requirements).WithMany(p => p.Historyreqs)
                .HasPrincipalKey(p => p.Requirementsid)
                .HasForeignKey(d => d.Requirementsid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HISTORYRE__REQUI__6B24EA82");

            entity.HasOne(d => d.Staff).WithMany(p => p.Historyreqs)
                .HasPrincipalKey(p => p.Staffid)
                .HasForeignKey(d => d.Staffid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HISTORYRE__STAFF__6A30C649");
        });

        modelBuilder.Entity<Loginadmin>(entity =>
        {
            entity.HasKey(e => e.Staffid).HasName("PK__LOGINADM__28B5063BBFE08846");

            entity.ToTable("LOGINADMIN");

            entity.Property(e => e.Staffid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("STAFFID");
            entity.Property(e => e.Passwordad)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("PASSWORDAD");
            entity.Property(e => e.Usernamead)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("USERNAMEAD");

            entity.HasOne(d => d.Staff).WithOne(p => p.Loginadmin)
                .HasPrincipalKey<Staff>(p => p.Staffid)
                .HasForeignKey<Loginadmin>(d => d.Staffid)
                .HasConstraintName("FK__LOGINADMI__STAFF__628FA481");
        });

        modelBuilder.Entity<Loginclient>(entity =>
        {
            entity.HasKey(e => e.Customerid).HasName("PK__LOGINCLI__61DBD788F5C7D37E");

            entity.ToTable("LOGINCLIENT");

            entity.Property(e => e.Customerid)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("CUSTOMERID");
            entity.Property(e => e.Passwordclient)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("PASSWORDCLIENT");
            entity.Property(e => e.Username)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("USERNAME");

            entity.HasOne(d => d.Customer).WithOne(p => p.Loginclient)
                .HasPrincipalKey<Company>(p => p.Customerid)
                .HasForeignKey<Loginclient>(d => d.Customerid)
                .HasConstraintName("FK__LOGINCLIE__CUSTO__4222D4EF");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NOTIFICA__3214EC07E8B4943E");

            entity.ToTable("NOTIFICATIONS");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Passwordresettoken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PASSWORD__3214EC276269FCFC");

            entity.ToTable("PASSWORDRESETTOKEN");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Expirytime)
                .HasColumnType("datetime")
                .HasColumnName("EXPIRYTIME");
            entity.Property(e => e.Isused).HasColumnName("ISUSED");
            entity.Property(e => e.Otp)
                .HasMaxLength(10)
                .HasColumnName("OTP");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PAYMENT__3214EC277186D5BD");

            entity.ToTable("PAYMENT");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("AMOUNT");
            entity.Property(e => e.Contractnumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CONTRACTNUMBER");
            entity.Property(e => e.Paymentstatus).HasColumnName("PAYMENTSTATUS");

            entity.HasOne(d => d.ContractnumberNavigation).WithMany(p => p.Payments)
                .HasPrincipalKey(p => p.Contractnumber)
                .HasForeignKey(d => d.Contractnumber)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PAYMENT__CONTRAC__7D439ABD");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PAYMENT___3214EC27DE6D22A4");

            entity.ToTable("PAYMENT_TRANSACTION");

            entity.HasIndex(e => e.TransactionCode, "UQ__PAYMENT___1EB66D56050392CE").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("AMOUNT");
            entity.Property(e => e.BankCode)
                .HasMaxLength(20)
                .HasColumnName("BANK_CODE");
            entity.Property(e => e.BankTransactionCode)
                .HasMaxLength(50)
                .HasColumnName("BANK_TRANSACTION_CODE");
            entity.Property(e => e.CardType)
                .HasMaxLength(20)
                .HasColumnName("CARD_TYPE");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("EMAIL");
            entity.Property(e => e.OrderInfo)
                .HasMaxLength(255)
                .HasColumnName("ORDER_INFO");
            entity.Property(e => e.PaymentDate)
                .HasColumnType("datetime")
                .HasColumnName("PAYMENT_DATE");
            entity.Property(e => e.PaymentId).HasColumnName("PAYMENT_ID");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("PAYMENT_METHOD");
            entity.Property(e => e.PaymentResult)
                .HasDefaultValue(0)
                .HasColumnName("PAYMENT_RESULT");
            entity.Property(e => e.ResponseCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("RESPONSE_CODE");
            entity.Property(e => e.TmnCode)
                .HasMaxLength(20)
                .HasColumnName("TMN_CODE");
            entity.Property(e => e.TransactionCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TRANSACTION_CODE");

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentTransactions)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("FK__PAYMENT_T__PAYME__02084FDA");
        });

        modelBuilder.Entity<Regulation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__REGULATI__3214EC272E7A2BEE");

            entity.ToTable("REGULATIONS");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("PRICE");
            entity.Property(e => e.ServiceGroupid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("SERVICE_GROUPID");

            entity.HasOne(d => d.ServiceGroup).WithMany(p => p.Regulations)
                .HasPrincipalKey(p => p.ServiceGroupid)
                .HasForeignKey(d => d.ServiceGroupid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REGULATIO__SERVI__04E4BC85");
        });

        modelBuilder.Entity<Requirement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__REQUIREM__3214EC278A57CF7F");

            entity.ToTable("REQUIREMENTS");

            entity.HasIndex(e => e.Requirementsid, "UQ__REQUIREM__8ECC5B5750A709E4").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Contractnumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CONTRACTNUMBER");
            entity.Property(e => e.Dateofrequest)
                .HasColumnType("datetime")
                .HasColumnName("DATEOFREQUEST");
            entity.Property(e => e.Descriptionofrequest).HasColumnName("DESCRIPTIONOFREQUEST");
            entity.Property(e => e.Requirementsid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("REQUIREMENTSID");
            entity.Property(e => e.Requirementsstatus)
                .HasMaxLength(40)
                .HasColumnName("REQUIREMENTSSTATUS");
            entity.Property(e => e.SupportCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("SUPPORT_CODE");

            entity.HasOne(d => d.ContractnumberNavigation).WithMany(p => p.Requirements)
                .HasPrincipalKey(p => p.Contractnumber)
                .HasForeignKey(d => d.Contractnumber)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REQUIREME__CONTR__6754599E");

            entity.HasOne(d => d.SupportCodeNavigation).WithMany(p => p.Requirements)
                .HasPrincipalKey(p => p.SupportCode)
                .HasForeignKey(d => d.SupportCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REQUIREME__SUPPO__66603565");
        });

        modelBuilder.Entity<Resetpassword>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RESETPAS__3214EC2721D6AC1A");

            entity.ToTable("RESETPASSWORD");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Customerid)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("CUSTOMERID");
            entity.Property(e => e.Passwordclient)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("PASSWORDCLIENT");
            entity.Property(e => e.Username)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("USERNAME");

            entity.HasOne(d => d.Customer).WithMany(p => p.Resetpasswords)
                .HasPrincipalKey(p => p.Customerid)
                .HasForeignKey(d => d.Customerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RESETPASS__CUSTO__44FF419A");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__REVIEW__3214EC27DFB96586");

            entity.ToTable("REVIEW");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Comment).HasColumnName("COMMENT");
            entity.Property(e => e.Dateofupdate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("DATEOFUPDATE");
            entity.Property(e => e.Requirementsid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("REQUIREMENTSID");

            entity.HasOne(d => d.Requirements).WithMany(p => p.Reviews)
                .HasPrincipalKey(p => p.Requirementsid)
                .HasForeignKey(d => d.Requirementsid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REVIEW__REQUIREM__72C60C4A");
        });

        modelBuilder.Entity<ReviewCriterion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__REVIEW_C__3214EC274BBB791C");

            entity.ToTable("REVIEW_CRITERIA");

            entity.HasIndex(e => e.CriteriaName, "UQ__REVIEW_C__60CA328C2590615F").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CriteriaName)
                .HasMaxLength(100)
                .HasColumnName("CRITERIA_NAME");
        });

        modelBuilder.Entity<ReviewDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__REVIEW_D__3214EC27994352B8");

            entity.ToTable("REVIEW_DETAIL");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CriteriaId).HasColumnName("CRITERIA_ID");
            entity.Property(e => e.ReviewId).HasColumnName("REVIEW_ID");
            entity.Property(e => e.Star).HasColumnName("STAR");

            entity.HasOne(d => d.Criteria).WithMany(p => p.ReviewDetails)
                .HasForeignKey(d => d.CriteriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REVIEW_DE__CRITE__7A672E12");

            entity.HasOne(d => d.Review).WithMany(p => p.ReviewDetails)
                .HasForeignKey(d => d.ReviewId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REVIEW_DE__REVIE__797309D9");
        });

        modelBuilder.Entity<ServiceGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SERVICE___3214EC27B2387B8A");

            entity.ToTable("SERVICE_GROUP");

            entity.HasIndex(e => e.ServiceGroupid, "UQ__SERVICE___AA7131537C2A599D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GroupName)
                .HasMaxLength(50)
                .HasColumnName("GROUP_NAME");
            entity.Property(e => e.ServiceGroupid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("SERVICE_GROUPID");
        });

        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SERVICE___3214EC2746DD85B0");

            entity.ToTable("SERVICE_TYPE");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ServiceGroupid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("SERVICE_GROUPID");
            entity.Property(e => e.ServiceTypename)
                .HasMaxLength(255)
                .HasColumnName("SERVICE_TYPENAME");

            entity.HasOne(d => d.ServiceGroup).WithMany(p => p.ServiceTypes)
                .HasPrincipalKey(p => p.ServiceGroupid)
                .HasForeignKey(d => d.ServiceGroupid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SERVICE_T__SERVI__4E88ABD4");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__STAFF__3214EC273109E01D");

            entity.ToTable("STAFF");

            entity.HasIndex(e => e.Staffid, "UQ__STAFF__28B5063A752E6CA4").IsUnique();

            entity.HasIndex(e => e.Staffphone, "UQ__STAFF__821DCC5863EF08DC").IsUnique();

            entity.HasIndex(e => e.Staffemail, "UQ__STAFF__FBDE419A33982621").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Department)
                .HasMaxLength(50)
                .HasColumnName("DEPARTMENT");
            entity.Property(e => e.Staffaddress)
                .HasMaxLength(100)
                .HasColumnName("STAFFADDRESS");
            entity.Property(e => e.Staffdate)
                .HasColumnType("datetime")
                .HasColumnName("STAFFDATE");
            entity.Property(e => e.Staffemail)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("STAFFEMAIL");
            entity.Property(e => e.Staffgender).HasColumnName("STAFFGENDER");
            entity.Property(e => e.Staffid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("STAFFID");
            entity.Property(e => e.Staffname)
                .HasMaxLength(40)
                .HasColumnName("STAFFNAME");
            entity.Property(e => e.Staffphone)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("STAFFPHONE");
        });

        modelBuilder.Entity<SupportType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SUPPORT___3214EC278F9DE104");

            entity.ToTable("SUPPORT_TYPE");

            entity.HasIndex(e => e.SupportName, "UQ__SUPPORT___7661AF1EAEDAD835").IsUnique();

            entity.HasIndex(e => e.SupportCode, "UQ__SUPPORT___E5AB82337639AD73").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.SupportCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("SUPPORT_CODE");
            entity.Property(e => e.SupportName)
                .HasMaxLength(40)
                .HasColumnName("SUPPORT_NAME");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
