using CsvHelper;
using CsvHelper.Configuration;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using System.Globalization;

namespace FinanceManager.Infrastructure.Parsers.Base
{
    public abstract class CsvTransactionFileParser<T> : ITransactionFileParser
    {
        public abstract string GetBankName();
        public abstract string GetDateFormat();
        public abstract IEnumerable<string> GetFileExtensions();
        public abstract IEnumerable<ParsedTransaction> StandardiseRecords(IEnumerable<T> file);

        public async Task<IEnumerable<ParsedTransaction>> ParseTransactionsFileAsync(Stream fileStream)
        {
            ArgumentNullException.ThrowIfNull(fileStream);

            try
            {
                using var streamReader = new StreamReader(fileStream, leaveOpen: true);
                using var csvReader = new CsvReader(streamReader, GetCsvConfiguration());

                csvReader.Context.TypeConverterOptionsCache.GetOptions<DateTime>().Formats = [GetDateFormat()];

                var fileRecords = await csvReader.GetRecordsAsync<T>().ToListAsync();
                return StandardiseRecords(fileRecords);
            }
            catch (CsvHelperException ex)
            {
                throw new InvalidOperationException($"Failed to parse CSV file using {GetBankName()} parser: {ex.Message}", ex);
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
                TrimOptions = TrimOptions.Trim,
                PrepareHeaderForMatch = args => args.Header.ToLower().Replace(" ", string.Empty),
            };  
        }
    }
}