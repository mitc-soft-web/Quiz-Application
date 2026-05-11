namespace Quiz_Application.Models.DTO
{
    public class DashBoardViewModel
    {
        // User Greeting
        public string Username { get; set; } = string.Empty;
        public DateTime MemberSince { get; set; }
        public int TotalCourse { get; set; }
        public double AverageScore { get; set; }
        public string BestCourse { get; set; } = string.Empty;
        public string WeakestSkill { get; set; } = string.Empty;
        public int TotalAssessments { get; set; }

        public List<AssessmentSummaryVm> RecentAssessments { get; set; } = new();

        public List<CoursePerformanceVm> SkillPerformances { get; set; } = new();

        // Suggestions Highlights
        public List<SuggestionVm> TopSuggestions { get; set; } = new();

        // Trend Data
        public List<AssessmentTrendVm> AssessmentTrends { get; set; } = new();

    }

    public class AssessmentSummaryVm
    {
        public Guid AssessmentId { get; set; }
        public string LanguaeName { get; set; } = string.Empty;
        public double Score { get; set; }
        public string Level { get; set; } = string.Empty;
        public DateTime TakenOn { get; set; }
    }

    public class CoursePerformanceVm
    {
        public string CourseName { get; set; } = string.Empty;
        public double AverageScore { get; set; }
        public string CurrentLevel { get; set; } = string.Empty;
    }

    public class SuggestionVm
    {
        public string LanguageName { get; set; } = string.Empty;
        public string ImprovementTip { get; set; } = string.Empty;
    }

    public class AssessmentTrendVm
    {
        public DateTime Date { get; set; }
        public double Score { get; set; }
    }
}
