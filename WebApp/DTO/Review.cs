namespace WebApp.DTO
{
    public class ReviewDTO
    {
        public string Requirementsid { get; set; } = null!;

        public string Comment { get; set; } = null!;

        public DateTime? Dateofupdate { get; set; }
        public List<ReviewDetails> ReviewDetails { get; set; } = new();
    }

    public class ReviewDetails
    {
        public string CriteriaName { get; set; } = null!;
        public int Star { get; set; }
    }
}
