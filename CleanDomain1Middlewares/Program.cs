using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

class Globals
{
    public const string TokenIssuer = "xxxxxxxxxx_issuer";
    public const string TokenAudience = "xxxxxxxxxx_audience";
    public const string TokenSecret = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
}

public class Order
{
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
}

public class OrderException : Exception
{
    public OrderException(string message) : base(message)
    {
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //
        // Configure Services
        //

        builder.Services.AddControllers();

        // JWT Authentication Configuration  
        var key = Encoding.ASCII.GetBytes(Globals.TokenSecret);
        builder.Services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                RoleClaimType = ClaimTypes.Role
            };
        });

        // Add Authorization  
        builder.Services.AddAuthorization();

        var app = builder.Build();


        //
        // Configure
        //

        // Error Middleware
        app.UseExceptionHandler("/error");

        // Logging Middleware  
        app.Use(async (context, next) =>
        {
            Console.WriteLine($"request method:{context.Request.Method}, path:{context.Request.Path}");

            await next.Invoke();

            Console.WriteLine($"response status code:{context.Response.StatusCode}");
        });

        app.UseRouting();

        // Authentication and Authorization middleware
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        // Error Handling Endpoint  
        app.Map("/error", (HttpContext http) =>
        {
            var feature = http.Features.Get<IExceptionHandlerFeature>();
            var exception = feature?.Error;

            var problemDetails = new ProblemDetails
            {
                Status = 500,
                Title = "An error occurred while processing your request.",
                Detail = exception?.Message
            };

            return Results.Problem(problemDetails);
        });


        //
        // Run
        //

        app.Run();
    }
}


[ApiController]
[Route("api/[controller]")]
public class OnlineStoreController : ControllerBase
{

    [Authorize]
    [HttpPost]
    public IActionResult ProcessOrder(Order order)
    {
        // 1. authenticate: validate token
        if (User.Identity.IsAuthenticated)
        {
            // 2. authorize: check if user has 'Customer' role
            if (User.IsInRole("Customer"))
            {
                // 3. validation: check Order data
                var isValidOrder = int.Parse(order.OrderId) > 0 && order.Amount > 0 ? true : false;
                if (isValidOrder)
                {
                    // 4. business logic
                    CompleteOrder(order);
                    return Ok("Order processed successfully.");
                }
                else
                {
                    return BadRequest("Invalid order data. Please review and correct any errors.");
                }
            }
            else
            {
                return Unauthorized("You do not have permission to process orders.");
            }
        }
        else
        {
            return Unauthorized("You must be logged in to place orders.");
        }
    }

    [HttpGet("token")]
    public IActionResult GenerateToken()
    {
        var token = GenerateJwtToken("John", "Customer");
        return Ok(token);
    }

    [HttpGet("non-user-token")]
    public IActionResult GenerateTokenNonUser()
    {
        var token = GenerateJwtToken(null, "Customer");
        return Ok(token);
    }

    [HttpGet("non-customer-token")]
    public IActionResult GenerateTokenNonCustomer()
    {
        var token = GenerateJwtToken("John", "");
        return Ok(token);
    }

    public string GenerateJwtToken(string username, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(Globals.TokenSecret);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, role),
        };
        if (username != null) claims.Add(new Claim(ClaimTypes.NameIdentifier, username));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Audience = Globals.TokenAudience,
            Issuer = Globals.TokenIssuer,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public void CompleteOrder(Order order)
    {
        if (order.Amount < 100)
        {
            throw new OrderException($"invalid OrderId:{order.OrderId}, insufficient amount:${order.Amount}");
        }

        Console.WriteLine($"Processing OrderId:{order.OrderId}, Amount:{order.Amount}");
    }
}



