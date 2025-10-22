namespace RestaurantManagement.Api.Entities;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Preparing,
    Ready,
    Served,
    Completed,
    Cancelled
}