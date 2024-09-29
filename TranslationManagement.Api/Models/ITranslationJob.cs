namespace TranslationManagement.Api.Models
{
    public interface ITranslationJob
    {
        public int Id { get; set; }
        public int TranslatorID { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public string OriginalContent { get; set; }
        public string TranslatedContent { get; set; }
        public double Price { get; set; }
    }
}
