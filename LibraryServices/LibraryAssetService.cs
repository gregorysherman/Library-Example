using System;
using LibraryData;
using LibraryData.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LibraryServices
{
    public class LibraryAssetService : ILibraryAsset
    {

        private LibraryContext context;

        public LibraryAssetService(LibraryContext context)
        {
            this.context = context;
        }

        public void Add(LibraryAsset asset)
        {
            if (asset != null)
            {
                context.Add(asset);
                context.SaveChanges();
            }
        }

        public IEnumerable<LibraryAsset> GetAll()
        {
            return context.LibraryAssets
                .Include(asset => asset.Status)
                .Include(asset => asset.Location);

        }


        public LibraryAsset GetById(int id)
        {
            return GetAll()
                .FirstOrDefault(asset => asset.Id == id);

        }

        public LibraryBranch GetCurrentLocation(int id)
        {
            return GetById(id).Location;
        }

        public string GetDeweyIndex(int id)
        {
            if (context.Books.Any(asset => asset.Id == id))
            {
                return context.Books.FirstOrDefault(book => book.Id == id).DeweyIndex;
            }
            return string.Empty;

        }

        public string GetIsbn(int id)
        {
            if (context.Books.Any(asset => asset.Id == id))
            {
                return context.Books.FirstOrDefault(book => book.Id == id).ISBN;
            }
            return string.Empty;
        }

        public string GetTitle(int id)
        {
            return GetById(id).Title;

        }

        public string GetType(int id)
        {
            IQueryable<Book> book = context.LibraryAssets.OfType<Book>()
                .Where(asset => asset.Id == id);

            return book.Any() ? "Book" : "Video";
        }

        public string GetAuthorOrDirection(int id)
        {
            bool isBook = context.LibraryAssets.OfType<Book>()
                 .Where(asset => asset.Id == id).Any();
            bool isVideo = context.LibraryAssets.OfType<Video>()
                .Where(asset => asset.Id == id).Any();

            return isBook ? context.Books.FirstOrDefault(asset => asset.Id == id).Author : context.Videos.FirstOrDefault(asset => asset.Id == id).Director
                ?? "Unknown";
        }
    }
}
