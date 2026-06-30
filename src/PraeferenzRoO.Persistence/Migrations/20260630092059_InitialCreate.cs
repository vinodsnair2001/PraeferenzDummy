using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PraeferenzRoO.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "countries",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    iso_code2 = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    iso_code3 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    is_eu_member = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    machine = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_countries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "finished_products",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    hs_code_value = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ex_work_price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    machine = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_finished_products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hs_codes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    parent_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    machine = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hs_codes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "materials",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    hs_code_value = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    origin_country_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    is_originating = table.Column<bool>(type: "boolean", nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    machine = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_materials", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "trade_agreements",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    effective_date = table.Column<DateOnly>(type: "date", nullable: false),
                    expiry_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    machine = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trade_agreements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    refresh_token_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    refresh_token_expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    role = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    machine = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_materials",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    finished_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    material_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_cost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    created_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    machine = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_materials", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_materials_finished_products",
                        column: x => x.finished_product_id,
                        principalSchema: "public",
                        principalTable: "finished_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_materials_materials",
                        column: x => x.material_id,
                        principalSchema: "public",
                        principalTable: "materials",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "origin_calculations",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    finished_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trade_agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    country_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_originating = table.Column<bool>(type: "boolean", nullable: true),
                    decision_summary = table.Column<string>(type: "text", nullable: true),
                    decision_tree_json = table.Column<string>(type: "text", nullable: true),
                    calculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    machine = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_origin_calculations", x => x.id);
                    table.ForeignKey(
                        name: "fk_origin_calculations_countries",
                        column: x => x.country_id,
                        principalSchema: "public",
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_origin_calculations_finished_products",
                        column: x => x.finished_product_id,
                        principalSchema: "public",
                        principalTable: "finished_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_origin_calculations_trade_agreements",
                        column: x => x.trade_agreement_id,
                        principalSchema: "public",
                        principalTable: "trade_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_rules",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    rule_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    rule_category = table.Column<int>(type: "integer", nullable: false),
                    rule_type = table.Column<int>(type: "integer", nullable: false),
                    expression = table.Column<string>(type: "text", nullable: true),
                    condition = table.Column<string>(type: "text", nullable: true),
                    parameters_json = table.Column<string>(type: "text", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    execution_order = table.Column<int>(type: "integer", nullable: false),
                    effective_date = table.Column<DateOnly>(type: "date", nullable: false),
                    expiry_date = table.Column<DateOnly>(type: "date", nullable: true),
                    trade_agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    country_id = table.Column<Guid>(type: "uuid", nullable: true),
                    hs_code_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    machine = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_rules", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_rules_countries",
                        column: x => x.country_id,
                        principalSchema: "public",
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_rules_hs_codes",
                        column: x => x.hs_code_id,
                        principalSchema: "public",
                        principalTable: "hs_codes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_rules_trade_agreements",
                        column: x => x.trade_agreement_id,
                        principalSchema: "public",
                        principalTable: "trade_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "origin_calculation_details",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origin_calculation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    rule_type = table.Column<int>(type: "integer", nullable: false),
                    passed = table.Column<bool>(type: "boolean", nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    evidence_json = table.Column<string>(type: "text", nullable: true),
                    execution_order = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    machine = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_origin_calculation_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_origin_calculation_details_origin_calculations",
                        column: x => x.origin_calculation_id,
                        principalSchema: "public",
                        principalTable: "origin_calculations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_countries_tenant_id",
                schema: "public",
                table: "countries",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uix_countries_iso_code2",
                schema: "public",
                table: "countries",
                column: "iso_code2",
                unique: true,
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "uix_countries_iso_code3",
                schema: "public",
                table: "countries",
                column: "iso_code3",
                unique: true,
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_finished_products_hs_code_value",
                schema: "public",
                table: "finished_products",
                column: "hs_code_value");

            migrationBuilder.CreateIndex(
                name: "ix_finished_products_tenant_id",
                schema: "public",
                table: "finished_products",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_hs_codes_code",
                schema: "public",
                table: "hs_codes",
                column: "code",
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_hs_codes_tenant_id",
                schema: "public",
                table: "hs_codes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uix_hs_codes_tenant_code",
                schema: "public",
                table: "hs_codes",
                columns: new[] { "tenant_id", "code" },
                unique: true,
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_materials_hs_code_value",
                schema: "public",
                table: "materials",
                column: "hs_code_value");

            migrationBuilder.CreateIndex(
                name: "ix_materials_origin_country_code",
                schema: "public",
                table: "materials",
                column: "origin_country_code");

            migrationBuilder.CreateIndex(
                name: "ix_materials_tenant_id",
                schema: "public",
                table: "materials",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_origin_calculation_details_origin_calculation_id",
                schema: "public",
                table: "origin_calculation_details",
                column: "origin_calculation_id");

            migrationBuilder.CreateIndex(
                name: "ix_origin_calculation_details_tenant_id",
                schema: "public",
                table: "origin_calculation_details",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_origin_calculations_country_id",
                schema: "public",
                table: "origin_calculations",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "ix_origin_calculations_finished_product_id",
                schema: "public",
                table: "origin_calculations",
                column: "finished_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_origin_calculations_tenant_id",
                schema: "public",
                table: "origin_calculations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_origin_calculations_trade_agreement_id",
                schema: "public",
                table: "origin_calculations",
                column: "trade_agreement_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_materials_finished_product_id",
                schema: "public",
                table: "product_materials",
                column: "finished_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_materials_material_id",
                schema: "public",
                table: "product_materials",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_materials_tenant_id",
                schema: "public",
                table: "product_materials",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "pix_product_materials_finished_product_material",
                schema: "public",
                table: "product_materials",
                columns: new[] { "finished_product_id", "material_id" },
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_product_rules_country_id",
                schema: "public",
                table: "product_rules",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_rules_hs_code_id",
                schema: "public",
                table: "product_rules",
                column: "hs_code_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_rules_rule_code",
                schema: "public",
                table: "product_rules",
                column: "rule_code");

            migrationBuilder.CreateIndex(
                name: "ix_product_rules_tenant_id",
                schema: "public",
                table: "product_rules",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_rules_trade_agreement_hs_code",
                schema: "public",
                table: "product_rules",
                columns: new[] { "trade_agreement_id", "hs_code_id" });

            migrationBuilder.CreateIndex(
                name: "ix_product_rules_trade_agreement_id",
                schema: "public",
                table: "product_rules",
                column: "trade_agreement_id");

            migrationBuilder.CreateIndex(
                name: "ix_trade_agreements_code",
                schema: "public",
                table: "trade_agreements",
                column: "code",
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_trade_agreements_tenant_id",
                schema: "public",
                table: "trade_agreements",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uix_trade_agreements_tenant_code",
                schema: "public",
                table: "trade_agreements",
                columns: new[] { "tenant_id", "code" },
                unique: true,
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "public",
                table: "users",
                column: "email",
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_users_tenant_id",
                schema: "public",
                table: "users",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uix_users_tenant_email",
                schema: "public",
                table: "users",
                columns: new[] { "tenant_id", "email" },
                unique: true,
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "uix_users_tenant_username",
                schema: "public",
                table: "users",
                columns: new[] { "tenant_id", "username" },
                unique: true,
                filter: "is_deleted = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Downward migrations are not supported. This project uses forward-only migrations.");
        }
    }
}
