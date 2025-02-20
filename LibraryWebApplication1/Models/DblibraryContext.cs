using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using LibraryWebApplication1.Models;
public partial class DblibraryContext : DbContext
{
    public DblibraryContext()
    {
    }
    public DblibraryContext(DbContextOptions<DblibraryContext> options)
        : base(options)
    {
    }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Subject> Subjects { get; set; }
    public virtual DbSet<Lecture> Lectures { get; set; }
    public virtual DbSet<Request> Requests { get; set; }
    public virtual DbSet<SchedulePart> ScheduleParts { get; set; }
    public virtual DbSet<Group> Groups { get; set; }
    public virtual DbSet<GroupSubject> GroupSubjects { get; set; }
    public virtual DbSet<TeacherSubject> TeacherSubjects { get; set; }
    public virtual DbSet<Auditory> Auditories { get; set; }
    public virtual DbSet<Course> Courses { get; set; }
    public virtual DbSet<Specialty> Specialties { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = "Server=DESKTOP-11NQTPR\\SQLEXPRESS;Database=DBScheduleManagerV4;Trusted_Connection=True;TrustServerCertificate=True;";
        optionsBuilder.UseSqlServer(connectionString, options =>
        {
            options.EnableRetryOnFailure();
            options.CommandTimeout(180);
        });
        optionsBuilder.EnableSensitiveDataLogging();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("userId");
            entity.Property(e => e.Surname)
                .HasMaxLength(50)
                .IsFixedLength()
                .HasColumnName("surname");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.HasMany(c => c.Requests)
            .WithOne(a => a.Teacher)
            .HasForeignKey(a => a.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(c => c.TeacherSubjects)
            .WithOne(a => a.Teacher)
            .HasForeignKey(a => a.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("Subject");
            entity.Property(e => e.SubjectId)
                .ValueGeneratedNever()
                .HasColumnName("subjectId");
            //entity.Ignore(e => e.UserId);
            entity.HasMany(c => c.GroupSubjects)
            .WithOne(a => a.Subject)
            .HasForeignKey(a => a.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(c => c.TeacherSubjects)
            .WithOne(a => a.Subject)
            .HasForeignKey(a => a.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Lecture>(entity =>
        {
            entity.ToTable("Lecture");
            entity.Property(e => e.LectureId)
                .ValueGeneratedNever()
                .HasColumnName("lectureId");
            entity.HasMany(c => c.Requests)
            .WithOne(a => a.Lecture)
            .HasForeignKey(a => a.LectureId)
            .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(c => c.ScheduleParts)
            .WithOne(a => a.Lecture)
            .HasForeignKey(a => a.LectureId)
            .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Request>(entity =>
        {
            entity.ToTable("Request");
            entity.Property(e => e.RequestId)
                .ValueGeneratedNever()
                .HasColumnName("requestId");
            entity.HasOne(d => d.Teacher).WithMany(p => p.Requests)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Request_User");
            entity.HasOne(d => d.Lecture).WithMany(p => p.Requests)
                .HasForeignKey(d => d.LectureId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Request_Lecture");
        });
        modelBuilder.Entity<SchedulePart>(entity =>
        {
            entity.ToTable("SchedulePart");
            entity.Property(e => e.PartId)
                .ValueGeneratedNever()
                .HasColumnName("partId");
            entity.HasOne(d => d.Lecture).WithMany(p => p.ScheduleParts)
                .HasForeignKey(d => d.LectureId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SchedulePart_Lecture");
            entity.HasOne(d => d.TeacherSubject).WithMany(p => p.ScheduleParts)
                .HasForeignKey(d => d.TeacherSubjectId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SchedulePart_TeacherSubject");
            entity.HasOne(d => d.Group).WithMany(p => p.ScheduleParts)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SchedulePart_Group");
            entity.HasOne(d => d.Auditory).WithMany(p => p.ScheduleParts)
                .HasForeignKey(d => d.AuditoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SchedulePart_Auditory");
        });
        modelBuilder.Entity<Group>(entity =>
        {
            entity.ToTable("Group");
            entity.Property(e => e.GroupId)
                .ValueGeneratedNever()
                .HasColumnName("groupId");
            entity.HasMany(c => c.GroupSubjects)
            .WithOne(a => a.Group)
            .HasForeignKey(a => a.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(c => c.ScheduleParts)
            .WithOne(a => a.Group)
            .HasForeignKey(a => a.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.Course).WithMany(p => p.Groups)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Group_Course");
            entity.HasOne(d => d.Specialty).WithMany(p => p.Groups)
                .HasForeignKey(d => d.SpecialtyId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Group_Specialty");
        });
        modelBuilder.Entity<GroupSubject>(entity =>
        {
            entity.ToTable("GroupSubject");
            entity.Property(e => e.ItemId)
                .ValueGeneratedNever()
                .HasColumnName("itemId");
            entity.HasOne(d => d.Group).WithMany(p => p.GroupSubjects)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_GroupSubject_Group");
            entity.HasOne(d => d.Subject).WithMany(p => p.GroupSubjects)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_GroupSubject_Subject");
        });
        modelBuilder.Entity<TeacherSubject>(entity =>
        {
            entity.ToTable("TeacherSubject");
            entity.Property(e => e.ItemId)
                .ValueGeneratedNever()
                .HasColumnName("itemId");
            entity.HasOne(d => d.Teacher).WithMany(p => p.TeacherSubjects)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TeacherSubject_Teacher");
            entity.HasOne(d => d.Subject).WithMany(p => p.TeacherSubjects)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TeacherSubject_Subject");
            entity.HasMany(c => c.ScheduleParts)
            .WithOne(a => a.TeacherSubject)
            .HasForeignKey(a => a.TeacherSubjectId)
            .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Auditory>(entity =>
        {
            entity.ToTable("Auditory");
            entity.Property(e => e.AuditoryId)
                .ValueGeneratedNever()
                .HasColumnName("AuditoryId");
            entity.HasMany(c => c.ScheduleParts)
            .WithOne(a => a.Auditory)
            .HasForeignKey(a => a.AuditoryId)
            .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Specialty>(entity =>
        {
            entity.ToTable("Specialty");
            entity.Property(e => e.SpecialtyId)
                .ValueGeneratedNever()
                .HasColumnName("specialtyId");
            entity.HasMany(c => c.Groups)
            .WithOne(a => a.Specialty)
            .HasForeignKey(a => a.SpecialtyId)
            .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("Course");
            entity.Property(e => e.CourseId)
                .ValueGeneratedNever()
                .HasColumnName("courseId");
            entity.HasMany(c => c.Groups)
            .WithOne(a => a.Course)
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
        });
        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    public DbSet<LibraryWebApplication1.Models.Lecture> Lecture { get; set; } = default!;
    public DbSet<LibraryWebApplication1.Models.Request> Request { get; set; } = default!;
    public DbSet<LibraryWebApplication1.Models.TeacherSubject> TeacherSubject { get; set; } = default!;
}