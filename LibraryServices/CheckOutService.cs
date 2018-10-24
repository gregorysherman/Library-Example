using System;
using System.Collections.Generic;
using System.Linq;
using LibraryData;
using LibraryData.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryServices
{
    public class CheckOutService : ICheckout
    {
        private LibraryContext context;
        public CheckOutService(LibraryContext context)
        {
            this.context = context;
        }

        public void Add(CheckOut newCheckout)
        {
            if (newCheckout != null)
            {

                context.Add(newCheckout);
                context.SaveChanges();

            }
        }

        public IEnumerable<CheckOut> GetAll()
        {
            return context.CheckOuts;
        }

        public CheckOut GetById(int id)
        {
            return GetAll().FirstOrDefault(e => e.Id == id);

        }

        public IEnumerable<CheckOutHistory> GetCheckoutHistory(int id)
        {
            return context.CheckOutHistories
                .Include(h => h.LibraryAsset)
                .Include(e => e.LibraryCard)
                .Where(e => e.LibraryAsset.Id == id);

        }
        public IEnumerable<Hold> GetCurrentHolds(int id)
        {
            return context.Holds
                .Include(e => e.LibraryAsset)
                .Where(e => e.LibraryAsset.Id == id);

        }
        public CheckOut GetLatestCheckout(int id)
        {
            return context.CheckOuts
                .Where(e => e.LibraryAsset.Id == id)
                .OrderByDescending(e => e.Since)
                .FirstOrDefault();
        }

        public DateTime GetCurrentHoldPlaced(int id)
        {
            return context.Holds
               .Include(e => e.LibraryAsset)
               .Include(e => e.LibraryCard)
               .FirstOrDefault(e => e.Id == id)
               .HoldPlaced;

  
        }

        public void MarkFound(int assetId)
        {

            UpdateStatus(assetId, "Available");
            RemoveExistingCheckouts(assetId);
            CloseExistingCheckoutHistory(assetId);

            context.SaveChanges();
        }

        private void UpdateStatus(int assetId, string status)
        {
            LibraryAsset item = context.LibraryAssets
           .FirstOrDefault(e => e.Id == assetId);

            context.Update(item);

            item.Status = context.Statuses
               .FirstOrDefault(s => s.Name == status);
        }

        private void CloseExistingCheckoutHistory(int assetId)
        {
            CheckOutHistory history = context.CheckOutHistories
                .FirstOrDefault(e => e.LibraryAsset.Id == assetId && e.CheckedIn == null);
            if (history != null)
            {
                context.Update(history);
                history.CheckedIn = DateTime.Now;
            }
        }

        private void RemoveExistingCheckouts(int assetId)
        {
            CheckOut checkout = context.CheckOuts
         .FirstOrDefault(e => e.LibraryAsset.Id == assetId);
            if (checkout != null)
            {
                context.Remove(checkout);
            }
        }

        public void MarkLost(int assetId)
        {
            UpdateStatus(assetId, "Lost");
            context.SaveChanges();
        }

        public void CheckOutItem(int assetId, int libraryCardId)
        {
            if (IsCheckedOut(assetId))
            {
                return;
            }

            LibraryAsset libraryAsset = context.LibraryAssets.FirstOrDefault(e => e.Id == assetId);

            UpdateStatus(assetId, "Checked Out");

            LibraryCard libraryCard = context.LibraryCards
                .Include(card => card.Id == libraryCardId)
                .FirstOrDefault(card => card.Id == libraryCardId);

            DateTime now = DateTime.Now;

            CheckOut checkout = new CheckOut
            {
                LibraryAsset = libraryAsset,
                LibraryCard = libraryCard,
                Since = now,
                Until = GetDefaultCheckoutTime(now)
            };
            context.Add(checkout);
            CheckOutHistory checkOutHistory = new CheckOutHistory
            {
                LibraryAsset = libraryAsset,
                LibraryCard = libraryCard,
                CheckedOut = now
            };
            context.Add(checkOutHistory);

            context.SaveChanges();

        }

        private DateTime GetDefaultCheckoutTime(DateTime now)
        {
            return now.AddDays(14);
        }

        public bool IsCheckedOut(int assetId)
        {
            return context.CheckOuts
                .Where(e => e.LibraryAsset.Id == assetId).Any();
        }

        public void PlaceHold(int assetId, int libraryCardId)
        {
            LibraryAsset libraryAsset = context.LibraryAssets.FirstOrDefault(e => e.Id == assetId);
            LibraryCard libraryCard = context.LibraryCards.FirstOrDefault(e => e.Id == libraryCardId);
            if (libraryAsset.Status.Name == "Available")
            {
                UpdateStatus(assetId, "On Hold");
            }

            Hold hold = new Hold
            {
                HoldPlaced = DateTime.Now,
                LibraryAsset = libraryAsset,
                LibraryCard = libraryCard
            };
            context.Add(hold);
            context.SaveChanges();
        }

        public void CheckInItem(int assetId, int libraryCardId)
        {
            RemoveExistingCheckouts(assetId);
            CloseExistingCheckoutHistory(assetId);

            IEnumerable<Hold> currentHolds = context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .Where(e => e.Id == assetId);

            if (currentHolds.Any())
            {

                CheckoutToEarliestHold(assetId, currentHolds);
            }

            UpdateStatus(assetId, "Available");
            context.SaveChanges();
        }

        private void CheckoutToEarliestHold(int assetId, IEnumerable<Hold> currentHolds)
        {
            Hold earliestHold = currentHolds
                 .OrderBy(h => h.HoldPlaced)
                 .FirstOrDefault();

            LibraryCard libraryCard = earliestHold.LibraryCard;
            context.Remove(earliestHold);
            context.SaveChanges();
            CheckOutItem(assetId, libraryCard.Id);



        }
        public string GetCurrentPatronName(int id)
        {
            Hold hold = context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryAsset)
                .FirstOrDefault(h => h.Id == id);

            int? cardId = hold?.LibraryCard.Id;

            return GetPatronName(cardId);
        }

        public string GetCurrentCheckedOutPatron(int id)
        {
            CheckOut checkedout = GetCheckOutByAssetId(id);
            if (checkedout == null)
            {
                return string.Empty;
            }

            int cardId = checkedout.LibraryCard.Id;
            return GetPatronName(cardId);
            
        }

        private CheckOut GetCheckOutByAssetId(int id)
        {
            return context.CheckOuts
                .Include(e => e.LibraryAsset)
                .Include(e => e.LibraryCard)
                .FirstOrDefault(e => e.LibraryAsset.Id == id);

        }

        private string GetPatronName(int? cardId)
        {
            Patron patron = context.Patrons
               .Include(e => e.LibraryCard)
               .FirstOrDefault(e => e.Id == cardId);

            return patron?.FirstName + " " + patron?.LastName;
        }
    }
}
