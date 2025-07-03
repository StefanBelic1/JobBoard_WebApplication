using Autofac;
using Autofac.Extensions.DependencyInjection;
using JobBoardAPI.Repository;
using JobBoardAPI.Repository.Common;
using JobBoardAPI.Service;
using JobBoardAPI.Service.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:3000") 
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    var configuration = builder.Configuration;
    var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    containerBuilder.RegisterInstance(connectionString).As<string>();

    containerBuilder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<JobService>().As<IJobService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<JobRepository>().As<IJobRepository>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<ApplicationService>().As<IApplicationService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<ApplicationRepository>().As<IApplicationRepository>().InstancePerLifetimeScope();
});

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend"); 

app.UseAuthorization();

app.MapControllers();

app.Run();