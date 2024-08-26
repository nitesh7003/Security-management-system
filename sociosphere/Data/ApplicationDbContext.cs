using Microsoft.EntityFrameworkCore;
using sociosphere.Models;
using System.Collections.Generic;

namespace sociosphere.Data
{
    public class ApplicationDbContext : DbContext
    {


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<userreg> userregs { get; set; }

        public DbSet<addflat> addflats { get; set; }

        public DbSet<alloteflat> alloteflats { get; set; }

        public DbSet<addcomplaint> addcomplaints { get; set; }

        public DbSet<announcement> announcements { get; set; }

        public DbSet<gatemanagement> gatemanagements { get; set; }

        public DbSet<billmanagement> billmanagements { get; set; }
    }
}

