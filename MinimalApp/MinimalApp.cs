using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace MinimalApp
{
    public class MinimalApp
    {
        public static async Task Main(string[] args) => await CreateHostBuilder(args).Build().RunAsync();
        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => 
            {
                webBuilder.Configure(_ =>
                {
                    _.UseDeveloperExceptionPage();
                    _.UseRouting();
                    _.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/", async context =>
                        {
                            context.Response.ContentType = "text/html";
                            using var db = new MinimalAppDbContext();
                            var items = await db.Items.ToListAsync();
                            var responseString = $"Add Item to In Memory DB</br>" +
                            $"<form action=\"/additem\" method=\"post\"><label for=\"name\">Name: </label></br>" +
                            $"<input type=\"text\" id=\"name\"/ name=\"name\" placeholder=\"Enter name\" maxlength=\"15\">" +
                            $"<button type=\"submit\">Add</button></form></br><style>table, th, td{{border: 1px solid black}}</style>" +
                            $"<table><tr><th>Id</th><th>Name</th>";
                            foreach (var i in items) responseString += $"<tr><td>{i.Id}</td><td>{i.Name}</td></tr>";
                            responseString += "</table>";
                            await context.Response.WriteAsync(responseString);
                        });

                        endpoints.MapPost("/addItem", async context =>
                        {
                            var form = await context.Request.ReadFormAsync();
                            form.TryGetValue("name", out StringValues name);
                            using var db = new MinimalAppDbContext();
                            await db.Items.AddAsync(new Item
                            {
                                Name = name.ToString()
                            });
                            await db.SaveChangesAsync();
                            context.Response.StatusCode = 200;
                            context.Response.Redirect("/");
                        });
                    });
                });
            });
    }
}
public class MinimalAppDbContext : DbContext
{
    public DbSet<Item> Items { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseInMemoryDatabase("MinimalAppDb");
}

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; }
}
