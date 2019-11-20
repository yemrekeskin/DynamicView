﻿// <auto-generated />
using System;
using DynamicView.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DynamicView.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20191106124820_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DynamicView.Entity.Contract", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.Property<int>("ContractType");

                    b.Property<DateTime?>("CreatedAt");

                    b.Property<Guid?>("CreatedById");

                    b.Property<string>("Description");

                    b.Property<DateTime?>("ExpiredDate");

                    b.Property<DateTime?>("ModifiedAt");

                    b.Property<Guid?>("ModifiedById");

                    b.Property<int>("State");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(500);

                    b.HasKey("Id");

                    b.ToTable("Contract");
                });

            modelBuilder.Entity("DynamicView.Entity.Phone", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("CreatedAt");

                    b.Property<Guid?>("CreatedById");

                    b.Property<DateTime?>("ModifiedAt");

                    b.Property<Guid?>("ModifiedById");

                    b.Property<int>("PhoneType");

                    b.Property<int>("State");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.HasKey("Id");

                    b.ToTable("Phone");
                });
#pragma warning restore 612, 618
        }
    }
}
