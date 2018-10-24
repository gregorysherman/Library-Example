using System;
using System.Collections.Generic;
using LibraryData.Models;

namespace LibraryData
{
    public interface ICheckout
    {
        IEnumerable<CheckOut> GetAll();
        IEnumerable<CheckOutHistory> GetCheckoutHistory(int id);
        IEnumerable<Hold> GetCurrentHolds(int id);

        DateTime GetCurrentHoldPlaced(int id);
        CheckOut GetById(int id);
        CheckOut GetLatestCheckout(int id);
        void Add(CheckOut newCheckout);
        void CheckOutItem(int assetId, int libraryCardId);
        void CheckInItem(int assetId, int libraryCardId);
        bool IsCheckedOut(int assetId);

        void PlaceHold(int assetId, int libraryCardId);
        string GetCurrentPatronName(int id);
        string GetCurrentCheckedOutPatron(int id);

        void MarkLost(int assetId);
        void MarkFound(int assetId);

    }
}
