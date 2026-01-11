using FluentValidation.AspNetCore;
using Medication_Reminder_API.Application.Interfaces;
using Medication_Reminder_API.Application.Services;
using Medication_Reminder_API.Application.Validators;
using Medication_Reminder_API.Infrastructure;
using Medication_Reminder_API.Infrastructure.Reposatories;
using Medication_Reminder_API.Infrastructure.Repositories;
using Medication_Reminder_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using static Medication_Reminder_API.Services.TestDoseGenerationService;

var builder = WebApplication.CreateBuilder(args);

// ===== 1) DbContext =====
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== 2) Identity =====
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

//TestDoseGenerationService

builder.Services.AddScoped<TestDoseGenerationService>();
builder.Services.AddScoped<AuthenticationService>();


// ===== 3) JWT Authentication =====
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5),
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// ===== 4) AutoMapper =====
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ===== 5) Fluent Validation =====
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<MedicationValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DoseLogDTOValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PatientValidator>();
//DoseGenerationBackgroundService reg

builder.Services.AddHostedService<DoseGenerationBackgroundService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICaregiverRepository, CaregiverRepository>();
builder.Services.AddScoped<IMedicationRepository, MedicationRepository>();
builder.Services.AddScoped<IDoseLogRepository, DoseLogRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();


// ---- Services ----
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<ICaregiverService, CaregiverService>();
builder.Services.AddScoped<IMedicationService, MedicationService>();
builder.Services.AddScoped<IDoseLogService, DoseLogService>();
builder.Services.AddScoped<IEmailService, EmailService>();



// ===== 6) DI for Services =====
builder.Services.AddScoped<IMedicationService, MedicationService>();
builder.Services.AddScoped<ICaregiverService, CaregiverService>();
builder.Services.AddScoped<IDoseLogService, DoseLogService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// MEMO Cahe
builder.Services.AddMemoryCache();

// ===== 7) CORS =====
//var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy(name: MyAllowSpecificOrigins,
//        policy =>
//        {
//            policy.WithOrigins("")  ãÍÇæáÉ ÝÑæäÊ ÈÇÁÊ ÈÇáÝÔá
//                  .AllowAnyHeader()
//                  .AllowAnyMethod();
//        });
//});

// ===== 8) Controllers + Swagger =====

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});


var app = builder.Build();

// ===== Middleware =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


await CreateRolesAsync(app);
app.Run();


static async Task CreateRolesAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "Doctor", "Caregiver", "Patient" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    string adminEmail = "admin@gmail.com";
    string adminPassword = "Admin123@#";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            IsActive = true,
            IsVisible = true,
            EmailConfirmed = true,
            TokenVersion = 0,
            FullName = "System Administrator",
        };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}
