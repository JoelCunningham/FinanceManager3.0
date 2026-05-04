using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinanceManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BudgetYears",
                columns: new[] { "Id", "Scope", "Year" },
                values: new object[,]
                {
                    { new Guid("071e6505-028e-67b8-a567-5e6a78f1441e"), 0, 2024 },
                    { new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), 2, 2026 },
                    { new Guid("6f8a9e01-2428-ebf5-2bb8-6a6e68ffbae3"), 1, 2025 }
                });

            migrationBuilder.InsertData(
                table: "CategoryGroups",
                columns: new[] { "Id", "Colour", "IsIncome", "Name" },
                values: new object[,]
                {
                    { new Guid("1646550e-6916-1079-ba01-e5cec7235b7c"), "#54478C", false, "Financial" },
                    { new Guid("1e1b0eec-efff-7720-dec2-be6f86e2218d"), "#048BA8", false, "Gifts" },
                    { new Guid("35996ab8-0fdc-3e69-07d3-038cf34b013d"), "#16DB93", false, "Housing" },
                    { new Guid("41fd5a96-dfa1-c346-f200-615f1e3819da"), "#54478C", true, "Employment" },
                    { new Guid("73cde36e-333b-716c-07cd-2034ff341b2d"), "#F1C453", false, "Transportation" },
                    { new Guid("a9499028-0ca8-09d8-c76b-b123fd5389c3"), "#048BA8", true, "Support" },
                    { new Guid("b4d9e2bd-f6ce-aaa3-f088-5db86b4b2c84"), "#0DB39E", false, "Health" },
                    { new Guid("c62f0785-cd46-1598-f91f-96cd5b0c2000"), "#83E377", false, "Investing" },
                    { new Guid("cbc8ceb8-bffb-28b0-9521-16dd5365e763"), "#B9E769", false, "Personal" },
                    { new Guid("d7f03733-edcb-08d6-7c42-058ddf10c22e"), "#EFEA5A", false, "Recreation" },
                    { new Guid("da8eb203-45f4-4246-be7a-32400f28af7f"), "#2C699A", true, "Investment" },
                    { new Guid("f6877abd-9e18-6cd8-d879-a8765e15f922"), "#2C699A", false, "Food" },
                    { new Guid("f9c4d5cf-ef6e-bb5a-c7ed-73bfe297e22d"), "#F29E4C", false, "Utilities" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Colour", "GroupId", "Name" },
                values: new object[,]
                {
                    { new Guid("0bcf2a54-f685-7120-acee-a233d4b6a1f7"), "#048BA8", new Guid("a9499028-0ca8-09d8-c76b-b123fd5389c3"), "Allowance" },
                    { new Guid("19c8c320-7676-8b16-b987-7270ce133291"), "#B9E769", new Guid("cbc8ceb8-bffb-28b0-9521-16dd5365e763"), "Lifestyle" },
                    { new Guid("1c068252-c4fb-42b0-2bc3-83a56d30049c"), "#EFEA5A", new Guid("d7f03733-edcb-08d6-7c42-058ddf10c22e"), "Activities" },
                    { new Guid("1cb76f90-ef3c-f911-c056-6c119ba798f6"), "#83E377", new Guid("c62f0785-cd46-1598-f91f-96cd5b0c2000"), "Real Estate" },
                    { new Guid("1f56ce83-31c8-a1b4-ccc9-91b871a862d8"), "#B9E769", new Guid("cbc8ceb8-bffb-28b0-9521-16dd5365e763"), "Tuition" },
                    { new Guid("2a02555c-eedb-63b8-6bf7-31cb7881eae3"), "#B9E769", new Guid("cbc8ceb8-bffb-28b0-9521-16dd5365e763"), "Grooming" },
                    { new Guid("3193562e-275e-0850-31b4-974eff007b73"), "#F1C453", new Guid("73cde36e-333b-716c-07cd-2034ff341b2d"), "Fuel" },
                    { new Guid("3203c998-b0d1-930e-14d9-643d0a8b013a"), "#F1C453", new Guid("73cde36e-333b-716c-07cd-2034ff341b2d"), "Taxi" },
                    { new Guid("39577e80-5652-74b3-2673-bf85e294c464"), "#83E377", new Guid("c62f0785-cd46-1598-f91f-96cd5b0c2000"), "Superannuation" },
                    { new Guid("41c98146-4c8a-c63a-8209-e939ca0ce622"), "#2C699A", new Guid("f6877abd-9e18-6cd8-d879-a8765e15f922"), "Coffee" },
                    { new Guid("43095cea-c7a8-ad7c-dfb7-d44c289a1df2"), "#16DB93", new Guid("35996ab8-0fdc-3e69-07d3-038cf34b013d"), "Furniture" },
                    { new Guid("47919a35-a424-d01a-27a4-71c0d5bddfa8"), "#0DB39E", new Guid("b4d9e2bd-f6ce-aaa3-f088-5db86b4b2c84"), "Medical Services" },
                    { new Guid("4a75b957-8f25-a61c-8dd5-35dd01fff8c1"), "#F1C453", new Guid("73cde36e-333b-716c-07cd-2034ff341b2d"), "Maintenance" },
                    { new Guid("4ba174b2-3014-fcc3-35f6-86e96db9d648"), "#F29E4C", new Guid("f9c4d5cf-ef6e-bb5a-c7ed-73bfe297e22d"), "Internet" },
                    { new Guid("4dd6c9d0-546e-ba8d-aed4-778239f3fa1f"), "#16DB93", new Guid("35996ab8-0fdc-3e69-07d3-038cf34b013d"), "Manchester" },
                    { new Guid("4fc43ea6-b1a9-bd2a-4b65-aeae8820b99e"), "#0DB39E", new Guid("b4d9e2bd-f6ce-aaa3-f088-5db86b4b2c84"), "Medication" },
                    { new Guid("52f17db3-796d-f82d-474f-f235393b52a8"), "#54478C", new Guid("1646550e-6916-1079-ba01-e5cec7235b7c"), "Bank Fees" },
                    { new Guid("54a2a109-285a-dbee-40a5-861c9aac1700"), "#B9E769", new Guid("cbc8ceb8-bffb-28b0-9521-16dd5365e763"), "Cosmetics" },
                    { new Guid("59a2c369-e974-67c7-d335-b3f9786ba939"), "#2C699A", new Guid("f6877abd-9e18-6cd8-d879-a8765e15f922"), "Dining Out" },
                    { new Guid("5dfae572-c2b6-48bb-8d04-ad97b5390701"), "#048BA8", new Guid("a9499028-0ca8-09d8-c76b-b123fd5389c3"), "Unemployment" },
                    { new Guid("6deea436-dfed-9b89-89ed-9e6928881a76"), "#54478C", new Guid("41fd5a96-dfa1-c346-f200-615f1e3819da"), "Bonus" },
                    { new Guid("6e7d0a5c-8711-75b0-5588-f8b580359994"), "#2C699A", new Guid("f6877abd-9e18-6cd8-d879-a8765e15f922"), "Groceries" },
                    { new Guid("704f1753-ddab-ef32-ddfc-6d7c845afee6"), "#048BA8", new Guid("a9499028-0ca8-09d8-c76b-b123fd5389c3"), "Pension" },
                    { new Guid("71aece19-9360-1fac-494e-f9205f50179c"), "#0DB39E", new Guid("b4d9e2bd-f6ce-aaa3-f088-5db86b4b2c84"), "Ambulance Cover" },
                    { new Guid("7617c09c-6e50-58e3-8d0d-90a1eba13580"), "#16DB93", new Guid("35996ab8-0fdc-3e69-07d3-038cf34b013d"), "Accommodation" },
                    { new Guid("762c574f-9032-6e8e-5523-2cb247e4fc79"), "#16DB93", new Guid("35996ab8-0fdc-3e69-07d3-038cf34b013d"), "Rent" },
                    { new Guid("78d04929-9683-26a2-e2df-ad7f359665c9"), "#EFEA5A", new Guid("d7f03733-edcb-08d6-7c42-058ddf10c22e"), "Entertainment" },
                    { new Guid("79aa7f1d-79b2-ee78-2d75-22de74b4f1da"), "#0DB39E", new Guid("b4d9e2bd-f6ce-aaa3-f088-5db86b4b2c84"), "Supplements" },
                    { new Guid("7b11f8f1-148c-9205-6970-4d08f603ea23"), "#F1C453", new Guid("73cde36e-333b-716c-07cd-2034ff341b2d"), "Insurance" },
                    { new Guid("7fdcc34a-438b-0e1d-39ac-2e2fd423267e"), "#83E377", new Guid("c62f0785-cd46-1598-f91f-96cd5b0c2000"), "EFTs" },
                    { new Guid("832dc26e-7be4-5e67-aa2c-c8153a3aaeee"), "#F1C453", new Guid("73cde36e-333b-716c-07cd-2034ff341b2d"), "Roadside Assistance" },
                    { new Guid("865c5185-1ab0-17bb-d45d-71c79518fa47"), "#54478C", new Guid("1646550e-6916-1079-ba01-e5cec7235b7c"), "Credit Card Fees" },
                    { new Guid("86dc2c35-a408-b39a-4af5-5f51084dde46"), "#F29E4C", new Guid("f9c4d5cf-ef6e-bb5a-c7ed-73bfe297e22d"), "Gas" },
                    { new Guid("8b0689a8-1fba-72a6-f92b-afe39f6a6488"), "#F29E4C", new Guid("f9c4d5cf-ef6e-bb5a-c7ed-73bfe297e22d"), "Phone" },
                    { new Guid("91ac66cd-33d2-eb83-613f-11fe0fdeb741"), "#B9E769", new Guid("cbc8ceb8-bffb-28b0-9521-16dd5365e763"), "Hygiene" },
                    { new Guid("92110c82-632c-8b70-4ca6-b854abf886cc"), "#F1C453", new Guid("73cde36e-333b-716c-07cd-2034ff341b2d"), "Registration" },
                    { new Guid("93a7d7ae-f9ba-6095-00e3-af69eb0b1af0"), "#048BA8", new Guid("1e1b0eec-efff-7720-dec2-be6f86e2218d"), "Charity" },
                    { new Guid("9519dc36-b632-2177-c506-4a9bad98aa42"), "#2C699A", new Guid("da8eb203-45f4-4246-be7a-32400f28af7f"), "Rental Income" },
                    { new Guid("99c42f85-be8d-816e-83d4-066daae18dc7"), "#F1C453", new Guid("73cde36e-333b-716c-07cd-2034ff341b2d"), "Fees" },
                    { new Guid("9b3c6308-2439-31e5-edae-9bf1bfbf1b37"), "#83E377", new Guid("c62f0785-cd46-1598-f91f-96cd5b0c2000"), "Stocks" },
                    { new Guid("a4bf5c0d-95de-99a1-21d5-57439d215eee"), "#0DB39E", new Guid("b4d9e2bd-f6ce-aaa3-f088-5db86b4b2c84"), "Fitness" },
                    { new Guid("af5d67de-3557-0a99-9592-ac0df06c47ff"), "#F29E4C", new Guid("f9c4d5cf-ef6e-bb5a-c7ed-73bfe297e22d"), "Water" },
                    { new Guid("b15edf39-0ccc-a4f2-52a7-d7d290b7c74e"), "#048BA8", new Guid("1e1b0eec-efff-7720-dec2-be6f86e2218d"), "Presents" },
                    { new Guid("b97d8891-3fd2-2d51-a300-bd3822a9df13"), "#B9E769", new Guid("cbc8ceb8-bffb-28b0-9521-16dd5365e763"), "Clothing" },
                    { new Guid("ba630310-8509-65fe-aa89-57586fd4ba87"), "#048BA8", new Guid("1e1b0eec-efff-7720-dec2-be6f86e2218d"), "Donations" },
                    { new Guid("c22dccb9-624f-b91c-75f7-e5d7d979ef6e"), "#F1C453", new Guid("73cde36e-333b-716c-07cd-2034ff341b2d"), "Public Transport" },
                    { new Guid("c93898d9-4345-1397-e986-f690aacedee7"), "#2C699A", new Guid("da8eb203-45f4-4246-be7a-32400f28af7f"), "Interest" },
                    { new Guid("ca2e2ad8-d0e5-f55a-bb28-651feb824a97"), "#F29E4C", new Guid("f9c4d5cf-ef6e-bb5a-c7ed-73bfe297e22d"), "Electricity" },
                    { new Guid("cb94032d-63ea-9a90-5828-869a0dce0938"), "#0DB39E", new Guid("b4d9e2bd-f6ce-aaa3-f088-5db86b4b2c84"), "Insurance" },
                    { new Guid("d045dbb5-c5ba-aec9-236a-a16e9c041d17"), "#EFEA5A", new Guid("d7f03733-edcb-08d6-7c42-058ddf10c22e"), "Books" },
                    { new Guid("dae31b3d-738a-f7e4-7133-36652f437f44"), "#16DB93", new Guid("35996ab8-0fdc-3e69-07d3-038cf34b013d"), "Maintenance" },
                    { new Guid("dcda26d0-a7a3-2196-2b57-9dffa9195e6d"), "#F1C453", new Guid("73cde36e-333b-716c-07cd-2034ff341b2d"), "Flights" },
                    { new Guid("df78a32b-c7f1-49ef-3d51-9107a38022b7"), "#2C699A", new Guid("da8eb203-45f4-4246-be7a-32400f28af7f"), "Dividends" },
                    { new Guid("dfd15343-dfc7-f1b4-8a16-ec37f108f71d"), "#F1C453", new Guid("73cde36e-333b-716c-07cd-2034ff341b2d"), "Car Hire" },
                    { new Guid("e21db71e-33ec-61b5-2932-fd1529589078"), "#16DB93", new Guid("35996ab8-0fdc-3e69-07d3-038cf34b013d"), "Rates & Taxes" },
                    { new Guid("e5187a8b-5cb7-2bb6-d389-55f5b8002b80"), "#16DB93", new Guid("35996ab8-0fdc-3e69-07d3-038cf34b013d"), "Mortgage" },
                    { new Guid("edde9471-7608-76af-dcf9-559581f8a684"), "#54478C", new Guid("41fd5a96-dfa1-c346-f200-615f1e3819da"), "Salary" },
                    { new Guid("ee1f4402-9744-4180-4056-5ac90335edd6"), "#54478C", new Guid("1646550e-6916-1079-ba01-e5cec7235b7c"), "Accountant" },
                    { new Guid("ee9dba81-9029-42f1-b299-2558dc5c433f"), "#54478C", new Guid("41fd5a96-dfa1-c346-f200-615f1e3819da"), "Freelance" },
                    { new Guid("fde256b0-0e08-12cb-43c4-7ad85ef80976"), "#EFEA5A", new Guid("d7f03733-edcb-08d6-7c42-058ddf10c22e"), "Alcohol" }
                });

            migrationBuilder.InsertData(
                table: "BudgetEntries",
                columns: new[] { "Id", "Amount", "BudgetYearId", "CategoryId", "Length", "Notes", "ScopePosition" },
                values: new object[,]
                {
                    { new Guid("11177dae-9d42-ac52-be04-f9fb49f998fc"), 100m, new Guid("6f8a9e01-2428-ebf5-2bb8-6a6e68ffbae3"), new Guid("59a2c369-e974-67c7-d335-b3f9786ba939"), 20, null, 2 },
                    { new Guid("431638db-b408-e7e3-9f8a-55c8f1337287"), 50m, new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), new Guid("54a2a109-285a-dbee-40a5-861c9aac1700"), 0, null, 9 },
                    { new Guid("5573a051-0da2-9490-f3cc-f8e3166cc49c"), 50m, new Guid("6f8a9e01-2428-ebf5-2bb8-6a6e68ffbae3"), new Guid("a4bf5c0d-95de-99a1-21d5-57439d215eee"), 24, null, 0 },
                    { new Guid("5aa80d28-32f0-eeef-bce0-b75c8342e1ad"), 4564.85m, new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), new Guid("edde9471-7608-76af-dcf9-559581f8a684"), 12, null, 0 },
                    { new Guid("65a84c8b-151b-f58e-429a-fac0ded1d207"), 50m, new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), new Guid("54a2a109-285a-dbee-40a5-861c9aac1700"), 0, null, 0 },
                    { new Guid("8aceb345-e684-1569-9e20-71d352267586"), 50m, new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), new Guid("54a2a109-285a-dbee-40a5-861c9aac1700"), 0, null, 8 },
                    { new Guid("98f9cfbc-5c58-b7a8-a508-937de00706c7"), 50m, new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), new Guid("a4bf5c0d-95de-99a1-21d5-57439d215eee"), 8, null, 2 },
                    { new Guid("9c8ffde0-85ce-e65f-24a3-c4dd504d5e23"), 50m, new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), new Guid("54a2a109-285a-dbee-40a5-861c9aac1700"), 0, null, 2 },
                    { new Guid("9f80f866-e242-df06-bf87-da99cb8da175"), 50m, new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), new Guid("54a2a109-285a-dbee-40a5-861c9aac1700"), 0, null, 10 },
                    { new Guid("b0de3032-d308-49fe-ff7f-efcbc925a3c5"), 100m, new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), new Guid("59a2c369-e974-67c7-d335-b3f9786ba939"), 12, null, 0 },
                    { new Guid("cad01ba1-c8d0-07b6-66c3-dff0ee8dfbdf"), 50m, new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), new Guid("54a2a109-285a-dbee-40a5-861c9aac1700"), 0, null, 12 },
                    { new Guid("e8d71a02-ee25-2f07-9d3c-18784939719c"), 50m, new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), new Guid("54a2a109-285a-dbee-40a5-861c9aac1700"), 0, null, 11 },
                    { new Guid("eeda34ee-9dcd-b6ea-1e24-619f98e5276c"), 50m, new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), new Guid("54a2a109-285a-dbee-40a5-861c9aac1700"), 0, null, 1 },
                    { new Guid("f1876e3c-76b0-944f-3491-a439f25bd493"), 50m, new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), new Guid("54a2a109-285a-dbee-40a5-861c9aac1700"), 0, null, 7 },
                    { new Guid("f3475e30-0f20-6ccb-e1bb-7c0e570e8b6a"), 50m, new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), new Guid("54a2a109-285a-dbee-40a5-861c9aac1700"), 0, null, 5 },
                    { new Guid("f72da5a7-11d7-2579-9576-db25276f548d"), 50m, new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), new Guid("54a2a109-285a-dbee-40a5-861c9aac1700"), 0, null, 6 },
                    { new Guid("f9048b38-24f4-54c4-3b53-6ea1be7a42ba"), 50m, new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), new Guid("54a2a109-285a-dbee-40a5-861c9aac1700"), 0, null, 4 },
                    { new Guid("fb406164-3442-2097-e9c0-f92167139ce6"), 50m, new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"), new Guid("54a2a109-285a-dbee-40a5-861c9aac1700"), 0, null, 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("11177dae-9d42-ac52-be04-f9fb49f998fc"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("431638db-b408-e7e3-9f8a-55c8f1337287"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("5573a051-0da2-9490-f3cc-f8e3166cc49c"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("5aa80d28-32f0-eeef-bce0-b75c8342e1ad"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("65a84c8b-151b-f58e-429a-fac0ded1d207"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("8aceb345-e684-1569-9e20-71d352267586"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("98f9cfbc-5c58-b7a8-a508-937de00706c7"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("9c8ffde0-85ce-e65f-24a3-c4dd504d5e23"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("9f80f866-e242-df06-bf87-da99cb8da175"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("b0de3032-d308-49fe-ff7f-efcbc925a3c5"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("cad01ba1-c8d0-07b6-66c3-dff0ee8dfbdf"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("e8d71a02-ee25-2f07-9d3c-18784939719c"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("eeda34ee-9dcd-b6ea-1e24-619f98e5276c"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("f1876e3c-76b0-944f-3491-a439f25bd493"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("f3475e30-0f20-6ccb-e1bb-7c0e570e8b6a"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("f72da5a7-11d7-2579-9576-db25276f548d"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("f9048b38-24f4-54c4-3b53-6ea1be7a42ba"));

            migrationBuilder.DeleteData(
                table: "BudgetEntries",
                keyColumn: "Id",
                keyValue: new Guid("fb406164-3442-2097-e9c0-f92167139ce6"));

            migrationBuilder.DeleteData(
                table: "BudgetYears",
                keyColumn: "Id",
                keyValue: new Guid("071e6505-028e-67b8-a567-5e6a78f1441e"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("0bcf2a54-f685-7120-acee-a233d4b6a1f7"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("19c8c320-7676-8b16-b987-7270ce133291"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("1c068252-c4fb-42b0-2bc3-83a56d30049c"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("1cb76f90-ef3c-f911-c056-6c119ba798f6"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("1f56ce83-31c8-a1b4-ccc9-91b871a862d8"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2a02555c-eedb-63b8-6bf7-31cb7881eae3"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3193562e-275e-0850-31b4-974eff007b73"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3203c998-b0d1-930e-14d9-643d0a8b013a"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("39577e80-5652-74b3-2673-bf85e294c464"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("41c98146-4c8a-c63a-8209-e939ca0ce622"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("43095cea-c7a8-ad7c-dfb7-d44c289a1df2"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("47919a35-a424-d01a-27a4-71c0d5bddfa8"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4a75b957-8f25-a61c-8dd5-35dd01fff8c1"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4ba174b2-3014-fcc3-35f6-86e96db9d648"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4dd6c9d0-546e-ba8d-aed4-778239f3fa1f"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4fc43ea6-b1a9-bd2a-4b65-aeae8820b99e"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("52f17db3-796d-f82d-474f-f235393b52a8"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("5dfae572-c2b6-48bb-8d04-ad97b5390701"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("6deea436-dfed-9b89-89ed-9e6928881a76"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("6e7d0a5c-8711-75b0-5588-f8b580359994"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("704f1753-ddab-ef32-ddfc-6d7c845afee6"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("71aece19-9360-1fac-494e-f9205f50179c"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7617c09c-6e50-58e3-8d0d-90a1eba13580"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("762c574f-9032-6e8e-5523-2cb247e4fc79"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("78d04929-9683-26a2-e2df-ad7f359665c9"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("79aa7f1d-79b2-ee78-2d75-22de74b4f1da"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7b11f8f1-148c-9205-6970-4d08f603ea23"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7fdcc34a-438b-0e1d-39ac-2e2fd423267e"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("832dc26e-7be4-5e67-aa2c-c8153a3aaeee"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("865c5185-1ab0-17bb-d45d-71c79518fa47"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("86dc2c35-a408-b39a-4af5-5f51084dde46"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("8b0689a8-1fba-72a6-f92b-afe39f6a6488"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("91ac66cd-33d2-eb83-613f-11fe0fdeb741"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("92110c82-632c-8b70-4ca6-b854abf886cc"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("93a7d7ae-f9ba-6095-00e3-af69eb0b1af0"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("9519dc36-b632-2177-c506-4a9bad98aa42"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("99c42f85-be8d-816e-83d4-066daae18dc7"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("9b3c6308-2439-31e5-edae-9bf1bfbf1b37"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("af5d67de-3557-0a99-9592-ac0df06c47ff"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b15edf39-0ccc-a4f2-52a7-d7d290b7c74e"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b97d8891-3fd2-2d51-a300-bd3822a9df13"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ba630310-8509-65fe-aa89-57586fd4ba87"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c22dccb9-624f-b91c-75f7-e5d7d979ef6e"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c93898d9-4345-1397-e986-f690aacedee7"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ca2e2ad8-d0e5-f55a-bb28-651feb824a97"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("cb94032d-63ea-9a90-5828-869a0dce0938"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("d045dbb5-c5ba-aec9-236a-a16e9c041d17"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("dae31b3d-738a-f7e4-7133-36652f437f44"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("dcda26d0-a7a3-2196-2b57-9dffa9195e6d"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("df78a32b-c7f1-49ef-3d51-9107a38022b7"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("dfd15343-dfc7-f1b4-8a16-ec37f108f71d"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("e21db71e-33ec-61b5-2932-fd1529589078"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("e5187a8b-5cb7-2bb6-d389-55f5b8002b80"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ee1f4402-9744-4180-4056-5ac90335edd6"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ee9dba81-9029-42f1-b299-2558dc5c433f"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("fde256b0-0e08-12cb-43c4-7ad85ef80976"));

            migrationBuilder.DeleteData(
                table: "BudgetYears",
                keyColumn: "Id",
                keyValue: new Guid("0c07d10a-dc11-ed59-4c90-753a73437fcb"));

            migrationBuilder.DeleteData(
                table: "BudgetYears",
                keyColumn: "Id",
                keyValue: new Guid("6f8a9e01-2428-ebf5-2bb8-6a6e68ffbae3"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("54a2a109-285a-dbee-40a5-861c9aac1700"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("59a2c369-e974-67c7-d335-b3f9786ba939"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a4bf5c0d-95de-99a1-21d5-57439d215eee"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("edde9471-7608-76af-dcf9-559581f8a684"));

            migrationBuilder.DeleteData(
                table: "CategoryGroups",
                keyColumn: "Id",
                keyValue: new Guid("1646550e-6916-1079-ba01-e5cec7235b7c"));

            migrationBuilder.DeleteData(
                table: "CategoryGroups",
                keyColumn: "Id",
                keyValue: new Guid("1e1b0eec-efff-7720-dec2-be6f86e2218d"));

            migrationBuilder.DeleteData(
                table: "CategoryGroups",
                keyColumn: "Id",
                keyValue: new Guid("35996ab8-0fdc-3e69-07d3-038cf34b013d"));

            migrationBuilder.DeleteData(
                table: "CategoryGroups",
                keyColumn: "Id",
                keyValue: new Guid("73cde36e-333b-716c-07cd-2034ff341b2d"));

            migrationBuilder.DeleteData(
                table: "CategoryGroups",
                keyColumn: "Id",
                keyValue: new Guid("a9499028-0ca8-09d8-c76b-b123fd5389c3"));

            migrationBuilder.DeleteData(
                table: "CategoryGroups",
                keyColumn: "Id",
                keyValue: new Guid("c62f0785-cd46-1598-f91f-96cd5b0c2000"));

            migrationBuilder.DeleteData(
                table: "CategoryGroups",
                keyColumn: "Id",
                keyValue: new Guid("d7f03733-edcb-08d6-7c42-058ddf10c22e"));

            migrationBuilder.DeleteData(
                table: "CategoryGroups",
                keyColumn: "Id",
                keyValue: new Guid("da8eb203-45f4-4246-be7a-32400f28af7f"));

            migrationBuilder.DeleteData(
                table: "CategoryGroups",
                keyColumn: "Id",
                keyValue: new Guid("f9c4d5cf-ef6e-bb5a-c7ed-73bfe297e22d"));

            migrationBuilder.DeleteData(
                table: "CategoryGroups",
                keyColumn: "Id",
                keyValue: new Guid("41fd5a96-dfa1-c346-f200-615f1e3819da"));

            migrationBuilder.DeleteData(
                table: "CategoryGroups",
                keyColumn: "Id",
                keyValue: new Guid("b4d9e2bd-f6ce-aaa3-f088-5db86b4b2c84"));

            migrationBuilder.DeleteData(
                table: "CategoryGroups",
                keyColumn: "Id",
                keyValue: new Guid("cbc8ceb8-bffb-28b0-9521-16dd5365e763"));

            migrationBuilder.DeleteData(
                table: "CategoryGroups",
                keyColumn: "Id",
                keyValue: new Guid("f6877abd-9e18-6cd8-d879-a8765e15f922"));
        }
    }
}
