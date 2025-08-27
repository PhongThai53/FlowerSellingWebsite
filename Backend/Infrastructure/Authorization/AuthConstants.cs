namespace FlowerSellingWebsite.Infrastructure.Authorization
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "Users";
        public const string Staff = "Staff";
        public const string Supplier = "Supplier";
    }
    public static class Permissions
    {
        // Users Management
        public const string ViewUsers = "users.view";
        public const string CreateUsers = "users.create";
        public const string UpdateUsers = "users.update";
        public const string DeleteUsers = "users.delete";
        // Order Management
        public const string ViewAllOrders = "orders.view.all";
        public const string ViewOwnOrders = "orders.view.own";
        public const string ViewAssignedOrders = "orders.view.assigned";
        public const string CreateOrders = "orders.create";
        public const string UpdateOrders = "orders.update";
        public const string DeleteOrders = "orders.delete";
        public const string AssignOrders = "orders.assign";
        // Product Management
        public const string ViewProducts = "products.view";
        public const string CreateProducts = "products.create";
        public const string UpdateProducts = "products.update";
        public const string DeleteProducts = "products.delete";
        public const string ManageStock = "products.stock.manage";
        // Supplier Management
        public const string ViewSuppliers = "suppliers.view";
        public const string CreateSuppliers = "suppliers.create";
        public const string UpdateSuppliers = "suppliers.update";
        public const string DeleteSuppliers = "suppliers.delete";
        // Report
        public const string ViewReports = "reports.view";
        public const string ExportReports = "reports.export";
    }
    public static class RolePermissions
    {
        public static IReadOnlyDictionary<string, IReadOnlyList<string>> DefaultRolePermissions { get; } = new Dictionary<string, IReadOnlyList<string>>
        {
            [Roles.Admin] = new List<string>
                {
                    Permissions.ViewUsers,
                    Permissions.CreateUsers,
                    Permissions.UpdateUsers,
                    Permissions.DeleteUsers,
                    Permissions.ViewAllOrders,
                    Permissions.CreateOrders,
                    Permissions.UpdateOrders,
                    Permissions.DeleteOrders,
                    Permissions.AssignOrders,
                    Permissions.ViewProducts,
                    Permissions.CreateProducts,
                    Permissions.UpdateProducts,
                    Permissions.DeleteProducts,
                    Permissions.ManageStock,
                    Permissions.ViewSuppliers,
                    Permissions.CreateSuppliers,
                    Permissions.UpdateSuppliers,
                    Permissions.DeleteSuppliers,
                    Permissions.ViewReports,
                    Permissions.ExportReports
                },
            [Roles.User] = new List<string>
                {
                    Permissions.ViewOwnOrders,
                    Permissions.CreateOrders,
                    Permissions.ViewProducts
                },
            [Roles.Staff] = new List<string>
                {
                    Permissions.ViewAssignedOrders,
                    Permissions.UpdateOrders,
                    Permissions.ViewProducts,
                    Permissions.ManageStock
                },
            [Roles.Supplier] = new List<string>
                {
                    Permissions.ViewProducts,
                    Permissions.CreateProducts,
                    Permissions.UpdateProducts,
                    Permissions.ManageStock,
                    Permissions.ViewReports
                }
        };
    }
}