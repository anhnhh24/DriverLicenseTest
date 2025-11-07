using System;
using System.Collections.Generic;
using DriverLicenseTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DriverLicenseTest.Infrastructure.Data;

public partial class DriverLicenseTestContext : DbContext
{
    public DriverLicenseTestContext()
    {
    }

    public DriverLicenseTestContext(DbContextOptions<DriverLicenseTestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AnswerOption> AnswerOptions { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<LicenseQuestion> LicenseQuestions { get; set; }

    public virtual DbSet<LicenseType> LicenseTypes { get; set; }

    public virtual DbSet<MockExam> MockExams { get; set; }

    public virtual DbSet<MockExamAnswer> MockExamAnswers { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<TrafficSign> TrafficSigns { get; set; }

    public virtual DbSet<UserStatistic> UserStatistics { get; set; }

    public virtual DbSet<UserWrongQuestion> UserWrongQuestions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server=localhost;database= DriverLicenseTest;uid=sa;pwd=123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnswerOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__AnswerOp__92C7A1FF7B275A94");

            entity.HasIndex(e => e.QuestionId, "idx_answer_question");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.OptionText).HasMaxLength(500);

            entity.HasOne(d => d.Question).WithMany(p => p.AnswerOptions)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__AnswerOpt__Quest__540C7B00");
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AspNetRo__3214EC0734B73AA1");

            entity.HasIndex(e => e.NormalizedName, "IX_AspNetRoles_NormalizedName").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AspNetUs__3214EC07681F6613");

            entity.HasIndex(e => e.NormalizedEmail, "IX_AspNetUsers_NormalizedEmail").IsUnique();

            entity.HasIndex(e => e.NormalizedUserName, "IX_AspNetUsers_NormalizedUserName").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK__AspNetUse__RoleI__3493CFA7"),
                    l => l.HasOne<AspNetUser>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK__AspNetUse__UserI__339FAB6E"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__AspNetUs__AF2760ADBD55DE7B");
                        j.ToTable("AspNetUserRoles");
                    });
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0B23E55C1F");

            entity.Property(e => e.CategoryName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<LicenseQuestion>(entity =>
        {
            entity.HasKey(e => new { e.LicenseTypeId, e.QuestionId }).HasName("PK__LicenseQ__982B9202632D129E");

            entity.HasIndex(e => e.QuestionId, "idx_license_question_id");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.LicenseType).WithMany(p => p.LicenseQuestions)
                .HasForeignKey(d => d.LicenseTypeId)
                .HasConstraintName("FK__LicenseQu__Licen__57DD0BE4");

            entity.HasOne(d => d.Question).WithMany(p => p.LicenseQuestions)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__LicenseQu__Quest__58D1301D");
        });

        modelBuilder.Entity<LicenseType>(entity =>
        {
            entity.HasKey(e => e.LicenseTypeId).HasName("PK__LicenseT__48F794F805E422E1");

            entity.HasIndex(e => e.LicenseCode, "UQ__LicenseT__516B6B64CA78C49D").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.LicenseCode).HasMaxLength(10);
            entity.Property(e => e.LicenseName).HasMaxLength(100);
            entity.Property(e => e.RequiredElimination).HasDefaultValue(1);
            entity.Property(e => e.VehicleType).HasMaxLength(100);
        });


        modelBuilder.Entity<MockExam>(entity =>
        {
            entity.HasKey(e => e.ExamId).HasName("PK__MockExam__297521C7C39CB108");

            entity.HasIndex(e => new { e.UserId, e.StartedAt }, "idx_mock_user");

            entity.Property(e => e.CompletedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FailedElimination).HasDefaultValue(false);
            entity.Property(e => e.PassStatus).HasMaxLength(10);

            entity.HasOne(d => d.LicenseType).WithMany(p => p.MockExams)
                .HasForeignKey(d => d.LicenseTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MockExams__Licen__662B2B3B");

            entity.HasOne(d => d.User).WithMany(p => p.MockExams)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__MockExams__UserI__65370702");
        });

        modelBuilder.Entity<MockExamAnswer>(entity =>
        {
            entity.HasKey(e => e.ExamAnswerId).HasName("PK__MockExam__CD77DFF677F337BF");

            entity.HasIndex(e => e.ExamId, "idx_mea_exam");

            entity.Property(e => e.AnsweredAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Exam).WithMany(p => p.MockExamAnswers)
                .HasForeignKey(d => d.ExamId)
                .HasConstraintName("FK__MockExamA__ExamI__6AEFE058");

            entity.HasOne(d => d.Question).WithMany(p => p.MockExamAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MockExamA__Quest__6BE40491");

            entity.HasOne(d => d.SelectedOption).WithMany(p => p.MockExamAnswers)
                .HasForeignKey(d => d.SelectedOptionId)
                .HasConstraintName("FK__MockExamA__Selec__6CD828CA");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06FAC2888991B");

            entity.HasIndex(e => e.QuestionNumber, "UQ__Question__3A4692873D041B4F").IsUnique();

            entity.HasIndex(e => e.CategoryId, "idx_category");

            entity.HasIndex(e => e.QuestionNumber, "idx_question_number");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DifficultyLevel)
                .HasMaxLength(20)
                .HasDefaultValue("Medium");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("ImageURL");
            entity.Property(e => e.IsElimination).HasDefaultValue(false);
            entity.Property(e => e.Points).HasDefaultValue(1);
            entity.Property(e => e.TimeLimit).HasDefaultValue(30);

            entity.HasOne(d => d.Category).WithMany(p => p.Questions)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Questions__Categ__503BEA1C");
        });

        modelBuilder.Entity<TrafficSign>(entity =>
        {
            entity.HasKey(e => e.SignId).HasName("PK__TrafficS__38A20B73DE176AA1");

            entity.HasIndex(e => e.SignCode, "UQ__TrafficS__EDEA1693660D51A6").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("ImageURL");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SignCode).HasMaxLength(50);
            entity.Property(e => e.SignName).HasMaxLength(200);
            entity.Property(e => e.SignType).HasMaxLength(50);
        });

        modelBuilder.Entity<UserStatistic>(entity =>
        {
            entity.HasKey(e => e.StatisticId).HasName("PK__UserStat__367DEB173F786B6C");

            entity.HasIndex(e => e.UserId, "UQ__UserStat__1788CC4D1078C232").IsUnique();

            entity.Property(e => e.AccuracyRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.AverageScore).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithOne(p => p.UserStatistic)
                .HasForeignKey<UserStatistic>(d => d.UserId)
                .HasConstraintName("FK__UserStati__UserI__2610A626");
        });

        modelBuilder.Entity<UserWrongQuestion>(entity =>
        {
            entity.HasKey(e => e.WrongQuestionId).HasName("PK__UserWron__1D6A135CF176E273");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.LastWrongAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.WrongCount).HasDefaultValue(1);

            entity.HasOne(d => d.Question).WithMany(p => p.UserWrongQuestions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserWrong__Quest__1209AD79");

            entity.HasOne(d => d.User).WithMany(p => p.UserWrongQuestions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserWrong__UserI__11158940");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
