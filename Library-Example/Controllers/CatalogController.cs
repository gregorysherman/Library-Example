using Microsoft.AspNetCore.Mvc;
using Library_Example.Models.Catalog;
using System.Collections.Generic;
using System.Linq;
using LibraryData.Models;
using LibraryData;

namespace Library_Example.Controllers
{
    public class CatalogController : Controller
    {
        private ILibraryAsset assets;

        public CatalogController(ILibraryAsset assets)
        {
            this.assets = assets;
        }

        public IActionResult index()
        {
            IEnumerable<LibraryAsset> assetModels = assets.GetAll();

            IEnumerable<AssetListingModel> ListingResult = assetModels
                .Select(result => new AssetListingModel
                {
                    Id = result.Id,
                    ImageUrl = result.ImageUrl,
                    AuthorOrDirector = assets.GetAuthorOrDirection(result.Id),
                    DeweyCallNumber = assets.GetDeweyIndex(result.Id),
                    Title = result.Title,
                    Type = assets.GetType(result.Id)

                });

            AssetIndexModel model = new AssetIndexModel {
                Assets = ListingResult
            };

            return View(model);
        }
    }
}