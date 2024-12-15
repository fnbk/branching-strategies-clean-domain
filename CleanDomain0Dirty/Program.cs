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

        var app = builder.Build();

        //
        // Configure
        //

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
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

    [HttpPost]
    public IActionResult ProcessOrder(Order order)
    {
        var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        // 1. authenticate: validate token
        var principal = ValidateToken(token);
        var identity = principal?.Identity as ClaimsIdentity;
        var isAuthenticated = identity != null ? identity.IsAuthenticated : false;

        if (isAuthenticated)
        {
            // 2. authorize: check if user has 'Customer' role
            var hasCustomerRole = identity.HasClaim(ClaimTypes.Role, "Customer");
            if (hasCustomerRole)
            {

                // 3. validation: check Order data
                var isValidOrder = int.Parse(order.OrderId) > 0 && order.Amount > 0 ? true : false;
                if (isValidOrder)
                {
                    try
                    {
                        // 4. business logic
                        CompleteOrder(order);
                        return Ok("Order processed successfully.");
                    }
                    catch (OrderException ex)
                    {
                        Console.WriteLine($"Error completing order: {ex.Message}");
                        return BadRequest("Order failed due to a processing error.");
                    }
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


    public ClaimsPrincipal ValidateToken(string token)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Globals.TokenSecret));

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = Globals.TokenIssuer,
                ValidAudience = Globals.TokenAudience,
                IssuerSigningKey = securityKey
            };

            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);
            return principal;
        }
        catch (Exception)
        {
            // Token validation failed
            return null;
        }
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



