using System;
using AutoMapper;
using FluentAssertions;
using OrderApi.Domain;
using OrderApi.Infrastructure.Automapper;
using OrderApi.Models.v1;
using Xunit;

namespace OrderApi.Test.Infrastrucutre.Automapper
{
   public class MappingProfileTests
    {
        private readonly IMapper _mapper;
        private readonly OrderModel _orderModel;

        public MappingProfileTests()
        {
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            _mapper = mockMapper.CreateMapper();

            _orderModel = new OrderModel
            {
                CustomerGuid = Guid.Parse("9f35b48d-cb87-4783-bfdb-21e36012930a"),
                CustomerFullName = "John Smith"
            };
        }

        [Fact]
        public void Map_Order_OrderModel_ShouldHaveValidConfig()
        {
            var configuration = new MapperConfiguration(cfg =>
                cfg.CreateMap<Order, OrderModel>());

            configuration.AssertConfigurationIsValid();
        }

        [Fact]
        public void Map_OrderModel_Order()
        {
            var order = _mapper.Map<Order>(_orderModel);

            order.CustomerGuid.Should().Be(_orderModel.CustomerGuid);
            order.CustomerFullName.Should().Be(_orderModel.CustomerFullName);
            order.OrderState.Should().Be(1);
        }
    }
}
