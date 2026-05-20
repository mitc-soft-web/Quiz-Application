using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quiz_Application.Migrations
{
    public partial class InitialPostgres : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CourseName = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Category = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Courses", x => x.Id));

            migrationBuilder.CreateTable(
                name: "UserResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    QuestionId = table.Column<Guid>(nullable: false),
                    SelectedOption = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_UserResponses", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    LanguageName = table.Column<string>(nullable: true),
                    CourseName = table.Column<string>(nullable: true),
                    CourseId = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Languages_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<string>(nullable: true),
                    CourseId = table.Column<Guid>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    Level = table.Column<string>(nullable: true),
                    ExternalCourseId = table.Column<int>(nullable: true),
                    TotalQuestions = table.Column<int>(nullable: true),
                    Score = table.Column<int>(nullable: true),
                    LanguageId = table.Column<Guid>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quizzes_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Quizzes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    QuestionText = table.Column<string>(nullable: true),
                    OptionA = table.Column<string>(nullable: true),
                    OptionB = table.Column<string>(nullable: true),
                    OptionC = table.Column<string>(nullable: true),
                    OptionD = table.Column<string>(nullable: true),
                    CorrectOption = table.Column<string>(nullable: true),
                    QuizId = table.Column<Guid>(nullable: false),
                    LanguageId = table.Column<Guid>(nullable: false),
                    Difficulty = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Questions_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    QuizId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    TotalQuestions = table.Column<int>(nullable: false),
                    CorrectAnswers = table.Column<int>(nullable: false),
                    Score = table.Column<int>(nullable: false),
                    CompletedDate = table.Column<DateTime>(nullable: false),
                    Remarks = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Results_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Results_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    QuestionId = table.Column<Guid>(nullable: false),
                    SelectedOption = table.Column<string>(nullable: true),
                    AnswerText = table.Column<string>(nullable: true),
                    IsCorrect = table.Column<bool>(nullable: false),
                    QuizId = table.Column<Guid>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Answers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Suggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    LanguageId = table.Column<Guid>(nullable: false),
                    CourseId = table.Column<Guid>(nullable: false),
                    ResultId = table.Column<Guid>(nullable: false),
                    Suggestions = table.Column<string>(nullable: false),
                    ResourceLink = table.Column<string>(nullable: false),
                    SavedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Suggestions_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Suggestions_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Suggestions_Results_ResultId",
                        column: x => x.ResultId,
                        principalTable: "Results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Suggestions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_Answers_QuestionId", "Answers", "QuestionId");
            migrationBuilder.CreateIndex("IX_Languages_CourseId", "Languages", "CourseId");
            migrationBuilder.CreateIndex("IX_Questions_LanguageId", "Questions", "LanguageId");
            migrationBuilder.CreateIndex("IX_Questions_QuizId", "Questions", "QuizId");
            migrationBuilder.CreateIndex("IX_Quizzes_LanguageId", "Quizzes", "LanguageId");
            migrationBuilder.CreateIndex("IX_Quizzes_UserId", "Quizzes", "UserId");
            migrationBuilder.CreateIndex("IX_Results_QuizId", "Results", "QuizId", unique: true);
            migrationBuilder.CreateIndex("IX_Results_UserId", "Results", "UserId");
            migrationBuilder.CreateIndex("IX_Suggestions_CourseId", "Suggestions", "CourseId");
            migrationBuilder.CreateIndex("IX_Suggestions_LanguageId", "Suggestions", "LanguageId");
            migrationBuilder.CreateIndex("IX_Suggestions_ResultId", "Suggestions", "ResultId");
            migrationBuilder.CreateIndex("IX_Suggestions_UserId", "Suggestions", "UserId");
            migrationBuilder.CreateIndex("IX_Users_CourseId", "Users", "CourseId");
            migrationBuilder.CreateIndex("IX_Users_Email", "Users", "Email", unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Answers");
            migrationBuilder.DropTable("Suggestions");
            migrationBuilder.DropTable("UserResponses");
            migrationBuilder.DropTable("Questions");
            migrationBuilder.DropTable("Results");
            migrationBuilder.DropTable("Quizzes");
            migrationBuilder.DropTable("Languages");
            migrationBuilder.DropTable("Users");
            migrationBuilder.DropTable("Courses");
        }
    }
}
