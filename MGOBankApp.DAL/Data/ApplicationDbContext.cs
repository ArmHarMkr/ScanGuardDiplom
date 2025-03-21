using System;
﻿using MGOBankApp.Domain.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGOBankApp.DAL.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FileScanEntity> FileScanEntities { get; set; }
        public DbSet<VulnerabilityEntity> VulnerabilityEntities { get; set; }
        public DbSet<NotificationEntity> NotificationEntities { get; set; }
        public DbSet<WebsiteScanEntity> WebsiteScanEntities { get; set; }
        public DbSet<SiteScanCountEntity> SiteScanCounts { get; set; }
        public DbSet<TGUserEntity> TGUserEntities { get; set; }
        public DbSet<ChatMessageEntity> ChatMessages { get; set; }
        public DbSet<NewsEntity> NewsEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            /*builder.Entity<ApplicationUser>(b =>
            {
                b.HasKey(u => u.Id);
            });

            // Связь между ApplicationUser и WebsiteScanEntity
            builder.Entity<WebsiteScanEntity>()
                .HasOne(w => w.ScanUser)
                .WithMany(u => u.WebsiteScans)
                .HasForeignKey(w => w.ScanUserId);

            // Связь между ApplicationUser и NotificationEntity (один-ко-многим)
            builder.Entity<NotificationEntity>()
                .HasOne(n => n.ApplicationUser)
                .WithMany(u => u.Notifications) // Добавьте это свойство в ApplicationUser, если нужно
                .HasForeignKey(n => n.ApplicationUserId);

            // Связь между WebsiteScanEntity и VulnerabilityEntity (один-ко-многим)
            builder.Entity<VulnerabilityEntity>()
                .HasOne(v => v.ScanEntity)
                .WithMany(w => w.Vulnerabilities) // Добавьте это свойство в WebsiteScanEntity
                .HasForeignKey(v => v.ScanEntityId);

            // Опционально: если FileScanEntity должен быть связан с ApplicationUser
            builder.Entity<FileScanEntity>()
                .HasOne(f => f.ScannedByUser) // Добавьте это свойство в FileScanEntity
                .WithMany(u => u.FileScans) // Добавьте это свойство в ApplicationUser
                .HasForeignKey(f => f.ApplicationUserId); // Добавьте это поле в FileScanEntity*/
        }
    }
}
