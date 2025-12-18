using CsvHelper;
using CsvHelper.Configuration;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using System.Globalization;

namespace FinanceManager.Infrastructure.Parsers.Base
{
    public abstract class CsvTransactionFileParser : ITransactionFileParser
    {
        public abstract string GetCompanyName();
        public abstract IEnumerable<string> GetFileExtensions();
        protected abstract ClassMap<BankRecord> GetClassMap();

        public async Task<IEnumerable<BankRecord>> ParseBankRecordsAsync(Stream fileStream)
        {
            ArgumentNullException.ThrowIfNull(fileStream);

            try
            {
                using var streamReader = new StreamReader(fileStream, leaveOpen: true);
                using var csvReader = new CsvReader(streamReader, GetCsvConfiguration());

                csvReader.Context.RegisterClassMap(GetClassMap());

                var records = new List<BankRecord>();
                return await csvReader.GetRecordsAsync<BankRecord>().ToListAsync();
            }
            catch (CsvHelperException ex)
            {
                throw new InvalidOperationException($"Failed to parse CSV file using {GetCompanyName()} parser: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while parsing the file: {ex.Message}", ex);
            }
        }

        protected virtual CsvConfiguration GetCsvConfiguration()
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim
            };
        }
    }
}