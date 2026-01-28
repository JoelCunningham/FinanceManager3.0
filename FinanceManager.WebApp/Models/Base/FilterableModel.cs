namespace FinanceManager.WebApp.Models.Base
{
    public class FilterableModel 
    {
        public TransactionFilters Filters { get; set; } = new TransactionFilters();
    }

    public class TransactionFilters()
    {   
        public string SearchTerm { get; set; } = string.Empty;
        public TransactionSort SortBy { get; set; } = TransactionSort.Date;
        public bool SortDescending { get; set; } = true;
        public TransferSource FilterBySource { get; set; } = TransferSource.All;
        public string? FilterByAccount { get; set; }
        public DateTime? FilterDateFrom { get; set; } = DateTime.Today.AddYears(-1);
        public DateTime? FilterDateTo { get; set; } = DateTime.Today;
        public decimal? FilterAmountMin { get; set; }
        public decimal? FilterAmountMax { get; set; }

        public void ClearFilters()
        {
            SortDescending = true;
            FilterBySource = TransferSource.All;
            FilterByAccount = null;
            FilterDateFrom = DateTime.Today.AddYears(-1);
            FilterDateTo = DateTime.Today;
            FilterAmountMin = null;
            FilterAmountMax = null;
        }
    }


    public enum TransactionSort
    {
        Date,
        Amount,
        FromAccount,
        ToAccount,
    }

    public enum TransferSource
    {
        All,
        User,
        Auto,
    }
}