using DbRepos;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

#region Dependency Inject FriendsService
//Services are typically added as Scoped as one scope is a Web client request
//- Transient objects are always different in the IndexModel and in the middleware.
//- Scoped objects are the same for a given request but differ across each new request.
//- Singleton objects are the same for every request.

//DI injects the DbRepos into csFriendService
//Services are typically added as Scoped as one scope is a Web client request
builder.Services.AddScoped<csFriendsDbRepos>();

//WebController have a matching constructor, so service must be created
//Services are typically added as Scoped as one scope is a Web client request
//builder.Services.AddSingleton<IFriendsService, csFriendsServiceModel>();
builder.Services.AddScoped<IFriendsService, csFriendsServiceDb>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
