﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ParallelEfContext.Context;

#nullable disable

namespace ParallelEfContext.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240111190023_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.1");

            modelBuilder.Entity("ParallelEfContext.Model.Letter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<char>("Char")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Letters");
                });
#pragma warning restore 612, 618
        }
    }
}
