﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SharedDB;

#nullable disable

namespace SharedDB.Migrations
{
    [DbContext(typeof(SharedDbContext))]
    [Migration("20250221083915_updateTokenDb_AccountName")]
    partial class updateTokenDb_AccountName
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SharedDB.DataModel+ServerStatusDb", b =>
                {
                    b.Property<int>("ServerStatusDbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ServerStatusDbId"));

                    b.Property<int>("CrowdedLevel")
                        .HasColumnType("int");

                    b.Property<string>("IpAddress")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Port")
                        .HasColumnType("int");

                    b.HasKey("ServerStatusDbId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("ServerInfo");
                });

            modelBuilder.Entity("SharedDB.DataModel+TokenDb", b =>
                {
                    b.Property<int>("TokenDbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TokenDbId"));

                    b.Property<int>("AccountDbId")
                        .HasColumnType("int");

                    b.Property<string>("AccountName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Expired")
                        .HasColumnType("datetime2");

                    b.Property<int>("Token")
                        .HasColumnType("int");

                    b.HasKey("TokenDbId");

                    b.HasIndex("AccountDbId")
                        .IsUnique();

                    b.ToTable("Token");
                });
#pragma warning restore 612, 618
        }
    }
}
