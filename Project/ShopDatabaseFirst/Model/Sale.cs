using System;
using System.Collections.Generic;

namespace ShopDatabaseFirst.Model;

public partial class Sale
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public string Client { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public DateTime Ordered { get; set; }

    public virtual Product Product { get; set; } = null!;
}
