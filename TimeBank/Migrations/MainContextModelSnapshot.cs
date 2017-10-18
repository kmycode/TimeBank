using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using TimeBank.Models.Data.MainContext;

namespace TimeBank.Migrations
{
    [DbContext(typeof(MainContext))]
    partial class MainContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.3");

            modelBuilder.Entity("TimeBank.Models.Data.MainContext.Work", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Works");
                });

            modelBuilder.Entity("TimeBank.Models.Data.MainContext.WorkTime", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Day");

                    b.Property<int>("DoneSeconds");

                    b.Property<int>("Month");

                    b.Property<int>("WorkId");

                    b.Property<int>("Year");

                    b.HasKey("Id");

                    b.ToTable("WorkTimes");
                });
        }
    }
}
