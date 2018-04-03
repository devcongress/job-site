using Plutonium.Reactor.Attributes;

// Dashboard

[assembly: MenuItem("Dashboard", controller: "Dashboard", route: "dashboard", position: 10, icon: "dashboard")]

// Admin

[assembly: MenuGroup("Admin", icon: "person_outline", roles: new[] { "admin" })]
[assembly: MenuItem("Users", controller: "Users", route: "list_users", position: 500, icon: "person", group: "Admin", permissions: new[] { "user.list" })]
[assembly: MenuItem("Roles", controller: "Roles", route: "list_roles", position: 600, icon: "group", group: "Admin", permissions: new[] { "role.list" })]
