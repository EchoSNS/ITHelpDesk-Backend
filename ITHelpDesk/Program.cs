using ITHelpDesk.Data;
using ITHelpDesk.Domain;
using ITHelpDesk.Repositories;
using ITHelpDesk.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

using ITHelpDesk.Domain.Department;
using AutoMapper;
using ITHelpDesk.Profiles;

var builder = WebApplication.CreateBuilder(args);

var hostSettings = builder.Configuration.GetSection("HostSettings");
var backendPort = hostSettings.GetValue<int>("BackendPort");
var frontendPort = hostSettings.GetValue<int>("FrontendPort");
var backendIp = hostSettings.GetValue<string>("BackendIp");

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(backendPort); // Allow access from any IP
});

// Add DbContext with SQL Server
builder.Services.AddDbContext<HelpDeskDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    //options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<HelpDeskDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 7;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // In Development, Allow All
            policy.SetIsOriginAllowed(origin => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // In Production, only allow specific origin
            policy.WithOrigins("http://192.168.10.84:4000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// Add controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    //options.JsonSerializerOptions.MaxDepth = 64; // Optional: Increase max depth if needed
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IT Help Desk API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthorization();

builder.Services.AddAuthentication(options =>
{
options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = true;
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, // Remove default 5 minute clock skew
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

    o.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.StatusCode = 401; // Unauthorized
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

// Register UserRoleService
builder.Services.AddScoped<IUserRoleService, UserRoleService>();

// Register EmailService
builder.Services.AddScoped<EmailService>();

// Register Token Service
builder.Services.AddScoped<ITokenService, TokenService>();

// Register Ticket Repository
builder.Services.AddScoped<ITicketRepository, TicketRepository>();

builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<IPositionService, PositionService>();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(MappingProfile));


builder.Services.AddLogging();


builder.Services.AddScoped<IDepartmentService, DepartmentService>();

builder.Services.AddScoped<ISubDepartmentService, SubDepartmentService>();
builder.Services.AddScoped<ISubDepartmentRepository, SubDepartmentRepository>();

// Register SMS Service
//builder.Services.AddHttpClient<ISmsService, SmsService>();
//builder.Services.AddScoped<ISmsService, SmsService>();

builder.WebHost.UseUrls($"http://{backendIp}:{backendPort}");

var app = builder.Build();

// Enable CORS
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowFrontend");
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesDepartmentsAndAdmin(services);
}

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage(); // Add this in development mode

}

app.UseExceptionHandler(appBuilder => {
    appBuilder.Run(async context => {
        context.Response.ContentType = "application/json";
        var errorResponse = new { message = "An error occurred. Please try again later." };
        await context.Response.WriteAsJsonAsync(errorResponse);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

//// Use HTTPS redirection
//app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

// Run the application
app.Run();

async Task SeedRolesDepartmentsAndAdmin(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var context = serviceProvider.GetRequiredService<HelpDeskDbContext>();

    // Seed Roles
    string[] roles = { "Admin", "IT", "Staff" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Seed Departments
    if (!context.Departments.Any())
    {
        var department = new Department { DepartmentName = "Operations" };
        context.Departments.Add(department);
        await context.SaveChangesAsync();

        var subDepartment = new SubDepartment { SubDepartmentName = "IT Department", DepartmentId = department.DepartmentId };
        context.SubDepartments.Add(subDepartment);
        await context.SaveChangesAsync();

        var position = new Position { PositionName = "Software Engineer", SubDepartmentId = subDepartment.SubDepartmentId };
        context.Positions.Add(position);
        await context.SaveChangesAsync();
    }

    // Fetch the first available position
    var assignedPosition = context.Positions.FirstOrDefault();

    // Seed Admin User
    var adminEmail = "add_email";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var newAdmin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "Test",
            MiddleName = "Optional",
            EmailConfirmed = true,
            PhoneNumber = "0123456789",
            IsStaff = true,
            PositionId = assignedPosition?.PositionId // Ensure position exists before assigning
        };

        var result = await userManager.CreateAsync(newAdmin, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
}

