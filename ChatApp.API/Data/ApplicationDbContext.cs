﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace ChatApp.API.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Relationship> Relationships { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<GroupRequest> GroupRequests { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
           
            // ChatMessage entity relationships
            builder.Entity<ChatMessage>(entity =>
            {
                entity.HasOne(d => d.FromUser)
                    .WithMany(p => p.ChatMessagesFromUsers)
                    .HasForeignKey(d => d.FromUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.ToUser)
                    .WithMany(p => p.ChatMessagesToUsers)
                    .HasForeignKey(d => d.ToUserId)
                    .OnDelete(DeleteBehavior.Cascade);



                entity.HasOne(d => d.ToGroup)
                    .WithMany(g => g.ChatMessages)
                    .HasForeignKey(c => c.ToGroupId).
                    OnDelete(DeleteBehavior.Cascade);

            });

            // Group and User many-to-many relationship via GroupMember
            builder.Entity<GroupMember>()
                .HasKey(gm => new { gm.GroupId, gm.UserId });

            builder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.Groups)
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);



            builder.Entity<Relationship>().
                HasKey(r => r.Id);

            builder.Entity<Relationship>()
                .HasOne(r=> r.User)
                .WithMany(u=>u.Friends)
                .HasForeignKey(k=> k.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            

            builder.Entity<FriendRequest>()
                .HasKey(r => new { r.SenderId, r.ReceiverId });

            builder.Entity<FriendRequest>()
                .HasOne(r => r.Sender)
                .WithMany(u => u.FriendRequestsSent)
                .HasForeignKey(r => r.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<FriendRequest>()
                .HasOne(r => r.Receiver)
                .WithMany(u => u.FriendRequestsReceived)
                .HasForeignKey(r => r.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);


            builder.Entity<GroupRequest>()
                .HasKey(r => new { r.SenderId, r.GroupId });

            builder.Entity<GroupRequest>()
                .HasOne(r => r.Sender)
                .WithMany(u => u.GroupRequestsSent)
                .HasForeignKey(r => r.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<GroupRequest>()
                .HasOne(r => r.Group)
                .WithMany(u => u.GroupRequestsReceived)
                .HasForeignKey(r => r.GroupId)
                .OnDelete(DeleteBehavior.NoAction);



        }
    }
}
