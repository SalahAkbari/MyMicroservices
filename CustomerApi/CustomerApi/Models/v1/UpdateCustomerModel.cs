using System;
using System.ComponentModel.DataAnnotations;

namespace CustomerApi.Models.v1
{
    public class UpdateCustomerModel : BaseCustomerModel
    {
        [Required]
        public Guid Id { get; set; }
    }
}
