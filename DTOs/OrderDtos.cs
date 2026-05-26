using System.ComponentModel.DataAnnotations;

namespace OrderSystem.DTOs;

public record CreateOrderItemRequest(
    [property: Range(1, int.MaxValue)] int ProductId,
    [property: Range(1, int.MaxValue)] int Quantity
);

public record CreateOrderRequest(
    [property: Required] IList<CreateOrderItemRequest> Items
);

public record OrderItemResponse(
    int Id,
    int ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal
);

public record OrderResponse(
    int Id,
    int UserId,
    string Status,
    decimal TotalPrice,
    DateTime CreatedAt,
    IEnumerable<OrderItemResponse> Items
);

public record UpdateOrderStatusRequest(
    [property: Required] string Status
);
