using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChinookHTMX.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Artist",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Artist__3214EC0709A11B34", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    ReportsTo = table.Column<int>(type: "INTEGER", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 70, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    Country = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 24, nullable: true),
                    Fax = table.Column<string>(type: "TEXT", maxLength: 24, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Employee__3214EC0738B4A554", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Employee__Report__4E88ABD4",
                        column: x => x.ReportsTo,
                        principalTable: "Employee",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Genre",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Genre__3214EC075565983F", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MediaTyp__3214EC07747EF97E", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Playlist",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Playlist__3214EC07F4D99BF1", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Album",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    ArtistId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Album__3214EC07B9A5C0AD", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Album__ArtistId__4CA06362",
                        column: x => x.ArtistId,
                        principalTable: "Artist",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Company = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 70, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    Country = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 24, nullable: true),
                    Fax = table.Column<string>(type: "TEXT", maxLength: 24, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    SupportRepId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Customer__3214EC07F1CFC9FD", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Customer__Suppor__4D94879B",
                        column: x => x.SupportRepId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Track",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AlbumId = table.Column<int>(type: "INTEGER", nullable: true),
                    MediaTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    GenreId = table.Column<int>(type: "INTEGER", nullable: true),
                    Composer = table.Column<string>(type: "TEXT", maxLength: 220, nullable: true),
                    Milliseconds = table.Column<int>(type: "INTEGER", nullable: false),
                    Bytes = table.Column<int>(type: "INTEGER", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(10, 2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Track__3214EC07065C803A", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Track__AlbumId__5441852A",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Track__GenreId__5535A963",
                        column: x => x.GenreId,
                        principalTable: "Genre",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Track__MediaType__5629CD9C",
                        column: x => x.MediaTypeId,
                        principalTable: "MediaType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Invoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    BillingAddress = table.Column<string>(type: "TEXT", maxLength: 70, nullable: true),
                    BillingCity = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    BillingState = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    BillingCountry = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    BillingPostalCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Total = table.Column<decimal>(type: "numeric(10, 2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Invoice__3214EC07D189B35A", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Invoice__Custome__4F7CD00D",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlaylistTrack",
                columns: table => new
                {
                    PlaylistId = table.Column<int>(type: "INTEGER", nullable: false),
                    TrackId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Playlist__A4A6282E7C13F6BC", x => new { x.PlaylistId, x.TrackId });
                    table.ForeignKey(
                        name: "FK__PlaylistT__Playl__52593CB8",
                        column: x => x.PlaylistId,
                        principalTable: "Playlist",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__PlaylistT__Track__534D60F1",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InvoiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    TrackId = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(10, 2)", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__InvoiceL__3214EC071BEC499F", x => x.Id);
                    table.ForeignKey(
                        name: "FK__InvoiceLi__Invoi__5070F446",
                        column: x => x.InvoiceId,
                        principalTable: "Invoice",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__InvoiceLi__Track__5165187F",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IFK_Artist_Album",
                table: "Album",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IPK_ProductItem",
                table: "Album",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IPK_Artist",
                table: "Artist",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IFK_Employee_Customer",
                table: "Customer",
                column: "SupportRepId");

            migrationBuilder.CreateIndex(
                name: "IPK_Customer",
                table: "Customer",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IFK_Employee_ReportsTo",
                table: "Employee",
                column: "ReportsTo");

            migrationBuilder.CreateIndex(
                name: "IPK_Employee",
                table: "Employee",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IPK_Genre",
                table: "Genre",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IFK_Customer_Invoice",
                table: "Invoice",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IPK_Invoice",
                table: "Invoice",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IFK_Invoice_InvoiceLine",
                table: "InvoiceLine",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IFK_ProductItem_InvoiceLine",
                table: "InvoiceLine",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IPK_InvoiceLine",
                table: "InvoiceLine",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IPK_MediaType",
                table: "MediaType",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IPK_Playlist",
                table: "Playlist",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IFK_Playlist_PlaylistTrack",
                table: "PlaylistTrack",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IFK_Track_PlaylistTrack",
                table: "PlaylistTrack",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IPK_PlaylistTrack",
                table: "PlaylistTrack",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IFK_Album_Track",
                table: "Track",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IFK_Genre_Track",
                table: "Track",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IFK_MediaType_Track",
                table: "Track",
                column: "MediaTypeId");

            migrationBuilder.CreateIndex(
                name: "IPK_Track",
                table: "Track",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceLine");

            migrationBuilder.DropTable(
                name: "PlaylistTrack");

            migrationBuilder.DropTable(
                name: "Invoice");

            migrationBuilder.DropTable(
                name: "Playlist");

            migrationBuilder.DropTable(
                name: "Track");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "Album");

            migrationBuilder.DropTable(
                name: "Genre");

            migrationBuilder.DropTable(
                name: "MediaType");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "Artist");
        }
    }
}
