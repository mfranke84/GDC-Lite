using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private static readonly List<DataCollectionOrder> Orders = new();

    [HttpPost]
    public IActionResult Create([FromBody] DataCollectionOrder order)
    {
        var newOrder = order with
        {
            OrderId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = "Draft"
        };

        Orders.Add(newOrder);
        return Ok(newOrder);
    }

    [HttpGet("{vin}")]
    public IActionResult GetByVin(string vin)
        => Ok(Orders.Where(o => o.Vin == vin));

    [HttpPost("{id}/validate")]
    public IActionResult Validate(Guid id)
    {
        var order = Orders.FirstOrDefault(o => o.OrderId == id);
        return Ok(order with { Status = "Validated" });
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok("GDC-Lite Order Service is alive");

}
