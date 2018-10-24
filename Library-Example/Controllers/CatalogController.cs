using Microsoft.AspNetCore.Mvc;
using Library_Example.Models.Catalog;
using System.Collections.Generic;
using System.Linq;
using LibraryData.Models;
using LibraryData;
using Library_Example.Models.Checkout;

namespace Library_Example.Controllers
{
    public class CatalogController : Controller
    {
        private ILibraryAsset assets;
        private ICheckout checkout;

        public CatalogController(ILibraryAsset assets, ICheckout checkout)
        {
            this.assets = assets;
            this.checkout = checkout;
        }

        public IActionResult Index()
        {
            IEnumerable<LibraryAsset> assetModels = assets.GetAll();

            IEnumerable<AssetListingModel> ListingResult = assetModels
                .Select(result => new AssetListingModel
                {
                    Id = result.Id,
                    ImageUrl = result.ImageUrl,
                    AuthorOrDirector = assets.GetAuthorOrDirector(result.Id),
                    DeweyCallNumber = assets.GetDeweyIndex(result.Id),
                    Title = result.Title,
                    Type = assets.GetType(result.Id)

                });

            AssetIndexModel model = new AssetIndexModel
            {
                Assets = ListingResult
            };

            return View(model);
        }
        public IActionResult Detail(int id)
        {
            //if (ModelState.IsValid)
            //{
            //    LibraryAsset asset = assets.GetById(id);

            //    AssetDetailModel model = new AssetDetailModel()
            //    {
            //        AssetId = id,
            //        Title = asset.Title,
            //        AuthorOrDirector = assets.GetAuthorOrDirection(id),
            //        //Type = assets.GetType(id),
            //        Year = asset.Year,
            //        ISBN = assets.GetIsbn(id),
            //        DeweyCallNumber = assets.GetDeweyIndex(id),
            //        Status = asset.Status.Name,
            //        Cost = asset.Cost,
            //        CurrentLocation = assets.GetCurrentLocation(id).Name,
            //        ImageUrl = asset.ImageUrl
            //        //PatronName = assets.

            //    };

            if (ModelState.IsValid)
            {
                LibraryAsset asset = assets.GetById(id);
                IEnumerable<AssetHoldModel> currentHolds = checkout.GetCurrentHolds(id)
                    .Select(a => new AssetHoldModel
                    {
                        HoldIsPlaced = checkout.GetCurrentHoldPlaced(id).ToString("d"),
                        PatronName = checkout.GetCurrentPatronName(id)
                    });

                AssetDetailModel model = new AssetDetailModel()
                {
                    AssetId = id,
                    Title = asset.Title,
                    Type = assets.GetType(id),
                    Year = asset.Year,
                    Cost = asset.Cost,
                    Status = asset.Status.Name,
                    ImageUrl = asset.ImageUrl,
                    AuthorOrDirector = assets.GetAuthorOrDirector(id),
                    CurrentLocation = assets.GetCurrentLocation(id).Name,
                    DeweyCallNumber = assets.GetDeweyIndex(id),
                    CheckoutHistory = checkout.GetCheckoutHistory(id),
                    ISBN = assets.GetIsbn(id),
                    LastestCheckOut = checkout.GetLatestCheckout(id),
                    PatronName = checkout.GetCurrentPatronName(id),
                    AssetHolds = currentHolds

                };
                return View(model);
            }
            return View("Error");
            //  return View(model);
            //  }
            //   return View("Error");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(int id)
        {

            LibraryAsset asset = assets.GetById(id);
            CheckoutModel model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = string.Empty,
                IsCheckedOut = checkout.IsCheckedOut(id)
            };

            return View(model);
        }

        public IActionResult Hold(int id)
        {

            LibraryAsset asset = assets.GetById(id);
            CheckoutModel model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = string.Empty,
                IsCheckedOut = checkout.IsCheckedOut(id),
                HoldCount = checkout.GetCurrentHolds(id).Count()
            };

            return View(model);
        }

        public IActionResult MarkLost(int id)
        {
            checkout.MarkLost(id);

            return RedirectToAction("Detail", new { id });
        }

        public IActionResult MarkFound(int id)
        {
            checkout.MarkFound(id);

            return RedirectToAction("Detail", new { id });
        }

        [HttpPost]
        public IActionResult PlaceCheckout(int assetId, int libraryCardId)
        {
            if (ModelState.IsValid)
            {
              checkout.CheckInItem(assetId, libraryCardId);
                return RedirectToAction("Detail", new { id = assetId });
            }
            return RedirectToAction("Checkout", new { assetId });

        }

        [HttpPost]
        public IActionResult Placehold(int assetId, int libraryCardId)
        {
            checkout.PlaceHold(assetId, libraryCardId);
            return RedirectToAction("Detail", new { id = assetId });
        }

        private CheckoutModel GetCheckoutModel(int id)
        {
            LibraryAsset asset = assets.GetById(id);
            CheckoutModel model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = string.Empty,
                IsCheckedOut = checkout.IsCheckedOut(id),
                HoldCount = checkout.GetCurrentHolds(id).Count()
            };
            return model;
        }
    }
}