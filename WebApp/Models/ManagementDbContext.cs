using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Models;

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

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Endow> Endows { get; set; }

    public virtual DbSet<Historyreq> Historyreqs { get; set; }

    public virtual DbSet<Loginadmin> Loginadmins { get; set; }

    public virtual DbSet<Loginclient> Loginclients { get; set; }

    public virtual DbSet<Passwordresettoken> Passwordresettokens { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

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
            entity.HasKey(e => e.Customerid).HasName("PK__ACCOUNT__61DBD7886583D014");

            entity.ToTable("ACCOUNT");

            entity.HasIndex(e => e.Rphonenumber, "UQ__ACCOUNT__D5AD45F3EA921F2D").IsUnique();

            entity.HasIndex(e => e.Rootaccount, "UQ__ACCOUNT__D6A078E45F3DF4F4").IsUnique();

            entity.Property(e => e.Customerid)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("CUSTOMERID");
            entity.Property(e => e.Dateofbirth)
                .HasColumnType("datetime")
                .HasColumnName("DATEOFBIRTH");
            entity.Property(e => e.Gender).HasColumnName("GENDER");
            entity.Property(e => e.Rootaccount)
                .HasMaxLength(40)
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
            entity.HasKey(e => e.Id).HasName("PK__ASSIGN__3214EC27D3697AD9");

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
                .HasConstraintName("FK__ASSIGN__REQUIREM__6754599E");

            entity.HasOne(d => d.Staff).WithMany(p => p.Assigns)
                .HasPrincipalKey(p => p.Staffid)
                .HasForeignKey(d => d.Staffid)
                .HasConstraintName("FK__ASSIGN__STAFFID__66603565");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__COMPANY__3214EC271228F811");

            entity.ToTable("COMPANY");

            entity.HasIndex(e => e.Customerid, "UQ__COMPANY__61DBD789888AA930").IsUnique();

            entity.HasIndex(e => e.Taxcode, "UQ__COMPANY__85178AA44ED63A85").IsUnique();

            entity.HasIndex(e => e.Cphonenumber, "UQ__COMPANY__87EF6223EEF6C07D").IsUnique();

            entity.HasIndex(e => e.Companyaccount, "UQ__COMPANY__A1F84A4E1AB1460E").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Accountissueddate)
                .HasColumnType("datetime")
                .HasColumnName("ACCOUNTISSUEDDATE");
            entity.Property(e => e.Caddress)
                .HasMaxLength(150)
                .HasColumnName("CADDRESS");
            entity.Property(e => e.Companyaccount)
                .HasMaxLength(40)
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
            entity.Property(e => e.Operatingstatus).HasColumnName("OPERATINGSTATUS");
            entity.Property(e => e.Taxcode)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("TAXCODE");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CONTRACT__3214EC2728FC8706");

            entity.ToTable("CONTRACTS");

            entity.HasIndex(e => e.Contractnumber, "UQ__CONTRACT__29BF5AB0456FC9CB").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
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

        modelBuilder.Entity<Endow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ENDOW__3214EC277330F122");

            entity.ToTable("ENDOW");

            entity.HasIndex(e => e.Endowid, "UQ__ENDOW__00ACFA5E5547A823").IsUnique();

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
                .HasConstraintName("FK__ENDOW__SERVICE_G__7C4F7684");
        });

        modelBuilder.Entity<Historyreq>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HISTORYR__3214EC27C5921C47");

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
                .HasConstraintName("FK__HISTORYRE__REQUI__6383C8BA");

            entity.HasOne(d => d.Staff).WithMany(p => p.Historyreqs)
                .HasPrincipalKey(p => p.Staffid)
                .HasForeignKey(d => d.Staffid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HISTORYRE__STAFF__628FA481");
        });

        modelBuilder.Entity<Loginadmin>(entity =>
        {
            entity.HasKey(e => e.Staffid).HasName("PK__LOGINADM__28B5063B9DFBEC82");

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
                .HasConstraintName("FK__LOGINADMI__STAFF__5AEE82B9");
        });

        modelBuilder.Entity<Loginclient>(entity =>
        {
            entity.HasKey(e => e.Customerid).HasName("PK__LOGINCLI__61DBD788786AFD13");

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

        modelBuilder.Entity<Passwordresettoken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PASSWORD__3214EC27F7A86F5B");

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
            entity.HasKey(e => e.Id).HasName("PK__PAYMENT__3214EC2703328E66");

            entity.ToTable("PAYMENT");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("AMOUNT");
            entity.Property(e => e.Contractnumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CONTRACTNUMBER");
            entity.Property(e => e.PaymentDate)
                .HasColumnType("datetime")
                .HasColumnName("PAYMENT_DATE");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("PAYMENT_METHOD");
            entity.Property(e => e.Paymentstatus).HasColumnName("PAYMENTSTATUS");
            entity.Property(e => e.TransactionCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TRANSACTION_CODE");

            entity.HasOne(d => d.ContractnumberNavigation).WithMany(p => p.Payments)
                .HasPrincipalKey(p => p.Contractnumber)
                .HasForeignKey(d => d.Contractnumber)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PAYMENT__CONTRAC__75A278F5");
        });

        modelBuilder.Entity<Regulation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__REGULATI__3214EC27D2C8734D");

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
                .HasConstraintName("FK__REGULATIO__SERVI__787EE5A0");
        });

        modelBuilder.Entity<Requirement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__REQUIREM__3214EC27D0783B91");

            entity.ToTable("REQUIREMENTS");

            entity.HasIndex(e => e.Requirementsid, "UQ__REQUIREM__8ECC5B5737B49D93").IsUnique();

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
                .HasConstraintName("FK__REQUIREME__CONTR__5FB337D6");

            entity.HasOne(d => d.SupportCodeNavigation).WithMany(p => p.Requirements)
                .HasPrincipalKey(p => p.SupportCode)
                .HasForeignKey(d => d.SupportCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REQUIREME__SUPPO__5EBF139D");
        });

        modelBuilder.Entity<Resetpassword>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RESETPAS__3214EC27E729E8D3");

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
            entity.HasKey(e => e.Id).HasName("PK__REVIEW__3214EC27AC77A3DB");

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
                .HasConstraintName("FK__REVIEW__REQUIREM__6B24EA82");
        });

        modelBuilder.Entity<ReviewCriterion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__REVIEW_C__3214EC2734041C8A");

            entity.ToTable("REVIEW_CRITERIA");

            entity.HasIndex(e => e.CriteriaName, "UQ__REVIEW_C__60CA328C5D49B22B").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CriteriaName)
                .HasMaxLength(100)
                .HasColumnName("CRITERIA_NAME");
        });

        modelBuilder.Entity<ReviewDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__REVIEW_D__3214EC271AFA8244");

            entity.ToTable("REVIEW_DETAIL");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CriteriaId).HasColumnName("CRITERIA_ID");
            entity.Property(e => e.ReviewId).HasColumnName("REVIEW_ID");
            entity.Property(e => e.Star).HasColumnName("STAR");

            entity.HasOne(d => d.Criteria).WithMany(p => p.ReviewDetails)
                .HasForeignKey(d => d.CriteriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REVIEW_DE__CRITE__72C60C4A");

            entity.HasOne(d => d.Review).WithMany(p => p.ReviewDetails)
                .HasForeignKey(d => d.ReviewId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REVIEW_DE__REVIE__71D1E811");
        });

        modelBuilder.Entity<ServiceGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SERVICE___3214EC273AFC2747");

            entity.ToTable("SERVICE_GROUP");

            entity.HasIndex(e => e.ServiceGroupid, "UQ__SERVICE___AA7131534A1C30A7").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__SERVICE___3214EC2722B97E9B");

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
            entity.HasKey(e => e.Id).HasName("PK__STAFF__3214EC2729D82DDC");

            entity.ToTable("STAFF");

            entity.HasIndex(e => e.Staffid, "UQ__STAFF__28B5063A47A4BF75").IsUnique();

            entity.HasIndex(e => e.Staffphone, "UQ__STAFF__821DCC5856EE7796").IsUnique();

            entity.HasIndex(e => e.Staffemail, "UQ__STAFF__FBDE419AD185E53F").IsUnique();

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
                .HasMaxLength(40)
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
            entity.HasKey(e => e.Id).HasName("PK__SUPPORT___3214EC27E32F066F");

            entity.ToTable("SUPPORT_TYPE");

            entity.HasIndex(e => e.SupportName, "UQ__SUPPORT___7661AF1E369F6927").IsUnique();

            entity.HasIndex(e => e.SupportCode, "UQ__SUPPORT___E5AB82339608DA3C").IsUnique();

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
