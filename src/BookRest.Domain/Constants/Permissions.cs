// Learn more at: https://codewithmukesh.com/blog/permission-based-authorization-in-aspnet-core/

namespace BookRest.Domain.Constants;

/// <summary>
/// Represents permissions for domain <see cref="Roles"/>
/// </summary>
public static class Permissions
{
    // Every permission claim uses this type.
    public const string ClaimType = "permission";

    // Policy names starting with this prefix get a policy generated on demand
    public const string Prefix = "Permissions.";

    public static readonly IReadOnlyList<string> All =
    [
        ..Restaurants.All,
        ..Orders.All,
        ..Roles.All
    ];

    /// <summary>
    /// Represents permissions for restaurants.
    /// </summary>
    public static class Restaurants
    {
        public const string View = "Permissions.Restaurants.View";
        public const string Create = "Permissions.Restaurants.Create";
        public const string Edit = "Permissions.Restaurants.Edit";
        public const string Delete = "Permissions.Restaurants.Delete";

        public static IReadOnlyList<string> All =>
        [
            View,
            Create,
            Edit,
            Delete
        ];
    }

    /// <summary>
    /// Represents permissions for orders.
    /// </summary>
    public static class Orders
    {
        public const string View = "Permissions.Orders.View";
        public const string Create = "Permissions.Orders.Create";
        public const string Edit = "Permissions.Orders.Edit";
        public const string Delete = "Permissions.Orders.Delete";

        public static IReadOnlyList<string> All =>
        [
            View,
            Create,
            Edit,
            Delete
        ];
    }

    /// <summary>
    /// Represents permissions for roles.
    /// </summary>
    public static class Roles
    {
        public const string Manage = "Permissions.Roles.Manage";

        public static IReadOnlyList<string> All =>
        [
           Manage,
        ];
    }
}
