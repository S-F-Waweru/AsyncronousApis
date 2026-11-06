using AsyncProductApi.Data;
using AsyncProductApi.Dtos;
using AsyncProductApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>options.UseSqlite("Data Source=requestDBnn.db"));

var app = builder.Build();

app.UseHttpsRedirection();

//StartEndpoint
app.MapPost("api/v1/products", async (AppDbContext context, ListingRequest listingRequest) =>
{
    if (listingRequest == null) 
        return Results.BadRequest();

    listingRequest.RequestStatus = "ACCEPT";
    listingRequest.EstimatedCompletionTime = "2026-10-31:21:00:00)";
    
    await context.AddAsync(listingRequest);
    await context.SaveChangesAsync(); 
    
     return Results.Accepted($"api/v1/productstatus/{listingRequest.Id}", listingRequest);
    
});

// Status endpoint

app.MapGet("api/v1/productstatus/{requestId}", (AppDbContext context, string requestId) =>
    {
        var listingRequest = context.ListingRequests.FirstOrDefault(lr => lr.RequestId == requestId);
        
        if(listingRequest == null)
            return Results.NotFound();
        
        ListingStatus listingStatus = new ListingStatus
        {
            RequestStatus = listingRequest.RequestStatus,
            ResourceUrl = String.Empty
        };

        if (listingStatus.RequestStatus!.ToUpper() == "COMPLETE")
        {
            listingStatus.ResourceUrl = $"api/v1/product/{Guid.NewGuid().ToString()}";
            // return Results.Ok(listingStatus);
            return Results.Redirect("https://localhost:5001/" + listingStatus.ResourceUrl);
        }
        
        listingStatus.EstimatedCompletionTime = "2026-10-31:21:00:00";
        
        return Results.Ok(listingStatus);
    });

//Final endpoint

app.MapGet("api/v1/products/{requestId}", (string requestId) =>
{
    return Results.Ok("This is where you would pass back the final result");
});

app.Run();
