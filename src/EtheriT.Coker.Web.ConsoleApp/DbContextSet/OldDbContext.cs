using EtheriT.Coker.Web.ConsoleApp.Models;
using EtheriT.Coker.Web.ConsoleApp.Models.OldDB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.ConsoleApp.DbContextSet
{
    public class OldDbContext : DbContext
    {
        public DbSet<Menus> Menus { get; set; }
        public DbSet<MenuSub> MenuSubs { get; set; }
        public DbSet<Menu_cont> MenuConts { get; set; }
        public DbSet<ShopInfo> ShopInfos { get; set; }
        public DbSet<tag> Tag { get; set; }
        public DbSet<prod_tag> ProdTag { get; set; }
        private string connectionString { get; set; }
        public OldDbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
