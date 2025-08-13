using FlowerSelling.Data;
using FlowerSellingWebsite.Infrastructure.Authorization;
using FlowerSellingWebsite.Models.Entities;

namespace FlowerSellingWebsite.Infrastructure.DbContext
{
    public static class SeedData
    {
        public static void Initialize(FlowerSellingDbContext context)
        {
            // 1. Seed Permissions
            foreach (var rolePerms in RolePermissions.DefaultRolePermissions)
            {
                foreach (var permName in rolePerms.Value)
                {
                    if (!context.Permissions.Any(p => p.PermissionName == permName))
                    {
                        context.Permissions.Add(new Permission
                        {
                            PermissionName = permName,
                            Description = null
                        });
                    }
                }
            }
            context.SaveChanges();

            // 2. Seed Roles
            foreach (var roleName in RolePermissions.DefaultRolePermissions.Keys)
            {
                if (!context.Roles.Any(r => r.RoleName == roleName))
                {
                    context.Roles.Add(new Role
                    {
                        RoleName = roleName,
                        Description = null
                    });
                }
            }
            context.SaveChanges();

            // 3. Seed RolePermissions
            foreach (var rolePair in RolePermissions.DefaultRolePermissions)
            {
                var role = context.Roles.First(r => r.RoleName == rolePair.Key);

                foreach (var permName in rolePair.Value)
                {
                    var perm = context.Permissions.First(p => p.PermissionName == permName);

                    if (!context.RolePermissions.Any(rp => rp.RoleId == role.Id && rp.PermissionId == perm.Id))
                    {
                        context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = perm.Id
                        });
                    }
                }
            }
            context.SaveChanges();
        }
    }
}
