﻿using System.ComponentModel.DataAnnotations;

namespace Library_Example.Models.Checkout
{
    public class CheckoutModel
    {
        [Key]
        [Required(ErrorMessage = "Library Card Id is required.")]
        public string LibraryCardId { get; set; }
        public string Title { get; set; }
        public int AssetId { get; set; }
        public string ImageUrl { get; set; }
        public int HoldCount { get; set; }
        public bool IsCheckedOut { get; set; }
    }
}
