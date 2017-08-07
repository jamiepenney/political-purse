using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using OfficeOpenXml;
using PoliticalPurse.Web.Models;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using PoliticalPurse.Web.Util;

namespace PoliticalPurse.Web.Services
{
    public class DonationUpdateService : DatabaseService
    {
        private const string PartyColumn = "Party";
        private const string AmountColumn = "Amount";
        private const string DoneeColumn = "Donee";
        private const string DoneeAddressColumn = "Donee address";
        private const string NumberOfDonationsColumn = "Number of donations";
        private const string NumberOfDoneesColumn = "Number of donees";
        private const string PostcodeColumn = "Postcode";
        private const string YearColumn = "Year";
        private const string DonationWorksheetName = "General";

        public DonationUpdateService(IOptions<DatabaseOptions> options) : base(options) {}

        public async Task<bool> UpdateDonations(Stream excelFile)
        {
            try
            {
                var donations = GetDonationsFromExcelFile(excelFile);

                using (var connection = GetConnection())
                using (var transaction = connection.BeginTransaction())
                {
                    await ClearExistingDonations(connection);
                    await SaveDonations(connection, donations);
                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        private async Task SaveDonations(IDbConnection connection, List<Donation> donations)
        {
            foreach (var batch in donations.Batch(100))
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO ""donation"" (""party"", ""amount"", ""donee"", ""donee_address"", ""number_of_donations"", ""number_of_donees"", ""postcode"", ""year"")
                    VALUES(@Party, @Amount, @Donee, @DoneeAddress, @NumberOfDonations, @NumberOfDonees, @Postcode, @Year)", batch);
            }
        }

        private async Task ClearExistingDonations(IDbConnection connection)
        {
            await connection.ExecuteAsync("TRUNCATE TABLE \"donation\" RESTART IDENTITY");
        }

        private List<Donation> GetDonationsFromExcelFile(Stream excelFile)
        {
            using (var package = new ExcelPackage(excelFile))
            {
                var worksheet = package.Workbook.Worksheets[DonationWorksheetName];
                int rowCount = worksheet.Dimension.Rows;

                var headers = new[] { PartyColumn, YearColumn, DoneeColumn, NumberOfDonationsColumn, NumberOfDoneesColumn, DoneeAddressColumn, PostcodeColumn, AmountColumn };
                var columns = new Dictionary<string, int>();

                for (int colIndex = 1; colIndex <= 12; colIndex++)
                {
                    var cell = worksheet.Cells[1, colIndex];
                    var cellValue = cell.Text;
                    if (headers.Contains(cellValue))
                    {
                        columns[cellValue] = colIndex;
                    }
                }

                var donations = new List<Donation>();
                for (int row = 2; row <= rowCount; row++)
                {
                    var donation = new Donation
                    {
                        Party = worksheet.Cells[row, columns[PartyColumn]].Text,
                        Amount = Convert.ToDecimal(worksheet.Cells[row, columns[AmountColumn]].Value),
                        Donee = worksheet.Cells[row, columns[DoneeColumn]].Text,
                        DoneeAddress = worksheet.Cells[row, columns[DoneeAddressColumn]].Text,
                        NumberOfDonations = ParseInt(worksheet.Cells[row, columns[NumberOfDonationsColumn]].Text),
                        NumberOfDonees = ParseInt(worksheet.Cells[row, columns[NumberOfDoneesColumn]].Text),
                        Postcode = worksheet.Cells[row, columns[PostcodeColumn]].Text,
                        Year = ParseInt(worksheet.Cells[row, columns[YearColumn]].Text)
                    };

                    donations.Add(donation);
                }
                return donations;
            }
        }

        private static int ParseInt(string str)
        {
            int val;
            if (int.TryParse(str, out val))
            {
                return val;
            }
            return 0;
        }
    }
}