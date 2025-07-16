using Microsoft.EntityFrameworkCore;
using SteamApp.Models.DTOs;
using SteamApp.Models.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class GradeEndpoints
{
    public static WebApplication MapGradeEndpoints(this WebApplication app)
    {
        // Apply authorization to all Grade endpoints
        var group = app.MapGroup("api/grades")
                        .WithTags("Grades")
                       .RequireAuthorization();

        // GET: /grades
        group.MapGet("/", async (ApplicationDbContext db) =>
            await db.Grades.ToListAsync())
            .WithName("GetAllGrades")
            .Produces<List<Grade>>(StatusCodes.Status200OK);

        // GET: /grades/{id}
        group.MapGet("/{id}", async (long id, ApplicationDbContext db) =>
            await db.Grades.FindAsync(id)
                is Grade grade
                    ? Results.Ok(grade)
                    : Results.NotFound())
            .WithName("GetGradeById")
            .Produces<Grade>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST: /grades
        group.MapPost("/", async (Grade grade, ApplicationDbContext db) =>
        {
            db.Grades.Add(grade);
            await db.SaveChangesAsync();
            return Results.Created($"/grades/{grade.Id}", grade);
        })
        .WithName("CreateGrade")
        .Accepts<Grade>("application/json")
        .Produces<Grade>(StatusCodes.Status201Created);

        // PUT: /grades/{id}
        group.MapPut("/{id}", async (long id, BaseUpdateDto input, ApplicationDbContext db) =>
        {
            var grade = await db.Grades.FindAsync(id);
            if (grade is null) return Results.NotFound();

            grade.Name = input.Name;
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("UpdateGrade")
        .Accepts<Grade>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        // DELETE: /grades/{id}
        group.MapDelete("/{id}", async (long id, ApplicationDbContext db) =>
        {
            var grade = await db.Grades.FindAsync(id);
            if (grade is null) return Results.NotFound();

            db.Grades.Remove(grade);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteGrade")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
