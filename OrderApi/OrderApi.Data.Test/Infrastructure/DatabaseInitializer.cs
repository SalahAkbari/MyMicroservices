using System;
using System.Linq;
using OrderApi.Data.Database;
using OrderApi.Domain;

namespace OrderApi.Data.Test.Infrastructure
{
    public class DatabaseInitializer
    {
        public static void Initialize(OrderContext context)
        {
            if (context.Order.Any())
            {
                return;
            }

            Seed(context);
        }

        private static void Seed(OrderContext context)
        {
            var orders = new[]
            {
                new Order
                {
                    Id = Guid.Parse("d3e3137e-ccc9-488c-9e89-50ba354738c2"),
                    OrderState = 2,
                    CustomerGuid = Guid.Parse("9f35b48d-cb87-4783-bfdb-21e36012930a"),
                    CustomerFullName = "John Smith"
                },
                new Order
                {
                    Id = Guid.Parse("bffcf83a-0224-4a7c-a278-5aae00a02c1e"),
                    OrderState = 2,
                    CustomerGuid = Guid.Parse("654b7573-9501-436a-ad36-94c5696ac28f"),
                    CustomerFullName = "Daniel Sandler"
                },
                new Order
                {
                    Id = Guid.Parse("58e5cd7d-856b-4224-bdff-bd8f85bf5a6d"),
                    OrderState = 2,
                    CustomerGuid = Guid.Parse("971316e1-4966-4426-b1ea-a36c9dde1066"),
                    CustomerFullName = "Michael West"
                }
            };

            context.Order.AddRange(orders);
            context.SaveChanges();
        }
    }
}