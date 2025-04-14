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

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Historyreq> Historyreqs { get; set; }

    public virtual DbSet<Loginadmin> Loginadmins { get; set; }

    public virtual DbSet<Loginclient> Loginclients { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

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
            entity.HasKey(e => e.Customerid).HasName("PK__ACCOUNT__61DBD7882255C557");

            entity.ToTable("ACCOUNT");

            entity.HasIndex(e => e.Rphonenumber, "UQ__ACCOUNT__D5AD45F3875D05C8").IsUnique();

            entity.HasIndex(e => e.Rootaccount, "UQ__ACCOUNT__D6A078E45B8FED91").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__Assign__3214EC2736013291");

            entity.ToTable("Assign");

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
                .HasConstraintName("FK__Assign__REQUIREM__68487DD7");

            entity.HasOne(d => d.Staff).WithMany(p => p.Assigns)
                .HasPrincipalKey(p => p.Staffid)
                .HasForeignKey(d => d.Staffid)
                .HasConstraintName("FK__Assign__STAFFID__6754599E");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__COMPANY__3214EC27B13599C1");

            entity.ToTable("COMPANY");

            entity.HasIndex(e => e.Customerid, "UQ__COMPANY__61DBD7897EBE38E2").IsUnique();

            entity.HasIndex(e => e.Taxcode, "UQ__COMPANY__85178AA4C516DD36").IsUnique();

            entity.HasIndex(e => e.Cphonenumber, "UQ__COMPANY__87EF62237EA7E08A").IsUnique();

            entity.HasIndex(e => e.Companyaccount, "UQ__COMPANY__A1F84A4E5BCE9668").IsUnique();

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
            entity.Property(e => e.Customertype).HasColumnName("CUSTOMERTYPE");
            entity.Property(e => e.Operatingstatus).HasColumnName("OPERATINGSTATUS");
            entity.Property(e => e.Taxcode)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("TAXCODE");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CONTRACT__3214EC2703A28828");

            entity.ToTable("CONTRACTS");

            entity.HasIndex(e => e.Contractnumber, "UQ__CONTRACT__29BF5AB00C19601B").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Contractnumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CONTRACTNUMBER");
            entity.Property(e => e.Customerid)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("CUSTOMERID");
            entity.Property(e => e.Enddate)
                .HasColumnType("datetime")
                .HasColumnName("ENDDATE");
            entity.Property(e => e.Original)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("ORIGINAL");
            entity.Property(e => e.ServiceTypename)
                .HasMaxLength(255)
                .HasColumnName("SERVICE_TYPEName");
            entity.Property(e => e.Startdate)
                .HasColumnType("datetime")
                .HasColumnName("STARTDATE");

            entity.HasOne(d => d.Customer).WithMany(p => p.Contracts)
                .HasPrincipalKey(p => p.Customerid)
                .HasForeignKey(d => d.Customerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CONTRACTS__CUSTO__5441852A");

            entity.HasOne(d => d.ServiceTypenameNavigation).WithMany(p => p.Contracts)
                .HasPrincipalKey(p => p.ServiceTypename)
                .HasForeignKey(d => d.ServiceTypename)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CONTRACTS__SERVI__534D60F1");
        });

        modelBuilder.Entity<Historyreq>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HISTORYR__3214EC27E176076F");

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
                .HasConstraintName("FK__HISTORYRE__REQUI__6477ECF3");

            entity.HasOne(d => d.Staff).WithMany(p => p.Historyreqs)
                .HasPrincipalKey(p => p.Staffid)
                .HasForeignKey(d => d.Staffid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HISTORYRE__STAFF__6383C8BA");
        });

        modelBuilder.Entity<Loginadmin>(entity =>
        {
            entity.HasKey(e => e.Staffid).HasName("PK__LOGINADM__28B5063B2F844074");

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
            entity.HasKey(e => e.Customerid).HasName("PK__LOGINCLI__61DBD78884294ABE");

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

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PAYMENT__3214EC27FF71A67F");

            entity.ToTable("PAYMENT");

            entity.HasIndex(e => e.TransactionCode, "UQ__PAYMENT__1EB66D564F1FB0E4").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("AMOUNT");
            entity.Property(e => e.Contractnumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CONTRACTNUMBER");
            entity.Property(e => e.Customerid)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("CUSTOMERID");
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
                .HasConstraintName("FK__PAYMENT__CONTRAC__797309D9");

            entity.HasOne(d => d.Customer).WithMany(p => p.Payments)
                .HasPrincipalKey(p => p.Customerid)
                .HasForeignKey(d => d.Customerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PAYMENT__CUSTOME__7A672E12");
        });

        modelBuilder.Entity<Requirement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__REQUIREM__3214EC2721139549");

            entity.ToTable("REQUIREMENTS");

            entity.HasIndex(e => e.Requirementsid, "UQ__REQUIREM__8ECC5B571257F134").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Customerid)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("CUSTOMERID");
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
            entity.Property(e => e.Staffid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("STAFFID");
            entity.Property(e => e.SupportName)
                .HasMaxLength(40)
                .HasColumnName("SUPPORT_NAME");

            entity.HasOne(d => d.Customer).WithMany(p => p.Requirements)
                .HasPrincipalKey(p => p.Customerid)
                .HasForeignKey(d => d.Customerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REQUIREME__CUSTO__5EBF139D");

            entity.HasOne(d => d.Staff).WithMany(p => p.Requirements)
                .HasPrincipalKey(p => p.Staffid)
                .HasForeignKey(d => d.Staffid)
                .HasConstraintName("FK__REQUIREME__STAFF__5FB337D6");

            entity.HasOne(d => d.SupportNameNavigation).WithMany(p => p.Requirements)
                .HasPrincipalKey(p => p.SupportName)
                .HasForeignKey(d => d.SupportName)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REQUIREME__SUPPO__60A75C0F");
        });

        modelBuilder.Entity<Resetpassword>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RESETPAS__3214EC2751A15DEB");

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
            entity.HasKey(e => e.Id).HasName("PK__REVIEW__3214EC27984C8A63");

            entity.ToTable("REVIEW");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Comment).HasColumnName("COMMENT");
            entity.Property(e => e.Customerid)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("CUSTOMERID");
            entity.Property(e => e.Dateofupdate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("DATEOFUPDATE");
            entity.Property(e => e.Requirementsid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("REQUIREMENTSID");
            entity.Property(e => e.Staffid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("STAFFID");

            entity.HasOne(d => d.Customer).WithMany(p => p.Reviews)
                .HasPrincipalKey(p => p.Customerid)
                .HasForeignKey(d => d.Customerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REVIEW__CUSTOMER__6D0D32F4");

            entity.HasOne(d => d.Requirements).WithMany(p => p.Reviews)
                .HasPrincipalKey(p => p.Requirementsid)
                .HasForeignKey(d => d.Requirementsid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REVIEW__REQUIREM__6E01572D");

            entity.HasOne(d => d.Staff).WithMany(p => p.Reviews)
                .HasPrincipalKey(p => p.Staffid)
                .HasForeignKey(d => d.Staffid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REVIEW__STAFFID__6C190EBB");
        });

        modelBuilder.Entity<ReviewCriterion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__REVIEW_C__3214EC27F7DBDAD7");

            entity.ToTable("REVIEW_CRITERIA");

            entity.HasIndex(e => e.CriteriaName, "UQ__REVIEW_C__60CA328C83827CB5").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CriteriaName)
                .HasMaxLength(100)
                .HasColumnName("CRITERIA_NAME");
        });

        modelBuilder.Entity<ReviewDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__REVIEW_D__3214EC274A1A762E");

            entity.ToTable("REVIEW_DETAIL");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CriteriaId).HasColumnName("CRITERIA_ID");
            entity.Property(e => e.ReviewId).HasColumnName("REVIEW_ID");
            entity.Property(e => e.Star).HasColumnName("STAR");

            entity.HasOne(d => d.Criteria).WithMany(p => p.ReviewDetails)
                .HasForeignKey(d => d.CriteriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REVIEW_DE__CRITE__75A278F5");

            entity.HasOne(d => d.Review).WithMany(p => p.ReviewDetails)
                .HasForeignKey(d => d.ReviewId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REVIEW_DE__REVIE__74AE54BC");
        });

        modelBuilder.Entity<ServiceGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SERVICE___3214EC27E351A701");

            entity.ToTable("SERVICE_GROUP");

            entity.HasIndex(e => e.ServiceGroupid, "UQ__SERVICE___AA7131532C0E16BE").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__SERVICE___3214EC273119E385");

            entity.ToTable("SERVICE_TYPE");

            entity.HasIndex(e => e.ServiceTypename, "UQ__SERVICE___08CAC1E09519BC06").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ServiceGroupid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("SERVICE_GROUPID");
            entity.Property(e => e.ServiceTypename)
                .HasMaxLength(255)
                .HasColumnName("SERVICE_TYPEName");

            entity.HasOne(d => d.ServiceGroup).WithMany(p => p.ServiceTypes)
                .HasPrincipalKey(p => p.ServiceGroupid)
                .HasForeignKey(d => d.ServiceGroupid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SERVICE_T__SERVI__4F7CD00D");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__STAFF__3214EC27AEBDFC3C");

            entity.ToTable("STAFF");

            entity.HasIndex(e => e.Staffid, "UQ__STAFF__28B5063A2C7BCFAA").IsUnique();

            entity.HasIndex(e => e.Staffphone, "UQ__STAFF__821DCC581D94337A").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Department)
                .HasMaxLength(50)
                .HasColumnName("DEPARTMENT");
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
            entity.HasKey(e => e.Id).HasName("PK__SUPPORT___3214EC2778A86ACC");

            entity.ToTable("SUPPORT_TYPE");

            entity.HasIndex(e => e.SupportName, "UQ__SUPPORT___7661AF1E22EE6C6E").IsUnique();

            entity.HasIndex(e => e.SupportCode, "UQ__SUPPORT___E5AB823391222DBB").IsUnique();

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
