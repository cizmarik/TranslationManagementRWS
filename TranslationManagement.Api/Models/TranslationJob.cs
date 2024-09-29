namespace TranslationManagement.Api.Models
{
    public class TranslationJob : ITranslationJob
    {
        public int Id { get; set; }

        /// <summary>
        /// 0 = don't have translator yet
        /// </summary>
        public int TranslatorID { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public string OriginalContent { get; set; }
        public string TranslatedContent { get; set; }
        public double Price { get; set; }
        
    }
}
