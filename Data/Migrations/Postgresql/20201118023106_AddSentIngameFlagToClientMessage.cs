﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations.Postgresql
{
    public partial class AddSentIngameFlagToClientMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SentIngame",
                table: "EFClientMessages",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SentIngame",
                table: "EFClientMessages");
        }
    }
}
