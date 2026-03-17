using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Services.Users;

namespace VehicleRegistryAPI.Data.Interceptors
{
    public class AuditableEntityInterceptor: SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;

        public AuditableEntityInterceptor(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null)
                return base.SavingChangesAsync(eventData, result, cancellationToken);

            // Obtener todas las entidades BaseEntity (incluyendo las marcadas para eliminación)
            var entries = context.ChangeTracker
                .Entries<BaseEntity>()
                .ToList();

            var userId = _currentUserService.UserId;

            foreach (var entry in entries)
            {
                // --- ENTIDADES NUEVAS ---
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = userId;

                    // Si se crea ya inactiva, registrar desactivación
                    if (!entry.Entity.IsActive && entry.Entity.DeactivatedAt == null)
                    {
                        entry.Entity.DeactivatedAt = DateTime.UtcNow;
                        entry.Entity.DeactivatedBy = userId;
                    }
                }

                // --- ENTIDADES MODIFICADAS ---
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = userId;

                    // Detectar cambios en IsActive
                    var isActiveProperty = entry.Property(e => e.IsActive);
                    if (isActiveProperty.IsModified)
                    {
                        if (!entry.Entity.IsActive && entry.Entity.DeactivatedAt == null)
                        {
                            // Se está desactivando
                            entry.Entity.DeactivatedAt = DateTime.UtcNow;
                            entry.Entity.DeactivatedBy = userId;
                        }
                        else if (entry.Entity.IsActive)
                        {
                            // Se está reactivando: limpiar campos de desactivación
                            entry.Entity.DeactivatedAt = null;
                            entry.Entity.DeactivatedBy = null;
                        }
                    }
                }

                // --- BORRADO LÓGICO (soft delete) ---
                // Solo si la entidad implementa ISoftDelete
                else if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified; // Convertir eliminación en modificación
                    entry.Entity.IsActive = false;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = userId;

                    // Registrar desactivación si no existe
                    if (entry.Entity.DeactivatedAt == null)
                    {
                        entry.Entity.DeactivatedAt = DateTime.UtcNow;
                        entry.Entity.DeactivatedBy = userId;
                    }
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
