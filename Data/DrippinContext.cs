using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Drippin.Models;

namespace Drippin.Data
{
    /// <summary>
    /// Representa el contexto principal de la base de datos de la aplicación,
    /// orquestando el mapeo objeto-relacional (ORM) y la persistencia de las entidades.
    /// Utilizado por todos los controladores del sistema para el acceso a datos.
    /// </summary>
    public class DrippinContext : DbContext
    {
        #region Constructor

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="DrippinContext"/> utilizando las opciones de configuración provistas.
        /// </summary>
        /// <param name="options">Opciones de configuración del contexto (cadena de conexión, proveedor, etc.).</param>
        public DrippinContext (DbContextOptions<DrippinContext> options)
            : base(options)
        {
        }

        #endregion

        #region Configuración del Modelo (Fluent API)

        /// <summary>
        /// Configura el esquema de la base de datos y define las relaciones entre entidades mediante Fluent API.
        /// </summary>
        /// <param name="modelBuilder">Constructor de modelos para la definición de entidades.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
           
            // Configuración de la relación entre Usuario y Role.
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Usuarios)  
                      .HasForeignKey(u => u.IdRol)
                      .HasConstraintName("FK_Usuario_Role") 
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired();
            });

            // Sembrado de datos iniciales (Seed Data) para la entidad Role.
            modelBuilder.Entity<Role>().HasData(
                new Role { IdRol = 1, NombreRol = "Administrador" },
                new Role { IdRol = 2, NombreRol = "Cliente" }
            );

            // Configuración de las relaciones para los ítems del carrito de compras.
            modelBuilder.Entity<CarritoItem>(entity =>
            {
                entity.HasOne(ci => ci.Usuario)
                      .WithMany(u => u.CarritoItems)
                      .HasForeignKey(ci => ci.IdUsuario)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Producto)
                      .WithMany() 
                      .HasForeignKey(ci => ci.proId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        #endregion

        #region Conjuntos de Datos (DbSet)

        /// <summary>
        /// Repositorio de productos del catálogo.
        /// </summary>
        public DbSet<Drippin.Models.Producto> Producto { get; set; } = default!;

        /// <summary>
        /// Repositorio de categorías de productos.
        /// </summary>
        public DbSet<Drippin.Models.Categoria> Categoria { get; set; }

        /// <summary>
        /// Repositorio de registros de acceso y autenticación.
        /// </summary>
        public DbSet<Drippin.Models.Acceso> Acceso { get; set; } = default!;

        /// <summary>
        /// Repositorio de perfiles de usuario.
        /// </summary>
        public DbSet<Drippin.Models.Usuario> Usuario { get; set; } = default!;

        /// <summary>
        /// Repositorio de roles de seguridad del sistema.
        /// </summary>
        public DbSet<Drippin.Models.Role> Role { get; set; } = default!;

        /// <summary>
        /// Repositorio de ítems contenidos en los carritos de compras.
        /// </summary>
        public DbSet<Drippin.Models.CarritoItem> ItemCarrito { get; set; }

        /// <summary>
        /// Repositorio de tandas o colecciones temporales de productos.
        /// </summary>
        public DbSet<Drippin.Models.Tanda> Tanda { get; set; }

        #endregion
    }
}
