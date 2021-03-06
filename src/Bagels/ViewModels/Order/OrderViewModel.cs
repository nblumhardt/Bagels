﻿using System.ComponentModel.DataAnnotations;

namespace Bagels.ViewModels.Order
{
    public class OrderViewModel
    {
        [Display(Name = "Name")]
        public string CustomerName { get; set; }

        [Display(Name = "Choice 1")]
        public string ItemId1 { get; set; }

        [Display(Name = "Choice 2")]
        public string ItemId2 { get; set; }
    }
}
