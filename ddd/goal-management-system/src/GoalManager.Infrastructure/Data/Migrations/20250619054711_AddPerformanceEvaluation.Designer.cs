﻿// <auto-generated />
using System;
using System.Collections.Generic;
using GoalManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GoalManager.Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250619054711_AddPerformanceEvaluation")]
    partial class AddPerformanceEvaluation
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("GoalManager.Core.GoalManagement.Goal", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("ActualValue")
                        .HasColumnType("int");

                    b.Property<int>("GoalSetId")
                        .HasColumnType("int");

                    b.Property<int>("GoalType")
                        .HasColumnType("int");

                    b.Property<int>("Percentage")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int?>("ProgressId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.ComplexProperty<Dictionary<string, object>>("GoalValue", "GoalManager.Core.GoalManagement.Goal.GoalValue#GoalValue", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<int>("GoalValueType")
                                .HasColumnType("int");

                            b1.Property<int>("MaxValue")
                                .HasColumnType("int");

                            b1.Property<int>("MidValue")
                                .HasColumnType("int");

                            b1.Property<int>("MinValue")
                                .HasColumnType("int");
                        });

                    b.HasKey("Id");

                    b.HasIndex("GoalSetId");

                    b.HasIndex("ProgressId")
                        .IsUnique()
                        .HasFilter("[ProgressId] IS NOT NULL");

                    b.HasIndex("Title");

                    b.ToTable("Goal");
                });

            modelBuilder.Entity("GoalManager.Core.GoalManagement.GoalPeriod", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("TeamId")
                        .HasColumnType("int");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TeamId", "Year")
                        .IsUnique();

                    b.ToTable("GoalPeriod");
                });

            modelBuilder.Entity("GoalManager.Core.GoalManagement.GoalProgress", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ActualValue")
                        .HasColumnType("int");

                    b.Property<string>("Comment")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int>("GoalId")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GoalId");

                    b.ToTable("GoalProgress");
                });

            modelBuilder.Entity("GoalManager.Core.GoalManagement.GoalSet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("PeriodId")
                        .HasColumnType("int");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.Property<int>("TeamId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TeamId", "PeriodId", "UserId")
                        .IsUnique();

                    b.ToTable("GoalSet");
                });

            modelBuilder.Entity("GoalManager.Core.Notification.NotificationItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("NotificationItem");
                });

            modelBuilder.Entity("GoalManager.Core.Organisation.Organisation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Organisation");
                });

            modelBuilder.Entity("GoalManager.Core.Organisation.Team", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("OrganisationId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OrganisationId", "Name")
                        .IsUnique();

                    b.ToTable("Team");
                });

            modelBuilder.Entity("GoalManager.Core.Organisation.TeamMember", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("MemberType")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("TeamId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TeamId");

                    b.ToTable("TeamMember");
                });

            modelBuilder.Entity("GoalManager.Core.PerformanceEvaluation.GoalEvaluation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ActualValue")
                        .HasColumnType("int");

                    b.Property<int>("GoalSetEvaluationId")
                        .HasColumnType("int");

                    b.Property<string>("GoalTitle")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<int>("Percentage")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<double?>("Point")
                        .HasColumnType("float");

                    b.ComplexProperty<Dictionary<string, object>>("GoalValue", "GoalManager.Core.PerformanceEvaluation.GoalEvaluation.GoalValue#GoalEvaluationValue", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<int>("MaxValue")
                                .HasColumnType("int");

                            b1.Property<int>("MidValue")
                                .HasColumnType("int");

                            b1.Property<int>("MinValue")
                                .HasColumnType("int");
                        });

                    b.HasKey("Id");

                    b.HasIndex("GoalSetEvaluationId");

                    b.HasIndex("GoalTitle");

                    b.ToTable("GoalEvaluation");
                });

            modelBuilder.Entity("GoalManager.Core.PerformanceEvaluation.GoalSetEvaluation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GoalSetId")
                        .HasColumnType("int");

                    b.Property<string>("PerformanceGrade")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("PerformanceScore")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("GoalSetId");

                    b.ToTable("GoalSetEvaluation");
                });

            modelBuilder.Entity("GoalManager.Core.GoalManagement.Goal", b =>
                {
                    b.HasOne("GoalManager.Core.GoalManagement.GoalSet", null)
                        .WithMany("Goals")
                        .HasForeignKey("GoalSetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GoalManager.Core.GoalManagement.GoalProgress", "GoalProgress")
                        .WithOne("CurrentGoal")
                        .HasForeignKey("GoalManager.Core.GoalManagement.Goal", "ProgressId");

                    b.Navigation("GoalProgress");
                });

            modelBuilder.Entity("GoalManager.Core.GoalManagement.GoalProgress", b =>
                {
                    b.HasOne("GoalManager.Core.GoalManagement.Goal", "Goal")
                        .WithMany("GoalProgressHistory")
                        .HasForeignKey("GoalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Goal");
                });

            modelBuilder.Entity("GoalManager.Core.Organisation.Team", b =>
                {
                    b.HasOne("GoalManager.Core.Organisation.Organisation", "Organisation")
                        .WithMany("Teams")
                        .HasForeignKey("OrganisationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organisation");
                });

            modelBuilder.Entity("GoalManager.Core.Organisation.TeamMember", b =>
                {
                    b.HasOne("GoalManager.Core.Organisation.Team", "Team")
                        .WithMany("TeamMembers")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Team");
                });

            modelBuilder.Entity("GoalManager.Core.PerformanceEvaluation.GoalEvaluation", b =>
                {
                    b.HasOne("GoalManager.Core.PerformanceEvaluation.GoalSetEvaluation", null)
                        .WithMany("GoalEvaluations")
                        .HasForeignKey("GoalSetEvaluationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GoalManager.Core.GoalManagement.Goal", b =>
                {
                    b.Navigation("GoalProgressHistory");
                });

            modelBuilder.Entity("GoalManager.Core.GoalManagement.GoalProgress", b =>
                {
                    b.Navigation("CurrentGoal")
                        .IsRequired();
                });

            modelBuilder.Entity("GoalManager.Core.GoalManagement.GoalSet", b =>
                {
                    b.Navigation("Goals");
                });

            modelBuilder.Entity("GoalManager.Core.Organisation.Organisation", b =>
                {
                    b.Navigation("Teams");
                });

            modelBuilder.Entity("GoalManager.Core.Organisation.Team", b =>
                {
                    b.Navigation("TeamMembers");
                });

            modelBuilder.Entity("GoalManager.Core.PerformanceEvaluation.GoalSetEvaluation", b =>
                {
                    b.Navigation("GoalEvaluations");
                });
#pragma warning restore 612, 618
        }
    }
}
