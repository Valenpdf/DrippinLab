using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Drippin.Models;

namespace Drippin.Data
{
    public class DrippinContext : DbContext
    {
        public DrippinContext (DbContextOptions<DrippinContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
           
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasOne(u => u.Role)
                 .WithMany(r => r.Usuarios)  
                .HasForeignKey(u => u.IdRol)
                .HasConstraintName("FK_Usuario_Role") 
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            });

            // **Añadir Roles Iniciales (SEED DATA)**
            modelBuilder.Entity<Role>().HasData(
                new Role { IdRol = 1, NombreRol = "Administrador" },
                new Role { IdRol = 2, NombreRol = "Cliente" }
            );

            modelBuilder.Entity<CarritoItem>(entity =>
            {
                entity.HasOne(ci => ci.Usuario)
                      .WithMany(u => u.CarritoItems) // Asegúrate de que Usuario tenga esta propiedad
                      .HasForeignKey(ci => ci.IdUsuario) // Usar IdUsuario como FK
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Producto)
                      .WithMany() // O WithMany(p => p.CarritoItems) si Producto tiene la colección
                      .HasForeignKey(ci => ci.proId)
                      .OnDelete(DeleteBehavior.Cascade);
            });



        }

        public DbSet<Drippin.Models.Producto> Producto { get; set; } = default!;
        public DbSet<Drippin.Models.Categoria> Categoria { get; set; }
        public DbSet<Drippin.Models.Acceso> Acceso { get; set; } = default!;
        public DbSet<Drippin.Models.Usuario> Usuario { get; set; } = default!;
        public DbSet<Drippin.Models.Role> Role { get; set; } = default!;
        public DbSet<Drippin.Models.CarritoItem> ItemCarrito { get; set; }

    }
}
