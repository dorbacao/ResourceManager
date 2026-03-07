using Microsoft.AspNetCore.Routing.Constraints;
using System.Collections.Generic;
using System.Text.Json.Serialization;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        // Registrar constraints que o Slim não inclui por padrão
        builder.Services.Configure<RouteOptions>(options =>
        {
            options.SetParameterPolicy<RegexInlineRouteConstraint>("regex");
        });
        // -------------------------------
        // SWAGGER
        // -------------------------------
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // -------------------------------
        // JSON
        // -------------------------------
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

        var app = builder.Build();

        // -------------------------------
        // SWAGGER UI
        // -------------------------------
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }



        var pendingEmails = new List<PendingEmail>();

        // -------------------------------
        // ENDPOINTS
        // -------------------------------
        app.MapPost("/email/pending", (CreateEmailRequest req) =>
        {
            var email = new PendingEmail(
                Id: Guid.NewGuid(),
                To: req.To,
                Subject: req.Subject,
                Body: req.Body,
                Status: "Pending");

            pendingEmails.Add(email);

            return Results.Ok(new StatusEmailRequest(email.Id, email.Status));
        })
        .WithName("CreatePendingEmail")
        ;

        app.MapPost("/email/confirm/{id:guid}", (Guid id) =>
        {
            var email = pendingEmails.FirstOrDefault(e => e.Id == id);

            if (email is null)
                return Results.NotFound();

            Console.WriteLine($"[EMAIL ENVIADO] Para: {email.To} | Assunto: {email.Subject}");

            var updated = email with { Status = "Sent" };
            pendingEmails.Remove(email);
            pendingEmails.Add(updated);

            return Results.Ok(new StatusEmailRequest(updated.Id, updated.Status));
        })
        .WithName("ConfirmEmail")
        ;

        app.MapPost("/email/cancel/{id:guid}", (Guid id) =>
        {
            var email = pendingEmails.FirstOrDefault(e => e.Id == id);

            if (email is null)
                return Results.NotFound();

            var updated = email with { Status = "Cancelled" };
            pendingEmails.Remove(email);
            pendingEmails.Add(updated);

            return Results.Ok(new StatusEmailRequest(updated.Id, updated.Status));
        })
        .WithName("CancelEmail")
        ;

        app.MapGet("/email/list/", () =>
        {
            return Results.Ok(pendingEmails);
        })
       .WithName("ListEmail")
       ;

        app.UsePathBase("/swagger/index.html");

        app.Run();
    }
}

// -------------------------------
// MODELOS E STORAGE
// -------------------------------

public record StatusEmailRequest(Guid Id,string Status);
public record PendingEmail(
    Guid Id,
    string To,
    string Subject,
    string Body,
    string Status);

public record CreateEmailRequest(
    string To,
    string Subject,
    string Body);


// -------------------------------
// JSON SOURCE GENERATOR
// -------------------------------
[JsonSerializable(typeof(PendingEmail[]))]
[JsonSerializable(typeof(CreateEmailRequest))]
[JsonSerializable(typeof(List<PendingEmail>))]
[JsonSerializable(typeof(StatusEmailRequest))]

internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
